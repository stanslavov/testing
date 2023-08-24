using System.Collections.Generic;

namespace Testing
{
    public class Data
    {
        public string Url { get; set; }

        public IEnumerable<ExcelSheet> Sheets { get; set; }
    }
}
