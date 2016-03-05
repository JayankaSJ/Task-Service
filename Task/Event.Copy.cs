using System;
using System.Diagnostics;

using Logging;

namespace Task.Event {

    public static class Copy {
        public static class RETURNCODE {
            public const int SUCCESS = 0;
            public const int NO_FILES_TO_COPY = 1;
            public const int TERMINATED = 2;
            public const int INITIALIZATION_ERROR = 4;
            public const int DISK_WRITE_ERROR = 5;
        }
        public static int Start(Event _event) {
            EventCopy ec = _event as EventCopy;

            string source = string.Format(@"{0}", ec.source);
            string Destination = string.Format(@"{0}", ec.destination);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "xcopy";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = string.Format("\"{0}\" \"{1}\" /e /y /I /K", source, Destination);
            startInfo.Verb = "runas";
            int eco = -1;
            string result = "Copy Process ";
            try {
                using (Process exeProcess = Process.Start(startInfo)) {
                    exeProcess.WaitForExit();
                    eco = exeProcess.ExitCode;

                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(ec.destination);
                    di.Attributes = System.IO.FileAttributes.Normal;

                    result += "Success";
                }
            }
            catch (Exception exp) {
                result +="Failed\r\n" + exp.ToString();
            }
            Log.Write(result);
            return eco;
        }
    }
}
