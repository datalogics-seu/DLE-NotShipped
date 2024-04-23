using System;
using Datalogics.PDFL;
using System.Collections.Generic;
using System.IO;

/*
 * Flatten a PDF portfolio to a standard PDF
 * Copyright (c) 2007-2021, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace FlattenPortfolio
{
    class FlattenPortfolio
    {
        static void Main(string[] args)
        {
            Console.WriteLine("FlattenPortfolio sample:");

            using (Library lib = new Library())
            {
                // PortfolioThreePDFs.pdf -- Acrobat made portfolio with 3 PDFs
                String sInput1 = Library.ResourceDirectory + "Sample_Input/PortfolioThreePDFs.pdf"; // "Sample_Input/mergepdf2.pdf"; 
                String sOutput = "../Flattened-out.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];
                if (args.Length > 1)
                    sOutput = args[1];

                using (Document doc1 = new Document(sInput1))
                {
                    try
                    {
                        Document docOut = new Document();

                        IList<FileAttachment> attachments = doc1.Attachments;
                        Console.WriteLine("\nDocument " + sInput1 + " has " + attachments.Count + " attachments");

                        // Not all attachments are PDFs.
                        foreach (FileAttachment fileA in attachments)
                        {
                            Console.WriteLine("Attachement is "+ fileA.FileName);

                            if (fileA.FileName.EndsWith(".pdf") )
                            {
                                // Save to a mem stream rather than a file
                                MemoryStream ms = new MemoryStream();
                                fileA.SaveToStream(ms);
                                Document tempPDF = new Document(ms); 

                                docOut.InsertPages(Document.LastPage, tempPDF, 0, Document.AllPages, PageInsertFlags.Bookmarks | PageInsertFlags.Threads |
                                PageInsertFlags.DoNotMergeFonts | PageInsertFlags.DoNotResolveInvalidStructureParentReferences | PageInsertFlags.DoNotRemovePageInheritance);
                                tempPDF.Close(); 
                                ms.Close();
                                ms.Dispose();
                   
                            }
                        }

                        // For best performance processing large documents, set the following flags.
                        Console.WriteLine("Saving to " + sOutput);
                        docOut.Save(SaveFlags.Full | SaveFlags.SaveLinearizedNoOptimizeFonts | SaveFlags.Compressed, sOutput);

                        docOut.Close();
                        doc1.Close();
                    }
                    catch(LibraryException ex)
                    {
                        Console.Out.WriteLine(ex.Message);
                    }

                }
            }
        }
    }
}

