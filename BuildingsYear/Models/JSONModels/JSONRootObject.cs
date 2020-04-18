using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildingsYear.Models.JSONModels
{
    public class Geometry
    {
        public string type { get; set; }
        public dynamic coordinates { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public IDictionary<string, object> properties { get; set; }
        public Geometry geometry { get; set; }
    }

    public class RootObject
    {
        public string type { get; set; }
        public string name { get; set; }
        public List<Feature> features { get; set; }
    }
}
