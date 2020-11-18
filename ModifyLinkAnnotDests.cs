using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates how to find link annotations and change the destination url.
 * 
 * For more detail see the description of the Annotations sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/working-with-annotations#annotations
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/.
 *
 */
namespace ModifyLinkAnnotDests
{
    class ModifyLinkAnnotDests
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ModifyLinkAnnotDests Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                // String sInput = Library.ResourceDirectory + "Sample_Input/Custom/IdexxJoeParzel/url_issue_1.pdf";  // Example of Action with javascript launch 
                // String sOutput = "../url_issue_1-out.pdf"; //
                String sInput = Library.ResourceDirectory + "Sample_Input/Custom/IdexxJoeParzel/url_issue_2.pdf";  // Example of Actiom with URI (annot 2)
                String sOutput = "../url_issue_2-out.pdf"; //

                if(args.Length > 0)
                    sInput = args[0];
                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput);

                Document doc = new Document(sInput);

                int totalPages = doc.NumPages;
                int pageNum = 0;
                int annotNum = 0;
                int numReplaced = 0;
                for (pageNum = 0; pageNum < doc.NumPages; pageNum++)
                {
                    Page pg = doc.GetPage(pageNum);

                    for (annotNum = 0; annotNum < pg.NumAnnotations; annotNum++)
                    {
                        Annotation ann = pg.GetAnnotation(annotNum);
                        Console.WriteLine("Page: " + pageNum + " Annot: " + annotNum + " Title: " + ann.Title);

                        if (ann is LinkAnnotation)
                        {
                            if (ann.PDFDict.Contains("A"))
                            {
                                Console.WriteLine("Action found");
                                PDFDict actionDict = (PDFDict)ann.PDFDict.Get("A");
                                //Console.WriteLine("JS = " + jsPDFString.ToString());

                                if (actionDict.Contains("JS"))
                                {
                                    PDFString jsPDFString = (PDFString)actionDict.Get("JS");
                                    Console.WriteLine("JS = " + jsPDFString.Value);
                                    PDFString newvalue = new PDFString("app.launchURL(\"https://www.datalogics.com\", true);", doc, false, false);
                                    actionDict.Put("JS", newvalue);
                                    numReplaced++;
                                }
                                if (actionDict.Contains("URI"))
                                {
                                    PDFString uriPDFString = (PDFString)actionDict.Get("URI");
                                    Console.WriteLine("URI = " + uriPDFString.Value);
                                    PDFString newvalue = new PDFString("https://www.datalogics.com", doc, false, false);
                                    actionDict.Put("URI", newvalue);
                                    numReplaced++;
                                }
                            }
                        } 
                    }
                } // for page loop
                if (numReplaced > 0)
                {
                    Console.WriteLine("Replaced " + numReplaced + " link destinations.");
                    doc.Save(SaveFlags.Full | SaveFlags.Linearized, sOutput);
                }
            }
        }
    }
}
