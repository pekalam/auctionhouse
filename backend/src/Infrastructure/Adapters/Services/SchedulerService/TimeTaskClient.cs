using System;
using System.Threading.Tasks;
using RestEase;

namespace Infrastructure.Adapters.Services.SchedulerService
{
    public interface ITimeTaskClient
    {
        [Header("X-API-Key")]
        string ApiKey { get; set; }

        [Post("task/set")]
        Task<TaskSheduledResponse> ScheduleTask<TValue>([Body] ScheduleRequest<TValue> scheduleRequest)
            where TValue : class;

        [Post("task/cancel")]
        Task CancelTask([Query] string type, [Query] Guid id);
    }
}
