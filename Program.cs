using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace NetmonCollectionTool
{
    class Program
    {  
        private static int delay;
        private static string SplitLine = "================================================\r\n";

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            Console.WriteLine("Waiting Network Monitor to write the Backlog into trace file...");

            while (!Netmon.IsAlive)
            {
                Thread.Sleep(10);
            }

            Environment.Exit(-1);

            return true;
        }

        static void Main(string[] args)
        {
            if (!CheckIsAdmin())
            {
                Console.WriteLine("Please run the CMD as administrator");

                Environment.Exit(0);
            }

            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

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
                CommandOption DestinationCO = app.Option("-d|--Destination", "The folder for saving the netmon trace, folder will be created if not exist. The default folder is C:/temp/netmon", CommandOptionType.SingleValue);

                app.OnExecute(() =>
                {
                    int Delay = Int32.Parse(DelayCO.Value() ?? "0");
                    long EventID = long.Parse(EventIDCO.Value() ?? "0");
                    string Source = SourceCO.Value();
                    int Count = Int32.Parse(CountCO.Value() ?? "1");
                    string LogCategory = LogCategoryCO.Value() ?? "Application";
                    string Destination = DestinationCO.Value() ?? "C:\\temp\\netmon";

                    delay = Delay;
                    EventLogWatcher.Register(LogCategory, EventID, Source, Count);
                    EventLogWatcher.OnReached += EventLog_Reached;
                    Netmon.Start(Destination);

                    Console.WriteLine($"\r\n{SplitLine}Capture finished! You can close the window.");

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

        private static bool CheckIsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void EventLog_Reached(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine($"Hit the preset event count, process to stop the netmon trace.");
                EventLogWatcher.Unregister();

                Netmon.Stop(delay);
            }
            catch (Exception ex)
            { 
            
            }
        }
    }
}
