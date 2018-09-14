using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DCTVersioniser
{
    public static class ImportExportFileDialogs
    {

        public static bool OpenFileDialog(string filter, string title, out string FileName, Settings settings)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog { Filter = filter, Title = title })
            {
                fileDialog.InitialDirectory = settings.CurrentLocation ?? string.Empty;
                fileDialog.ShowDialog();
                if (!string.IsNullOrEmpty(fileDialog.FileName))
                    settings.CurrentLocation = fileDialog.FileName;
                FileName = fileDialog.FileName;
                return string.IsNullOrEmpty(FileName);
            }
        }

        public static string SelectLocationOfClarionCommandLine()
        {

            using (var clarionBinFolder = new FolderBrowserDialog { Description = "Select Your Clarion Bin Folder" })
            {
                
                clarionBinFolder.SelectedPath = string.Empty;
                var dr = clarionBinFolder.ShowDialog();
                if (dr == DialogResult.Cancel)
                {
                    return null;
                }
                else
                {
                    return clarionBinFolder.SelectedPath;
                }
            }

        }



     
    }
}
