using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddAgree.Models
{
    public class GisException : Exception
    {
        public GisException() { }
        public GisException(string message) : base(message) { }
        public GisException(string message, Exception innerException) : base(message, innerException) { }
    }
}
