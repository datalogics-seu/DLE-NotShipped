using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Datalogics.PDFL;

/*
 * The RepeatingFormXObject sample program creates a new PDF file, creating a Form object to hold 
 * any boilerplate content that needs to be repeated on each page.  Unique content on top
 * The Form object is a representation
 * of a Form XObject in the PDF Document (not to be confused with an AcroForm)
 * .
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace RepeatingFormXObject
{

    // Structure to hold the customer data. Expand as needed
    public struct customerInfo
    {
        public string name;
        public string address;
        public string city;
        public string state;
        public string zip;
        public string phone;

        public customerInfo(string cName, string addr, string ci, string st, string z, string ph)
        {
            name = cName;
            address = addr;
            city = ci;
            state = st;
            zip = z;
            phone = ph;
        }
    }
    // Structure to hold the statement data. Expand as needed
    public struct statementData
    {
        public string date;
        public string description;
        public double charges;
        public double credit;
        public double balance;

        public statementData(string d, string desc, double ch, double cr, double bal)
        {
            date = d;
            description = desc;
            charges = ch;
            credit = cr;
            balance = bal;
        }
    }
    class RepeatingForm
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RepeatingFormXObject Sample:");

            // Insert some data.  We could read this from an excel file or XML or other source, but for
            // this sample, we will just insert it here.
            customerInfo customer1 = new customerInfo("Acme, Inc", "123 Main Street", "New York", "NY", "54321", "1 (800) 555-1212");
            statementData transaction1 = new statementData("01/03/2020", "Balance Forward",  50.00, 0, 50.00);
            statementData transaction2 = new statementData("01/13/2020", "Water", 20.00, 0, 70.00);
            statementData transaction3 = new statementData("01/22/2020", "Electric", 110.00, 0, 180.00);
            statementData transaction4 = new statementData("02/03/2020", "Cable", 0, 40.00, 140.00);
            statementData transaction5 = new statementData("02/06/2020", "Phone", 50.00, 0, 190.00);

            var sw = Stopwatch.StartNew(); 

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sTemplate = Library.ResourceDirectory + "Sample_Input/DuckyAccountStatement.pdf";
                String sOutput = "../RepeatingForm-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Output file: " + sOutput);
                int repeatMax = 100000;  // Record count...

                // Setup the Form - the template portion of the PDF that is repeated on each page
                // For this example, we will read this in from another document. 
                // Alternatively, the application could create the appropriate Text, Path and Image elements
                // and add them to Form Content.  
                Document sourceDoc = new Document(sTemplate);
                Page sourcePage = sourceDoc.GetPage(0); // get the first page
                Form templateForm = new Form(sourcePage.Content);

                // Create the output document
                Document doc = new Document();

                //Text t = new Text();
                Font f = new Font("Garamond", FontCreateFlags.Embedded | FontCreateFlags.Subset);  // embedding will extend the time
                GraphicState gs = new GraphicState();
                TextState ts = new TextState();
                Matrix m = new Matrix();
                gs.FillColor = new Color(0.0); // DeviceGray colorspace (black)
                Rect pageRect = new Rect(0, 0, 612, 792);

                for (int i = 1; i <= repeatMax; i++)
                {
                    if ((i % 10000) == 0)
                    {
                        Console.WriteLine("Processing record " + i);
                        Console.WriteLine("Time elapsed: " + sw.ElapsedMilliseconds + " milliseconds.");
                    }

                    Page docpage = doc.CreatePage(Document.LastPage, pageRect);
                    docpage.Content.AddElement(templateForm); // add the template to the output page

                    // add the unique content per page
                    Text t = new Text();

                    // customer data
                    m = new Matrix().Translate(120, 649).Scale(10.0, 10.0);
                    TextRun tr = new TextRun(customer1.name, f, gs, ts, m);
                    t.AddRun(tr);
                    m = new Matrix().Translate(120, 635).Scale(10.0, 10.0);
                    tr = new TextRun(customer1.address, f, gs, ts, m);
                    t.AddRun(tr);
                    m = new Matrix().Translate(120, 621).Scale(10.0, 10.0);
                    String s = customer1.city + ", " + customer1.state + "   " + customer1.zip;
                    tr = new TextRun(s, f, gs, ts, m);
                    t.AddRun(tr);
                    m = new Matrix().Translate(120, 607).Scale(10.0, 10.0);
                    tr = new TextRun(customer1.phone, f, gs, ts, m);
                    t.AddRun(tr);

                    // transaction data - for the sample, transaction data is in fixed positions.
                    // We'll pass the start x and y. Each row is about 27 points below the prior row.
                    // Limited to 5 rows for this template.

                    ProcessTransaction(37, 450, ref t, ref tr, transaction1, f, gs, ts);
                    ProcessTransaction(37, 423, ref t, ref tr, transaction2, f, gs, ts); 
                    ProcessTransaction(37, 396, ref t, ref tr, transaction3, f, gs, ts); 
                    ProcessTransaction(37, 369, ref t, ref tr, transaction4, f, gs, ts); 
                    ProcessTransaction(37, 342, ref t, ref tr, transaction5, f, gs, ts); 

                    // Document number at bottom of each page
                    m = new Matrix().Translate(475, 115).Scale(11.0, 11.0);
                    tr = new TextRun("Document: ", f, gs, ts, m);
                    t.AddRun(tr);
                    m = new Matrix().Translate(525, 115).Scale(11.0, 11.0);
                    tr = new TextRun(i.ToString(), f, gs, ts, m);
                    t.AddRun(tr);
                    docpage.Content.AddElement(t); 

                    docpage.UpdateContent(); // Update the PDF page with the changed content
                    tr.Dispose();
                    t.Dispose();
                    docpage.Dispose();
                }

                doc.EmbedFonts(EmbedFlags.None);
                doc.Save(SaveFlags.Full, sOutput);
            }
            sw.Stop();
            TimeSpan timeElapsed = sw.Elapsed;
            Console.WriteLine("Time elapsed: " + timeElapsed.TotalMilliseconds + " milliseconds");

        }


        static void ProcessTransaction(double x, double y, ref Text t, ref TextRun tr, statementData transaction, Font f, GraphicState gs, TextState ts)
        {

            double pointsize = 10.0;
            // The x position of the five elements of a transaction are
            // Date 37 9x)  Descriprtion 132 (x+95)   Charges 267 (x+230) Credits 360 (x+323)  Balance 450 (x+413)
            Matrix m = new Matrix().Translate(x, y).Scale(pointsize, pointsize);
            tr = new TextRun(transaction.date, f, gs, ts, m);
            t.AddRun(tr);
            m = new Matrix().Translate(x + 95, y).Scale(pointsize, pointsize);
            tr = new TextRun(transaction.description, f, gs, ts, m);
            t.AddRun(tr);
            m = new Matrix().Translate(x + 230, y).Scale(pointsize, pointsize);
            tr = new TextRun(transaction.charges.ToString(), f, gs, ts, m);
            t.AddRun(tr);
            m = new Matrix().Translate(x + 323, y).Scale(pointsize, pointsize);
            tr = new TextRun(transaction.credit.ToString(), f, gs, ts, m);
            t.AddRun(tr);
            m = new Matrix().Translate(x + 413, y).Scale(pointsize, pointsize);
            tr = new TextRun(transaction.balance.ToString(), f, gs, ts, m);
            t.AddRun(tr);
        }

    }
}
