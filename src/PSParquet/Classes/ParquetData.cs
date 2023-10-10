using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSParquet.Classes
{
    public class ParquetData
    {
        public Type Type { get; set; }
        public string Parameter { get; set; }
        public dynamic[] Data { get; set; }
    }
}
