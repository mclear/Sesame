using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading;
using NFCRing.Service.Common;
using System.Runtime.InteropServices;
using System.IO;

namespace NFCRing.Service.Core
{
    public class ServiceCore
    {
        private static bool _debug = false;
        private CompositionContainer container;
        protected static string appPath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
        private ServiceState state = ServiceState.Stopped;

        [DllImport("WinAPIWrapper", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PCSC_GetID([In, Out] IntPtr id, [In, Out] IntPtr err);

        [ImportMany]
        IEnumerable<Lazy<INFCRingServicePlugin>> plugins;

        Thread readerThread = null;

        public ServiceCore(bool isDebug)
        {
            _debug = isDebug;
            if (isDebug)
                try
                {
                    if (File.Exists(appPath + "\\log-old.txt"))
                    {
                        File.Delete(appPath + "\\log-old.txt");
                    }
                    if (File.Exists(appPath + "\\log.txt"))
                    {
                        File.Move(appPath + "\\log.txt", appPath + "\\log-old.txt");
                    }
                }
                catch { }
        }

        // load plugins
        public void LoadPlugins()
        {
            Core.ServiceCore.Log("Plugins loading");
            try
            {
                // load extension catalog
                var catalog = new AggregateCatalog();
                //Adds all the parts found in the same assembly as the Program class
                catalog.Catalogs.Add(new DirectoryCatalog(appPath + @"\plugins\"));

                //Create the CompositionContainer with the parts in the catalog
                container = new CompositionContainer(catalog);

                //Fill the imports of this object
                try
                {
                    this.container.ComposeParts(this);
                }
                catch (CompositionException ex)
                {
                    //LogEntry(ex, "Unable to load extensions");
                }
            }
            catch (Exception ex)
            {

            }
            Core.ServiceCore.Log(plugins.Count().ToString() + " Plugin(s) loaded");
        }

        // thread to monitor nfc reader
        private void ScanForId()
        {
            Core.ServiceCore.Log("NFC Reading started");
            string currentId = "";
            // basically keep running until we're told to stop
            while(state == ServiceState.Starting || state == ServiceState.Running)
            {
                // start dll and just call it 
                IntPtr idloc = Marshal.AllocHGlobal(100);
                IntPtr errloc = Marshal.AllocHGlobal(100);
                int len = PCSC_GetID(idloc, errloc);
                string error = Marshal.PtrToStringAnsi(errloc);
                string id = "";
                if(len > 0)
                    id = Marshal.PtrToStringAnsi(idloc, len);
                //else
                //    Core.ServiceCore.Log("Read error: " + error);

                Marshal.FreeHGlobal(idloc);
                Marshal.FreeHGlobal(errloc);

                // check the id of the token
                if (currentId == "" && id != "")
                {
                    Core.ServiceCore.Log("NFCTagDownEvent");
                    currentId = id;
                    // we just got a new token (state change)
                    int i = 0;
                    foreach(Lazy<INFCRingServicePlugin> plugin in plugins)
                    {
                        plugin.Value.NCFRingDown(id);
                        i++;
                        Core.ServiceCore.Log("Plugin " + i + " passed TagDown event");
                    }
                }
                else if (currentId != "" && id == "")
                {
                    Core.ServiceCore.Log("NFCTagUpEvent");
                    string origId = currentId;
                    currentId = "";
                    // we just lost the token (state change)
                    int i = 0;
                    foreach (Lazy<INFCRingServicePlugin> plugin in plugins)
                    {
                        plugin.Value.NCFRingUp(origId);
                        i++;
                        Core.ServiceCore.Log("Plugin " + i + " passed TagDown event");
                    }
                }
                // sleep for configured delay?
                Thread.Sleep(100);
            }
            Core.ServiceCore.Log("NFC Reading stopped");
        }

        public void Stop()
        {
            Core.ServiceCore.Log("Core stopping");
            state = ServiceState.Stopping;
            // give the reader loop a chance to exit
            Thread.Sleep(200);
            // the NFC thread will stop by itself now
            if(readerThread != null)
            {
                if (readerThread.IsAlive)
                    readerThread.Abort();
                readerThread = null;
            }
            // unload plugins
            //if(container != null)
            //    container.Dispose();

            state = ServiceState.Stopped;
            Core.ServiceCore.Log("Core stopped");
        }

        public void Start()
        {
            Core.ServiceCore.Log("Core starting");
            //System.Threading.Thread.Sleep(10000);
            state = ServiceState.Starting;

            readerThread = new Thread(new ThreadStart(ScanForId));
            readerThread.Start();
            state = ServiceState.Running;
            Core.ServiceCore.Log("Core started");
        }

        public static void Log(string message)
        {
            if(_debug)
            { 
                try
                {
                    File.AppendAllText(appPath + "\\log.txt", DateTime.Now.ToString("yy-MM-dd HH:mm:ss ") + message + Environment.NewLine);
                }
                catch { }
                }
        }
    }
}
