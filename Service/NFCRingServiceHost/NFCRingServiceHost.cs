using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NFCRing.Service.Core;
using System.ServiceModel;
using System.Threading;
using NFCRing.Service.Host.Properties;

namespace NFCRing.Service.Host
{
    public partial class NFCRingServiceHost : ServiceBase
    {
        ServiceCore core = null;
        public ServiceHost serviceHost = null;

        public NFCRingServiceHost()
        {
            CanHandleSessionChangeEvent = true;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Core.ServiceCore.Log("Service starting");
            // fire up the service core
            if(core == null)
            {
                core = new ServiceCore(Settings.Default.isDebug);
            }
            core.Start(); // dont do this until they logon // need to check if they're already logged on and starting it manually
            core.LoadPlugins();
            //// wanna listen for IPC - Named pipes using WCF
            //if (serviceHost != null)
            //{
            //    serviceHost.Close();
            //}
            //serviceHost = new ServiceHost(typeof(RPCService));
            //serviceHost.Open();
            Core.ServiceCore.Log("Service started");
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            Core.ServiceCore.Log("Session state changed: " + changeDescription.Reason.ToString());
            if (changeDescription.Reason == SessionChangeReason.SessionLock) 
            {
                // stop reading threads
                core.Stop();
                Core.ServiceCore.Log("Workstation Locked");
            }
            else if (changeDescription.Reason == SessionChangeReason.SessionUnlock || changeDescription.Reason == SessionChangeReason.SessionLogon)
            {
                // wait a few seconds and start reading
                Timer timer = new Timer((x) =>
                {
                    core.Start();
                }, null, 2000, Timeout.Infinite);
                Core.ServiceCore.Log("Workstation unlocking or logging on");
            }
        }

        protected override void OnStop()
        {
            Core.ServiceCore.Log("Service stopping");
            core.Stop();
            core = null;
            // stop rpc
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
            Core.ServiceCore.Log("Service stopped");
        }
    }
}
