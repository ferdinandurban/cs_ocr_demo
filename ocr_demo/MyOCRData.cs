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
        
        private string datePattern1 = "(0?[1-9]|[12][0-9]|3[01])/(0?[1-9]|1[012])/((19|20)\\d\\d)";
        private string postalCodeUS = @"\d{5}([ \-]\d{4})?";
        private string postalCodeGB = "GIR[ ]?0AA|((AB|AL|B|BA|BB|BD|BH|BL|BN|BR|BS|BT|CA|CB|CF|CH|CM|CO|CR|CT|CV|CW|DA|DD|DE|DG|DH|DL|DN|DT|DY|E|EC|EH|EN|EX|FK|FY|G|GL|GY|GU|HA|HD|HG|HP|HR|HS|HU|HX|IG|IM|IP|IV|JE|KA|KT|KW|KY|L|LA|LD|LE|LL|LN|LS|LU|M|ME|MK|ML|N|NE|NG|NN|NP|NR|NW|OL|OX|PA|PE|PH|PL|PO|PR|RG|RH|RM|S|SA|SE|SG|SK|SL|SM|SN|SO|SP|SR|SS|ST|SW|SY|TA|TD|TF|TN|TQ|TR|TS|TW|UB|W|WA|WC|WD|WF|WN|WR|WS|WV|YO|ZE)(\\d[\\dA-Z]?[ ]?\\d[ABD-HJLN-UW-Z]{2}))|BFPO[ ]?\\d{1,4}";
        private string postalCodeCZ = @"\d{3}[ ]?\d{2}";

        public MyOCRData()
        {

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
                    if(s != null)
                    {
                        lines.Add(removeLineEndings(s));
                    }
                    
                } while (iter.Next(PageIteratorLevel.TextLine));
            }
            catch (Exception e)
            {
                throw new System.Exception("OCR Parsing Error", e);
            }

            processDocument(lines);
        }

        private void processDocument(List<string> source)
        {
            var lines = source.Where(str => !string.IsNullOrWhiteSpace(str)).Distinct().ToList();

            string result = "";

            foreach (var kw in topicKeywords)
            {
                if (findInDocument(lines, kw, out result))
                {
                    this.topic = Regex.Replace(result, kw + "[^a-zA-Z0-9]", "", RegexOptions.IgnoreCase);
                    break;
                }
            }

            foreach (var kw in dateKeywords)
            {
                if (findInDocument(lines, kw, out result))
                {
                    if(getDate(result)) break;
                }
            }
            foreach (var kw in toKeywords)
            {
                if (findInDocument(lines, kw, out result))
                {
                    this.to = Regex.Replace(result, kw, "", RegexOptions.IgnoreCase);
                    break;
                }
            }
            foreach (var kw in greetingsKeywords)
            {
                if (findInDocument(lines, kw, out result))
                {
                    int idx = (lines == null ? -1 : lines.IndexOf(result));

                    this.from = lines.ElementAt(idx + 1);
                    break;
                }
            }
            foreach (var kw in typeKeywords)
            {
                if (findInDocument(lines, kw, out result))
                {
                    if(kw.Equals("invoice"))
                    {
                        findAddress(source);
                    }

                    this.type = kw;
                    break;
                }
            }
        }

        public bool findInDocument(List<string> source, string keyword, out string output)
        {
            IEnumerable<string> results;

            try
            {
                results = source.Where(s => s.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception e) 
            {
                output = null;
                return false;
            }
            
            if (results != null && results.Any())
            {
                output = results.First();
                return true;
            }
            else
            {
                output = "";
                return false;
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

            regex = new Regex(@"(19|2)\d\d");
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

        private void findAddress(List<string> source)
        {
            Regex regex = new Regex(postalCodeGB);

            var address = from line in source
                          let match = regex.Match(line)
                          where match.Success
                          select new
                          {
                                idx = source.IndexOf(line),
                                line1 = source[source.IndexOf(line) - 2],
                                line2 = source[source.IndexOf(line) - 1],
                                line3 = line
                       };
        }
    }
}
