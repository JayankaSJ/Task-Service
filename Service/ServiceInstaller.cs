using System.Reflection;
using System.Configuration.Install;
using System.ServiceProcess;


namespace Service {
    [System.ComponentModel.RunInstaller(true)]
    public class ServiceInstaller : Installer {

        #region Configurations
        const string ServiceServiceName = "Task Service";
        const string ServiceDisplayName = "Task Service";
        const string ServiceDescription = "Task Service";

        const ServiceStartMode ServiceStartType = ServiceStartMode.Automatic;

        const ServiceAccount Account = ServiceAccount.LocalSystem;
        const string Username = null;
        const string Password = null;


        private System.ServiceProcess.ServiceInstaller serviceinstaller;
        private ServiceProcessInstaller serviceprocessinstaller;
        
        #endregion
        public ServiceInstaller() {
            this.InitializeComponent();
        }
        void InitializeComponent() {
            this.serviceinstaller = new System.ServiceProcess.ServiceInstaller();
            this.serviceprocessinstaller = new ServiceProcessInstaller();

            this.serviceinstaller.Description = ServiceDescription;
            this.serviceinstaller.ServiceName = ServiceServiceName;
            this.serviceinstaller.DisplayName = ServiceDisplayName;
            this.serviceinstaller.StartType = ServiceStartType;

            this.serviceprocessinstaller.Account = Account;
            this.serviceprocessinstaller.Username = Username;
            this.serviceprocessinstaller.Password = Password;

            this.Installers.AddRange(new Installer[]{
                this.serviceprocessinstaller,
                this.serviceinstaller
            });
        }

    }

    public static class Manager {
        private static readonly string Path = Assembly.GetExecutingAssembly().Location;

        public static bool Start(string _name) {
            ServiceController Scontrol = new ServiceController(_name);
            try {
                Scontrol.Start();
                Scontrol.WaitForStatus(ServiceControllerStatus.Running);
                return true;
            }
            catch {
                return false;
            }
        }
        public static bool Stop(string _name) {
            ServiceController Scontrol = new ServiceController(_name);
            try {
                Scontrol.Stop();
                Scontrol.WaitForStatus(ServiceControllerStatus.Stopped);
                return true;
            }
            catch {
                return false;
            }
        }
        public static bool Install() {
            try {
                ManagedInstallerClass.InstallHelper(
                    new string[] { 
                        Path
                    });
                return true;
            }
            catch {
                return false;
            }
        }
        public static bool Uninstall() {
            try {
                ManagedInstallerClass.InstallHelper(
                    new string[] { 
                        "/u", Path
                    });
                return true;
            }
            catch {
                return false;
            }
        }
    }
}
