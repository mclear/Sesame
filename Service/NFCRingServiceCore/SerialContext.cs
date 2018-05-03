using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NFCRing.Service.Core
{
    public class SerialContext
    {
        private static Dictionary<string, double> CurrentIds = new Dictionary<string, double>();
        static object idSyncRoot = new object();
        const int TagTimeoutMS = 200;

        private static Dictionary<string, SerialConnection> OpenPorts = new Dictionary<string, SerialConnection>();
        static object readerSyncRoot = new object();

        DateTime StartTime = DateTime.Now;

        Timer t = null;

        public SerialContext()
        {
            EnumeratePorts();
            t = new Timer(CheckPorts, new object(), 10000, 5000);
        }

        private void CheckPorts(object state)
        {
            lock(readerSyncRoot)
            {
                List<string> deadReaders = new List<string>();
                foreach(KeyValuePair<string, SerialConnection> kvp in OpenPorts)
                {
                    if(!kvp.Value.Port.IsOpen)
                    {
                        deadReaders.Add(kvp.Key);
                    }
                }
                foreach(string s in deadReaders)
                {
                    OpenPorts.Remove(s);
                }
                EnumeratePorts();
            }
        }

        public void Stop()
        {
            lock(readerSyncRoot)
            {
                foreach (SerialConnection sp in OpenPorts.Values)
                {
                    if (sp.Port.IsOpen)
                    {
                        sp.Port.Write("STOP\n");
                    }
                }
                Thread.Sleep(500);
                foreach (SerialConnection sp in OpenPorts.Values)
                {
                    sp.Port.DataReceived -= P_DataReceived;
                    if (sp.Port.IsOpen)
                        sp.Port.Close();
                }
                OpenPorts.Clear();
            }

            lock (idSyncRoot)
            {                
                CurrentIds.Clear();
            }
        }

        public List<string> GetReaders()
        {
            return OpenPorts.Keys.ToList();
        }

        // I need to re-run this occasionally in case a new device has been added?
        private void EnumeratePorts()
        {
            lock (readerSyncRoot)
            {
                string[] ports = SerialPort.GetPortNames();
                List<string> devices = new List<string>();
                foreach (string portName in ports)
                {
                    if (OpenPorts.ContainsKey(portName))
                    {
                        continue;
                    }
                    SerialPort p = new SerialPort(portName, 115200);
                    if (!p.IsOpen)
                    {
                        try
                        {
                            p.Open();
                            if (p.IsOpen)
                            {
                                Thread.Sleep(500);
                                p.Write("SHAKE\n");
                                int i = 0;
                                while(p.BytesToRead <= 0 && i < 50)
                                {
                                    Thread.Sleep(10);
                                }
                                if (p.BytesToRead <= 0)
                                    continue;
                                string s = p.ReadLine();
                                if (s.Trim() == "NFC Reader")
                                {
                                    p.Write("IDENT\n");
                                    i = 0;
                                    while (p.BytesToRead <= 0 && i < 50)
                                    {
                                        Thread.Sleep(10);
                                    }
                                    if (p.BytesToRead <= 0)
                                        continue;
                                    s = p.ReadLine();
                                    p.Write("START\n");
                                    i = 0;
                                    while (p.BytesToRead <= 0 && i < 50)
                                    {
                                        Thread.Sleep(10);
                                    }
                                    if (p.BytesToRead <= 0)
                                        continue;
                                    s = p.ReadLine();
                                    devices.Add(s.Trim());
                                    p.DataReceived += P_DataReceived;
                                    OpenPorts.Add(portName, new SerialConnection() { Port = p, ReaderName = s });
                                }
                                else
                                {
                                    p.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // couldnt open the port for some reason. already open elsewhere?
                        }
                    }
                }
            }
        }

        private void P_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string s = (sender as SerialPort)?.ReadLine();
            s = s.Trim();
            if (s == "OK")
                return;
            lock (idSyncRoot)
            {
                if (CurrentIds.ContainsKey(s))
                {
                    CurrentIds[s] = (DateTime.Now - StartTime).TotalMilliseconds;
                }
                else
                {
                    CurrentIds.Add(s, (DateTime.Now - StartTime).TotalMilliseconds);
                }
            }
        }

        public List<string> GetIds()
        {
            lock(idSyncRoot)
            {
                List<string> deadTokens = new List<string>();
                foreach(KeyValuePair<string, double> kvp in CurrentIds)
                {
                    if((DateTime.Now - StartTime).TotalMilliseconds > (kvp.Value + 200))
                    {
                        deadTokens.Add(kvp.Key);
                    }
                }
                foreach (string id in deadTokens)
                    CurrentIds.Remove(id);
                return CurrentIds.Keys.ToList();
            }
        }
    }

    class SerialConnection
    {
        public SerialPort Port { get; set; }
        public string ReaderName { get; set; }
    }
}
