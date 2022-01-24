using Microsoft.Extensions.CommandLineUtils;
using System;

namespace TraceCollector
{
    class Program
    {  
        private static int delay;

        static void Main(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication();

            app.HelpOption("-?|-h|-help");
            app.Description = "Trace capture tool";

            try
            {
                CommandOption DelayCO = app.Option("-p|--Postpone", "Postpone (in seconds) before stop the trace, default value is 0.", CommandOptionType.SingleValue);
                CommandOption EventIDCO = app.Option("-eid|--EventID", "Event ID in the Event Log", CommandOptionType.SingleValue);
                CommandOption SourceCO = app.Option("-es|--EventSource", "Event Source in the Event Log", CommandOptionType.SingleValue);
                CommandOption CountCO = app.Option("-c|--EventCount", "How many events triggered to stop the capture.", CommandOptionType.SingleValue);
                CommandOption LogCategoryCO = app.Option("-lc|--LogCategory", "The type of the Event (Application, Security, Setup, System), the default is Application.", CommandOptionType.SingleValue);
                CommandOption DestinationCO = app.Option("-d|--Destination", "!!Not Implemented.", CommandOptionType.SingleValue);

                app.OnExecute(() =>
                {
                    int Delay = Int32.Parse(DelayCO.Value() ?? "0");
                    long EventID = long.Parse(EventIDCO.Value() ?? "0");
                    string Source = SourceCO.Value();
                    int Count = Int32.Parse(CountCO.Value() ?? "1");
                    string LogCategory = LogCategoryCO.Value() ?? "Application";
                    string Destination = DestinationCO.Value() ?? "C:\\temp";

                    delay = Delay;

                    Netmon.Start();
                    EventLogWatcher.Register(LogCategory, EventID, Source, Count);
                    EventLogWatcher.OnReached += EventLog_Reached;

                    Console.WriteLine("Press enter to stop manually");
                    Console.ReadLine();

                    EventLogWatcher.Unregister();
                    Netmon.Stop(0);

                    return 0;
                });

                app.Execute(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void EventLog_Reached(object sender, EventArgs e)
        {
            EventLogWatcher.Unregister();

            Netmon.Stop(delay);
        }
    }
}
