using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;
using System.IO;

/*
 * 
 * A sample which demonstrates the use of the DLE API to walk through
 * the AcroForm fields in a PDF file. This version reports additional signature information
 *
 * Copyright (c) 2007-2015, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.  Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 *
 */



namespace FormWalker
{
    class FormWalker
    {
        static string[] get_button_info(PDFDict field)
        {
            string svalue;
            string sdefault_value;
            PDFObject entry;

            entry = field.Get("V");
            if (entry is PDFName)
            {
                PDFName field_value_name = (PDFName)entry;
                svalue = "Value: " + field_value_name.Value;
            }
            else
                svalue = "";

            entry = field.Get("DV");
            if (entry is PDFName)
            {
                PDFName field_defvalue_name = (PDFName)entry;
                sdefault_value = "Default Value: " + field_defvalue_name.Value;
            }
            else
                sdefault_value = "";

            return new string[]{svalue, sdefault_value};
        }

        static string[] get_text_info(PDFDict field)
        {
            string svalue;
            string sdefault_value;
            string sMax_Length;
            PDFObject entry;

            entry = field.Get("V");
            if (entry is PDFString)
            {
                PDFString value = (PDFString)entry;
                svalue = "Value: " + value.Value;
            }
            else
                svalue = "";

            entry = field.Get("DV");
            if (entry is PDFString)
            {
                PDFString defvalue = (PDFString)entry;
                sdefault_value = "Default Value: " + defvalue.Value;
            }
            else
                sdefault_value = "";

            entry = field.Get("MaxLen");
            if (entry is PDFInteger)
            {
                PDFInteger int_entry = (PDFInteger)entry;
                int nMax_Length = 0;
                nMax_Length = int_entry.Value;
                sMax_Length = String.Format("Max Length: {0}", nMax_Length);
            }
            else
                sMax_Length = "";

            return new string[]{svalue,sdefault_value,sMax_Length};
        }

        static string[] get_choice_info(PDFObject field)
        {
            //PCG: TODO
            return new string[]{"",};
        }

        static string[] get_sig_info(PDFDict field)
        {
            String filterName = null;
            String signerName=null;
            String contactInfo=null;
            String location = null;
            String reason = null;
            String sigM = null;

            PDFObject entry;

            entry = field.Get("V");
            if (entry is PDFDict)
            {
                // Extracting Signature Dictionary entries, as per Table 8.102 in the PDF (v1.7) Reference.
                var sigDict = entry as PDFDict;
                filterName = "Filter: " + (sigDict.Get("Filter") as PDFName).Value;

                //The following are optional.
                signerName = (sigDict.Contains("Name") ? "Name: " + (sigDict.Get("Name") as PDFString).Value : "");
                reason = (sigDict.Contains("Reason") ? "Reason: " + (sigDict.Get("Reason") as PDFString).Value : "");
                contactInfo = (sigDict.Contains("ContactInfo") ? "ContactInfo: " + (sigDict.Get("ContactInfo") as PDFString).Value : "");
                location = (sigDict.Contains("Location") ? "Location: " + (sigDict.Get("Location") as PDFString).Value : "");
                sigM = (sigDict.Contains("M") ? "M: " + (sigDict.Get("M") as PDFString).Value : ""); 


                return new string[] { filterName, signerName, contactInfo, location, reason, sigM};
            }

            return new string[] { "", };
        }


        static void enumerate_field(PDFObject field_entry, string prefix)
        {
            string name_part;
            string field_name;
            string alternate_name=null;
            string mapping_name = null;
            string field_type;
            string[] field_info=null;
            int field_flags=0;
            bool additional_actions = false;
            bool javascript_formatting = false;
            bool javascript_calculation = false;
            bool javascript_validation = false;
            PDFObject entry;

            if (field_entry is PDFDict)
            {
                PDFDict field = (PDFDict)field_entry;
                entry = (PDFString)field.Get("T");
                if (entry is PDFString)
                {
                    PDFString t = (PDFString)entry;
                    name_part = t.Value;
                }
                else
                    return;
                
                if(prefix =="")
                {
                    field_name = name_part;
                }
                else
                { 
                    field_name = string.Format("{0}.{1}", prefix, name_part);
                }

                entry = field.Get("Kids");
                if (entry is PDFArray)
                {
                    PDFArray kids = (PDFArray)entry;
                    for(int i=0; i < kids.Length; i++)
                    {
                        PDFObject kid_entry = kids.Get(i);
                        enumerate_field(kid_entry, field_name);
                    }
                }
                else //no kids, so we are at an end-node.
                {
                    Console.WriteLine("Name: " + field_name);

                    entry = field.Get("TU");
                    if (entry is PDFString)
                    {
                        PDFString tu = (PDFString)entry;
                        alternate_name = tu.Value;
                    }

                    entry = field.Get("TM");
                    if (entry is PDFString)
                    {
                        PDFString tm = (PDFString)entry;
                        mapping_name = tm.Value;
                    }

                    entry = field.Get("Ff");
                    if (entry is PDFInteger)
                    {
                        PDFInteger ff = (PDFInteger)entry;
                        field_flags = ff.Value;
                    }

                    entry = field.Get("AA");
                    if (entry is PDFDict)
                    {
                        additional_actions = true;
                        var aadict = entry as PDFDict;
                        javascript_formatting = aadict.Contains("F");
                        javascript_calculation = aadict.Contains("C");
                        javascript_validation = aadict.Contains("V");
                    }

                    entry = field.Get("FT");
                    if (entry is PDFName)
                    {
                        PDFName field_type_name = (PDFName)entry;
                        switch (field_type_name.Value)
                        {
                            case "Btn": field_type = "Button"; field_info = get_button_info(field); break;
                            case "Tx": field_type = "Text"; field_info = get_text_info(field); break;
                            case "Ch": field_type = "Choice"; field_info = get_choice_info(field); break;
                            case "Sig": field_type = "Signature"; field_info = get_sig_info(field); break;
                            default: field_type = field_type_name.Value; return;
                        }
                    }
                    else
                        field_type = "None";

                    if (alternate_name != null)
                        Console.WriteLine("Alternate Name: " + alternate_name);
                    if (mapping_name != null)
                        Console.WriteLine("Mapping Name: " + mapping_name);
                    if(additional_actions)
                        Console.WriteLine("Additional Actions: Javascript {0}{1}{2}.",
                            javascript_validation?"Validation, ":"",
                            javascript_calculation?"Calculation, ":"",
                            javascript_formatting?"Formatting":"");

                    Console.WriteLine("Type: " + field_type);

                    if (field_flags != 0)
                    {
                        bool[] flags = new bool[28];
                        for(int bitpos=1; bitpos < flags.Length;bitpos++){
                            flags[bitpos] = (0 != (field_flags & (0x1 << bitpos-1)));
                        }
                        if (field_type == "Signature")
                                Console.WriteLine(String.Format("Signature Flags: {0:x8}: requires {1}{2}{3}{4}{5}{6}{7}", field_flags,
                                    flags[1]?"Filter, ":"",
                                    flags[2]?"SubFilter, ":"",
                                    flags[3]?"V, ":"",
                                    flags[4]?"Reason, ":"",
                                    flags[5]?"LegalAttestation, ":"",
                                    flags[6]?"AddRevInfo, ":"",
                                    flags[7]?"DigestMethod":""
                                    ));
                        else
                                Console.WriteLine(String.Format("Format Flags: {0:x8}: {1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}", field_flags,
                                    flags[1]?"ReadOnly ":"",
                                    flags[2]?"Required ":"",
                                    flags[3]?"NoExport ":"",
                                    flags[13]?"MultiLine ":"",
                                    flags[14]?"Password ":"",
                                    flags[15]?"NoToggleToOff ":"",
                                    flags[16]?"Radio ":"",
                                    flags[17]?"PushButton ":"",
                                    flags[18]?"Combo ":"",
                                    flags[19]?"Edit ":"",
                                    flags[20]?"Sort ":"",
                                    flags[21]?"FileSelect ":"",
                                    flags[22]?"MultiSelect ":"",
                                    flags[23]?"DoNotSpellCheck ":"",
                                    flags[24]?"DoNotScroll ":"",
                                    flags[25]?"Comb ":"",
                                    flags[26]?(field_type=="Text"?"RichText":(field_type=="Button"?"RadiosInUnison":"?")):"",
                                    flags[27]?"CommitOnSelChange ":""
                                    ));
                    }

                    foreach (string item in field_info)
                    {
                        if (item != "")
                            Console.WriteLine(item);
                    }
                    Console.WriteLine("");
                }
            }     
        }

        static void DisplayRootDictionary(PDFDict formsRoot)
        {
            PDFObject entry;
            bool bNeedAppearances = false;
            int nSigFlags = 0;
            bool bCalcOrder= false;
            bool bDefaultResource = false;
            bool bDefaultAppearance= false;
            bool bXFAForms = false;
            int QuadMode = -1;
            string sQuadMode="unkown";

            entry = formsRoot.Get("NeedAppearances");
            if (entry is PDFBoolean)
            {
                PDFBoolean needAppearances = (PDFBoolean)entry;
                bNeedAppearances = needAppearances.Value;
            }

            Console.WriteLine("NeedAppearances: " + (bNeedAppearances ? "True" : "False"));

            entry = formsRoot.Get("SigFlags");
            if (entry is PDFInteger)
            {
                PDFInteger sigFlags = (PDFInteger)entry;
                nSigFlags = sigFlags.Value;
            }

            if (nSigFlags == 0)
                Console.WriteLine("Document has no signatures.");
            else
            {
                if ((nSigFlags & 1) == 1)
                    Console.WriteLine("Document has signatures.");
                if ((nSigFlags & 2) == 2)
                    Console.WriteLine("Signatures: Document may append only.");
            }

            entry = formsRoot.Get("CO");
            if (entry is PDFDict)
                bCalcOrder = true;
            Console.WriteLine(String.Format("Calculation Order Dictionary is {0}present.",(bCalcOrder?"":"not ")));

            entry = formsRoot.Get("DR");
            if (entry is PDFDict)
                bDefaultResource = true;
            Console.WriteLine(String.Format("Default Resource Dictionary is {0}present.", (bDefaultResource? "" : "not ")));
 
            entry = formsRoot.Get("DA");
            if (entry is PDFString)
                bDefaultAppearance = true;
            Console.WriteLine(String.Format("Default Appearance String is {0}present.", (bDefaultAppearance ? "" : "not ")));

            entry = formsRoot.Get("Q");
            if (entry is PDFInteger)
            {
                PDFInteger quad_entry = (PDFInteger)entry;
                QuadMode = quad_entry.Value;
            }
            switch (QuadMode)
            {
                case -1: sQuadMode = "not present"; break;
                case 0: sQuadMode = "Left"; break;
                case 1: sQuadMode = "Centered"; break;
                case 2: sQuadMode = "Right"; break;
            }
            Console.WriteLine(String.Format("Default Quad Mode is {0}.", sQuadMode));

            entry = formsRoot.Get("XFA");
            if (entry is PDFString)
                bXFAForms = true;
            Console.WriteLine(String.Format("XFA Forms are {0}present.", (bXFAForms ? "" : "not ")));
            Console.WriteLine("");
        }


        static void Main(string[] args)
        {
            Console.WriteLine("FormWalker Sample:");
            FileStream fs = new FileStream("FormWalkerOutput.log", FileMode.Create);
            // First, save the standard output.
            TextWriter tmp = Console.Out;
            StreamWriter sw = new StreamWriter(fs);
            Console.SetOut(sw);
    
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput1 = Library.ResourceDirectory + "Sample_Input/MultipleSignatures.pdf";
                if (args.Length > 0)
                    sInput1 = args[0];

                Document doc = new Document(sInput1);
                Console.WriteLine("Opened document " + sInput1);
                Console.WriteLine("");

                PDFObject form_entry = doc.Root.Get("AcroForm");
                if (form_entry is PDFDict)
                {
                    PDFDict form_root = (PDFDict)form_entry;
                    DisplayRootDictionary(form_root);

                    PDFObject fields_entry = form_root.Get("Fields");
                    if(fields_entry is PDFArray)
                    {
                        PDFArray fields = (PDFArray)fields_entry;
                        for(int i=0; i <fields.Length; i++){
                            PDFObject field_entry = fields.Get(i);
                            enumerate_field(field_entry, "");
                        }
                    }
                }
                Console.SetOut(tmp);

                sw.Close();

                Console.WriteLine("Done.");
            }                
        }
    }
}