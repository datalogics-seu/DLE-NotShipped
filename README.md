# DLE-notshipped

APDFL DLE (mostly CSharp) samples -- Derived from demos, blogs or other sources

** AddHeaderFooter.cs - Adds a haeader and footer to the first page. Uses the Page BoundingBox (or BBox) to determine the area of the page that  
contains content and then adds the header/footer text to the areas above and below the current BBox. 

** AddTextToDocument-WithTimer.cs - Tests performance/speed of the OCRParams Performance options (Default vs. Faster vs. MoreAccuracy vs. BestAccuracy).  Unscientific test showed that the Faster option took 25%-50% of the elapsed time of BestAccuracy. v15.0.4 and higher. Place in \Sample_Source\OpticalCharacterRecognition\AddTextToDocumentWithTimer

** CheckForMissingAppearanceStreams.cs - Examine any Widget annotations (not all annots) for fields that have a missing appearance and also have a value.  v15.0.4 and higher. Place in \Sample_Source\Annotations\

** ConvertTextToOutlines.cs - "Tricks" APDFL into converting all text to outlines by adding a small translucent element to each page and then calling the flattener with the "UseTextOutlines" option (equivalent to the Acrobat -> Flattener Preview "Convert All Text To Outlines" ).  v15.0.4 and higher. Place in \Sample_Source\ModifyContent\

** FlattenAnnotations.cs - Converts annotation appearance streams to page content. Note: Output not exactly the same; might need to be transparency flattened?
v18.0.3 and higher. Place in \Sample_Source\Annotations

** FlattenPortfolio.cs - Flatten a PDF portfolio to a standard PDF - removes attachments and inserts any PDF attachments as regular pages in the document

** FlattenTransparency-ConvertTextToCurves.cs - Converts text elements to path elements via the TransparencyFlattener feature.  Place in \Sample_Source\ContentModification

** FlatTextToPDF - converts a flat text file with some PCL codes to a PDF document using a monospace font. 

** FormWalker.cs - Walks through AcroForm field of a PDF, describing the widget annotation properties (should work for static XFA too). PGallot sample, updated with a bit more info on signature fields.  v15.0.4 and higher. Place in \Sample_Source\InformationExtraction\FormWalker

** ImposeTwoPages.cs - Basic imposition sample. Imports content stream of two source pages into Form XObjects and inserts and positions them onto new, larger spread that fits both. v15.0.4 and higher. Place in \Sample_Source\ContentModification

** ModifyLinkAnnotDests.cs - Walk through the link annots and change the JS or URI Actions that contain the destination URL.   v15.0.4 and higher. Place in \Sample_Source\ContentModification\ModifyLinkAnnotDests

** PDFOptimizerSampleByteArray.java - Optimization sample that reads/writes to a byte array and uses ByteArrayInputStream and ByteArrayOutputStream.  To construct the ImageInputStream/ImageOutputStream requires v15.0.4 and higher. Place in \samples\PDFOptimizerSample folder.

** RasterizeAndRunOCR.cs - For documents with pages where the content doesn't lend itself to running the OCR directly (e.g. vector objects), it can be better to rasterize the contents to an image, then OCR the image.
  
 ** RepeatingFormXObject.cs - Sample creates a Form object to hold boilerplate content that needs to be repeated on each page; and adds unique content on top for each page. For use with account statements, credit card statements, etc. Creates from 1 to N unique pages. Place in \Sample_Source\ContentCreation\RepeatingFormXObject folder.  Uses DuckyAccountStatement.pdf as the source template

** SplitPDFVariations.cs - Sample that demonstrates splitting a PDF document based on page intervals or bookmarks or by hits on key search strings.  v15.0.4 and higher.  From the blog article https://gist.github.com/datalogics-seu/4e62fd26ffcb82a30bf2458110e4b341#file-splitpdfvariations-cs

** TextExtract-FromRegionsAndLinks.cs - Captures the text from specific target locations and from underneath link rectangles by checking the Word box against the target region 
