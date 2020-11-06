# DLE-notshipped

APDFL DLE (mostly C#) samples -- Derived from demos, blogs or other sources

** AddTextToDocument-WithTimer.cs - Tests performance/speed of the OCRParams Performance options (Default vs. Faster vs. MoreAccuracy vs. BestAccuracy).  Test (unscientific) showed the Faster option took 25%-50% of the elapsed time of BestAccuracy. v15.0.4 and higher. Place in \Sample_Source\OpticalCharacterRecognition\AddTextToDocumentWithTimer

** FormWalker.cs - Walks through AcroForm field of a PDF, describing the widget annotation properties (should work for statix XFA too). PGallot sample, updated with a bit more info on signature fields.  v15.0.4 and higher. Place in \Sample_Source\InformationExtraction\FormWalker

** PDFOptimizerSampleByteArray.java - sample reads/writes to a byte array and uses ByteArrayInputStream and ByteArrayOutputStream.  To construct the ImageInputStream/ImageOutputStream requires v15.0.4 and higher. Place in \samples\PDFOptimizerSample folder.
 
 ** RepeatingFormXObject.cs - sample creates a Form object to hold boilerplate content that needs to be repeated on each page with unique content added on top for each page. For use with account statements, credit card statements, etc. Creates from 1 to N unique pages. Place in \Sample_Source\ContentCreation\RepeatingFormXObject folder.  Uses DuckyAccountStatement.pdf as the source template
