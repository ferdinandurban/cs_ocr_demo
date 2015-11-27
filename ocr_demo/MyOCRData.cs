using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

using System.Text.RegularExpressions;

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
        public string date { get; set; }
        public string type { get; set; }



        private static string[] topicKeywords = { "subject" };
        private static string[] toKeywords = { "dear", "customer name" };
        private static string[] typeKeywords = { "invoice", "order" };
        private static string[] dateKeywords = { "/", "january", "february", "march", "april", "may", "june", "july", "august", "september", "october", "november", "december" };
        private static string[] greetingsKeywords = { "Best regards", "Sincerely", "Yours faithfully" };


        // regular expressions for dates
        private string datePattern1 = "(0?[1-9]|[12][0-9]|3[01])/(0?[1-9]|1[012])/((19|20)\\d\\d)";

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

        public void clearData()
        {
            filename = "";
            created = new DateTime();
            from = "";
            to = "";
            topic = "";
            date = "";
            type = "";
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

            processDocumentLines(lines);
        }

        private void processDocumentLines(List<string> source)
        {
            bool contains = false;
            bool hasType = false;
            bool hasDate = false;
            bool hasTopic = false;
            bool hasFrom = false;
            bool hasTo = false;
            int index = 0;

            // remove empty lines
            var lines = source.Where(str => !string.IsNullOrWhiteSpace(str)).Distinct().ToList();

            foreach (var line in lines)
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
                }

                // search for dates
                if (!hasDate)
                {
                    foreach (var kw in dateKeywords)
                    {
                        if (contains = line.Contains(kw, StringComparison.OrdinalIgnoreCase))
                        {
                            if (getDate(removeLineEndings(line)))
                            {   
                                hasDate = true;
                            }
                        }
                    }

                    // if no relevant date has been found
                    if(!hasDate) this.date = "";
                }

                // search for greetings
                if (!hasFrom)
                {
                    foreach (var kw in greetingsKeywords)
                    {
                        if (contains = line.Contains(kw, StringComparison.OrdinalIgnoreCase))
                        {
                            string l = lines.ElementAt(index + 1);
                            this.from = removeLineEndings(l);
                            hasFrom = true;
                        }
                    }
                }

                // search To
                if(!hasTo)
                {
                    foreach (var kw in toKeywords)
                    {
                        if (contains = line.Contains(kw, StringComparison.OrdinalIgnoreCase))
                        {
                            this.to = removeLineEndings(getName(line, kw));
                            hasTo = true;                            
                        }
                    }
                }

                // search topic
                if (!hasTopic)
                {
                    foreach (var kw in topicKeywords)
                    {
                        if (contains = line.Contains(kw, StringComparison.OrdinalIgnoreCase))
                        {
                            this.topic = removeLineEndings(getName(line, kw));
                            hasTopic = true;
                        }
                    }
                }

                index++;
            }
        }

        private string removeLineEndings(string line)
        {
            return Regex.Replace(line, @"\t|\n|\r", "");
        }

        private bool getDate(string line)
        {
            // for date in DD/MM/YY format
            Regex regex = new Regex(datePattern1);
            Match match = regex.Match(line);

            if(match.Success)
            {
                this.date = match.Value;
                return true;
            }

            regex = new Regex(@"(19|20)\d\d");
            match = regex.Match(line);

            if (match.Success)
            {
                this.date = line;
                return true;
            }
            
            return false;
        }
        
        private string getName(string line, string pattern)
        {
            if (line.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return line.Substring(pattern.Length + 1);
            }
            
            return "";
        }        

    }
}
