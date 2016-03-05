using System;
using System.IO;
using System.Windows.Forms;

using Logging;

namespace Manager {
    static class EntryPoint {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args) {
#if !DEBUG
            if (!AdminPrivilages.IsAdmin()) {
                MessageBox.Show("Application require Admin Privilages", "Admin Privilages", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            } 
#endif
            if (!File.Exists("database.db")) {
                Task.Task.InitializeTable();
                Task.Event.EventCopy.InitializeTable();
                Task.Event.EventFormat.InitializeTable();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
