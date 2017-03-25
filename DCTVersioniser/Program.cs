using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using System.IO;

namespace DCTVersioniser
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var dctProcessing = new ClarionDictionaryProcessing();
            dctProcessing.ProcessDictionary();


            //Clipboard.SetText(json);

        }
       
       
    }
}
