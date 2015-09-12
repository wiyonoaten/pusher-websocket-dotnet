using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Diagnostics
{
    public class TraceSource : EventSource
    {
        public TraceSource(string eventSourceName) 
            : base(eventSourceName)
        {
        }

        public void TraceEvent(TraceEventType eventType, int id, string message)
        {
            TraceEvent(eventType, id, "{0}", message);
        }

        [Event(1)]
        public void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
        {
            MethodBase method = typeof(TraceSource).GetMethod(CallerName(), new[] { typeof(TraceEventType), typeof(int), typeof(string), typeof(object[]) });
            EventAttribute attr = (EventAttribute)method.GetCustomAttributes(typeof(EventAttribute)).FirstOrDefault();

            attr.Level = TraceEventType2EventLevel(eventType);
            attr.Message = format;

            WriteEvent(1, args);
        }

        private string CallerName([CallerMemberName] string caller = null)
        {
            return caller;
        }
        
        private static EventLevel TraceEventType2EventLevel(TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Information:
                    return EventLevel.Informational;
                case TraceEventType.Warning:
                    return EventLevel.Warning;
                case TraceEventType.Verbose:
                    return EventLevel.Verbose;
                case TraceEventType.Error:
                    return EventLevel.Error;
                case TraceEventType.Critical:
                    return EventLevel.Critical;
                default:
                    return EventLevel.LogAlways;
            }
        }
    }
}
