using System.ServiceProcess;

namespace Manager {
    public static class Service {

        public static bool StartService(string serviceName) {
            ServiceController service = new ServiceController(serviceName);
            try {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
                return true;
            }
            catch {
                return false;
            }
        }

        public static bool StopService(string serviceName) {
            ServiceController service = new ServiceController(serviceName);
            try {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped);
                return true;
            }
            catch {
                return false;
            }
        }
    }
}
