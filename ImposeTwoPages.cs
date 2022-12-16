using System;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates taking the contents of two pages and imposing them onto a 
 * single larger spread merging one PDF document into another. 
 *
 * Copyright (c) 2007-2022, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ImposeTwoPages
{
    class ImposeTwoPages
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MergePDF Sample:");

            using (Library lib = new Library())
            {
                lib.AllowRelaxedSyntax = true;
                String sInput1 = Library.ResourceDirectory + "Sample_Input/ExtractText.pdf";
                String sInput2 = Library.ResourceDirectory + "Sample_Input/ExtractUnicodeText.pdf";
                String sOutput = "../ImposeOutput.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];
                if (args.Length > 1)
                    sInput2 = args[1];
                if (args.Length > 2)
                    sOutput = args[2];

                Console.WriteLine("ImposeTwoPages: combining " + sInput1 + " and " + sInput2 + " and writing to " + sOutput);

                using (Document doc1 = new Document(sInput1))
                {
                    using (Document doc2 = new Document(sInput2))
                    {
                        Page pageLeft = doc1.GetPage(0);
                        Page pageRight = doc2.GetPage(0);

                        // both input sample documents are US Letter (612x792)
                        // make an output page twice as wide
                        Document outputDoc = new Document();
                        Rect outPageRect = new Rect(0, 0, pageLeft.MediaBox.Width*2, pageLeft.MediaBox.Height);
                        Page outPage1 = outputDoc.CreatePage(Document.BeforeFirstPage, outPageRect);
                        //Page outPage1 = new Page(outputDoc, 0, outPageRect);

                        Form pageLeftForm = new Form(pageLeft.Content);
                        Form pageRightForm = new Form(pageRight.Content);

                        // add contents of left side to the output page
                        outPage1.Content.AddElement(pageLeftForm);

                        // add contents of right side to the output page
                        // reposition the second form by shifting to the right the width
                        // of the source page (both source files are the same width)
                        pageRightForm.Translate(pageRight.MediaBox.Width, 0);
                        outPage1.Content.AddElement(pageRightForm);

                        //update the content of the output page
                        outPage1.UpdateContent();

                        // For best performance processing large documents, set the following flags.
                        outputDoc.Save(SaveFlags.Full | SaveFlags.SaveLinearizedNoOptimizeFonts | SaveFlags.Compressed, sOutput);
                    }
                }
            }
        }
    }
}
