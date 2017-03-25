using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace DCTVersioniser
{
    public class ClarionDictionaryProcessing
    {
        

        void ExportDictionary()
        {
            Console.WriteLine("Exporting DCT to Temp");
            StartProcess(new ProcessStartInfo
            {
                FileName = ClarionLocation + "\\ClarionCl.exe",
                Arguments = GetExportCommandLine()
            });
        }

        string GetTempDictionaryName()
        {
            return $"{ Path.GetTempPath()}{Path.GetFileNameWithoutExtension(Dictionary)}.DCTX";
        }
        private string GetExportCommandLine()
        {
            return $"-dx {Dictionary} {GetTempDictionaryName()}";
        }
        private string GetFileName()
        {
            return Path.GetTempPath() + Path.GetFileNameWithoutExtension(Dictionary) + ".DCTX";
        }

        private void StartProcess(ProcessStartInfo info)
        {
            info.ErrorDialog = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = false;
            Process process = new Process() { StartInfo = info };
            process.Start();
            process.WaitForExit(1000 * 60 * 5);    // Wait up to five minutes.
        }

        string _dctToEdit { get; set; }

        string GetLocationOfClarionCL()
        {
            string clarionClLocation = AccuraFileDialog.GetClarionCLLocation();
            if (clarionClLocation == null  || !File.Exists(clarionClLocation + "\\ClarionCl.Exe"))
            {
                AccuraFileDialog.SelectClarionCL(out clarionClLocation);
            }
            return clarionClLocation;
        }

      
        private string ClarionLocation { get; set; }
        string GetImportCommandLine(string dctxFileName)
        {
            var dctName = Path.GetDirectoryName(dctxFileName) + "\\" + Path.GetFileNameWithoutExtension(dctxFileName) + ".dct";
            return $"-di {dctName} {dctxFileName}";
        }
        void ImportDictionary(string dctxFileName)
        {
            Console.WriteLine("Importing JSON into DCT");
            StartProcess(new ProcessStartInfo
            {
                FileName = ClarionLocation + "\\ClarionCl.exe",
                Arguments = GetImportCommandLine(dctxFileName)
            });
        }
        public void ProcessDictionary()
        {

            ClarionLocation = GetLocationOfClarionCL();
            if (File.Exists(ClarionLocation + "\\ClarionCl.Exe"))
            {
                var dct = string.Empty;
                if (AccuraFileDialog.OpenFileDialog("DCT Files (.dct)|*.dct|JSON Files (.dct)|*.json", "Select DCT File for export or Json File for import", out dct))
                    return;
                Dictionary = dct;
                if (Path.GetExtension(dct).ToUpper() == "DCT")
                {
                    var jsonFileName = Path.GetDirectoryName(dct) + "\\" + Path.GetFileNameWithoutExtension(dct) + ".json";
                    ExportDictionary();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(GetFileName());
                    string json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
                    WriteFile(jsonFileName, json);
                    File.Delete(GetTempDictionaryName());
                }
                else
                {

                    var json = File.ReadAllText(dct);
                    var dctxFileName = Path.GetDirectoryName(dct) + "\\" + Path.GetFileNameWithoutExtension(dct) + ".dctx";
                    XmlDocument newDoc = JsonConvert.DeserializeXmlNode(json);
                    using (FileStream fs = new FileStream(dctxFileName,
                            FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        newDoc.Save(fs);
                    }
                    ImportDictionary(dctxFileName);
                }
                //Remove the created dictionary
                
            }
        }
        public void WriteFile(string FileName, string text)
        {
            System.IO.File.WriteAllText(FileName, text);
        }
        string Dictionary
        {
            get { return _dctToEdit; }
            set
            {
                _dctToEdit = value;

            }
        }
    }
}
