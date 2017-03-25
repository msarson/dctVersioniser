using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace DCTVersioniser
{
    public static class AccuraFileDialog
    {
        public static bool OpenFileDialog(string filter, string title, out string FileName)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog { Filter = filter, Title = title })
            {
                var tpsDCTX = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "tpsPath", null);
                fileDialog.InitialDirectory = tpsDCTX ?? string.Empty;
                fileDialog.ShowDialog();
                if (!string.IsNullOrEmpty(fileDialog.FileName))
                    Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "tpsPath", Path.GetDirectoryName(fileDialog.FileName));
                FileName = fileDialog.FileName;
                return string.IsNullOrEmpty(FileName);
            }
        }
        private static void SetClarionLocationInRegistry(FolderBrowserDialog sfd)
        {
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "clarionclpath", sfd.SelectedPath);
        }
        public static string GetClarionCLLocation()
        {
            return (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\msarson\dctversioniser", "clarionclpath", null);
        }
        public static bool SelectClarionCL(out string FileName)
        {
            using (FolderBrowserDialog sfd = new FolderBrowserDialog { Description = "Select Clarion Bin Folder" })
            {
                var clarionFolder = string.Empty;
                sfd.SelectedPath = "";

                sfd.ShowDialog();
                if (sfd.SelectedPath == string.Empty)
                {
                    FileName = null;
                    return false;
                }

                SetClarionLocationInRegistry(sfd);
                FileName = sfd.SelectedPath;
            }
            return true;
        }
    }
}
