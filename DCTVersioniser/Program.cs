using McMaster.Extensions.CommandLineUtils;
using Microsoft.Win32;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace DCTVersioniser
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            CommandLineApplication.Execute<Program>(args);
        }


        [Option("--p <PATHTOCLARION>", Description = "Path to clarioncl.exe")]
        private string pathToClarionCL { get; }

        [Option("--h", Description =  "Turn on/off history saving of json")]
        public bool TurnOnHistory { get; set; }

        [Option("--f <FILETOPROCESS>", Description = "The file to process (DCT/JSON)")]
        public string dctFileToProcess { get; set; }

      
        private void OnExecute()

        {
            var continueToProcess = true;

            if (TurnOnHistory)
            {
                var historyValue = (int?)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "history", null);
                switch (historyValue)
                {
                    case null:
                    case 0:
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "history", 1);
                        Console.WriteLine("History is turned on");
                        break;
                    default:
                        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "history", 0);
                        Console.WriteLine("History is turned off");
                        break;
                }

                return;
            }
            if (pathToClarionCL != null)
            {
                if (!pathToClarionCL.ToLower().Contains("clarioncl.exe"))
                {
                    Console.WriteLine($"You passed: {pathToClarionCL}");
                    Console.WriteLine("Passed clarioncl.exe name is invalid please use eg. c:\\clarion10\\bin\\clarioncl.exe.");
                    continueToProcess = false;
                }
                if (continueToProcess && !File.Exists(pathToClarionCL))
                {
                    Console.WriteLine("clarioncl.exe does not exist at that location");
                    continueToProcess = false;
                }
            } else
            {

            }
            if (continueToProcess && dctFileToProcess != null)
            {

                if (!dctFileToProcess.ToLower().Contains(".json") && !dctFileToProcess.ToLower().Contains(".dct"))
                {
                    Console.WriteLine("Please specify a DCT or JSON file to parse");
                    continueToProcess = false;
                }
                if (continueToProcess && !File.Exists(dctFileToProcess))
                {
                    Console.WriteLine("File to process could not be found");
                    continueToProcess = false;
                }
            }
            if (continueToProcess)
            {
                var dctProcessing = new ClarionDictionaryProcessing(pathToClarionCL, dctFileToProcess);
                dctProcessing.ProcessDictionary();
            }
            else
            {
                //Console.WriteLine("Invalid file names were passed as parameters");
                Console.WriteLine("For help use DCTversioniser --help");

            }
            
        }
       
       
    }
}
