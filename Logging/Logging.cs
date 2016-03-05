using System;
using System.IO;

namespace Logging {
    public static class Log {
        static StreamWriter sw;
        public static void Write(string _entry) {
            try {
				sw = new StreamWriter(string.Format(@"{0}\log.txt", AppDomain.CurrentDomain.BaseDirectory), true);
                sw.WriteLine(string.Format("[{0}] : {1}", DateTime.Now, _entry));
                sw.Flush();
                sw.Close();
            }
            catch {
            }
        }
    }
}