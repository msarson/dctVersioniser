using CommandLine;
using CommandLine.Text;

namespace DCTVersioniser
{
    public class Options
    {
        [Option('h', "togglehistory", HelpText = "Turn on/off history saving of json", Required = false)]
        public bool TurnOnHistory { get; set; }

        [Option('c', "ccl", HelpText = "The full path and name of ClarionCl.exe",Required =false)]
        public string ClarionClPath { get; set; }
        [Option('f', "file", HelpText = "The file to process (DCT/JSON)",Required =false)]
        public string FileToProcess { get; set; }
        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("<>", "<>"),
                Copyright = new CopyrightInfo("<>", 2012),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("<>");
            help.AddPreOptionsLine("Usage: app -pSomeone");
            help.AddOptions(this);
            return help;
        }
    }
}
