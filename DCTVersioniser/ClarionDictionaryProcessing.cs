﻿using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace DCTVersioniser
{
    public class ClarionDictionaryProcessing
    {

        private void ConvertDctxToJsonAndSave()
        {
            var doc = new XmlDocument();
            doc.Load(TemporyDctxName);
            string json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
            WriteFile(ExportJsonFileName, json);
        }

        /// <summary>
        /// Exports a clarion dct to a dctx file
        /// </summary>
        void ExportDctxToTemp()
        {
            Console.WriteLine("Exporting DCT");
            StartProcess(new ProcessStartInfo
            {
                FileName = ClarionLocation + "\\ClarionCl.exe",
                Arguments = ExportCommandLine
            });
        }
        private void ExportDictionaryToJson()
        {

            ExportDctxToTemp();
            ConvertDctxToJsonAndSave();
            File.Delete(TemporyDctxName);
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
        void ImportDictionary()
        {
            Console.WriteLine("Importing JSON into DCT");
            StartProcess(new ProcessStartInfo
            {
                FileName = ClarionLocation + "\\ClarionCl.exe",
                Arguments = ImportCommandLine
            });
        }

        private void ImportDictionaryFromJson()
        {
            var json = File.ReadAllText(DctOrJsonLocation);
            XmlDocument newDoc = JsonConvert.DeserializeXmlNode(json);
            using (FileStream fs = new FileStream(ImportDctxName,
                    FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                newDoc.Save(fs);
            ImportDictionary();
            File.Delete(ImportDctxName);
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

            ClarionLocation = GetLocationOfClarionCL();
            if (File.Exists(ClarionLocation + "\\ClarionCl.Exe"))
            {
                var dct = string.Empty;
                if (ImportExportFileDialogs.OpenFileDialog("DCT Files (.dct)|*.dct|JSON Files (.dct)|*.json", "Select DCT File for export or Json File for import", out dct))
                    return;
                DctOrJsonLocation = dct;
                string ext = Path.GetExtension(DctOrJsonLocation);
                if (Path.GetExtension(DctOrJsonLocation).ToUpper() == ".DCT")
                    ExportDictionaryToJson();
                else
                    ImportDictionaryFromJson();
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
            File.WriteAllText(FileName, text);
        }
    }
}
