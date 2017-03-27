using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace DCTVersioniser
{
    public class ClarionDictionaryProcessing
    {
        private Options options;

        public ClarionDictionaryProcessing(Options options)
        {
            this.options = options;
        }

        private void ConvertDctxToJsonAndSave()
        {
            var doc = new XmlDocument();
            doc.Load(TemporyDctxName);
            string json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
            WriteFile(ExportJsonFileName, json);
            WriteFile(ExportJsonFileNameHistory, json);
        }

        /// <summary>
        /// Exports a clarion dct to a dctx file
        /// </summary>
        bool ExportDctxToTemp()
        {
            Console.WriteLine("Exporting DCT");
            var startInfo = new ProcessStartInfo
            {
                FileName = ClarionLocation + "\\ClarionCl.exe",
                Arguments = ExportCommandLine
            };
            if (!StartProcess(startInfo))
                return false;
            return true;
        }
        private void ExportDictionaryToJson()
        {

            if (ExportDctxToTemp())
            {
                ConvertDctxToJsonAndSave();
                File.Delete(TemporyDctxName);
            }
            else
            {
                Console.WriteLine("Failed to export dictionary. Press any key to exit.");
                Console.ReadLine();
            }
        }


        /// <summary>
        /// Find out location of the clarioncl.exe folder and if the file
        /// exists
        /// </summary>
        /// <returns>string Location of the file</returns>
        string GetLocationOfClarionCL()
        {
            string clarionClLocation = ImportExportFileDialogs.GetClarionCLLocation();
            if (clarionClLocation == null || !File.Exists(clarionClLocation + "\\ClarionCl.Exe"))
                ImportExportFileDialogs.SelectClarionCL(out clarionClLocation);
            return clarionClLocation;
        }

        /// <summary>
        /// Attempt to import the newly created dctx file into a new or existing clarion
        /// DCT
        /// </summary>
        /// <param name="dctxFileName"></param>
        bool ImportDictionary()
        {
            Console.WriteLine("Importing JSON into DCT");
            var startInfo = new ProcessStartInfo
            {
                FileName = ClarionLocation + "\\ClarionCl.exe",
                Arguments = ImportCommandLine
            };
            return StartProcess(startInfo);
        }

        private void ImportDictionaryFromJson()
        {
            var json = File.ReadAllText(DctOrJsonLocation);
            XmlDocument newDoc = JsonConvert.DeserializeXmlNode(json);
            using (FileStream fs = new FileStream(ImportDctxName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                newDoc.Save(fs);
            if (!ImportDictionary())
            {
                Console.WriteLine("Failed to import Json file. Press Any Key");
                Console.ReadLine();
            }
            File.Delete(ImportDctxName);
        }

        /// <summary>
        /// Start a new shell process
        /// </summary>
        /// <param name="info"></param>
        private bool StartProcess(ProcessStartInfo info)
        {
            info.ErrorDialog = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = false;
            Process process = new Process() { StartInfo = info };
            if (!process.Start())
            {
                //process failed to start
                return false;
            }
            if (!process.WaitForExit(1000 * 60 * 5))    // Wait up to five minutes.
            {
                //process failed and timed out
                return false;
            }
            return true;
        }



        private string ClarionLocation { get; set; }
        string DctOrJsonLocation { get; set; }

        /// <summary>
        /// Gets the ClarionCl command line to export the dct to a dctx
        /// </summary>
        /// <returns>string command line for exporting to dctx</returns>
        private string ExportCommandLine
        {
            get
            {
                return $@"-dx ""{DctOrJsonLocation}"" ""{TemporyDctxName}""";
            }
        }
        string DateTimeStamp
        {
            get
            {
                return "--" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "--" +
                    DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
            }
        }
        string ExportJsonFileNameHistory
        {
            get
            {
                return Path.GetDirectoryName(DctOrJsonLocation) + "\\DCT-History\\" + Path.GetFileNameWithoutExtension(DctOrJsonLocation) + DateTimeStamp + ".json";
            }
        }
        string ExportJsonFileName
        {
            get
            {
                return Path.GetDirectoryName(DctOrJsonLocation) + "\\" + Path.GetFileNameWithoutExtension(DctOrJsonLocation) + ".json";
            }
        }

        /// <summary>
        /// Gets the ClarionCl command line to import a dctx into a dct
        /// creating or overwriting the dct in the process
        /// </summary>
        /// <returns>string command line for exporting to dctx</returns>
        string ImportCommandLine
        {
            get
            {
                return $@"-di ""{ImportDictionaryName}"" ""{ImportDctxName}""";
            }
        }

        string ImportDctxName
        {
            get
            {
                return Path.GetDirectoryName(DctOrJsonLocation) + "\\" + Path.GetFileNameWithoutExtension(DctOrJsonLocation) + ".dctx";
            }
        }


        string ImportDictionaryName
        {
            get
            {
                return Path.GetDirectoryName(ImportDctxName) + "\\" + Path.GetFileNameWithoutExtension(ImportDctxName) + ".dct";
            }
        }


        /// <summary>
        /// Location of tempory dctx file
        /// </summary>
        /// <returns>string Location of the file</returns>
        string TemporyDctxName
        {
            get
            {
                return $"{Path.GetTempPath()}{Path.GetFileNameWithoutExtension(DctOrJsonLocation)}.DCTX";
            }
        }

        /// <summary>
        /// starting block for the conversion
        /// </summary>
        public void ProcessDictionary()
        {
            if (options.ClarionClPath != null)
                ClarionLocation = Path.GetDirectoryName(options.ClarionClPath);
            else
                ClarionLocation = GetLocationOfClarionCL();
            if (File.Exists(ClarionLocation + "\\ClarionCl.Exe"))
            {
                var dct = string.Empty;
                if (options.FileToProcess != null)
                {
                    dct = options.FileToProcess;
                }
                else
                {
                    if (ImportExportFileDialogs.OpenFileDialog("DCT Files (.dct)|*.dct|JSON Files (.dct)|*.json", "Select DCT File for export or Json File for import", out dct))
                        return;
                }
                DctOrJsonLocation = dct;
                switch (Path.GetExtension(DctOrJsonLocation).ToUpper())
                {
                    case ".DCT":
                        ExportDictionaryToJson();
                        break;
                    case ".JSON":
                        ImportDictionaryFromJson();
                        break;
                    default:
                        Console.WriteLine("Invalid file selected");
                        Console.ReadLine();
                        break;
                }
            }
        }

        /// <summary>
        /// Write a string to disk
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="text"></param>
        public void WriteFile(string FileName, string text)
        {
            var DirName = Path.GetDirectoryName(FileName);
            DirectoryInfo di = Directory.CreateDirectory(DirName);
            File.WriteAllText(FileName, text);
        }
    }
}
