using System;
using System.Collections;
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

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            var controller = new ServiceController(_serviceInstaller.ServiceName);
            try
            {
                if (controller.Status == ServiceControllerStatus.Running | controller.Status == ServiceControllerStatus.Paused)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    controller.Close();
                }
            }
            catch (Exception ex)
            {
                var source = $"{_serviceInstaller.ServiceName} Installer";
                const string log = "Application";

                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, log);
                }

                var eventLog = new EventLog {Source = source};

                eventLog.WriteEntry(string.Concat(@"The service could not be stopped. Please stop the service manually. Error: ", ex.Message), EventLogEntryType.Error);
            }
            finally
            {
                base.OnBeforeUninstall(savedState);
            }
        }
    }
}
