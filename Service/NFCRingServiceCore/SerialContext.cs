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
        object syncRoot = new object();
        const int TagTimeoutMS = 200;

        private static Dictionary<string, SerialPort> OpenPorts = new Dictionary<string, SerialPort>();
        DateTime StartTime = DateTime.Now;

        public SerialContext()
        {
            EnumeratePorts();
            foreach (SerialPort sp in OpenPorts.Values)
            {
                if (sp.IsOpen)
                    sp.Write("START\n");
            }
        }

        public void Stop()
        {
            lock(syncRoot)
            {
                foreach (SerialPort sp in OpenPorts.Values)
                {
                    if (sp.IsOpen)
                    {
                        sp.Write("STOP\n");
                    }
                }
                Thread.Sleep(500);
                CurrentIds.Clear();
                foreach(SerialPort sp in OpenPorts.Values)
                {
                    sp.DataReceived -= P_DataReceived;
                    if (sp.IsOpen)
                        sp.Close();
                }
                OpenPorts.Clear();
            }
        }

        public List<string> GetReaders()
        {
            return OpenPorts.Keys.ToList();
        }

        // I need to re-run this occasionally in case a new device has been added?
        private void EnumeratePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            List<string> devices = new List<string>();
            foreach (string portName in ports)
            {
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
                            string s = p.ReadLine();
                            if (s.Trim() == "NFC Reader")
                            {
                                p.Write("IDENT\n");
                                s = p.ReadLine();
                                devices.Add(s.Trim());
                                p.DataReceived += P_DataReceived;
                                OpenPorts.Add(s, p);
                            }
                            else
                            {
                                p.Close();
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        // couldnt open the port for some reason. already open elsewhere?
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
            lock (syncRoot)
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
            lock(syncRoot)
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
}
