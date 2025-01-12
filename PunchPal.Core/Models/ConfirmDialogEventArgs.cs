using System;
using System.Threading.Tasks;

namespace PunchPal.Core.Models
{
    public class ConfirmDialogEventArgs : EventArgs
    {
        public Task<bool> Result { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public ControlAppearance Appearance { get; set; } = ControlAppearance.Primary;
    }
}
