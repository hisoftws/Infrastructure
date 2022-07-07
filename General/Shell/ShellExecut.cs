using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace General.Shell
{
    public static class ShellExecut
    {
        public static string Bash(this string cmd)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("/bin/bash", "")
            };
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.StandardInput.WriteLine(cmd);
            //process.StandardInput.WriteLine("netstat -an |grep ESTABLISHED |wc -l");
            process.StandardInput.Close();
            var info = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Dispose();
            //var lines = cpuInfo.Split('\n');
            //foreach (var item in lines)
            //{
            //    Console.WriteLine("行记录：" + item);
            //}
            return info;
        }
    }
}
