using System;

namespace Core.Common.SchedulerService
{
    public class ScheduledTaskId
    {
        public Guid Value { get; }

        public ScheduledTaskId(Guid value)
        {
            Value = value;
        }
    }
}
