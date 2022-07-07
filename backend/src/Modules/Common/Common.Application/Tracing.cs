using Common.Application.Events;
using System.Diagnostics;
using System.Reflection;

namespace Common.Application
{
    public static class Tracing
    {
        internal static ActivitySource? Source { get; private set; }

        internal static void InitializeTracingSource()
        {
            var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name;
            Source = new ActivitySource(assemblyName!);
        }

        public static Activity? StartTracing(string activityName, CorrelationId correlationId)
        {
            var actCtx = new ActivityContext(ActivityTraceId.CreateFromString(correlationId.Value),
            //Activity.Current?.SpanId ?? 
            ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);
            var activity = Source?.StartActivity(ActivityKind.Internal, name: activityName, parentContext: actCtx);
            return activity;
        }
    }
}
