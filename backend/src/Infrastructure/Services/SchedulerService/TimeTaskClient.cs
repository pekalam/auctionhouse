﻿using System;
using System.Threading.Tasks;
using RestEase;

namespace Infrastructure.Services.SchedulerService
{
    public class CancelTaskRequest
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
    }

    public interface ITimeTaskClient
    {
        [Header("X-API-Key")]
        string ApiKey { get; set; }

        [Post("task/set")]
        Task<TaskSheduledResponse> ScheduleTask<TValue>([Body] ScheduleRequest<TValue> scheduleRequest)
            where TValue : class;

        [Post("task/cancel")]
        Task CancelTask([Body] CancelTaskRequest cancelTaskRequest);
    }
}
