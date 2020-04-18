using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildingsYear.Models
{
    public class ConnectionOptions
    {
        public string ConnString { get; set; }
        public string ConnStringGDAL { get; set; }
        public string BaseProvider { get; set; }
    }
}
