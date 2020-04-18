using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildingsYear.Models.JSONModels
{
    public class JsonLayer
    {
        public string Name { get; set; }
        public string[] Attributes { get; set; }
        public string DataSource { get; set; }
        public string Provider { get; set; }
    }
}
