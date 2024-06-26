using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * Process a document by imaging the page andusing the optical
 * character recognition engine. Place the image and the processed
 * text in an output pdf
 * 
 * Copyright (c) 2007-2022, Datalogics, Inc. All rights reserved.
 *
 */

namespace FlattenAndAddText
{
    class FlattenAndAddText
    {
        static void Main(string[] args)
        {
            Console.WriteLine("FlattenAndAddText Sample:");

            String sInput = Library.ResourceDirectory + "Sample_Input/scanned_images.pdf";
            String sOutput = "../FlattenAndAddText-out.pdf";

            if (args.Length > 0)
                sInput = args[0];
            if (args.Length > 1)
                sOutput = args[1];

            Console.WriteLine("Input file: " + sInput);
            Console.WriteLine("Writing output to: " + sOutput);


            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                // Using a higher image resolution gets better results form the OCR engine
                // but the page must be imaged to match this value.
                const int resolution = 2400;

                OCRParams ocrParams = new OCRParams();

                //The OCRParams.Languages parameter controls which languages the OCR engine attempts
                //to detect. By default the OCR engine searches for English.
                List<LanguageSetting> langList = new List<LanguageSetting>();
                LanguageSetting languageOne = new LanguageSetting(Language.English, false);
                langList.Add(languageOne);

                //You could add additional languages for the OCR engine to detect by adding 
                //more entries to the LanguageSetting list. 

                //LanguageSetting languageTwo = new LanguageSetting(Language.Japanese, false);
                //langList.Add(languageTwo);
                ocrParams.Languages = langList;

                // If your image resolution is not 300 dpi, specify it here. Specifying a
                // correct resolution gives better results for OCR, especially with
                // automatic image preprocessing.
                ocrParams.Resolution = resolution;

                using (OCREngine ocrEngine = new OCREngine(ocrParams))
                {
                    //Create a document object
                    using (Document outDoc = new Document())
                    {
                        Document inDoc = new Document(sInput);

                        for (int numPage = 0; numPage < inDoc.NumPages; numPage++)
                        {
                            /////////////////////////////////////////////////////
                            // PART ONE: Extract page image and flatten
                            /////////////////////////////////////////////////////
                            Page pg = inDoc.GetPage(numPage);

                            /////////////////////////////////////////////////////////
                            //
                            //  First, we'll make an image that is exactly 2400 pixels
                            //  wide at a resolution of 2400 DPI.
                            //  
                            ////////////////////////////////////////////////////////

                            // Create a PageImageParams with the default settings.
                            PageImageParams pip = new PageImageParams();
                            pip.PageDrawFlags = DrawFlags.UseAnnotFaces;

                            // Set the PixelWidth to be exactly 2400 pixels.
                            // We don't need to set the PixelHeight, as the Library will calculate
                            // this for us automatically based on the PixelWidth and resolution.
                            pip.PixelWidth = resolution;
                            pip.HorizontalResolution = pip.VerticalResolution = resolution;

                            // Again, we'll create an image of the entire page using the page's
                            // CropBox as the exportRect.  The default ColorSpace is DeviceRGB, 
                            // so the image will be DeviceRGB.
                            Datalogics.PDFL.Image pageImage = pg.GetImage(pg.CropBox, pip);

                            /////////////////////////////////////////////////////
                            // PART TWO: Run OCR over the flattened page image
                            /////////////////////////////////////////////////////

                            // Create a PDF page which is the size of the image.
                            // Matrix.A and Matrix.D fields, respectively.
                            // There are 72 PDF user space units in one inch.
                            Rect pageRect = new Rect(0, 0, pageImage.Matrix.A, pageImage.Matrix.D);
                            using (Page docpage = outDoc.CreatePage(Document.LastPage, pageRect))
                            {
                                docpage.Content.AddElement(pageImage);
                                docpage.UpdateContent();
                            }

                            using (Page page = outDoc.GetPage(numPage))
                            {
                                Content content = page.Content;
                                Element elem = content.GetElement(0);
                                Image image = (Image)elem;
                                //PlaceTextUnder creates a form with the image and the generated text
                                //under the image. The original image in the page is then replaced by
                                //by the form.
                                Form form = ocrEngine.PlaceTextUnder(image, outDoc);
                                content.RemoveElement(0);
                                content.AddElement(form, Content.BeforeFirst);
                                page.UpdateContent();
                            }
                        }
                        outDoc.Save(SaveFlags.Full, sOutput);
                    }
                }
            }
        }
    }
}
