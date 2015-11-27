using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace ocr_demo
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            if (source == null || toCheck == null)
            {
                return false;
            }
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }

    class MyOCRData
    {
        public string filename { get; set; }
        public DateTime created { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string topic { get; set; }
        public DateTime date { get; set; }
        public string type { get; set; }

        

        private static string[] keywords = {"to", "topic", "subject", "merchant", "date", "customer" };
        private static string[] typeKeywords = { "invoice", "order" };

        private static Dictionary<string, string> resultsDict;

        public MyOCRData()
        {
            resultsDict = new Dictionary<string, string>();
            
        }

        // copy constructor
        public MyOCRData(MyOCRData _ocrData)
        {
            filename = _ocrData.filename;
            created = _ocrData.created;
            from = _ocrData.from;
            to = _ocrData.to;
            topic = _ocrData.topic;
            date = _ocrData.date;
            type = _ocrData.type;
        }

        public void doOCR(string filePath)
        {
            List<string> lines = new List<string>();

            try
            {
                var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
                var img = Pix.LoadFromFile(filePath);
                var page = engine.Process(img);
                var iter = page.GetIterator();

                iter.Begin();

                // get document lines
                do
                {
                    string s = iter.GetText(PageIteratorLevel.TextLine);
                    lines.Add(s);
                } while (iter.Next(PageIteratorLevel.TextLine));
            }
            catch (Exception e)
            {
                throw new System.Exception("OCR Parsing Error", e);
            }

            processDocumentLines(lines, keywords);
        }

        private void processDocumentLines(List<string> lines, string[] keywords)
        {
            bool contains = false;
            bool hasType = false;

            foreach(var line in lines)
            {
                contains = false;

                // search for type
                if (!hasType)
                {
                    foreach (var kw in typeKeywords)
                    {
                        if (contains = line.Contains(kw, StringComparison.OrdinalIgnoreCase))
                        {
                            type = kw;
                            hasType = true;
                            break;
                        }
                    }

                    // if no type keywords has been found, assing an empty string
                    if (!contains)
                    {
                        type = "";
                        hasType = true;
                    }
                }

                // search for other keywords
                foreach (var kw in keywords)
                {
                    if (contains = line.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    {
                        //resultsDict.Add(kw, "");
                    }
                }
            }

            string s = "";


        }
    }
}
