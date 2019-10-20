using System;

namespace Infrastructure.Adapters.Services.SchedulerService
{
    public class TimeTaskRequest<T> where T : class
    {
        public Guid Id { get; set; }
        public T Values { get; set; }
    }
}