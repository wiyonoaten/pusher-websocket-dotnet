using Android.Util;
using System.Text;

namespace System.Diagnostics
{
    public class TraceSource
    {
        private readonly string _source;

        public TraceSource(string source)
        {
            _source = source;
        }

        public void TraceEvent(
            TraceEventType eventType,
            int id,
            string message)
        {
            Log.WriteLine(
                TraceEventType2LogPriority(eventType),
                MakeTag(id),
                message);
        }

        public void TraceEvent(
            TraceEventType eventType,
            int id,
            string format,
            params object[] args)
        {
            Log.WriteLine(
                TraceEventType2LogPriority(eventType),
                MakeTag(id),
                format,
                args);
        }

        private string MakeTag(int id)
        {
            return new StringBuilder()
                .Append(_source)
                .Append("(")
                .Append(id)
                .Append(")")
                .ToString();
        }

        private LogPriority TraceEventType2LogPriority(TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Information:
                    return LogPriority.Info;
                case TraceEventType.Warning:
                    return LogPriority.Warn;
                case TraceEventType.Verbose:
                    return LogPriority.Verbose;
                case TraceEventType.Error:
                case TraceEventType.Critical:
                    return LogPriority.Error;
                case TraceEventType.Suspend:
                    return LogPriority.Assert;
                default:
                    return LogPriority.Debug;
            }
        }
    }
}
