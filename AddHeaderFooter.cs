using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * 
 * A sample which demonstrates the use of the Datalogics PDF Library API to add a header and footer to the first page of 
 * a PDF document.  It uses the Page BoundingBox (or BBox) to determine the area of the page that  
 * contains content and then adds the header/footer text to the areas above and below the current BBox. 
 * 
 * Note: The BBox is not a property of a page with specified values like the CropBox and MediaBox, 
 * but is instead derived from the content. It is a an array of four numbers giving the left, bottom, 
 * right, and top coordinates, respectively, that encompass the marking content (all text, graphics, 
 * and images) on the page. It is not, necessarily, a box that exactly fits the visible content; meaning  
 * that there may be some additional space. 
 * 
 * 
 *
 * Copyright (c) 2007-2010, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.  Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 *
 */

namespace AddHeaderFooter
{
    class AddHeaderFooter
    {
        // ****  Set the Header and Footer values here **** //
        public static string headerHorAlign = "center"; // options: left, center, right
        public static string headerVertAlign = "center"; // options: top, center, bottom
        public static string headerText = "SampleHeaderText";
        public static string headerFont = "Courier"; // Courier can be used on any system, regardless of whether the font is on the system
        public static double headerPointSize = 14;   // font size for the  text
        public static double headerMinSpace = 16;    // minimum required space 

        public static string footerHorAlign = "right"; // options: left, center, right
        public static string footerVertAlign = "bottom"; // options: top, center, bottom
        public static string footerText = "SampleFooterText";
        public static string footerFont = "Arial"; //not all fonts are found on all systems
        public static double footerPointSize = 14; // font size for the  text
        public static double footerMinSpace = 16;  // minimum required space 

        // Structure to hold the Header data 
        // * This could be expanded to include additional features such as
        // *    left/right/top/bottom margins
        // *    color info
        // *    embedding and subsetting font true/false option
        // *    fixed position settings rather than auto-adjusting
        public struct headerInfo
        { 
            public string hAlign;
            public string vAlign;
            public string text;
            public string font;
            public double fsize;
            public double minspace;

            public headerInfo(string hA, string vA, string t, string f, double fs, double minS)
            {
                hAlign = hA;
                vAlign = vA;
                text = t;
                font = f;
                fsize = fs;
                minspace = minS;
            }
        }

        // structure to hold the Footer data 
        public struct footerInfo
        {
            public string hAlign;
            public string vAlign;
            public string text;
            public string font; 
            public double fsize;
            public double minspace;

            public footerInfo(string hA, string vA, string t, string f, double fs, double minS)
            {
                hAlign = hA;
                vAlign = vA;
                text = t;
                font = f;
                fsize = fs;
                minspace = minS;
            }
          }



        static void Main(string[] args)
        {
            Console.WriteLine("AddHeaderFooter Sample:");
            // header and footer for page 1
            // can expand to setup headers/footers for body pages, indexes, etc.
            headerInfo header1 = new headerInfo(headerHorAlign, headerVertAlign, headerText, headerFont, headerPointSize, headerMinSpace);
            footerInfo footer1 = new footerInfo(footerHorAlign, footerVertAlign, footerText, footerFont, footerPointSize, footerMinSpace);
            
            using (Library lib = new Library())
            {
                string filename = null;
                if (args.Length < 1)
                {
                    Console.Write("Enter filename: ");
                    filename = Console.ReadLine();
                }
                else
                    filename = args[0];

                Document doc1 = new Document(filename);
                Page page = doc1.GetPage(0);

                BuildHeaderFooter(page, header1, footer1);  //headerInfo, footerInfo

                doc1.Save(SaveFlags.Full | SaveFlags.Linearized, "AddHeaderFooter-out.pdf");
            }
        }

        static void BuildHeaderFooter(Page page, headerInfo header, footerInfo footer)
        {
            BuildHeader(page, header);
            BuildFooter(page, footer);
        }
        
        static void BuildHeader(Page page, headerInfo header)
        {
            Rect bbox = page.BBox;
            Rect cropbox = page.CropBox;
            double headerSpace = calcHeaderSpace(cropbox, bbox);

            if (headerSpace < header.minspace)
            {
                Console.WriteLine("  -- Not enough space to place a header");
            }
            else
            {
                Text t = new Text();
                Font f = new Font(header.font);

                GraphicState gs = new GraphicState();
                gs.FillColor = new Color(0.0);    // DeviceGray black

                double fsize = header.fsize;      // pointsize of text
                TextState ts = new TextState();
                ts.FontSize = fsize;

                // Create a temporary matrix and textrun to determine the width of the text
                Matrix mTemp = new Matrix().Translate(0, 0).Scale(1, 1);
                TextRun trTemp = new TextRun(header.text, f, gs, ts, mTemp);
                double headerStartX = calcHeaderFooterXPos(cropbox, bbox, header.hAlign, trTemp);
                double headerStartY = calcHeaderYPos(cropbox, bbox, header.vAlign, trTemp, header.fsize);
                Console.WriteLine("headerStartX " + headerStartX + "    headerStartY " + headerStartY);

                mTemp.Dispose();
                trTemp.Dispose();

                Matrix m = new Matrix().Translate(headerStartX, headerStartY).Scale(1, 1);
                TextRun tr = new TextRun(header.text, f, gs, ts, m);

                t.AddRun(tr);
                page.Content.AddElement(t);
                page.UpdateContent();
                t.Dispose();
            }
        }

        static void BuildFooter(Page page, footerInfo footer)
        {
            Rect bbox = page.BBox;
            Rect cropbox = page.CropBox;
            double footerSpace = calcFooterSpace(cropbox, bbox);

            if (footerSpace < footer.minspace)
            {
                Console.WriteLine("  -- Not enough space to place a footer");
            }
            else
            {
                Text t = new Text();
                Font f = new Font(footer.font);

                GraphicState gs = new GraphicState();
                gs.FillColor = new Color(0.0);    // DeviceGray black

                double fsize = footer.fsize;             // pointsize
                TextState ts = new TextState();
                ts.FontSize = fsize;

                // Create a temporary matrix and textrun to determine the width of the text
                Matrix mTemp = new Matrix().Translate(0, 0).Scale(1, 1);
                TextRun trTemp = new TextRun(footer.text, f, gs, ts, mTemp);
                double footerStartX = calcHeaderFooterXPos(cropbox, bbox, footer.hAlign, trTemp);
                double footerStartY = calcFooterYPos(cropbox, bbox, footer.vAlign, trTemp, footer.fsize);
                Console.WriteLine("footerStartX " + footerStartX + "    footerStartY " + footerStartY);

                mTemp.Dispose();
                trTemp.Dispose();

                Matrix m = new Matrix().Translate(footerStartX, footerStartY).Scale(1, 1);
                TextRun tr = new TextRun(footer.text, f, gs, ts, m);

                t.AddRun(tr);
                page.Content.AddElement(t);
                page.UpdateContent();
                t.Dispose();
            }
        }

        /* Calculate the starting X position of the Header or Footer */
        static double calcHeaderFooterXPos(Rect cropbox, Rect bbox, string align, TextRun tr)
        {
            double width = bbox.URx - bbox.LLx;
            double startX = 0;

            if (align.ToLower().Equals("left"))
            {
                //move to the left edge of bounding box
                startX = bbox.LLx;
            } 
            if (align.ToLower().Equals("center"))
            {
                // from the left edge of the bbox, move to the right half the width of the bounding box minus half the width of the text
                startX = bbox.LLx + (width / 2) - (tr.Advance / 2); 
            }
            if (align.ToLower().Equals("right"))
            {
                //move to the right edge of bounding box minus the width of the text
                startX = bbox.URx - tr.Advance;
            }
            return startX;
        }

        /* Calculate the starting Y position of the Header */
        static double calcHeaderYPos(Rect cropbox, Rect bbox, string align, TextRun tr, double fsize)
        {
            double width = bbox.URx - bbox.LLx;
            double height = cropbox.URy - bbox.URy;
            double startY = 0;

            if (align.ToLower().Equals("top"))
            {
                // move to the top edge of crop box minus the height of the text
                startY = cropbox.URy - fsize;
            }
            if (align.ToLower().Equals("center"))
            {
                // move halfway between the top of cropbox and top of BBox minus half the height of the text
                startY = cropbox.URy - (height/2) - (fsize/2); // 
            }
            if (align.ToLower().Equals("bottom"))
            {
                // move to the top of the BBox
                startY = bbox.URy;
            }
            return startY;
        }

        /* Calculate the starting Y position of the Footer */
        static double calcFooterYPos(Rect cropbox, Rect bbox, string align, TextRun tr, double fsize)
        {
            double width = bbox.URx - bbox.LLx;
            double height = bbox.LLy - cropbox.LLy;
            double startY = 0;

            if (align.ToLower().Equals("top"))
            {
                // move to the bottom edge of bbox minus the height of the text
                startY = bbox.LLy - fsize;
            }
            if (align.ToLower().Equals("center"))
            {
                // move halfway between the bottom of the BBox and the bottom of the cropbox minus half the height of the text
                startY = bbox.LLy - (height / 2) - (fsize / 2); 
            }
            if (align.ToLower().Equals("bottom"))
            {
                // move to the bottom of the cropbox
                startY = cropbox.LLy;
            }
            return startY;
        }
 
        static double calcHeaderSpace(Rect cropbox, Rect bbox)
        {
            /*
            Console.WriteLine("CropBox values");
            Console.WriteLine("  -- LLx:" + cropbox.LLx);
            Console.WriteLine("  -- LLy:" + cropbox.LLy);
            Console.WriteLine("  -- URx:" + cropbox.URx);
            Console.WriteLine("  -- URy:" + cropbox.URy);
            Console.WriteLine("BBox values");
            Console.WriteLine("  -- LLx:" + bbox.LLx);
            Console.WriteLine("  -- LLy:" + bbox.LLy);
            Console.WriteLine("  -- URx:" + bbox.URx);
            Console.WriteLine("  -- URy:" + bbox.URy);
            */

            double headerVertSpace = cropbox.URy - bbox.URy;
            Console.WriteLine("\nAvailable Header vertical space: " + headerVertSpace + " points");
            return headerVertSpace;
        }

        static double calcFooterSpace(Rect cropbox, Rect bbox)
        {
            double footerVertSpace = bbox.LLy - cropbox.LLy;
            Console.WriteLine("Available Footer vertical space: " + footerVertSpace + " points");
            return footerVertSpace;
        }

    }
}