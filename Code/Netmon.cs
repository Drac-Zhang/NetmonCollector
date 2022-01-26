using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetmonCollectionTool
{
    public static class Netmon
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);

        private static Process NetmonProcess;
        public static bool IsAlive
        {
            get {
                if (NetmonProcess != null)
                {
                    return NetmonProcess.HasExited;
                }

                return true;
            }
        }

        public static void Start()
        {
            CancelExisingNetmon();
            StartProcess("C:\\temp\\netmon");
        }

        private static void StartProcess(string Destination)
        {
            if (!Directory.Exists(Destination))
            {
                Directory.CreateDirectory(Destination);
            }

            Console.WriteLine("Starting netmon capture");
            ProcessStartInfo NetmonStartInfo = new ProcessStartInfo();
            NetmonStartInfo.FileName = "cmd";
            NetmonStartInfo.RedirectStandardError = false;
            NetmonStartInfo.RedirectStandardOutput = false;
            NetmonStartInfo.RedirectStandardInput = false;
            NetmonStartInfo.UseShellExecute = false;
            NetmonStartInfo.CreateNoWindow = false;
            NetmonStartInfo.Arguments = $"/c nmcap /CaptureProcesses /network * /capture /file {Destination}\\trace.chn: 100M /TerminateWhen /KeyPress c";

            //Netmon Process
            NetmonProcess = new Process();
            NetmonProcess.StartInfo = NetmonStartInfo;
            NetmonProcess.Start();

            Console.WriteLine("Capturing...");

            NetmonProcess.WaitForExit();
        }

        private static void NetmonProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
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
        }
    }
}
