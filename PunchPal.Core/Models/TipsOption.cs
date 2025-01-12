using System;

namespace PunchPal.Core.Models
{
    public class TipsOption
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public ControlAppearance Appearance { get; set; }
        public TimeSpan Duration { get; set; }
        public TipsOption(string title, string message, ControlAppearance appearance, TimeSpan? duration = null)
        {
            Title = title;
            Message = message;
            Appearance = appearance;
            Duration = duration ?? TimeSpan.FromSeconds(3);
        }
    }
}
