using System;
using System.Linq;

namespace DCTJson
{
    public class Audit
    {
        public string CreateUser { get; set; }
        public string CreateDate { get; set; }
        public string CreateTime { get; set; }
        public string CreateVersionNumber { get; set; }
        public string ModifiedUser { get; set; }
        public string ModifiedVersionNumber { get; set; }
    }
}
