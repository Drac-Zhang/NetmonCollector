using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetmonCollectionTool
{
    public class ProcessBase
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        private Process process;
        private string Filename;
        private string Arguments;

        public bool HasExited
        {
            get 
            {
                if (process != null)
                {
                    return process.HasExited;
                }

                return true;
            }
        }

        public ProcessBase(string Filename, string Arguments)
        {
            this.Filename = Filename;
            this.Arguments = Arguments;
        }

        public void Start()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Filename;
            startInfo.RedirectStandardError = false;
            startInfo.RedirectStandardOutput = false;
            startInfo.RedirectStandardInput = false;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = false;
            startInfo.Arguments = Arguments;

            //Netmon Process
            process = new Process();
            process.StartInfo = startInfo;
            process.Start();
        }

        public void Stop(int Delay)
        {
            if (Delay != 0)
            {
                Thread.Sleep(Delay * 1000);
            }

            UInt32 CommandCtrlC = 0;
            GenerateConsoleCtrlEvent(CommandCtrlC, Convert.ToUInt32(process.Id));
        }
    }
}
