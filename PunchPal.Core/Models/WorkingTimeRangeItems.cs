using System;
using System.Collections.Generic;

namespace PunchPal.Core.Models
{
    public class WorkingTimeRangeItems
    {
        public WorkingTimeRange Work { get; set; }
        public WorkingTimeRange Lunch { get; set; }
        public WorkingTimeRange Dinner { get; set; }
    }
}
