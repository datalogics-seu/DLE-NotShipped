using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * 
 * This sample shows how to flatten transparencies in a PDF document.
 *
 * PDF files can have objects that are partially or fully transparent, and thus
 * can blend in various ways with objects behind them. Transparent graphics or images
 * can be stacked in a PDF file, with each one contributing to the final result that
 * appears on the page. The process to flatten a set of transparencies merges them
 * into a single image on the page.
 *
 * For more detail see the description of the FlattenTransparency sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/layers_and_transparencies#flattentransparency 
 * 
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */


namespace FlattenTransparency
{
    class FlattenTransparency
    {
        static void Main(string[] args)
        {
            Console.WriteLine("FlattenTransparency - ConvertTextToCurves sample:");

        	using (Library lib = new Library())
            {
                String sInput = Library.ResourceDirectory + "Sample_Input/FourPagesSimpleRotations.pdf"; 
                String sOutput = "../FlattenTransparency-out-curves.pdf";  

                if (args.Length > 0)
                    sInput = args[0];
                if (args.Length > 1)
                    sOutput = args[1];


                // Open a document with multiple pages.
                Document doc = new Document(sInput);
            
                int totalPages = doc.NumPages;

                ExtendedGraphicState egs = new ExtendedGraphicState();
                egs.OpacityForOtherThanStroking = 0.9;
                egs.OpacityForStroking = 0.9;
                GraphicState gs = new GraphicState();
                gs.FillColor = new Color(0, 0, 0);
                gs.ExtendedGraphicState = egs;
                // TextState ts = new TextState();

                Path pa = new Path();
                pa.PaintOp = PathPaintOpFlags.Fill;
                pa.GraphicState = gs;

                Point Point0 = new Point(0, 0);
                Point Point1 = new Point(1, 1);
                pa.MoveTo(Point0);
                pa.AddLine(Point1);
                pa.ClosePath();

                for (int i = 0; i < doc.NumPages; i++)
                {
                    Page pg = doc.GetPage(i);
                    pg.Content.AddElement(pa);
                    pg.UpdateContent();
                    pg.Dispose();
                }

                // Set up some parameters for the flattening.
                FlattenTransparencyParams ftParams = new FlattenTransparencyParams();

                ftParams.Quality = 50;
                ftParams.UseTextOutlines = true;
                
                doc.FlattenTransparency(ftParams);
                doc.Save(SaveFlags.Full, sOutput);
            }
		}
    }
}
