using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DCTVersioniser
{
    public static class ImportExportFileDialogs
    {

        public static bool OpenFileDialog(string filter, string title, out string FileName)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog { Filter = filter, Title = title })
            {
                fileDialog.InitialDirectory = CurrentDictionaryLocation ?? string.Empty;
                fileDialog.ShowDialog();
                if (!string.IsNullOrEmpty(fileDialog.FileName))
                    CurrentDictionaryLocation = fileDialog.FileName;
                FileName = fileDialog.FileName;
                return string.IsNullOrEmpty(FileName);
            }
        }

        public static bool SelectClarionCL(out string FileName)
        {
            using (var clarionBinFolder = new FolderBrowserDialog { Description = "Select Clarion Bin Folder" })
            {
                var clarionFolder = string.Empty;
                clarionBinFolder.SelectedPath = string.Empty;

                clarionBinFolder.ShowDialog();
                if (clarionBinFolder.SelectedPath == string.Empty)
                {
                    FileName = null;
                    return false;
                }

                ClarionBinFolderLocation = clarionBinFolder.SelectedPath;
                FileName = clarionBinFolder.SelectedPath;
            }
            return true;
        }

        public static string ClarionBinFolderLocation
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

        public static string CurrentDictionaryLocation
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
    }
}
