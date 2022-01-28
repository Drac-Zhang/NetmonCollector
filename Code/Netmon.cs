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

        private static Process NetmonProcess;
        private static string SplitLine = "================================================\r\n";

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

        public static void Start(string Destination)
        {
            CancelExisingNetmon();
            StartProcess(Destination);
        }

        private static void StartProcess(string Destination)
        {
            string TargetFolder = Destination.Replace("/", "\\").TrimEnd('\\');

            if (!Directory.Exists(TargetFolder))
            {
                Directory.CreateDirectory(TargetFolder);
            }

            Console.WriteLine("Starting netmon capture");
            ProcessStartInfo NetmonStartInfo = new ProcessStartInfo();
            NetmonStartInfo.FileName = "nmcap";
            NetmonStartInfo.RedirectStandardError = false;
            NetmonStartInfo.RedirectStandardOutput = false;
            NetmonStartInfo.RedirectStandardInput = false;
            NetmonStartInfo.UseShellExecute = false;
            NetmonStartInfo.CreateNoWindow = false;
            NetmonStartInfo.Arguments = $"/CaptureProcesses /network * /capture /file {TargetFolder}\\trace.chn: 100M";

            //Netmon Process
            NetmonProcess = new Process();
            NetmonProcess.StartInfo = NetmonStartInfo;
            NetmonProcess.Start();

            NetmonProcess.WaitForExit();
        }

        private static void CancelExisingNetmon()
        {
            Process[] Ps = Process.GetProcessesByName("nmcap");

            Console.WriteLine($"{SplitLine}Checking whether any nmcap.exe is running...");

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

            Console.WriteLine(SplitLine);
        }

        public static void Stop(int Delay)
        {
            if (Delay != 0)
            {
                Console.WriteLine($"{SplitLine}\r\nHit target event count, postpone the stop as per the setting ({Delay}) seconds.\r\n\r\n{SplitLine}");
                Thread.Sleep(Delay * 1000);
            }

            Console.WriteLine($"\r\n{SplitLine}\r\nStopping the netmon capture.\r\n\r\n{SplitLine}");

            UInt32 CommandCtrlC = 0;
            GenerateConsoleCtrlEvent(CommandCtrlC, Convert.ToUInt32(NetmonProcess.Id));
        }
    }
}
