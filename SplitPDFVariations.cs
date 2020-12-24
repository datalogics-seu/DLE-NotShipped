using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Datalogics.PDFL;

/*
 * 
 * A sample which demonstrates splitting a PDF document based on page intervals or bookmarks or by hits on 
 * key search strings. To split a document, the application needs to create a new, empty document and insert pages 
 * from the source document into the target documents(s)
 * 
 * This type of application/process might be used for splitting consolidated statement type reports - 
 * for example, a 1000 page financial PDF that is comprised of smaller 3-5 page reports representing individual accounts.
 * 
 * Copyright (c) 2007-2010, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.  Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 *
 */


namespace SplitPDFVariations
{
    class SplitPDFVariations
    {
        static void Main(string[] args)
        {
            string inputFile = "..\\..\\Resources\\Sample_Input\\Constitution.pdf";  // input document
 
            bool splitByBookmarks = false;           // extract by bookmarks if they exist
                                                     // 
            bool splitByTextString = false;          // extract by specified search string
            string splitTextString = "BREF APER�U";  //         string to search for
            bool splitByPageInterval = true;         // extract by specified number of page interval
            int splitPageInterval = 2;               //         page interval to use 

            List<int> listOfPageNumsToSplit = new List<int>();

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                Document doc = new Document(inputFile); //
                Console.WriteLine("Opened document " + inputFile);

                if (splitByTextString)
                {
                    FindTextUntagged(doc, splitTextString, listOfPageNumsToSplit);
                }
                else if (splitByBookmarks)
                {
                    Bookmark rootBookmark = doc.BookmarkRoot;
                    Console.WriteLine("Number of bookmarks = " + rootBookmark.Count);
                    EnumerateBookmarks(rootBookmark, listOfPageNumsToSplit);
                }
                else if (splitByPageInterval)
                {
                    FindPageSets(doc, splitPageInterval, listOfPageNumsToSplit);
                }

                if (listOfPageNumsToSplit.Count > 0)
                    SplitPDF(doc, listOfPageNumsToSplit);
                else
                    Console.WriteLine("No pages to split. Exiting.");

            }
        }

        static void SplitPDF(Document doc,  List<int> listOfPageNumsToSplit)
        {
            int numFiles = listOfPageNumsToSplit.Count;
            int numPagesToSplit = 0;
            Console.WriteLine("Splitting into " + numFiles + " files.");
            try
            {
                for (int j = 0; j < numFiles; j++)
                {
                    Document outDoc = new Document();
                    if (j < numFiles - 1)
                        numPagesToSplit = listOfPageNumsToSplit[j + 1] - listOfPageNumsToSplit[j];
                    else
                        numPagesToSplit = doc.NumPages - listOfPageNumsToSplit[j];

                    outDoc.InsertPages(Document.BeforeFirstPage, doc, listOfPageNumsToSplit[j], numPagesToSplit, PageInsertFlags.Bookmarks | PageInsertFlags.Threads);
                    outDoc.Save(SaveFlags.Full, "Split" + j + ".pdf");
                }
            }
            catch (ApplicationException ae)
            {
                Console.WriteLine(ae.Message);
            }
        }

        static void EnumerateBookmarks(Bookmark bMark, List<int> listOfPageNumsToSplit)
        {
            if (bMark != null)
            {
                Console.WriteLine("Bookmark Title: " + bMark.Title);
                ViewDestination vDest = bMark.ViewDestination;
                int count = 0;

                if (vDest != null)
                {
                    Console.WriteLine("Bookmark Destination = page: " + vDest.PageNumber);

                    // Multiple bookmarks can point to the same destination page, so skip repeats
                    if (listOfPageNumsToSplit.Contains(vDest.PageNumber) == false)
                        listOfPageNumsToSplit.Add(vDest.PageNumber);
                    count++;
                }
                EnumerateBookmarks(bMark.FirstChild, listOfPageNumsToSplit);
			    EnumerateBookmarks(bMark.Next, listOfPageNumsToSplit);
            }
        }


        static void FindPageSets(Document doc,  int splitPageInterval, List<int> listOfPageNumsToSplit)
        {
            int nPages = doc.NumPages;

            // PDF page numbers are 0 based (add 1 to get the user sequential page number).
            // Get the modulo (remainder). If the remainder is 0, then split on that page.
            // For example: 5 page document, split interval of 2, you want to split the
            // document at pages 0, 2, 4 (internal PDF page number) a.k.a pages 1, 3, 5.

            if (splitPageInterval < 1)
                splitPageInterval = 1;  // prevents invalid split interval / divide by 0 problems 

            listOfPageNumsToSplit.Add(0);   //Always split on the first page (page 0)

            for (int i = 1; i < doc.NumPages; i++)
            {
                if (i % splitPageInterval == 0)
                    listOfPageNumsToSplit.Add(i);
            }
        }
 

        /* This function is copied primarily from the TextExtract sample,
         * but modified to skip writing out the text that it finds
         */
       static void FindTextUntagged(Document doc, String splitTextString, List<int> listOfPageNumsToSplit)
        {
            // setup the WordFinderConfig
            WordFinderConfig wordConfig = new WordFinderConfig();
            wordConfig.IgnoreCharGaps = false;
            wordConfig.IgnoreLineGaps = false;
            wordConfig.NoAnnots = false;
            wordConfig.NoEncodingGuess = false;
            // Std Roman treatment for custom encoding; overrides the noEncodingGuess option
            wordConfig.UnknownToStdEnc = false;
            wordConfig.DisableTaggedPDF = false;    // legacy mode WordFinder creation
            wordConfig.NoXYSort = true;
            wordConfig.PreserveSpaces = false;
            wordConfig.NoLigatureExp = false;
            wordConfig.NoHyphenDetection = false;
            wordConfig.TrustNBSpace = false;
            wordConfig.NoExtCharOffset = false;     // text extraction efficiency
            wordConfig.NoStyleInfo = false;         // text extraction efficiency

            WordFinder wordFinder = new WordFinder(doc, WordFinderVersion.Latest, wordConfig);

            int nPages = doc.NumPages;
            IList<Word> pageWords = null;
    
            for (int i = 0; i < nPages; i++) 
            {
                pageWords = wordFinder.GetWordList(i);
                
                String textToExtract = "";

                // By default, this searches the entire page word list. 
                // You could limit it to the first X (e.g. 200) number of words as shown below if you know that the
                // search string will fall within a certain number of words.  If you wanted to only look within
                // a specific quadrant of a page (e.g. lower right corner), you would need to get the bounding box
                // of each Word and compare that to your target area. 
                int wordLoop = Math.Min(pageWords.Count,200);

                for (int wordnum = 0; wordnum < pageWords.Count; wordnum++)
                //for (int wordnum = 0; wordnum < wordLoop; wordnum++)  // limit by the fixt X number of Words
                {
                    Word wInfo;
                    wInfo = pageWords[wordnum];
                    string s = wInfo.Text;

                    // Check for hyphenated words that break across a line.  
                    if (((wInfo.Attributes & WordAttributeFlags.HasSoftHyphen) == WordAttributeFlags.HasSoftHyphen) &&
                        ((wInfo.Attributes & WordAttributeFlags.LastWordOnLine) == WordAttributeFlags.LastWordOnLine))
                    {
                        // For the purposes of this sample, we'll remove all hyphens.  In practice, you may need to check 
                        // words against a dictionary to determine if the hyphenated word is actually one word or two.
                        string[] splitstrs = s.Split(new Char[] {'-', '\u00ad'});
                        textToExtract += splitstrs[0] + splitstrs[1];
                    }
                    else
                        textToExtract += s;

                    // Check for space adjacency and add a space if necessary.
                    if ((wInfo.Attributes & WordAttributeFlags.AdjacentToSpace) == WordAttributeFlags.AdjacentToSpace)
                    {
                        textToExtract += " ";
                    }
                    // Check for a line break and add one if necessary
                    if ((wInfo.Attributes & WordAttributeFlags.LastWordOnLine) == WordAttributeFlags.LastWordOnLine)
                        textToExtract += "\n";
                }
                
                // 
                if (textToExtract.ToUpper().Contains(splitTextString))
                    {
                    Console.WriteLine("Found " + splitTextString + " on page " + i);
                    listOfPageNumsToSplit.Add(i);
                }

                // Release requested WordList
                for (int wordnum = 0; wordnum < pageWords.Count; wordnum++)
                    pageWords[wordnum].Dispose();
            }
        }

   }
}
