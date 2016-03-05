using System;
using System.Diagnostics;
using System.IO;
using System.Management;

using Logging;

namespace Task.Event {
    public static class Format {
        public static class RETURNCODE {
            public const int SUCCESS = 0;
            public const int FAILED = -1;
            public const int INCORRECT_PARAMETERS_SUPPLIED = 1;
            public const int FATAL_ERROR = 4;
            public const int INVAID_RESPONSE = 5;
            public const int CANT_CREATE_FILE = 6;
        }
        public static int Format_Management_Object(Event _event) {
            EventFormat ef = _event as EventFormat;
            int success = RETURNCODE.FAILED;
            string query = string.Format(@"SELECT * FROM Win32_Volume WHERE DRIVELETTER = '{0}'", ef.driveLetter);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject vi in searcher.Get()) {
                try {
                    vi.InvokeMethod("Format", new object[] { ef.fileSystem, ef.quick, ef.allocation, ef.compression });
                    success = RETURNCODE.SUCCESS;
                }
                catch(Exception e) {
                    Log.Write(e.ToString());
                    success = RETURNCODE.FAILED;
                }

            }
            return success;
        }

        private static bool BatchWrite(EventFormat _ef) {
            string driveletter = string.Format(@"{0}:", _ef.driveLetter);
            string filesystem = string.Format(@"/FS:{0}", _ef.fileSystem);
            string quickformat = string.Format(@"{0}", ((_ef.quick) ? "/Q" : ""));

            string query = string.Format(@"FORMAT {0} /Y {1} {2}", driveletter, filesystem, quickformat);

            try {
                StreamWriter sw = File.CreateText(@"temp.bat");
                sw.WriteLine("@echo off");
                sw.WriteLine(query);
                sw.Close();
                return true;
            }
            catch (Exception e) {
                Log.Write(e.ToString());
                return false;
            }
        }
        private static int StartFormat() {
            Process FormatProcess = new Process();
            FormatProcess.StartInfo.FileName = @"temp.bat";
            FormatProcess.StartInfo.UseShellExecute = false;
            FormatProcess.StartInfo.CreateNoWindow = true;
            FormatProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            FormatProcess.StartInfo.Verb = "runas";
            string result = "Format Process ";
            try {
                FormatProcess.Start();
                FormatProcess.WaitForExit();
                File.Delete(@"temp.bat");
                result += "Success";
            }
            catch (Exception exp) {
                result += "Failed\r\n" + exp.ToString();
            }
            Log.Write(result);
            return FormatProcess.ExitCode;
        }
        public static int Format_CommandLine_Process(Event _event) {
            EventFormat ef = _event as EventFormat;

            if(!BatchWrite(ef)){
                return RETURNCODE.CANT_CREATE_FILE;
            }
            int result = StartFormat();
            switch(result){
                case 0:
                    return RETURNCODE.SUCCESS;
                case 1:
                    return RETURNCODE.INCORRECT_PARAMETERS_SUPPLIED;
                case 4:
                    return RETURNCODE.FATAL_ERROR;
                case 5:
                    return RETURNCODE.INVAID_RESPONSE;
                default:
                    return RETURNCODE.FAILED;
            }
        }
    }
}
