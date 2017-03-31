using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml;

namespace DCTVersioniser
{
    public class ClarionDictionaryProcessing
    {
        Options options;

        public ClarionDictionaryProcessing(Options options)
        {
            this.options = options;
        }

        void ConvertDctxToJsonAndSave()
        {
            var doc = new XmlDocument();
            doc.Load(TemporyDctxName);
            var json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
            Console.WriteLine("Saving file: " + ExportJsonFileName);
            WriteFile(ExportJsonFileName, json);
            if (SaveHistory)
            {
                Console.WriteLine("Saving file: " + ExportJsonFileNameHistory);
                WriteFile(ExportJsonFileNameHistory, json);
            }
        }

        static bool SaveHistory
        {
            get
            {
                return GetRegValue("history") == null || (int)GetRegValue("history") == 0 ? false : true;
                // Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "history", null) == null ? 0 : 1;
            }
        }

        static object GetRegValue(string value, object defaultValue=null)
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", value, defaultValue);
        }

        /// <summary>
        /// Exports a clarion dct to a dctx file
        /// </summary>
        bool ExportDctxToTemp()
        {
            Console.WriteLine("Exporting DCT To Temp Location");
            if (!StartProcess(GetStartInfoForExport()))
                return false;
            return true;
        }

        void ExportDictionaryToJson()
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
            var clarionClLocation = ImportExportFileDialogs.ClarionBinFolderLocation;
            if (clarionClLocation == null || !File.Exists(clarionClLocation + "\\ClarionCl.Exe"))
                ImportExportFileDialogs.SelectClarionCL(out clarionClLocation);
            return clarionClLocation;
        }

        ProcessStartInfo GetStartInfoForExport()
        {
            return new ProcessStartInfo
            {
                FileName = ClarionLocation + "\\ClarionCl.exe",
                Arguments = ExportCommandLine,
                ErrorDialog = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            };
        }

        ProcessStartInfo GetStartInfoForImport()
        {
            return new ProcessStartInfo
            {
                FileName = ClarionLocation + "\\ClarionCl.exe",
                Arguments = ImportCommandLine,
                ErrorDialog = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            };
        }

        /// <summary>
        /// Attempt to import the newly created dctx file into a new or existing clarion
        /// DCT
        /// </summary>
        /// <param name="dctxFileName"></param>
        bool ImportDictionary()
        {
            Console.WriteLine("Importing JSON into DCT");
            return StartProcess(GetStartInfoForImport());
        }

        void ImportDictionaryFromJson()
        {
            var xmlValue = JsonConvert.DeserializeXmlNode(File.ReadAllText(FileToBeProcessed));
            using (FileStream fs = new FileStream(ImportDctxName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                xmlValue.Save(fs);
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
        bool StartProcess(ProcessStartInfo info)
        {
            Process process = new Process() { StartInfo = info };
            return process.Start() && process.WaitForExit(1000 * 60 * 5);
        }

        string ClarionLocation { get; set; }
        string DateTimeStamp
        {
            get
            {
                return "--" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "--" +
                    DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
            }
        }

        /// <summary>
        /// Gets the ClarionCl command line to export the dct to a dctx
        /// </summary>
        /// <returns>string command line for exporting to dctx</returns>
        string ExportCommandLine
        {
            get
            {
                return $@"-dx ""{FileToBeProcessed}"" ""{TemporyDctxName}""";
            }
        }
        string ExportJsonFileName
        {
            get
            {
                return Path.GetDirectoryName(FileToBeProcessed) + "\\" + Path.GetFileNameWithoutExtension(FileToBeProcessed) + ".json";
            }
        }
        string ExportJsonFileNameHistory
        {
            get
            {
                return Path.GetDirectoryName(FileToBeProcessed) + "\\DCT-History\\" + Path.GetFileNameWithoutExtension(FileToBeProcessed) + DateTimeStamp + ".json";
            }
        }
        string FileToBeProcessed { get; set; }

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
                return Path.GetDirectoryName(FileToBeProcessed) + "\\" + Path.GetFileNameWithoutExtension(FileToBeProcessed) + ".dctx";
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
                return $"{Path.GetTempPath()}{Path.GetFileNameWithoutExtension(FileToBeProcessed)}.DCTX";
            }
        }

        /// <summary>
        /// starting block for the conversion
        /// </summary>
        public void ProcessDictionary()
        {
            ClarionLocation = options.ClarionClPath != null ? Path.GetDirectoryName(options.ClarionClPath) : GetLocationOfClarionCL();
            if (!File.Exists(ClarionLocation + "\\ClarionCl.Exe"))
                return;
            var dct = string.Empty;
            if (options.FileToProcess == null)
            {
                if (ImportExportFileDialogs.OpenFileDialog("DCT Files (.dct)|*.dct|JSON Files (.json)|*.json", "Select the file to be processed.", out dct))
                    return;
            }
            else
                dct = options.FileToProcess;
            FileToBeProcessed = dct;
            switch (Path.GetExtension(FileToBeProcessed).ToUpper())
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

        /// <summary>
        /// Write a string to disk
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="text"></param>
        public void WriteFile(string FileName, string text)
        {
            var directoryInfo = Directory.CreateDirectory(Path.GetDirectoryName(FileName));
            File.WriteAllText(FileName, text);
        }
    }
}
