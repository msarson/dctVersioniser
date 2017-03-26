using System;
using System.IO;

namespace DCTVersioniser
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var continueToProcess = true;
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if(options.ClarionClPath != null)
                {
                    if (!options.ClarionClPath.ToLower().Contains("clarioncl.exe"))
                    {
                        Console.WriteLine("Passed clarioncl.exe name is invalid please use eg. c:\\clarion10\\bin\\clarioncl.exe.");
                        continueToProcess = false;
                    }
                    if (continueToProcess && !File.Exists(options.ClarionClPath))
                    {
                        Console.WriteLine("clarioncl.exe does not exist at that location");
                        continueToProcess = false;
                    }
                }

                if (continueToProcess && options.FileToProcess != null)
                {

                    if(!options.FileToProcess.ToLower().Contains(".json") && !options.FileToProcess.ToLower().Contains(".dct"))
                    {
                        Console.WriteLine("Please specify a DCT or JSON file to parse");
                        continueToProcess = false;
                    }
                    if (continueToProcess && !File.Exists(options.FileToProcess))
                    {
                        Console.WriteLine("File to process could not be found");
                        continueToProcess = false;
                    }
                }

                if (continueToProcess)
                {
                    var dctProcessing = new ClarionDictionaryProcessing(options);
                    dctProcessing.ProcessDictionary();
                }
                else
                {
                    //Console.WriteLine("Invalid file names were passed as parameters");
                    Console.WriteLine("For help use DCTversioniser --help");

                }
                // Values are available here
             
            }

       
            


            //Clipboard.SetText(json);

        }
       
       
    }
}
