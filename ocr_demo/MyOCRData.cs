using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocr_demo
{
    class MyOCRData
    {
        public string filename { get; set; }
        public DateTime created { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string topic { get; set; }
        public DateTime date { get; set; }
        public string type { get; set; }

        private static string[] keywords = {"invoice", "to", "topic", "subject", "merchant" };
         
        public MyOCRData()
        {
            created = DateTime.Now;
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
    }
}
