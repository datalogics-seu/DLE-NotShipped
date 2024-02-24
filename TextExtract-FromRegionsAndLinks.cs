using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Datalogics.PDFL;

/*
 * A sample which demonstrates the use of the DLE API for capturing
 * the text from specific target locations and from underneath link rectangles.
 * 
 * Copyright (c) 2007-2010, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.  Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 */
namespace TextExtract_FromRegionsAndLinks
{
    class TextExtract_FromRegionsAndLinks
    {

        // store a set of Left, Right, Bottom and Top values of user specified target areas
        public class UserTargetRegion
        {
            public double L { get; set; }
            public double R { get; set; }
            public double B { get; set; }
            public double T { get; set; }
            public string Description { get; set; }
        }
        // store a set of Left, Right, Bottom and Top values of link areas
        public class LinkRegion
        {
            public double L { get; set; }
            public double R { get; set; }
            public double B { get; set; }
            public double T { get; set; }
            public string Title { get; set; }
            public string ActionType { get; set; }
            public string ActionDestination { get; set; }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Capture the text from user specified target regions and underneath links");

            string sInput = Library.ResourceDirectory + "Sample_Input/PageRegionsWithLinks.pdf"; ;  // input document

            if (args.Length > 0)
                sInput = args[0];

            var userRegions = new List<UserTargetRegion>();
            var linkRegions = new List<LinkRegion>();

            //Three user target areas
            userRegions.Add(new UserTargetRegion
            {
                L = 0.0,     //LLx
                B = 692.0,   //LLy
                R = 150.0,   //URx
                T = 792.0,   //URy
                Description = "Upper Left header",
            });
            userRegions.Add(new UserTargetRegion
            {
                L = 450.0,   //LLx
                B = 0.0,     //LLy
                R = 612.0,   //URx
                T = 100.0,   //URy
                Description = "Lower Right footer",
            });


            using (Library lib = new Library())
            {
                Document doc = new Document(sInput);
                Console.WriteLine("Input file:  " + sInput);

                // setup the WordFinderConfig 
                WordFinderConfig wordConfig = new WordFinderConfig();
                wordConfig.IgnoreCharGaps = false;
                wordConfig.IgnoreLineGaps = false;
                wordConfig.NoAnnots = false;
                wordConfig.NoEncodingGuess = false;
                wordConfig.UnknownToStdEnc = false;
                wordConfig.DisableTaggedPDF = false;
                wordConfig.NoXYSort = true;
                wordConfig.PreserveSpaces = false;
                wordConfig.NoLigatureExp = false;
                wordConfig.NoHyphenDetection = false;
                wordConfig.TrustNBSpace = false;
                wordConfig.NoExtCharOffset = false;     // text extraction efficiency
                wordConfig.NoStyleInfo = false;         // text extraction efficiency
                wordConfig.PreciseQuad = true;          // enable PreciseQuad to get tighter Word bounding boxes

                WordFinder wordFinder = new WordFinder(doc, WordFinderVersion.Latest, wordConfig);

                CaptureTextFromRegions(doc, wordFinder, userRegions, linkRegions);

            }
        }

        // **********************************************************
        // Iterate through annotations, getting their locations
        // -- only interested in GoTo, GoToR (Go to Remote),
        // -- Launch and URI links for this application
        // **********************************************************
        static void FindAllLinks(Page page, List<LinkRegion> linkRegions, ref int linkCount)
        {
            string actionType = "";
            string actionDestination = "";
            linkCount = 0;
            linkRegions.Clear();

            for (int i = 0; i < page.NumAnnotations; i++)
            {
                Annotation annot = page.GetAnnotation(i);
                if (annot is Datalogics.PDFL.LinkAnnotation)  // (annot.Subtype == "Link")
                {
                    linkCount++; 
                    LinkAnnotation la = (LinkAnnotation)annot;
                    if (la.Action is GoToAction)
                    {
                        GoToAction gotoAction = (GoToAction)la.Action;
                        actionType = "Goto page";
                        actionDestination = gotoAction.Destination.PageNumber.ToString();
                    }
                    else if (la.Action is RemoteGoToAction)
                    {
                        RemoteGoToAction rgotoAction = (RemoteGoToAction)la.Action;
                        actionType = "Remote goto";
                        actionDestination = rgotoAction.FileSpecification.Path;

                    }
                    else if (la.Action is LaunchAction)
                    {
                        LaunchAction launchAction = (LaunchAction)la.Action;
                        actionType = "Launch";
                        actionDestination = launchAction.FileSpecification.Path;
                    }
                    else if (la.Action is URIAction)
                    {
                        URIAction uriAction = (URIAction)la.Action;
                        actionType = "URI";
                        actionDestination = uriAction.URI;
                    }
                    else
                        actionType = "n/a";

                    linkRegions.Add(new LinkRegion
                    {
                        L = annot.Rect.Left,
                        R = annot.Rect.Right,
                        B = annot.Rect.Bottom,
                        T = annot.Rect.Top,
                        Title = annot.Title,                   // not all annots have titles
                        ActionType = actionType,               // will be Goto, GotoR, Launch or URI
                        ActionDestination = actionDestination  //
                    });
                }

            }
        }

        static void ProcessUserTargetRegions(List<UserTargetRegion> userRegions, IList<Word> pageWords, System.IO.StreamWriter logfile)
        {
            string textToExtract = "";
            int userTargetNum = 0;
            double userTargetFudge = 0.0;

            /* The "fudge" factors are used to adjust the link or target rectangles for any word-quads 
             * that do not quite fit into closely cropped rectangles , espically for pages with a lot of 
             * links on heavy, closely packed text. 
             * The optical appearance of a glyph does not necessarily match a Word quad
             * because the optical appearance of a glyph does not typically fill up a character’s bounding box. 
             * The difference will vary depending on the font, even when using the same pointsize across fonts.  
             * It has been noted that the average of a glyph’s height compared to its pointsize is about 70%.  
             * For example, in a 12 point font, a capital M may only be about 8 ½  points high. 
             * 
             * The WordFinderConfig now supports the PreciseQuad option which will keep the returned Word quad values closer 
             * to the drawing glyphs. But, a fudge factor may stll be needed if glyphs overlap the target rectangle.
             */

             /* 
            -- The origin of a PDF page is not always 0,0. If the user target values were specified from a 
            -- visual representation of the page, rather than from internal coordinate values, you may need 
            -- to adjust the user specified values by the amount of the page origin (left and bottom) 
                userReg.L = userReg.L + curPage.CropBox.Left;
                userReg.R = userReg.R + curPage.CropBox.Left;
                userReg.B = userReg.B + curPage.CropBox.Bottom;
                userReg.T = userReg.T + curPage.CropBox.Bottom;
            */

            foreach (UserTargetRegion userReg in userRegions)
            {
                userTargetNum++;
                textToExtract = "User target area #" + userTargetNum + 
                    " Description: [" + userReg.Description + 
                    "] Text: [";

                for (int wordnum = 0; wordnum < pageWords.Count; wordnum++)
                {
                    Word wInfo;
                    wInfo = pageWords[wordnum];
                    string s = wInfo.Text;

                    IList<Quad> QuadList = wInfo.Quads;
                    foreach (Quad Q in QuadList)
                    {
                        double L = Q.BottomLeft.H;
                        double R = Q.TopRight.H;
                        double T = Q.TopRight.V;
                        double B = Q.BottomLeft.V;

                        if (Math.Ceiling(L) >= (Math.Floor(userReg.L) - userTargetFudge)
                            && Math.Floor(R) <= (Math.Ceiling(userReg.R) + userTargetFudge)
                            && Math.Floor(T) <= (Math.Ceiling(userReg.T) + userTargetFudge)
                            && Math.Ceiling(B) >= (Math.Floor(userReg.B) - userTargetFudge))
                        {
                            textToExtract += s;
                            if ((wInfo.Attributes & WordAttributeFlags.AdjacentToSpace) == WordAttributeFlags.AdjacentToSpace)
                            {
                                textToExtract += " ";
                            }
                        }

                    } // end for each quad

                } // end for each Word loop
                textToExtract += "]";
                logfile.WriteLine(textToExtract);
            } // end foreach User Target area loop 

        }

        static void ProcessLinkRegions(List<LinkRegion> linkRegions, IList<Word> pageWords, System.IO.StreamWriter logfile)
        {
            double linkFudgeH = 4.0;              // Horizontal fudge factor (in points)
            double linkFudgeV = 2.0;              // Vertical fudge factor (in points)
            int linkNum = 0;
            string textToExtract = "";

            foreach (LinkRegion linkReg in linkRegions)
            {
                linkNum++;
                textToExtract = "";
                textToExtract += "Link #" + linkNum + 
                    " Coordinates: [" + string.Format("{0:0.00}", linkReg.L) + "," + string.Format("{0:0.00}", linkReg.B) +
                    " " + string.Format("{0:0.00}", linkReg.R) + "," + string.Format("{0:0.00}", linkReg.T) + "] " +
                    "ActionType: [" + linkReg.ActionType + "] " +
                    "Destination: [" + linkReg.ActionDestination + "] " +
                    "Text: [";

                for (int wordnum = 0; wordnum < pageWords.Count; wordnum++)
                {
                    Word wInfo;
                    wInfo = pageWords[wordnum];
                    string s = wInfo.Text;

                    IList<Quad> QuadList = wInfo.Quads;
                    foreach (Quad Q in QuadList)
                    {
                        double L = Q.BottomLeft.H;
                        double R = Q.TopRight.H;
                        double T = Q.TopRight.V;
                        double B = Q.BottomLeft.V;

                        if (Math.Ceiling(L) >= (Math.Floor(linkReg.L) - linkFudgeH)
                            && Math.Floor(R) <= (Math.Ceiling(linkReg.R) + linkFudgeH)
                            && Math.Floor(T) <= (Math.Ceiling(linkReg.T) + linkFudgeV)
                            && Math.Ceiling(B) >= (Math.Floor(linkReg.B) - linkFudgeV))
                        {
                            textToExtract += s;
                            if ((wInfo.Attributes & WordAttributeFlags.AdjacentToSpace) == WordAttributeFlags.AdjacentToSpace)
                                textToExtract += " ";

                            // Check for a line break and add one if necessary
                            // if ((wInfo.Attributes & WordAttributeFlags.LastWordOnLine) == WordAttributeFlags.LastWordOnLine)
                            //    textToExtract += "\n";
                        }

                    }  // end for each Quad loop

                }  // end for each page Word loop
                textToExtract += "]";
                logfile.WriteLine(textToExtract);
            } // end for each link loop

        }

        static void CaptureTextFromRegions(Document doc, WordFinder wordFinder, List<UserTargetRegion> userRegions, List<LinkRegion> linkRegions)
        {
            int nPages = doc.NumPages;
            int linkCount = 0;
            IList<Word> pageWords = null;

            Encoding utf8WithoutBOM = new UTF8Encoding(false);
            System.IO.StreamWriter logfile = new System.IO.StreamWriter("TextExtract_FromRegionsAndLinks.txt", false, utf8WithoutBOM);

            for (int i = 0; i < nPages; i++)
            {
                Page curPage = doc.GetPage(i);
                FindAllLinks(curPage, linkRegions, ref linkCount);
                pageWords = wordFinder.GetWordList(i);

                Console.WriteLine("<page " + (i + 1) + "> has " + userRegions.Count + " user target regions, " + linkCount + " links (" + curPage.NumAnnotations + " annotations)" + " and " + pageWords.Count + " Words");
                logfile.WriteLine("<page " + (i + 1) + "> has " + userRegions.Count + " user target regions, " + linkCount + " links (" + curPage.NumAnnotations + " annotations)" + " and " + pageWords.Count + " Words");

                //* Go through all of the user specified target areas
                ProcessUserTargetRegions(userRegions, pageWords, logfile); 

                // Go through all the link regions and check for words that fit
                ProcessLinkRegions(linkRegions, pageWords, logfile); 
             
                // Release the WordList
                for (int wordnum = 0; wordnum < pageWords.Count; wordnum++)
                    pageWords[wordnum].Dispose();

            }  // end for loop of pages
            Console.WriteLine("Processed " + nPages + " pages.");
            logfile.Close();
        }

   }
}
