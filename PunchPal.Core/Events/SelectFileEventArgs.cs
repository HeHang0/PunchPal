using System;

namespace PunchPal.Core.Events
{
    public class SelectFileEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public string[] FileNames { get; set; }
        public string Filter { get; set; }
        public bool Multiselect { get; set; }

        public SelectFileEventArgs(string filters, bool multiselect = false)
        {
            Filter = filters;
            Multiselect = multiselect;
        }
    }
}
