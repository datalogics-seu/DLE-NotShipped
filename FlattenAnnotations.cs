using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates how to flatten annotations (take the appearance stream of the annotation and convert 
 * it to regular page content).  This is only done for annotations that have appearance streams.
 * 
 * You can provide your own file names for these values in the code, or you can enter your own file names as
 * command line parameters.
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace FlattenAnnotations
{
    class FlattenAnnotations
    {
        static void Main(string[] args)
        {
            Console.WriteLine("FlattenAnnotations Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput1 = Library.ResourceDirectory + "Sample_Input/FlattenAnnotations.pdf"; // sample_annotations.pdf";
                String sOutput = "../FlattenAnnotations-out-flattened-v4.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];

                Document sourceDoc = new Document(sInput1);

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Moving annotations appearances from " + sInput1 + " to page content and writing to " + sOutput);

                int totalPages = sourceDoc.NumPages;
                int pageCounter = 0;

                while (pageCounter < totalPages)
                {
                    Page sourcePage = sourceDoc.GetPage(pageCounter);
                    Content content = sourcePage.Content;

                    int nAnnots = sourcePage.NumAnnotations;
                    Console.WriteLine("Page 1 " + " has " + nAnnots + " annotations.");
                    bool contentChanged = false;

                    // Iterate through the annotations, flatten and delete
                    // -- When iterating through content that you will possibly be deleting, you typically want to loop 
                    // -- from back to front so that the index remains undisturbed.
                    // -- But, in this case, we want to make sure the annots are added to the page content in order
                    // -- in case their appearance overlaps. So we will just delete the annots at the end.
                    for (int i = 0; i < nAnnots; i++)
                        {
                        Annotation ann = sourcePage.GetAnnotation(i);

                        Form form = ann.NormalAppearance;
                        if (form != null)
                        {
                            Console.WriteLine("Adding Annotation " + i + " to page content.");                            
                            Matrix m = new Matrix(1.0, 0.0, 0.0, 1.0, ann.Rect.Left, ann.Rect.Bottom); // set the position
                            form.Matrix = m;
                            sourcePage.Content.AddElement(form);
                            contentChanged = true;
                        }
                        else
                        {
                            Console.WriteLine("Annotation " + i + " on page " + pageCounter + " has no appearance stream.");
                        }
                    }
                    if (contentChanged)
                    {
                        sourcePage.UpdateContent();
                    }

                    // Now, remove the Annots dictionary (no need to iterate each annotation since we are deleting all of them)
                    if (sourcePage.PDFDict.Contains("Annots"))
                        {
                        sourcePage.PDFDict.Remove("Annots");
                    }

                    sourcePage.Dispose();
                    pageCounter++;
                } // end page loop

                sourceDoc.Save(SaveFlags.Full, sOutput);
            }
        }
    }
}
