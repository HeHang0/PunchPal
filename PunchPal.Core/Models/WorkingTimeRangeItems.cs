namespace PunchPal.Core.Models
{
    public class WorkingTimeRangeItems
    {
        public WorkingTimeRange Work { get; set; }
        public WorkingTimeRange Lunch { get; set; }
        public WorkingTimeRange Dinner { get; set; }

        public string Text =>
            (Work != null ? $"工作时间：{Work.RangeText.Replace(" ", "")}" : "") +
            (Lunch != null ? $" 午休时间：{Lunch.RangeText.Replace(" ", "")}" : "") +
            (Dinner != null ? $" 晚餐时间：{Dinner.RangeText.Replace(" ", "")}" : "");
    }
}
