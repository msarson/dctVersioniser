using System;
using System.Linq;

namespace DCTJson
{
    public class Dictionary
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string DctxFormat { get; set; }
        public Dictionaryversion DictionaryVersion { get; set; }
        public Comment Comment { get; set; }
        public Table[] Table { get; set; }
        public Relation[] Relation { get; set; }
    }
}
