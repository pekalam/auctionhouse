using System.Diagnostics;

namespace Common.Application
{
    public static class TracingExtensions
    {
        public static void TraceOkStatus(this Activity? activity, string? description = null) 
            => SetStatusTags(activity, "OK", description);

        public static void TraceErrorStatus(this Activity? activity, string? description = null)
            => SetStatusTags(activity, "ERROR", description);

        public static void TraceUnsetStatus(this Activity? activity, string? description = null)
            => SetStatusTags(activity, "UNSET", description);

        private static void SetStatusTags(this Activity? activity, string status, string? description)
        {
            if (activity == null) return;

            Activity.Current?.SetTag("otel.status_code", "UNSET");
            if (description != null) Activity.Current?.SetTag("otel.status_description", description);
        }
    }
}
