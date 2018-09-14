using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace DCTVersioniser
{
    public class ClarionDictionaryProcessing
    {
        const string JsonFileFilter = "DCT Files (.dct)|*.dct|JSON Files (.json)|*.json";

        private readonly string ClarionClPath;
        private readonly string FileToProcess;
        private readonly Settings settings;

        public ClarionDictionaryProcessing(string clarionClPath, string fileToProcess)
        {
            ClarionClPath = clarionClPath;
            FileToProcess = fileToProcess;
            settings = new Settings();
        }

        void ConvertDctxToJsonAndSave()
        {
            var dictionary = new XmlDocument();
            dictionary.Load(GetTemporyDctxName());
            var json = JsonConvert.SerializeXmlNode(dictionary, Newtonsoft.Json.Formatting.Indented);

            WriteFile(GetJsonName(), json);
            if (settings.SaveHistory)
            {
                Console.WriteLine("Saving file: " + GetExportJsonFileNameHistory());
                WriteFile(GetExportJsonFileNameHistory(), json);
            }
        }

        /// <summary>
        /// Exports a clarion dct to a dctx file
        /// </summary>
        bool ExportDctxToTemp()
        {
            Console.WriteLine("Exporting DCT To Temp Location");
            if (StartProcess(GetStartInfoForExport()) != ProcessResult.Success)
                return false;
            return true;
        }

        void ExportDictionaryToJson()
        {
            if (ExportDctxToTemp())
            {
                ConvertDctxToJsonAndSave();
                File.Delete(GetTemporyDctxName());
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
            string binDirectory = settings.GetBinDirectoryLocation();
            if (binDirectory == null || !File.Exists(binDirectory + "\\ClarionCl.Exe"))
            {
                binDirectory = ImportExportFileDialogs.SelectLocationOfClarionCommandLine();
                if (binDirectory != null)
                    settings.SetBinDirectoryLocation(binDirectory);
            }
            return binDirectory;
        }

        private string GetDateTimeStamp()
        {
            return "--" +
                DateTime.Now.Year.ToString() + "-" +
                DateTime.Now.Month.ToString() + "-" +
                DateTime.Now.Day.ToString() + "--" +
                DateTime.Now.Hour.ToString() + "-" +
                DateTime.Now.Minute.ToString() + "-" +
                DateTime.Now.Second.ToString();
        }

        private string GetExportJsonFileNameHistory()
        {
            if (!Directory.Exists(Path.GetDirectoryName(FileToBeProcessed) + "\\DCT-History"))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FileToBeProcessed) + "\\DCT-History");
            }
            return Path.GetDirectoryName(FileToBeProcessed) + "\\DCT-History\\" +
                Path.GetFileNameWithoutExtension(FileToBeProcessed) + GetDateTimeStamp() + ".json";
        }

        /// <summary>
        /// Gets the ClarionCl command line to import a dctx into a dct
        /// creating or overwriting the dct in the process
        /// </summary>
        /// <returns>string command line for exporting to dctx</returns>
        private string GetImportCommandLine()
        {
            return $@"-di ""{GetImportDictionaryName()}"" ""{GetImportDctxName()}""";
        }

        private string GetImportDctxName()
        {
            return Path.GetDirectoryName(FileToBeProcessed) + "\\" + Path.GetFileNameWithoutExtension(FileToBeProcessed) + ".dctx";
        }

        private string GetImportDictionaryName()
        {
            return Path.GetDirectoryName(GetImportDctxName()) + "\\" + Path.GetFileNameWithoutExtension(GetImportDctxName()) + ".dct";
        }

        private string GetJsonName()
        {
            return Path.GetDirectoryName(FileToBeProcessed) + "\\" + Path.GetFileNameWithoutExtension(FileToBeProcessed) + ".json";
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
                Arguments = GetImportCommandLine(),
                ErrorDialog = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            };
        }


        /// <summary>
        /// Location of tempory dctx file
        /// </summary>
        /// <returns>string Location of the file</returns>
        private string GetTemporyDctxName()
        {
            return $"{Path.GetTempPath()}{Path.GetFileNameWithoutExtension(FileToBeProcessed)}.DCTX";
        }

        ImportResult ImportDictionary()
        {
            Console.WriteLine("Importing JSON into DCT");
            ProcessResult startProcess = StartProcess(GetStartInfoForImport());
            return startProcess == ProcessResult.Success ? ImportResult.Success : ImportResult.Failure;
        }

        void ImportDictionaryFromJson()
        {
            var xmlValue = JsonConvert.DeserializeXmlNode(File.ReadAllText(FileToBeProcessed));
            using (FileStream fs = new FileStream(GetImportDctxName(), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                xmlValue.Save(fs);
            if (ImportDictionary() != ImportResult.Success)
            {
                Console.WriteLine("Failed to import Json file. Press Any Key");
                Console.ReadLine();
            }
            else
                Console.WriteLine("Action Complete");
            File.Delete(GetImportDctxName());
        }

        void ProcessSelectedFile()
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

        ProcessResult StartProcess(ProcessStartInfo info)
        {
            Process process = new Process() { StartInfo = info };
            if (process.Start() && process.WaitForExit(1000 * 60 * 5))
            {
                return ProcessResult.Success;
            }
            else
            {
                return ProcessResult.Failure;
            }
        }

        string CommandLineLocation { get; set; }

        /// <summary>
        /// Gets the ClarionCl command line to export the dct to a dctx
        /// </summary>
        /// <returns>string command line for exporting to dctx</returns>
        string ExportCommandLine
        {
            get
            {
                return $@"-dx ""{FileToBeProcessed}"" ""{GetTemporyDctxName()}""";
            }
        }
        string FileToBeProcessed { get; set; }

        /// <summary>
        /// starting block for the conversion
        /// </summary>
        public void ProcessDictionary()
        {
            if (ClarionClPath == null)
            {
                CommandLineLocation = GetCommandLineLocation();
            }
            else
            {
                CommandLineLocation = Path.GetDirectoryName(ClarionClPath);
            }

            if (CommandLineLocation == null || !File.Exists(CommandLineLocation + "\\ClarionCl.Exe"))
            {
                Console.WriteLine("No clarion command line location available");
                return;
            }
            var dictionary = string.Empty;
            if (FileToProcess != null)
            {
                dictionary = FileToProcess;
            }
            else
            {
                if (ImportExportFileDialogs.OpenFileDialog(JsonFileFilter, "Select the file to be processed.", out dictionary, settings))
                {
                    return;
                }
            }

            FileToBeProcessed = dictionary;
            ProcessSelectedFile();
        }

        /// <summary>
        /// Write a string to disk
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="text"></param>
        public void WriteFile(string FileName, string text)
        {
            Console.WriteLine("Saving file: " + FileName);
            var directoryInfo = Directory.CreateDirectory(Path.GetDirectoryName(FileName));
            File.WriteAllText(FileName, text);
        }
    }
}
