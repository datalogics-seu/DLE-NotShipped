package com.datalogics.pdfl.samples.DocumentOptimization.PDFOptimizerSample;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.BufferedReader;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.nio.file.Files;

import javax.imageio.ImageIO;
import javax.imageio.stream.ImageInputStream;
import javax.imageio.stream.ImageOutputStream;

import com.datalogics.PDFL.*;
/*
 * 
 * Use the PDFOptimizerSample to experiment with the PDF Optimization feature in the Adobe PDF Library.
 * 
 * This variation of the sample reads/writes to a byte array and uses ByteArrayInputStream and ByteArrayOutputStream 
 * to construct the ImageInputStream/ImageOutputStream that DLE needs
 * 
 * For more detail see the description of the PDFOptimizerSample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/using-the-pdf-optimizer-to-manage-the-size-of-pdf-documents
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class PDFOptimizerSampleByteArray {

    /**
     * @param args
     */
    public static void main(String[] args) throws Exception {
        System.out.println("PDFOptimizer byte array sample:");

        Library lib = new Library();
        try {

            String sInput = Library.getResourceDirectory() + "Sample_Input/FindImageResolutions.pdf";
            String sOutput = "PDFOptimizer-out-bytearray.pdf";
            if (args.length > 0)
                sInput = args[0];
            if (args.length > 1 )
                sOutput = args[1];

            System.out.println ( "Will optimize " + sInput + " and save as " + sOutput );
            
            PDFOptimizer opt = new PDFOptimizer();
            try {
                // For testing, we will read from the file to a byte array
                File inFile = new File(sInput);
                //long beforeLength = inFile.length();
                byte[] inByteArr = Files.readAllBytes(inFile.toPath());

                // we now have a byte [].  Create a doc object 
                ByteArrayInputStream inBais = new ByteArrayInputStream(inByteArr);
                ImageInputStream iisInPDF = ImageIO.createImageInputStream(inBais);
                Document doc = new Document(iisInPDF);


                boolean linearizeBefore = opt.getOption(OptimizerOption.LINEARIZE);
                opt.setOption(OptimizerOption.LINEARIZE, true);
                boolean linearizeAfter = opt.getOption(OptimizerOption.LINEARIZE);
                if (linearizeBefore != linearizeAfter)
                    System.out.println("Successfully set PDF Option Linearize to ON.");
                else
                    System.out.println("Failed to set PDF Option Linearize to ON!");


                // optimize to a BAOS and write that to a byte []
                ByteArrayOutputStream baos = new ByteArrayOutputStream();
                ImageOutputStream iosOutPDF = ImageIO.createImageOutputStream(baos);
                opt.optimize(doc, iosOutPDF);
             
                String length = Long.toString(iosOutPDF.length());
                System.out.println("iosOUTPDF length " +  length);
                iosOutPDF.close();
                System.out.println("baos size " +  baos.size());  // 
                
                byte[] data = baos.toByteArray();
                System.out.println("data byte array length " +  data.length);  //

                // For testing, write the byte array to a file
                OutputStream  os = new FileOutputStream(sOutput); 
                os.write(data);
                os.close();

                System.out.flush();

            }
            finally
            {
                opt.delete();
            }

        } 
        finally
        {
            lib.delete();
        }

    }

}
