using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;

namespace NFCRing.Service.Host
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private readonly ServiceInstaller _serviceInstaller;

        public ProjectInstaller()
        {
            var serviceProcessInstaller = new ServiceProcessInstaller();
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.AfterInstall += OnAfterInstall;
            _serviceInstaller = new ServiceInstaller();
            _serviceInstaller.ServiceName = "NFCRingService";
            _serviceInstaller.Description = "Service for lock and unlock your computer by NFC Ring";
            _serviceInstaller.StartType = ServiceStartMode.Automatic;
            Installers.Add(serviceProcessInstaller);
            Installers.Add(_serviceInstaller);
        }

        private void OnAfterInstall(object sender, InstallEventArgs e)
        {
            var processStartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "net.exe",
                Arguments = "start " + _serviceInstaller.ServiceName
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();
        }
    }
}
