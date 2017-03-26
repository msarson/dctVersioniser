using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace DCTVersioniser
{
    public class ClarionDictionaryProcessing
    {

        /// <summary>
        /// Exports a clarion dct to a dctx file
        /// </summary>
        void ExportDictionary()
        {
            Console.WriteLine("Exporting DCT");
            StartProcess(new ProcessStartInfo
            {
                FileName = ClarionLocation + "\\ClarionCl.exe",
                Arguments = GetExportCommandLine()
            });
        }

        /// <summary>
        /// Gets the ClarionCl command line to export the dct to a dctx
        /// </summary>
        /// <returns>string command line for exporting to dctx</returns>
        private string GetExportCommandLine()
        {
            
            return $@"-dx ""{DctOrJsonLocation}"" ""{GetTempDctxName()}""";
        }

        /// <summary>
        /// Get the dictionary name from a passed in dctx name
        /// </summary>
        /// <param name="dctxFileName"></param>
        /// <returns></returns>
        string GetDictionaryName(string dctxFileName)
        {
            return Path.GetDirectoryName(dctxFileName) + "\\" + Path.GetFileNameWithoutExtension(dctxFileName) + ".dct";
        }


        /// <summary>
        /// Gets the ClarionCl command line to import a dctx into a dct
        /// creating or overwriting the dct in the process
        /// </summary>
        /// <returns>string command line for exporting to dctx</returns>
        string GetImportCommandLine(string dctxFileName)
        {
           
            return $@"-di ""{GetDictionaryName(dctxFileName)}"" ""{dctxFileName}""";
        }

        /// <summary>
        /// Find out location of the clarioncl.exe folder and if the file
        /// exists
        /// </summary>
        /// <returns>string Location of the file</returns>
        string GetLocationOfClarionCL()
        {
            string clarionClLocation = AccuraFileDialog.GetClarionCLLocation();
            if (clarionClLocation == null || !File.Exists(clarionClLocation + "\\ClarionCl.Exe"))
                AccuraFileDialog.SelectClarionCL(out clarionClLocation);
            return clarionClLocation;
        }


        /// <summary>
        /// Get location of tempory dctx file
        /// </summary>
        /// <returns>string Location of the file</returns>
        string GetTempDctxName()
        {
            return $"{Path.GetTempPath()}{Path.GetFileNameWithoutExtension(DctOrJsonLocation)}.DCTX";
        }

        /// <summary>
        /// Attempt to import the newly created dctx file into a new or existing clarion
        /// DCT
        /// </summary>
        /// <param name="dctxFileName"></param>
        void ImportDictionary(string dctxFileName)
        {
            Console.WriteLine("Importing JSON into DCT");
            StartProcess(new ProcessStartInfo
            {
                FileName = ClarionLocation + "\\ClarionCl.exe",
                Arguments = GetImportCommandLine(dctxFileName)
            });
        }


        /// <summary>
        /// Start a new shell process
        /// </summary>
        /// <param name="info"></param>
        private void StartProcess(ProcessStartInfo info)
        {
            info.ErrorDialog = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = false;
            Process process = new Process() { StartInfo = info };
            process.Start();
            process.WaitForExit(1000 * 60 * 5);    // Wait up to five minutes.
        }



        private string ClarionLocation { get; set; }
        string DctOrJsonLocation { get; set; }
        
        /// <summary>
        /// starting block for the conversion
        /// </summary>
        public void ProcessDictionary()
        {

            ClarionLocation = GetLocationOfClarionCL();
            if (File.Exists(ClarionLocation + "\\ClarionCl.Exe"))
            {
                var dct = string.Empty;
                if (AccuraFileDialog.OpenFileDialog("DCT Files (.dct)|*.dct|JSON Files (.dct)|*.json", "Select DCT File for export or Json File for import", out dct))
                    return;
                DctOrJsonLocation = dct;
                string ext = Path.GetExtension(DctOrJsonLocation);
                if (Path.GetExtension(DctOrJsonLocation).ToUpper() == ".DCT")
                {
                    var jsonFileName = Path.GetDirectoryName(DctOrJsonLocation) + "\\" + Path.GetFileNameWithoutExtension(DctOrJsonLocation) + ".json";
                    ExportDictionary();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(GetTempDctxName());
                    string json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
                    WriteFile(jsonFileName, json);
                    File.Delete(GetTempDctxName());
                }
                else
                {

                    var json = File.ReadAllText(DctOrJsonLocation);
                    var dctxFileName = Path.GetDirectoryName(DctOrJsonLocation) + "\\" + Path.GetFileNameWithoutExtension(DctOrJsonLocation) + ".dctx";
                    XmlDocument newDoc = JsonConvert.DeserializeXmlNode(json);
                    using (FileStream fs = new FileStream(dctxFileName,
                            FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                        newDoc.Save(fs);
                    ImportDictionary(dctxFileName);
                }
                //Remove the created dictionary

            }
        }

        /// <summary>
        /// Write a string to disk
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="text"></param>
        public void WriteFile(string FileName, string text)
        {
            System.IO.File.WriteAllText(FileName, text);
        }
    }
}
