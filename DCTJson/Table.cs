using System;
using System.Linq;

namespace DCTJson
{
    public class Table
    {
        public string Guid { get; set; }
        public string Ident { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Prefix { get; set; }
        public string Driver { get; set; }
        public string DriverOption { get; set; }
        public string Owner { get; set; }
        public string Path { get; set; }
        public string Thread { get; set; }
        public string Bindable { get; set; }
        public Audit1 Audit { get; set; }
        public object Option { get; set; }
        public Field[] Field { get; set; }
        public string Create { get; set; }
        public object Key { get; set; }
        public object Alias { get; set; }
        public object Trigger { get; set; }
        public string Reclaim { get; set; }
        public string Encrypt { get; set; }
        public Comment1 Comment { get; set; }
        public string Usage { get; set; }
    }
}
