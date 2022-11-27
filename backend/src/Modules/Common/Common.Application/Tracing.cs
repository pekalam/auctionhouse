using Common.Application.Commands;
using Common.Application.Events;
using System.Diagnostics;
using System.Reflection;

namespace Common.Application
{
    public static class Tracing
    {
        private const string ParentSpanIdExtraDataKey = "ParentSpanId";

        private static Lazy<ActivitySource> _source 
            // set to publication-only in order to not use locks
            = new(InitializeTracingSource, LazyThreadSafetyMode.PublicationOnly);

        private static ActivitySource InitializeTracingSource()
        {
            var assemblyName = GetDefaultActivitySourceName();
            return new ActivitySource(assemblyName!);
        }
        public static string GetDefaultActivitySourceName() => Assembly.GetEntryAssembly()!.GetName().Name!;

        public static Activity? StartActivity(string activityName, in ActivityContext parentContext) => _source.Value.StartActivity(ActivityKind.Internal, name: activityName, parentContext: parentContext);

        /// <summary>
        /// Starts a new activity or activity with parent context obtained from <see cref="Activity.Current"/> 
        /// </summary>
        public static Activity? StartActivity(string activityName, CorrelationId correlationId)
        {
            return _source.Value.StartActivity(ActivityKind.Internal, name: activityName, parentContext: Activity.Current?.Context ?? CreateNewActivityContext());

            ActivityContext CreateNewActivityContext() => new ActivityContext(ActivityTraceId.CreateFromString(correlationId.Value),
                    ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);
        }

        /// <summary>
        /// Starts an activity with parentContext context obtained from data in command context. If it's not possible then starts a new activity
        /// </summary>
        public static Activity? StartActivityFromCommandContext(string activityName, CommandContext commandContext)
        {
            if (!commandContext.ExtraData.ContainsKey(ParentSpanIdExtraDataKey))
            {
                return StartActivity(activityName, commandContext.CorrelationId);
            }

            return StartActivity(activityName, 
                new ActivityContext(
                       ActivityTraceId.CreateFromString(commandContext.CorrelationId.Value),
                       ActivitySpanId.CreateFromString(commandContext.ExtraData[ParentSpanIdExtraDataKey]),
                       ActivityTraceFlags.Recorded)
                );
        }

        public static void SetActivityContextData(CommandContext commandContext) 
        {
            if(Activity.Current is null || Activity.Current.Context.TraceId.ToString() != commandContext.CorrelationId.Value)
            {
                return;
            }

            commandContext.ExtraData[ParentSpanIdExtraDataKey] = Activity.Current.Context.SpanId.ToString();
        }
    }
}
