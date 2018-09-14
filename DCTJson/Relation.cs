using System;
using System.Linq;

namespace DCTJson
{
    public class Relation
    {
        public string Guid { get; set; }
        public string PrimaryTable { get; set; }
        public string ForeignTable { get; set; }
        public string PrimaryKey { get; set; }
        public string ForeignKey { get; set; }
        public string Delete { get; set; }
        public string Update { get; set; }
        public Audit4 Audit { get; set; }
        public object ForeignMapping { get; set; }
        public object PrimaryMapping { get; set; }
        public string ForeignAlias { get; set; }
        public string PrimaryAlias { get; set; }
        public Option Option { get; set; }
    }
}
