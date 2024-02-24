using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;
using System.Diagnostics;

/*
 * 
 * A sample that converts a flat text file with some PCL codes to a PDF document using a monospace font. It will detect:
 *   -- Form Feeds (x0C) 
 *   -- the sequence "<ESC>&l0O" (portrait)
 *                    "<ESC>&l1O" (landscape)
 *                    "<ESC>(s010H" (font size 10) 
 *                    "<ESC>(s016H" (font size 9). 
 *   (where the <ESC> is x1B) 
 *
 * Copyright (c) 2017, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.  Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 */

namespace FlatTextToPDF
{
    class FlatTextToPDF
    {      
        static int Main(string[] args)
        {
            Console.WriteLine("FlatTextToPDF Sample:");
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: FlatTextToPDF.exe inputfile.txt outputfile.pdf");
                return 1;
            }
            String inputFile = args[0];
            String outputFile = args[1];

            var sw = Stopwatch.StartNew();  // start a timer

            using (Library lib = new Library())
            {
 
                Rect portraitRect = new Rect(0, 0, 595, 842); // A4 -- alternatenately use Rect(0, 0, 612, 792) U.S. letter portrait
                Rect landscapeRect = new Rect(0, 0, 842, 595); // A4 -- alternatenately use  Rect(0, 0, 792, 612) U.S. letter landscape
                Rect pageRect = portraitRect;  // the Rect that will be used when creating pages. Default portraitRect.

                int pageNum = 0; // PDF pages are 0 based 

                // Setup the Text, Font, GraphicState and TextState
                // * Text runs will be added to the Text object
                // * By default, text will be filled with the fill color from the graphic state
                Text t = new Text();
                Font f = new Font("Courier"); // Courier can be used on any system, regarless of whether the font is on the system
                // Font f = new Font("Consolas", FontCreateFlags.Embedded | FontCreateFlags.Subset); // Alternative

                GraphicState gs = new GraphicState();
                gs.FillColor = new Color(0.0);    // DeviceGray black


                // Setup some initial settings and positions
                double fsize = 10.0;             // initial pointsize
                double lineLeading = 10.0;       // amount to move down between each line (may equal fsize)
                TextState ts = new TextState();

                // other things we will need
                double xposPort = 36;          // initial x position ~13 mm
                double yposPort = 806;         // initial y position (842-36 == 13 mm from top)
                double xposLand = 36;          // initial x position ~13 mm
                double yposLand = 559;         // initial y position (595-36 ==  13 mm from top)

                double xpos = xposPort;        // initial x position -- default to portrait
                double ypos = yposPort;        // initial y position

                Matrix m = new Matrix().Translate(xpos, ypos).Scale(fsize, fsize);
                TextRun tr = new TextRun("", f, gs, ts, m);

                // PCL escape sequences
                // *   -- Form Feed (x0C) 
                // *   -- the sequence "<ESC>&l0O" (portrait)
                // *                    "<ESC>&l1O" (landscape)
                // *                    "<ESC>(s010H" (font size 10) 
                // *                    "<ESC>(s016H" (font size 9) 
                // *   (where the <ESC> is x1B)

                char ff = '\f';    // Form Feed -- (char)0x0C; 
                String ffStr = ff.ToString();
                int ffPos = -1;

                bool portraitOrientation = true;

                char esc = (char)0x1B;
                String escStr = esc.ToString();

                String portSeq = escStr + "&l0O";
                String landSeq = escStr + "&l1O";
                String font10Seq = escStr + "(s010H";
                String font9Seq = escStr + "(s016H";

               
                // ******************************************************************
                // Read each line of the input file into a string array. Each element
                // of the array is one line of the file.
                // ******************************************************************
                int lineCount = 0;
                bool skip = false;
                string[] lines = System.IO.File.ReadAllLines(inputFile, Encoding.GetEncoding("windows-1252"));  //"iso-8859-1"
               
                // check first line and create initial page
                    if (lines[0].Contains(landSeq))
                    {
                        Console.WriteLine("Starting page is landscape");
                        pageRect = landscapeRect;
                        portraitOrientation = false;
                        xpos = xposLand;
                        ypos = yposLand; 
                        skip = true;
                    }
                    else
                        if (lines[0].Contains(portSeq))
                        {
                            Console.WriteLine("Starting page is portrait");
                            pageRect = portraitRect;
                            portraitOrientation = true;
                            xpos = xposPort;
                            ypos = yposPort;
                            skip = true;
                        }
                        else
                        {
                            Console.WriteLine("Starting page defaulting to portrait");
                            pageRect = portraitRect;
                            portraitOrientation = true;
                        }
                Document doc = new Document();
                Page docpage = doc.CreatePage(Document.BeforeFirstPage, pageRect);

                foreach (string line in lines)
                {
                    String lineStr = line;  //don't trim here
                    lineCount++;

                    // check for portrait vs landscape
                    if (lineStr.Contains(portSeq))
                    {
                        Console.WriteLine("Change to portrait -- line: " + lineCount);
                        pageRect = portraitRect;
                        portraitOrientation = true; 
                        skip = true;
                    }
                    if (lineStr.Contains(landSeq))
                    {
                        Console.WriteLine("Change to landscape -- line: " + lineCount);
                        pageRect = landscapeRect;
                        portraitOrientation = false; 
                        skip = true;
                    }

                    // check for font size changes
                    if (lineStr.Contains(font10Seq))
                    {
                        Console.WriteLine("Font size change to 10 -- line: " + lineCount);
                        fsize = lineLeading = 10.0; 
                        skip = true;
                    }
                    if (lineStr.Contains(font9Seq))
                    {
                        Console.WriteLine("Font size change to 9 -- line: " + lineCount);
                        fsize = lineLeading = 9.0;
                        skip = true;
                    }
                    if (skip)
                    {
                        skip = false;
                        continue; // lines with font or orientation commands do not have text, so drop them
                     }

                    // check for Form Feed
                    // * FF is expected at the very start of a line, but may have text on the remainder of the line
                    ffPos = lineStr.IndexOf("\f");  
                    if (ffPos>=0)
                    {
                        Console.WriteLine("Form Feed -- line: " + lineCount + " pos: " + ffPos);
                        lineStr = lineStr.Remove(ffPos,1);
                        ffPos = -1;

                        // add all text content to current page
                        docpage.Content.AddElement(t);
                        docpage.UpdateContent();
                        t.Dispose();
                        docpage.Dispose();

                        // start another page and reset the top left position
                        docpage = doc.CreatePage(pageNum, pageRect);
                        t = new Text();
                        pageNum++;
                        if (portraitOrientation)
                        {
                            xpos = xposPort;
                            ypos = yposPort; 
                        }
                        else
                        {
                            xpos = xposLand;
                            ypos = yposLand; 
                        }
                    }

                    //After removing the FF (if any), adjust the matrix and add the line to a text run
                    ypos = ypos - lineLeading; 
                    Matrix m1 = new Matrix().Translate(xpos, ypos).Scale(fsize, fsize);

                    lineStr = lineStr.TrimEnd();
                    tr = new TextRun(lineStr, f, gs, ts, m1);
                    t.AddRun(tr);
                }

                //add the remaining contents to the last page
                docpage.Content.AddElement(t);
                docpage.UpdateContent();

                doc.EmbedFonts(EmbedFlags.None);
                doc.Save(SaveFlags.Full, outputFile); 

            }
            sw.Stop();
            TimeSpan timeElapsed = sw.Elapsed;

            Console.WriteLine("Time elapsed: " + timeElapsed.TotalMilliseconds + " milliseconds");
            return 0;

        }
    }
}