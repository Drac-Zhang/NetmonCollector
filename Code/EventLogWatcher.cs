using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetmonCollectionTool
{
    public static class EventLogWatcher
    {
        private static EventLog eventLog;
        public static EventHandler OnReached;

        private static long ID;
        private static string Source;
        private static int TargetCount;
        private static int CurrentCount = 0;

        public static void Register(string LogCategory, long EventID, string EventSource, int Count)
        {
            ID = EventID;
            Source = EventSource;
            TargetCount = Count;

            Console.WriteLine("Attaching to Event Log.");
            eventLog = new EventLog();
            eventLog.Log = LogCategory;

            eventLog.EntryWritten += EventLog_EntryWritten;
            eventLog.EnableRaisingEvents = true;

            Console.WriteLine("Succeeded to attach.");
        }
        private static void EventLog_EntryWritten(object sender, EntryWrittenEventArgs e)
        {
            EventLogEntry entry = e.Entry;

            bool InstanceCheck = (e.Entry.InstanceId == ID) || (ID == 0);
            bool SourceCheck = (e.Entry.Source == Source) || (String.IsNullOrEmpty(Source));

            if (InstanceCheck && SourceCheck)
            {
                CurrentCount++;
                Console.WriteLine($"{CurrentCount}/{TargetCount} event(s) hit.");

                if (CurrentCount >= TargetCount)
                {
                    OnReached(null, null);
                }
            }
        }

        public static void Unregister()
        {
            Console.WriteLine("Unattaching the Event Log.");
            if (eventLog != null)
            {
                eventLog.EntryWritten -= EventLog_EntryWritten;
                eventLog.EnableRaisingEvents = false;
            }
        }
    }
}
