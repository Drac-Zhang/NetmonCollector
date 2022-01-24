using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TraceCollector
{
    public static class Netmon
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        private static Process NetmonProcess;

        public static void Start()
        {
            CancelExisingNetmon();
            StartProcess();
        }

        private static void StartProcess()
        {
            Console.WriteLine("Starting netmon capture");
            ProcessStartInfo NetmonStartInfo = new ProcessStartInfo();
            NetmonStartInfo.FileName = "nmcap";
            NetmonStartInfo.RedirectStandardError = true;
            NetmonStartInfo.RedirectStandardOutput = true;
            NetmonStartInfo.RedirectStandardInput = true;
            NetmonStartInfo.UseShellExecute = false;
            NetmonStartInfo.CreateNoWindow = false;
            NetmonStartInfo.Arguments = "/CaptureProcesses /network * /capture /file C:\\temp\\trace.chn: 100M /TerminateWhen /KeyPress c";

            //Netmon Process
            NetmonProcess = new Process();
            NetmonProcess.StartInfo = NetmonStartInfo;
            NetmonProcess.Start();

            Console.WriteLine("Capturing...");
        }

        private static void CancelExisingNetmon()
        {
            Process[] Ps = Process.GetProcessesByName("nmcap");

            if (Ps.Length == 0)
            {
                Console.WriteLine("No running nmcap.exe detected");
            }
            else
            {
                Console.WriteLine($"{Ps.Length} existing nmcap.exe detected, start cancelling");

                foreach (Process P in Ps)
                {
                    Console.WriteLine($"PID: {P.Id} killed.");
                    P.Kill();
                }
            }
        }

        public static void Stop(int Delay)
        {
            if (Delay != 0)
            {
                Console.WriteLine($"Hit target event count, postpone the stop as per the setting ({Delay}) seconds");
                Thread.Sleep(Delay * 1000);
            }

            Console.WriteLine("Stopping the netmon capture.");
            UInt32 CommandCtrlC = 0;
            GenerateConsoleCtrlEvent(CommandCtrlC, Convert.ToUInt32(NetmonProcess.Id));

            NetmonProcess.WaitForExit();
        }
    }
}
