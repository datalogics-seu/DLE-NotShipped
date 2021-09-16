using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Datalogics.PDFL;

/*
 * The RepeatingFormXObject sample is a demonstration of creating a large volume of PDF pages to for an invoicing 
 * or customer statement type of application.  In this sample, some of the content is static and has fixed positions.
 * The sample imports a tempate PDF and creates a Form object to hold that boilerplate content that needs to be repeated 
 * on each page.  Unique content is then added on top. 
 * The Form object is a representation of a PDF Form XObject (not to be confused with an AcroForm)
 * 
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace RepeatingFormXObject
{

    // public string customerInfoFont = "Garamond";
    // public duble ptSize = "10.0";

    // Structure to hold the customer data. Expand as needed
    public struct customerInfo
    {
        public string name;
        public string address;
        public string city;
        public string state;
        public string zip;
        public string phone;

        // position data.  Assume it is fixed positions,  per template. 
        public double name_x;
        public double name_y;
        public double address_x;
        public double address_y;
        public double city_x;
        public double city_y;
        public double zip_x;
        public double zip_y;
        public double phone_x;
        public double phone_y;

        public customerInfo(string name, string address, string city, string state, string zip, string phone)
       {
            this.name = name;
            this.address = address;
            this.city = city;
            this.state = state;
            this.zip = zip;
            this.phone = phone;

            // Assume that the customer info data pieces are located at fixed positions, per template.
            // For the ducky template, the x,y coordinates are ( PDF origin is at the lower left):
            this.name_x = 120;
            this.name_y = 649;
            this.address_x = 120;
            this.address_y = 635;
            this.city_x = 120;
            this.city_y = 621;
            this.zip_x = 120;
            this.zip_y = 621; // City, State and Zip are all on the same line, concatenated, so this is not needed.
            this.phone_x = 120;
            this.phone_y = 607;
        }
    }
    // Structure to hold the transaction data. Expand as needed
    public struct transactionData
    {
        public string date;
        public string description;
        public double charges;
        public double credit;
        public double balance;

        //public double pos_x;
        //public double pos_y;

        public transactionData(string date, string description, double charges, double credit, double balance)
        {
            this.date = date;
            this.description = description;
            this.charges = charges;
            this.credit = credit;
            this.balance = balance;

            //this.pos_x = pos_x;
            //this.pos_y = pos_y;
        }
    }
    class RepeatingForm
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RepeatingFormXObject Sample:");

            int repeatMax = 100;  // Record count...
            string customerInfoFont = "Garamond";
            double customerInfoPointSize = 10.0;
            string transactionDataFont = "Arial";
            double transactionDataPointSize = 10.0;
            double transactionData_xPos = 0;
            double transactionData_yStartPos = 0;
            double transactionData_leading = 27;
            double transactionData_count = 0;


            // Populate some customer transaction data.  This could be read from a CSV or XML file or other source, one per customer. 
            // But for this sample, we will just insert it here.
            // Could pass in position data as well.
            customerInfo customer1 = new customerInfo("Acme, Inc", "123 Main Street", "New York", "NY", "54321", "1 (800) 555-1212");
            transactionData transaction1 = new transactionData("01/03/2020", "Balance Forward",  50.00, 0, 50.00);
            transactionData transaction2 = new transactionData("01/13/2020", "Water", 20.00, 0, 70.00);
            transactionData transaction3 = new transactionData("01/22/2020", "Electric", 110.00, 0, 180.00);
            transactionData transaction4 = new transactionData("02/03/2020", "Cable", 0, 40.00, 140.00);
            transactionData transaction5 = new transactionData("02/06/2020", "Phone", 50.00, 0, 190.00);

            var sw = Stopwatch.StartNew(); 

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sTemplate = Library.ResourceDirectory + "Sample_Input/DuckyAccountStatement.pdf";
                String sOutput = "../RepeatingForm-out-" + repeatMax + ".pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Output file: " + sOutput);

                // Setup the Form... the boilerplate portion that is repeated on each page.
                // For this example, we will pull this from another PDF document. 
                // Alternatively, the application could create the appropriate Text, Path and Image elements
                // and add them to Form Content.  
                Document sourceDoc = new Document(sTemplate);
                Page sourcePage = sourceDoc.GetPage(0); // get the first page
                Form templateForm = new Form(sourcePage.Content);

                // Create the output document
                Document doc = new Document();

                Font fC = new Font(customerInfoFont, FontCreateFlags.Embedded | FontCreateFlags.Subset);  // will embed the font
                Font fT = new Font(transactionDataFont, FontCreateFlags.Embedded | FontCreateFlags.Subset);  
                GraphicState gs = new GraphicState();
                TextState ts = new TextState();
                Matrix m = new Matrix();
                gs.FillColor = new Color(0.0); // DeviceGray colorspace (black)
                Rect pageRect = new Rect(0, 0, 612, 792);

                for (int i = 1; i <= repeatMax; i++)
                {
                    if ((i % 10000) == 0)
                    {
                        // Every 10K, output the timer
                        Console.WriteLine("Processing record " + i);
                        Console.WriteLine("Time elapsed: " + sw.ElapsedMilliseconds + " milliseconds.");
                    }

                    Page docpage = doc.CreatePage(Document.LastPage, pageRect);

                    // add the template to the page
                    docpage.Content.AddElement(templateForm); 

                    // add the unique content per page
                    Text t = new Text();

                    // call for each customer. Only 1 for the sample
                    DisplayCustomerInfo(customer1, ref t, fC, gs, ts, customerInfoPointSize);

                    // transaction data - for the sample, transaction data is in fixed positions.
                    // We'll pass the start x and y. Each row is about 27 points below the prior row.
                    // Limited to 5 rows for this template.

                    ProcessTransaction(transaction1, 37, 450, ref t, fT, gs, ts, transactionDataPointSize);
                    ProcessTransaction(transaction2, 37, 423, ref t, fT, gs, ts, transactionDataPointSize); 
                    ProcessTransaction(transaction3, 37, 396, ref t, fT, gs, ts, transactionDataPointSize); 
                    ProcessTransaction(transaction4, 37, 369, ref t, fT, gs, ts, transactionDataPointSize); 
                    ProcessTransaction(transaction5, 37, 342, ref t, fT, gs, ts, transactionDataPointSize); 

                    // Add document number at bottom of each page
                    m = new Matrix().Translate(475, 115).Scale(11.0, 11.0);
                    TextRun tr = new TextRun("Document: " + i.ToString(), fC, gs, ts, m);
                    t.AddRun(tr);
                    docpage.Content.AddElement(t); 

                    docpage.UpdateContent(); // Update the PDF page with the changed content
                    tr.Dispose();
                    t.Dispose();
                    docpage.Dispose();
                }

                doc.EmbedFonts(EmbedFlags.None);
                doc.Save(SaveFlags.Full, sOutput);

                // Dispose of things
                templateForm.Dispose();
                sourcePage.Dispose();
                sourceDoc.Dispose();
                doc.Dispose();
            }

            sw.Stop();
            TimeSpan timeElapsed = sw.Elapsed;
            Console.WriteLine("Document closed and saved. Total time elapsed: " + timeElapsed.TotalSeconds + " seconds");
            Console.WriteLine("   or " + repeatMax/timeElapsed.TotalSeconds + " pages per second");

        }


        static void DisplayCustomerInfo(customerInfo customer, ref Text t, Font f, GraphicState gs, TextState ts, double pointSize)
        {
            Matrix m1 = new Matrix().Translate(customer.name_x, customer.name_y).Scale(pointSize, pointSize); // customerInfoPointSize
            TextRun tr1 = new TextRun(customer.name, f, gs, ts, m1);
            t.AddRun(tr1);
            m1 = new Matrix().Translate(customer.address_x, customer.address_y).Scale(pointSize, pointSize);
            tr1 = new TextRun(customer.address, f, gs, ts, m1);
            t.AddRun(tr1);
            m1 = new Matrix().Translate(customer.city_x, customer.city_y).Scale(pointSize, pointSize);
            String s = customer.city + ", " + customer.state + "   " + customer.zip;
            tr1 = new TextRun(s, f, gs, ts, m1);
            t.AddRun(tr1);
            m1 = new Matrix().Translate(customer.phone_x, customer.phone_y).Scale(pointSize, pointSize);
            tr1 = new TextRun(customer.phone, f, gs, ts, m1);
            t.AddRun(tr1);
            
            m1.Dispose();
            tr1.Dispose();
        }
        static void ProcessTransaction(transactionData transaction, double x, double y, ref Text t, Font f, GraphicState gs, TextState ts, 
            double pointSize)
        {
            // The x position of the five elements of a transaction are
            // Date 37 9x)  Descriprtion 132 (x+95)   Charges 267 (x+230) Credits 360 (x+323)  Balance 450 (x+413)
            Matrix m = new Matrix().Translate(x, y).Scale(pointSize, pointSize);
            TextRun tr = new TextRun(transaction.date, f, gs, ts, m);
            t.AddRun(tr);
            m = new Matrix().Translate(x + 95, y).Scale(pointSize, pointSize);
            tr = new TextRun(transaction.description, f, gs, ts, m);
            t.AddRun(tr);
            m = new Matrix().Translate(x + 230, y).Scale(pointSize, pointSize);
            tr = new TextRun(transaction.charges.ToString(), f, gs, ts, m);
            t.AddRun(tr);
            m = new Matrix().Translate(x + 323, y).Scale(pointSize, pointSize);
            tr = new TextRun(transaction.credit.ToString(), f, gs, ts, m);
            t.AddRun(tr);
            m = new Matrix().Translate(x + 413, y).Scale(pointSize, pointSize);
            tr = new TextRun(transaction.balance.ToString(), f, gs, ts, m);
            t.AddRun(tr);
        }

    }
}
