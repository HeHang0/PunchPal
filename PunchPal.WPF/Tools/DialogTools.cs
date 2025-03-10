using Microsoft.Win32;
using PunchPal.Core.Events;

namespace PunchPal.WPF.Tools
{
    public class DialogTools
    {
        public static string[] OnFileSelecting(EventManager.FileDialogOption e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = e.Filter,
                Multiselect = e.Multiselect
            };
            if (openFileDialog.ShowDialog() != true)
            {
                return new string[] { };
            }
            if (e.Multiselect)
            {
                return openFileDialog.FileNames ?? new string[] { };
            }
            else
            {
                return new string[] { openFileDialog.FileName };
            }
        }

        public static string OnSaveDialog(EventManager.SaveDialogOption e)
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Title = e.Title,
                Filter = e.Filter,
                DefaultExt = e.DefaultExt,
                FileName = e.FileName,
                AddExtension = e.AddExtension
            };
            if (saveFileDialog.ShowDialog() != true)
            {
                return string.Empty;
            }
            return saveFileDialog.FileName;
        }
    }
}
