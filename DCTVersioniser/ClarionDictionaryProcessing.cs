using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace DCTVersioniser
{
    public class ClarionDictionaryProcessing
    {
        Options options;
        Settings settings;
        public ClarionDictionaryProcessing(Options options)
        {
            this.options = options;
            settings = new Settings();
        }

        void ConvertDctxToJsonAndSave()
        {
            var doc = new XmlDocument();
            doc.Load(TemporyDctxName);
            var json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
            Console.WriteLine("Saving file: " + JsonName);
            WriteFile(JsonName, json);
            if (settings.SaveHistory)
            {
                Console.WriteLine("Saving file: " + ExportJsonFileNameHistory);
                WriteFile(ExportJsonFileNameHistory, json);
            }
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
                Console.Write("Action Complete");
                return;
            }
            Console.WriteLine("Failed to export dictionary. Press any key to exit.");
            Console.ReadLine();
        }
      

        /// <summary>
        /// Find out location of the clarioncl.exe folder and if the file
        /// exists
        /// </summary>
        /// <returns>string Location of the file</returns>
        string GetCommandLineLocation()
        {
            var binDir = settings.BinDirectoryLocation;
            if (binDir == null || !File.Exists(binDir + "\\ClarionCl.Exe"))
            {
                binDir =  ImportExportFileDialogs.SelectClarionCL();
                if (binDir != null)
                    settings.BinDirectoryLocation = binDir;
            }
            return binDir;
        }

  
        ProcessStartInfo GetStartInfoForExport()
        {
            return new ProcessStartInfo
            {
                FileName = CommandLineLocation + "\\ClarionCl.exe",
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
                FileName = CommandLineLocation + "\\ClarionCl.exe",
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
            else
                Console.WriteLine("Action Complete");
            File.Delete(ImportDctxName);
        }

        void ProcessAction()
        {
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
        /// Start a new shell process
        /// </summary>
        /// <param name="info"></param>
        bool StartProcess(ProcessStartInfo info)
        {
            Process process = new Process() { StartInfo = info };
            return process.Start() && process.WaitForExit(1000 * 60 * 5);
        }

        string CommandLineLocation { get; set; }
        string DateTimeStamp
        {
            get
            {
                return "--" +
                    DateTime.Now.Year.ToString() + "-" +
                    DateTime.Now.Month.ToString() + "-" +
                    DateTime.Now.Day.ToString() + "--" +
                    DateTime.Now.Hour.ToString() + "-" +
                    DateTime.Now.Minute.ToString() + "-" +
                    DateTime.Now.Second.ToString();
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
        string JsonName
        {
            get
            {
                return Path.GetDirectoryName(FileToBeProcessed) + "\\" + Path.GetFileNameWithoutExtension(FileToBeProcessed) + ".json";
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
            if (options.ClarionClPath != null)
                CommandLineLocation = Path.GetDirectoryName(options.ClarionClPath);
            else
                CommandLineLocation = GetCommandLineLocation();
            if (CommandLineLocation == null || !File.Exists(CommandLineLocation + "\\ClarionCl.Exe"))
            {
                Console.WriteLine("No clarion command line location available");
                return;
            }
            var dct = string.Empty;
            if (options.FileToProcess != null)
                dct = options.FileToProcess;
            else
            {
                if (ImportExportFileDialogs.OpenFileDialog("DCT Files (.dct)|*.dct|JSON Files (.json)|*.json", "Select the file to be processed.", out dct,settings))
                    return;
            }
            FileToBeProcessed = dct;
            ProcessAction();
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
