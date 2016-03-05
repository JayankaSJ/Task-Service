/*
 * Created by SharpDevelop.
 * User: JayankaS
 * Date: 4/2/2015
 * Time: 09:43 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Principal;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace Manager{
	
	/// <summary>
	/// Description of AdminPrivilages.
	/// </summary>
	public static class AdminPrivilages{
		
		[DllImport("user32")]
        public static extern UInt32 SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        internal const int BCM_FIRST = 0x1600;
        internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C);
        
        public static bool IsVistaOrHigher(){
            return Environment.OSVersion.Version.Major < 6;
        }
        /// <summary>
        /// Checks if the process is elevated
        /// </summary>
        /// <returns>If is elevated</returns>
        public static bool IsAdmin(){
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Add a shield icon to a button
        /// </summary>
        /// <param name="button">The button</param>
       	public static void AddShieldToButton(Button button){
            button.FlatStyle = FlatStyle.System;
            SendMessage(button.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
        }

        /// <summary>
        /// Restart the current process with administrator credentials
        /// </summary>
     	public static void RestartElevated(){
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Application.ExecutablePath;
            startInfo.Verb = "runas";
            try{
                Process p = Process.Start(startInfo);
            }
            catch{
                return; //If cancelled, do nothing
            }

            Application.Exit();
        }

        internal static class SystemRegistry {

            public static bool FirstRunStatus { get; set; }
            private static string source {
                get {
                    string args = string.Empty;
                    if (!string.IsNullOrEmpty(Arguments)) {
                        args = Arguments;
                    }
                    return string.Format(@"""{0}"" {1}", Application.ExecutablePath, args);
                }
            }
            public static string ModuleRegName { get; set; }
            public static string Arguments { get; set; }

            public static bool Startup() {
                try {
                    RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    if (registryKey.GetValue(ModuleRegName) == null) {
                        registryKey.SetValue(ModuleRegName, source);
                        FirstRunStatus = true;
                    }
                    else {
                        FirstRunStatus = false;
                    }
                    registryKey.Close();//dispose of the Key
                    return true;
                }
                catch {
                    Console.WriteLine("Error");
                    return false;
                }
            }

        }

    }
}
