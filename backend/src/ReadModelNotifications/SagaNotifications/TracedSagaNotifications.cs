﻿using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using System.Diagnostics;

namespace ReadModelNotifications.SagaNotifications
{
    internal static class TracedSagaNotificationsExtensions
    {
        public static Task AddActivityDisposingContinuation(this Task task, Activity? activity)
        {
            if (activity == null) return task;
            return task.ContinueWith(static (t, state) => //passing state in order to avoid creating delegate on every call
             {
                 ((Activity?)state)?.Dispose();
                 return t;
             }, activity, TaskContinuationOptions.ExecuteSynchronously).Unwrap(); //exec synchronously to avoid calling continuation on new thread
        }
    }

    internal class TracedSagaNotifications : ISagaNotifications
    {
        private readonly ISagaNotifications _sagaNotifications;

        public TracedSagaNotifications(ISagaNotifications sagaNotifications)
        {
            _sagaNotifications = sagaNotifications;
        }

        public Task AddUnhandledEvent<T>(CorrelationId correlationId, T @event) where T : Event
        {
            var activity = Tracing.StartActivity(nameof(AddUnhandledEvent), correlationId);
            return _sagaNotifications.AddUnhandledEvent(correlationId, @event).AddActivityDisposingContinuation(activity);
        }

        public Task AddUnhandledEvents<T>(CorrelationId correlationId, IEnumerable<T> @event) where T : Event
        {
            var activity = Tracing.StartActivity(nameof(AddUnhandledEvents), correlationId);
            return _sagaNotifications.AddUnhandledEvents(correlationId, @event).AddActivityDisposingContinuation(activity);
        }

        public Task MarkEventAsHandled<T>(CorrelationId correlationId, T @event) where T : Event
        {
            var activity = Tracing.StartActivity(nameof(MarkEventAsHandled), correlationId);
            return _sagaNotifications.MarkEventAsHandled(correlationId, @event).AddActivityDisposingContinuation(activity);
        }

        public Task MarkSagaAsCompleted(CorrelationId correlationId, Dictionary<string, object>? extraData = null)
        {
            var activity = Tracing.StartActivity(nameof(MarkSagaAsCompleted), correlationId);
            return _sagaNotifications.MarkSagaAsCompleted(correlationId, extraData).AddActivityDisposingContinuation(activity);
        }

        public Task MarkSagaAsFailed(CorrelationId correlationId, Dictionary<string, object>? extraData = null)
        {
            var activity = Tracing.StartActivity(nameof(MarkSagaAsFailed), correlationId);
            return _sagaNotifications.MarkSagaAsFailed(correlationId, extraData).AddActivityDisposingContinuation(activity);
        }

        public Task RegisterNewSaga(CorrelationId correlationId, CommandId commandId)
        {
            var activity = Tracing.StartActivity(nameof(RegisterNewSaga), correlationId);
            return _sagaNotifications.RegisterNewSaga(correlationId, commandId).AddActivityDisposingContinuation(activity);
        }
    }
}
