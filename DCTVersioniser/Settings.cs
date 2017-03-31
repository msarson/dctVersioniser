using Microsoft.Win32;
using System.IO;

namespace DCTVersioniser
{
    public class Settings
    {
        public string CurrentLocation
        {
            get
            {
                return (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "tpsPath", null);
            }
            set
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "tpsPath", Path.GetDirectoryName(value));
            }
        }
        public string BinDirectoryLocation
        {
            set
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "clarionclpath", value);
            }
            get
            {
                return (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "clarionclpath", null);
            }
        }
        public bool SaveHistory
        {
            get
            {
                return GetRegValue("history") != null && (int)GetRegValue("history") != 0;
            }
        }
        object GetRegValue(string value, object defaultValue = null)
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", value, defaultValue);
        }
    }
}
