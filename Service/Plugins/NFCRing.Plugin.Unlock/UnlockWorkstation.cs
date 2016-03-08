using NFCRing.Service.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NFCRing.Plugin.Unlock
{
    [Export(typeof(INFCRingServicePlugin))]
    public class UnlockWorkstation : INFCRingServicePlugin
    {
        TcpListener listener;
        Thread listenThread;
        bool runListenLoop = false;
        List<TcpClient> Clients = new List<TcpClient>();

        public void PluginLoad()
        {
            // this is where we open the listen socket. important
            if(listener != null)
            {
                listener.Stop();
                listener = null;
            }
            listener = new TcpListener(IPAddress.Any, 28416); // no reason
            // need to use another thread to listen for incoming connections
            listenThread = new Thread(new ThreadStart(listen));
            runListenLoop = true;
            listenThread.Start();
        }

        private void listen()
        {
            if (listener != null)
                listener.Start(3);
            while(runListenLoop && listener != null)
            {
                try
                {
                    TcpClient tc = listener.AcceptTcpClient();
                    // save the client to call it when an event happens (that we're listening for)
                    Clients.Add(tc);
                    NFCRing.Service.Core.ServiceCore.Log("Unlock Workstation: Client connected");
                }
                catch(Exception ex)
                {
                    // we failed to accept a connection. should log and work out why
                }                    
            }
            //if (listener != null)
            //    listener.Stop();
        }
        public void PluginUnload()
        {
            // shut down the listening socket otherwise it'll fail to create next time
            runListenLoop = false;
            try
            {
                listener.Stop();
                listener = null;
            }
            catch(Exception ex)
            {
                // probably died on Stop(). find a nice way to kill it
            }
        }

        public void NCFRingUp(string id, Dictionary<string, object> parameters, SystemState state)
        {
            // this space intentionally left blank
        }

        public void NCFRingDown(string id, Dictionary<string, object> parameters, SystemState state)
        {
            if(state.SessionStatus != SessionState.Locked && state.SessionStatus != SessionState.LoggedOff)
            {
                // we dont need to do anything if it's already active
                return;
            }
            // intially we'll send the ID to replace existing reader functionality.
            // then we'll swap to sending a username and password (ideally it'll be encrypted)
            List<TcpClient> dead = new List<TcpClient>();
            NFCRing.Service.Core.ServiceCore.Log("Unlock Workstation: Send data to " + Clients.Count + " clients");
            foreach (TcpClient tc in Clients)
            {
                try
                {
                    if (tc.Connected)
                    {
                        tc.GetStream().Write(System.Text.Encoding.ASCII.GetBytes(id), 0, id.Length);
                        tc.GetStream().Flush();
                    }
                    else
                    {
                        dead.Add(tc);
                    }
                }
                catch (Exception ex)
                {
                    // it blew up
                    tc.Close(); // maybe i shouldnt do this here?
                    dead.Add(tc);
                }
            }
            foreach (TcpClient tc in dead)
            {
                Clients.Remove(tc);
            }
        }

        public void NFCRingDataRead(string id, byte[] data, Dictionary<string, object> parameters, SystemState state)
        {
            // this space intentionally left blank
            // wouldnt it be neat if i could store an encrypted credential in the data section?
        }

        public string GetPluginName()
        {
            return "Unlock Workstation (network)";
        }

        public List<Parameter> GetParameters()
        {
            throw new NotImplementedException();
        }
    }
}
