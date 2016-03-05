using System.ServiceProcess;
using Logging;

namespace Service {
    public class Service : ServiceBase{

        #region Configurations
        const string ServiceServiceName = "Task Service"; 
        #endregion

        static void Main(string[] args) {
            if(args.Length == 1){
                switch(args[0]){
                    case "install":
                        Manager.Install();
                        break;
                    case "start":
                        Manager.Start(ServiceServiceName);
                        break;
                    case "stop":
                        Manager.Stop(ServiceServiceName);
                        break;
                    case "restart":
                        Manager.Start(ServiceServiceName);
                        Manager.Stop(ServiceServiceName);
                        break;
                    case "uninstall":
                        Manager.Uninstall();
                        break;
                }
                return;
            }
            ServiceBase[] ServicesToRun = new ServiceBase[]{
                new Service()
            };
            ServiceBase.Run(ServicesToRun);
        }
        private void ServiceInstaller(){
            this.ServiceName = ServiceServiceName;
        }

        protected override void OnStart(string[] args) {
            string suc = "Service starting";
            try {
                ServiceEngine.Start();
                suc += " Success";
            }
            catch (System.Exception e) {
                suc += " Failed\r\n" + e.ToString();
            }
            Log.Write(suc);
            base.OnStart(args);
        }
        protected override void OnStop() {
            string suc = "Service stopping";
            try {
                ServiceEngine.Stop();
                suc += " Success";
            }
            catch (System.Exception e) {
                suc += " Failed\r\n" + e.ToString();
            }
            Log.Write(suc);
            base.OnStop();
        }
    }
}
