using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;
using System.Diagnostics;

/*
 * Process a document using the optical recognition engine.
 * Then place the image and the processed text in an output pdf
 * 
 * For more detail see the description of AddTextToDocument on our Developers site, 
 * https://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/optical-character-recognition/
 * 
 * Copyright (c) 2007-2019, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace AddTextToDocument
{
    class AddTextToDocument
    {
        //This function will find every image in the document and add text if 
        //possible
        static void AddTextToImages(Document doc, Content content, OCREngine engine)
        {
            for (int index = 0; index < content.NumElements; index++)
            {
                Element e = content.GetElement(index);
                if (e is Datalogics.PDFL.Image)
                {
                    //PlaceTextUnder creates a form with the image and the generated text
                    //under the image. The original image in the page is then replaced by
                    //by the form.
                    Form form = engine.PlaceTextUnder((Image)e, doc);
                    content.RemoveElement(index);
                    content.AddElement(form, index -1);
                }
                else if (e is Container)
                {
                    AddTextToImages(doc, (e as Container).Content, engine);
                }
                else if (e is Group)
                {
                    AddTextToImages(doc, (e as Group).Content, engine);
                }
                else if (e is Form)
                {
                    AddTextToImages(doc, (e as Form).Content, engine);
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("AddTextToDocument Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/scanned_images.pdf";
                String sOutput = "../AddTextToDocument-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];
                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput);
                Console.WriteLine("Writing output to: " + sOutput);

                OCRParams ocrParams = new OCRParams();

                using (Document doc = new Document(sInput)) 
                {
                    var segmodeList = new List<PageSegmentationMode>();
                    var performanceOpt = new List<Datalogics.PDFL.Performance>();
                    performanceOpt.Add(Datalogics.PDFL.Performance.Default);
                    performanceOpt.Add(Datalogics.PDFL.Performance.Faster);
                    performanceOpt.Add(Datalogics.PDFL.Performance.MoreAccuracy);
                    performanceOpt.Add(Datalogics.PDFL.Performance.BestAccuracy);
                    int index = 0;

                    foreach (Datalogics.PDFL.Performance perfOpt in performanceOpt)
                    {
                        ocrParams.PageSegmentationMode = PageSegmentationMode.Automatic;
                        ocrParams.Performance = perfOpt;
                        Console.WriteLine("\nUsing Performance option " + perfOpt.ToString() + " and PageSegmentationMode.Automatic");

                        var sw = Stopwatch.StartNew(); 
                        using (OCREngine ocrEngine = new OCREngine(ocrParams))
                        {
                            for (int numPage = 0; numPage < doc.NumPages; numPage++)
                            {
                                using (Page page = doc.GetPage(numPage))
                                {
                                    Content content = page.Content;
                                    sw = Stopwatch.StartNew(); 
                                    AddTextToImages(doc, content, ocrEngine);
                                    //page.UpdateContent();
                                    sw.Stop();
                                    TimeSpan timeElapsed = sw.Elapsed;
                                    Console.WriteLine("  Page " + numPage + " -- Time elapsed: " + timeElapsed.TotalMilliseconds + " milliseconds");
                                }
                            }
                            index++;
                            //doc.Save(SaveFlags.Full, sOutput);
                        }
                    }

                    /* 
                    ocrParams.PageSegmentationMode = PageSegmentationMode.Automatic;
                    ocrParams.Performance = Performance.MoreAccuracy;
                    sw = Stopwatch.StartNew();
                    using (OCREngine ocrEngine = new OCREngine(ocrParams))
                    {
                        for (int numPage = 0; numPage < doc.NumPages; numPage++)
                        {
                            using (Page page = doc.GetPage(numPage))
                            {
                                Content content = page.Content;
                                sw = Stopwatch.StartNew(); 
                                AddTextToImages(doc, content, ocrEngine);
                                //page.UpdateContent();
                                sw.Stop();
                                TimeSpan timeElapsed = sw.Elapsed;
                                Console.WriteLine("\nUsing Performance.MoreAccuracy and PageSegmentationMode.Automatic");
                                Console.WriteLine("  Page " + numPage + " -- Time elapsed: " + timeElapsed.TotalMilliseconds + " milliseconds");

                            }
                        }
                        //doc.Save(SaveFlags.Full, sOutput);
                    }

                    ocrParams.PageSegmentationMode = PageSegmentationMode.Automatic;
                    ocrParams.Performance = Performance.BestAccuracy;
                    sw = Stopwatch.StartNew();
                    using (OCREngine ocrEngine = new OCREngine(ocrParams))
                    {
                        for (int numPage = 0; numPage < doc.NumPages; numPage++)
                        {
                            using (Page page = doc.GetPage(numPage))
                            {
                                Content content = page.Content;
                                sw = Stopwatch.StartNew();
                                AddTextToImages(doc, content, ocrEngine);
                                //page.UpdateContent();
                                sw.Stop();
                                TimeSpan timeElapsed = sw.Elapsed;
                                Console.WriteLine("\nUsing Performance.BestAccuracy and PageSegmentationMode.Automatic");
                                Console.WriteLine("  Page " + numPage + " -- Time elapsed: " + timeElapsed.TotalMilliseconds + " milliseconds");

                            }
                        }
                        //doc.Save(SaveFlags.Full, sOutput);
                    }
                     */


                }
            }
        }

    }
}
