using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * 
 * This sample shows how to convert text to outlines in a PDF document.
 *
 * The transparency flattener has an option to convert text runs to outlines. So, this sample adds a tiny translucent 
 * element to the bottom of each page before invoking the flattener with the UseTextOutlines option.
 *
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ConvertTextToOutlines
{
    class ConvertTextToOutlines
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ConvertTextToOutlines sample:");

        	using (Library lib = new Library())
            {

                String sInput1 = Library.ResourceDirectory + "Sample_Input/TextSearch.pdf";
                String sOutput1 =  "../TextSearch-converted.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];
                if (args.Length > 1)
                    sOutput1 = args[1];
           
                // Open a document with multiple pages.
                Document doc = new Document(sInput1);

                // Iterate over the pages of the document and add a translucent element to the pages
                int totalPages = doc.NumPages;
                int pageCounter = 0;

                while (pageCounter < totalPages)
                {
                    Page pg = doc.GetPage(pageCounter);
                   
                    Path tinyLine = new Path();
                    GraphicState gs = tinyLine.GraphicState;

                    gs.FillColor = new Color(1.0, 1.0, 1.0);      // White
                    gs.StrokeColor = new Color(1.0, 1.0, 1.0);    // white
                    tinyLine.PaintOp = PathPaintOpFlags.Stroke; ; // PathPaintOpFlags.EoFill
                    gs.Width = 1.0;

                    ExtendedGraphicState xgs = new ExtendedGraphicState();
                    xgs.BlendMode = BlendMode.Normal;
                    xgs.OpacityForStroking = 0.5;
                    xgs.OpacityForOtherThanStroking = 0.5;
                    gs.ExtendedGraphicState = xgs;
                    tinyLine.GraphicState = gs;

                    Point Origin = new  Point(0.0, 0.0);
                    Point Point1 = new Point(1.0, 1.0); // line that goes from 0,0 to 1,1
                    tinyLine.MoveTo(Origin);
                    tinyLine.AddLine(Point1);
                    tinyLine.ClosePath();
                    pg.Content.AddElement(tinyLine);   // Add the new element to the Content of the page.     
                    pg.UpdateContent();                // Update the PDF page with the changed content

                    pg.Dispose();
                    pageCounter++;
                }

                // Set up some parameters for the flattening.
                    FlattenTransparencyParams ftParams = new FlattenTransparencyParams();

                // The Quality setting indicates the percentage (0%-100%) of vector information
                // that is preserved.  Lower values result in higher rasterization of vectors.
                ftParams.Quality = 100;
                ftParams.UseTextOutlines = true;
                    

                doc.FlattenTransparency(ftParams, 0, Document.LastPage);
                Console.WriteLine("Flattened a multi-page document " + sInput1 + " as " + sOutput1 + ".");
                doc.Save(SaveFlags.Full, sOutput1);                
            }
		}
    }
}
