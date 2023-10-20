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
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.Win32;
using System.Management;
using NFCRing.Libraries;

namespace NFCRing.Service.Core
{
    public class ServiceCore
    {
        private static bool _debug = false;
        private CompositionContainer container;
        protected static string appPath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
        private ServiceState state = ServiceState.Stopped;
        public static SystemState SystemStatus = new SystemState();
        private Config ApplicationConfiguration;

        TcpListener credentialListener;
        Thread credentialListenThread;
        TcpListener registrationListener;
        Thread registrationListenThread;
        bool runListenLoops = false;
        //private List<Client> Connections = new List<Client>();

        //[DllImport("WinAPIWrapper", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int PCSC_GetID([In, Out] IntPtr id, [In, Out] IntPtr err);

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
            Log("Plugins loading");
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
                foreach (Lazy<INFCRingServicePlugin> plugin in plugins)
                {
                    try
                    {
                        //Log("Starting plugin");
                        plugin.Value.PluginLoad();
                        Log("Plugin " + plugin.Value.GetPluginName() + " passed Load event");
                    }
                    catch(Exception ex)
                    {
                        Log("Plugin threw an excception on Load event");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception loading plugins: " + ex.Message);
            }
            Log(plugins.Count().ToString() + " Plugin(s) loaded");
        }

        // thread to monitor nfc reader
        private void ScanForId()
        {
            Log("NFC Reading started");
            //Thread.Sleep(10000);
            List<string> currentTokens = new List<string>();
            SCardContext sCardContext = new SCardContext();
            //SerialContext serialContext = new SerialContext();
            // basically keep running until we're told to stop
            while(state == ServiceState.Starting || state == ServiceState.Running)
            {
                // start dll and just call it 
                //IntPtr idloc = Marshal.AllocHGlobal(100);
                //IntPtr errloc = Marshal.AllocHGlobal(100);
                //int len = PCSC_GetID(idloc, errloc);
                //string error = Marshal.PtrToStringAnsi(errloc);
                //string id = "";
                //if(len > 0)
                //    id = Marshal.PtrToStringAnsi(idloc, len);
                ////else
                ////    Log("Read error: " + error);

                //Marshal.FreeHGlobal(idloc);
                //Marshal.FreeHGlobal(errloc);
                List<string> ls = sCardContext.GetIds();
                //ls = ls.Concat(serialContext.GetIds()).ToList();
                foreach (string id in ls)
                {
                    //string id = ls.FirstOrDefault() ?? "";
                    // check the id of the token
                    if (!currentTokens.Contains(id) && id != "")
                    {
                        Log("NFCTagDownEvent");
                        // we just got a new token (state change)

                        // load parameters from config
                        if (SystemStatus.AwaitingToken)
                        {
                            // this is where we capture it and show the next screen
                            if (SystemStatus.RegistrationClient != null)
                            {
                                TcpClient c = SystemStatus.RegistrationClient;
                                ServiceCommunication.SendNetworkMessage(ref c, JsonConvert.SerializeObject(new NetworkMessage() { Type = MessageType.Token, Token = id }));
                                SystemStatus.RegistrationClient = c;
                            }
                        }
                        else
                        {
                            // check config
                            foreach (User u in ApplicationConfiguration.Users)
                            {
                                string hashedToken = Crypto.Hash(Crypto.Hash(id) + u.Salt);
                                foreach (Event e in u.Events)
                                {
                                    if (hashedToken == e.Token)
                                    {
                                        foreach (Lazy<INFCRingServicePlugin> plugin in plugins)
                                        {
                                            if (plugin.Value.GetPluginName() == e.PluginName)
                                            {
                                                plugin.Value.NCFRingDown(id, e.Parameters, SystemStatus);

                                                Log("Plugin " + plugin.Value.GetPluginName() + " passed TagDown event");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    currentTokens.Remove(id);
                }
                foreach (string id in currentTokens)
                {
                    Log("NFCTagUpEvent");
                    // we just lost the token (state change)
                    if (SystemStatus.AwaitingToken)
                    {
                        // they just lifted it. reset this
                        SystemStatus.AwaitingToken = false;
                    }
                    else
                    {
                        // check config
                        foreach (User u in ApplicationConfiguration.Users)
                        {
                            string hashedToken = Crypto.Hash(Crypto.Hash(id) + u.Salt);
                            foreach (Event e in u.Events)
                            {
                                if (hashedToken == e.Token)
                                {
                                    foreach (Lazy<INFCRingServicePlugin> plugin in plugins)
                                    {
                                        if (plugin.Value.GetPluginName() == e.PluginName)
                                        {
                                            plugin.Value.NCFRingUp(id, e.Parameters, SystemStatus);
                                            Log("Plugin " + plugin.Value.GetPluginName() + " passed TagUp event");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                currentTokens = ls;
                // sleep for configured delay?
                Thread.Sleep(100);
            }
            //serialContext.Stop();
            Log("NFC Reading stopped");
        }

        public void Stop()
        {
            Log("Core stopping");
            state = ServiceState.Stopping;
            // give the reader loop a chance to exit
            Thread.Sleep(200);
            // the NFC thread will stop by itself now
            if(readerThread != null)
            {
                if (readerThread.IsAlive)
                    readerThread.Join();
                readerThread = null;
            }
            StopNetwork();
            // we need to unload plugins now
            foreach (Lazy<INFCRingServicePlugin> plugin in plugins)
            {
                try
                {
                    //Log("Starting plugin");
                    Log("Plugin " + plugin.Value.GetPluginName() + " passed Unload event");
                    plugin.Value.PluginUnload();
                }
                catch (Exception ex)
                {
                    Log("Plugin threw an excception on Unload event: " + ex.Message);
                }
            }

            SaveConfig();
            state = ServiceState.Stopped;
            Log("Core stopped");
        }

        public void Start()
        {
            Log("Core starting");
            //System.Threading.Thread.Sleep(10000);
            LoadConfig();
            state = ServiceState.Starting;
            InitialiseNetwork();
            readerThread = new Thread(new ThreadStart(ScanForId));
            readerThread.Start();
            state = ServiceState.Running;
            Log("Core started");
        }

        private void InitialiseNetwork()
        {
            Log("Initialising Network");
            if (registrationListener != null)
            {
                registrationListener.Stop();
                registrationListener = null;
            }
            if (credentialListener != null)
            {
                credentialListener.Stop();
                credentialListener = null;
            }

            credentialListener = new TcpListener(IPAddress.Loopback, 28416); // no reason
            registrationListener = new TcpListener(IPAddress.Loopback, 28417); // no reason

            runListenLoops = true;
            // credential provider listener
            credentialListenThread = new Thread(new ThreadStart(listenForCredentialProvider));
            credentialListenThread.Start();

            // need to use another thread to listen for incoming connections
            registrationListenThread = new Thread(new ThreadStart(listenForRegistration));
            registrationListenThread.Start();
        }

        private void StopNetwork()
        {
            Log("Network Shutting Down");
            runListenLoops = false;
            try
            {
                SystemStatus.CredentialData.ProviderActive = false;
                SystemStatus.CredentialData.Client = null;
                SystemStatus.AwaitingToken = false;
                SystemStatus.RegistrationClient = null;
                credentialListener.Stop();
                credentialListener = null;
                registrationListener.Stop();
                registrationListener = null;
            }
            catch (Exception ex)
            {
                Log("TCP error stopping listener: " + ex.Message);
            }
        }

        
        private void listenForCredentialProvider()
        {
            Log("Credential Network Active");
            if (credentialListener != null)
                credentialListener.Start(3);
            while (runListenLoops && credentialListener != null && (state == ServiceState.Running || state == ServiceState.Starting))
            {
                try
                {
                    TcpClient tc = credentialListener.AcceptTcpClient();
                    // save the client to call it when an event happens (that we're listening for)
                    Log("TCP: credential client connected");

                    Thread newClientThread = new Thread(new ParameterizedThreadStart(ReadNetwork));
                    newClientThread.Start(tc);
                    SystemStatus.CredentialData.Client = tc;
                    SystemStatus.CredentialData.ProviderActive = true;
                    //Connections.Add(new Client() { ClientConnection = tc, ClientProcess = newClientThread });
                }
                catch (Exception ex)
                {
                    // we failed to accept a connection. should log and work out why
                    Log("TCP: Accept credential client failed: " + ex.Message);
                }
                if (SystemStatus.CredentialData.Client == null || !SystemStatus.CredentialData.Client.Connected)
                {
                    SystemStatus.CredentialData.ProviderActive = false;
                    SystemStatus.CredentialData.Client = null;
                }
            }
            Log("Credential Network Inactive");
        }

        private void listenForRegistration()
        {
            Log("Registration Network Active");
            if (registrationListener != null)
                registrationListener.Start(3);
            while (runListenLoops && registrationListener != null && (state == ServiceState.Running || state == ServiceState.Starting))
            {
                try
                {
                    TcpClient tc = registrationListener.AcceptTcpClient();
                    // save the client to call it when an event happens (that we're listening for)
                    Log("TCP: registration client connected");

                    Thread newClientThread = new Thread(new ParameterizedThreadStart(ReadNetwork));
                    newClientThread.Start(tc);
                    SystemStatus.RegistrationClient = tc;
                    //Connections.Add(new Client() { ClientConnection = tc, ClientProcess = newClientThread });
                }
                catch (Exception ex)
                {
                    // we failed to accept a connection. should log and work out why
                    Log("TCP: Accept registration client failed: " + ex.Message);
                }
                if (SystemStatus.RegistrationClient == null || !SystemStatus.RegistrationClient.Connected)
                {
                    SystemStatus.AwaitingToken = false;
                    SystemStatus.RegistrationClient = null;
                }
            }
            Log("Registration Network Inactive");
        }

        public void ReadNetwork(object tc)
        {
            TcpClient client = tc as TcpClient;
            while ((state == ServiceState.Running || state == ServiceState.Starting) && client != null && client.Connected)
            {
                // do a read
                try
                {
                    string message = ServiceCommunication.ReadNetworkMessage(ref client);
                    // do something with data the provider sent us
                    if (message == "")
                    {
                        goto EndConnection;
                    }
                    else
                    {
                        NetworkMessage nm;
                        Log("Message received from network: " + message);
                        try
                        {
                            nm = JsonConvert.DeserializeObject<NetworkMessage>(message);
                        }
                        catch(Exception ex)
                        {
                            Log(message);
                            continue;
                        }
                        // the first message should probably be the type of message we're receiving
                        switch (nm.Type)
                        {
                            case MessageType.CancelRegistration:
                                {
                                    SystemStatus.AwaitingToken = false;
                                    break;
                                }
                            case MessageType.GetToken:
                                {
                                    // we don't need to do anything here except store this connection and wait for a ring swipe
                                    // SystemStatus.CredentialData.Client
                                    SystemStatus.RegistrationClient = client;
                                    SystemStatus.AwaitingToken = true;
                                    break;
                                }
                            case MessageType.RegisterToken:
                                {
                                    // save this token against this username to be selected from the list later
                                    RegisterToken(nm.Username, nm.Token, nm.TokenFriendlyName);
                                    break;
                                }
                            case MessageType.GetState:
                                {
                                    // return the current configuration for this user
                                    bool userfound = false;
                                    UserServerState uss = new UserServerState();
                                    if (ApplicationConfiguration.Users != null)
                                    {
                                        foreach (User u in ApplicationConfiguration.Users)
                                        {
                                            if (u.Username == nm.Username)
                                            {
                                                uss.UserConfiguration = u;
                                                // also need to send a list of plugins.
                                                userfound = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (!userfound)
                                        uss.UserConfiguration = new User();
                                    uss.Plugins = new List<PluginInfo>();
                                    foreach(Lazy<INFCRingServicePlugin> p in plugins)
                                    {
                                        uss.Plugins.Add(new PluginInfo() { Name = p.Value.GetPluginName(), Parameters = p.Value.GetParameters() });
                                    }
                                    ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(uss));
                                    break;
                                }
                            case MessageType.Message:
                                {
                                    Log(nm.Message);
                                    break;
                                }
                            case MessageType.AssociatePluginToToken:
                                {
                                    // get a plugin name, token, and if this plugin requires a credential, do the provider swap and lock the pc
                                    RegisterCredential(nm.Username, nm.Password, nm.Token, nm.PluginName);
                                    break;
                                }
                            case MessageType.UserCredential:
                                {
                                    //Log(message);
                                    Log("Credential received");
                                    if (SystemStatus.RegistrationClient != null && SystemStatus.RegistrationClient.Connected)
                                    {
                                        TcpClient otc = SystemStatus.RegistrationClient;
                                        ServiceCommunication.SendNetworkMessage(ref otc, JsonConvert.SerializeObject(new NetworkMessage(MessageType.UserCredential) { Username = nm.Username, Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(nm.Password)) }));
                                        SystemStatus.RegistrationClient = otc;
                                    }
                                    // so this is where the user registration credential provider sends an actual user credential 
                                    break;
                                }
                            case MessageType.Delete:
                                {
                                    Log("Deleting item");
                                    if(String.IsNullOrEmpty(nm.Username))
                                    {
                                        break; // no username? lets not modify the config
                                    }
                                    if(!String.IsNullOrEmpty(nm.Token) && !String.IsNullOrEmpty(nm.PluginName))
                                    {
                                        // delete an event
                                        RemoveEvent(nm.Username, nm.Token, nm.PluginName);
                                    }
                                    else if(!String.IsNullOrEmpty(nm.Token))
                                    {
                                        // delete a token entirely (this will also delete all its events)
                                        RemoveToken(nm.Username, nm.Token);
                                    }
                                    break;
                                }
                            case MessageType.RegisterAll:
                                {
                                    Log("Registering new token against all plugins");
                                    string dht = RegisterToken(nm.Username, nm.Token, nm.TokenFriendlyName);
                                    foreach(Lazy<INFCRingServicePlugin> p in plugins)
                                    {
                                        RegisterCredential(nm.Username, nm.Password, dht, p.Value.GetPluginName());
                                    }
                                    break;
                                }
                            case MessageType.UpdateFriendlyName:
                                {
                                    Log("Update token friendly name");

                                    UpdateFriendlyName(nm);

                                    break;
                                }
                            default:
                                // failed
                                Log("Unknown network message received: " + message);
                                break;                            
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log("TCP Client disconnected");
                    if (client.Connected)
                        client.Close();
                }
            }
        EndConnection:
            Log("TCP Client network loop ended");
            if (client.Connected)
                client.Close();
            client = null;

            if (SystemStatus.RegistrationClient == null || !SystemStatus.RegistrationClient.Connected)
                SystemStatus.AwaitingToken = false;
            if (SystemStatus.CredentialData.Client == null || !SystemStatus.CredentialData.Client.Connected)
                SystemStatus.CredentialData.ProviderActive = false;
        }

        private void UpdateFriendlyName(NetworkMessage networkMessage)
        {
            var token = networkMessage.Token;

            var isUpdated = false;

            if (ApplicationConfiguration.Users != null)
            {
                foreach (var user in ApplicationConfiguration.Users)
                {
                    var existToken = user.Tokens.FirstOrDefault(x => x.Key == token);
                    if (Equals(existToken, default(KeyValuePair<string, string>)))
                        continue;

                    user.Tokens[token] = networkMessage.TokenFriendlyName;
                    isUpdated = true;
                    break;
                }
            }

            if (isUpdated)
                SaveConfig();
        }

        private bool LoadConfig()
        {
            // read in our JSON file
            // decrypt?
            // store this in an object of some kind
            try
            {
                if (File.Exists(appPath + @"\Application.config"))
                {
                    string sc = File.ReadAllText(appPath + @"\Application.config");
                    ApplicationConfiguration = JsonConvert.DeserializeObject<Config>(sc);
                    Log("Configuration loaded from " + appPath + @"\Application.config");
                    return true;
                }
                else
                {
                    Log("No configuration file to read");
                    ApplicationConfiguration = new Config();
                    ApplicationConfiguration.Users = new List<User>();
                    ApplicationConfiguration.Users.Add(new User()
                    {
                        Username = GetCurrentUsername(),
                        Events = new List<Event>(),
                        Salt = new Random().Next(1000000, 9999999).ToString(),
                        Tokens = new Dictionary<string, string>()
                    });
                    return true;
                }
            }
            catch(Exception ex)
            {
                Log("Failed to read application config: " + ex.Message);
            }
            return false;
        }

        private bool SaveConfig()
        {
            File.WriteAllText(appPath + @"\Application.config", JsonConvert.SerializeObject(ApplicationConfiguration, Formatting.Indented));
            Log("Configuration saved to " + appPath + @"\Application.config");
            return true;
        }

        private void RemoveToken(string user, string token)
        {
            //string hashedToken = Crypto.Hash(rawToken);

            foreach (User u in ApplicationConfiguration.Users)
            {
                //string token = Crypto.Hash(hashedToken + u.Salt);
                if (u.Username.ToLower() == user.ToLower())
                {
                    if (u.Tokens.ContainsKey(token))
                        u.Tokens.Remove(token);
                    List<Event> remove = new List<Event>();
                    foreach(Event e in u.Events)
                    {
                        if (e.Token == token)
                            remove.Add(e);
                    }
                    foreach(Event e in remove)
                    {
                        u.Events.Remove(e);
                    }
                }
            }
            Log("Token deregistered");
            SaveConfig();
        }

        private void RemoveEvent(string user, string token, string pluginName)
        {
            foreach (User u in ApplicationConfiguration.Users)
            {
                if (u.Username.ToLower() == user.ToLower())
                {
                    List<Event> remove = new List<Event>();
                    foreach (Event e in u.Events)
                    {
                        if (e.Token == token && e.PluginName == pluginName)
                            remove.Add(e);
                    }
                    foreach (Event e in remove)
                    {
                        u.Events.Remove(e);
                    }
                }
            }
            Log("Plugin deregistered");
            SaveConfig();

        }

        private string RegisterToken(string user, string rawToken, string name)
        {
            // hash the token
            // remove the token registered anywhere else
            User target = null;
            string hashedToken = Crypto.Hash(rawToken);

            if (ApplicationConfiguration.Users != null)
            {
                foreach(User u in ApplicationConfiguration.Users)
                {
                    if (u.Tokens.ContainsKey(Crypto.Hash(hashedToken + u.Salt)))
                        RemoveToken(u.Username, rawToken);
                    if (u.Username == user)
                        target = u;
                }
            }
            else
            {
                ApplicationConfiguration.Users = new List<User>();
            }
            if(target == null)
            {
                User u = new User();
                u.Username = user;
                u.Events = new List<Event>();
                u.Tokens = new Dictionary<string, string>();
                u.Salt = new Random().Next(1000000, 9999999).ToString();
                ApplicationConfiguration.Users.Add(u);
                target = u;
            }
            string dht = Crypto.Hash(hashedToken + target.Salt);
            Log("Token registered");
            target.Tokens.Add(dht, name);
            SaveConfig();
            return dht;
        }

        private void RegisterCredential(string user, string password, string tokenId, string pluginName)
        {
            string loggedInUser = GetCurrentUsername();
            string domain = "";
                // do some work
            //if(loggedInUser.ToLower() == user.ToLower())
            //{
                // username and domain
            domain = loggedInUser.Substring(0,loggedInUser.LastIndexOf('\\'));
            user = user.Replace(domain + @"\", "");
            //}
            if (loggedInUser.Substring(loggedInUser.LastIndexOf('\\')+1).ToLower() == user.ToLower())
            {
                // check to see if there is a domain?

                // password is already encoded
                foreach(User u in ApplicationConfiguration.Users)
                {
                    if(u.Username.ToLower() == GetCurrentUsername().ToLower())
                    {
                        Lazy<INFCRingServicePlugin> lp = plugins.Where(x => x.Value.GetPluginName() == pluginName).FirstOrDefault();
                        if (lp == null)
                            break;
                        Dictionary<string, object> p = new Dictionary<string, object>();
                        if (lp.Value.GetParameters().Where(y => y.Name == "Username").FirstOrDefault() != null)
                            p.Add("Username", user);
                        if (lp.Value.GetParameters().Where(y => y.Name == "Password").FirstOrDefault() != null)
                            p.Add("Password", password);
                        if (lp.Value.GetParameters().Where(y => y.Name == "Domain").FirstOrDefault() != null)
                            p.Add("Domain", domain);

                        u.Events.Add(new Event()
                        {
                            PluginName = pluginName,
                            Token = tokenId,
                            Parameters = p
                        });
                        break;
                    }
                }
            }
            SaveConfig();
            // make the registration credential provider not active on the system anymore
            //UseNFCCredential();
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

        private string GetCurrentUsername()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            return (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
        }
    }
}
