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

        public static string SelectClarionCL()
        {

            using (var clarionBinFolder = new FolderBrowserDialog { Description = "Select Clarion Bin Folder" })
            {
                var clarionFolder = string.Empty;
                clarionBinFolder.SelectedPath = string.Empty;

                clarionBinFolder.ShowDialog();
                if (clarionBinFolder.SelectedPath == string.Empty)

                    return null;


                return clarionBinFolder.SelectedPath;
            }

        }



     
    }
}
