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

namespace NFCRing.Service.Core
{
    public class ServiceCore
    {
        private static bool _debug = false;
        private CompositionContainer container;
        protected static string appPath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
        private ServiceState state = ServiceState.Stopped;
        public static SystemState SystemStatus = new SystemState();
        private Config ApplicationConfiguration = new Config();

        string user1 = "";

        TcpListener credentialListener;
        Thread credentialListenThread;
        TcpListener registrationListener;
        Thread registrationListenThread;
        bool runListenLoops = false;
        //private List<Client> Connections = new List<Client>();

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
                //    Log("Read error: " + error);

                Marshal.FreeHGlobal(idloc);
                Marshal.FreeHGlobal(errloc);

                // check the id of the token
                if (currentId == "" && id != "")
                {
                    Log("NFCTagDownEvent");
                    currentId = id;
                    // we just got a new token (state change)

                    // load parameters from config
                    if (SystemStatus.AwaitingToken)
                    {
                        // this is where we capture it and show the next screen
                        if(SystemStatus.RegistrationClient != null)
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
                            foreach (Event e in u.Events)
                            {
                                if (id == e.Token)
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
                else if (currentId != "" && id == "")
                {
                    Log("NFCTagUpEvent");
                    string origId = currentId;
                    currentId = "";
                    // we just lost the token (state change)
                    if (SystemStatus.AwaitingToken)
                    {
                        // they just lifted it. reset this
                        SystemStatus.AwaitingToken = false;
                    }
                    else
                    {
                        // check config
                        foreach(User u in ApplicationConfiguration.Users)
                        {
                            foreach(Event e in u.Events)
                            {
                                if (origId == e.Token)
                                {
                                    foreach (Lazy<INFCRingServicePlugin> plugin in plugins)
                                    {
                                        if (plugin.Value.GetPluginName() == e.PluginName)
                                        {
                                            plugin.Value.NCFRingUp(origId, e.Parameters, SystemStatus);
                                            Log("Plugin " + plugin.Value.GetPluginName() + " passed TagUp event");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // sleep for configured delay?
                Thread.Sleep(100);
            }
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
            //foreach (Client tc in Connections)
            //{
            //    try
            //    {
            //        if (tc.ClientConnection.Connected)
            //            tc.ClientConnection.Close();
            //        if (tc.ClientProcess.IsAlive)
            //            tc.ClientProcess.Join();
            //    }
            //    catch(Exception ex)
            //    {
            //        Log("TCP error stopping connection: " + ex.Message);
            //    }
            //}
            //Connections.Clear();
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
                            case MessageType.GetUserCredential:
                                {
                                    // this is where the second custom credential provider steals the encrypted username and password and sends it to the service
                                    UseRegistrationCredential();
                                    LockWorkstation();
                                    System.Threading.Thread.Sleep(500);
                                    UseNFCCredential();
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
                                    UseNFCCredential(); // make super sure we're back to the real logins
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
                Log("No configuration file to read");
            }
            catch(Exception ex)
            {
                Log("Failed to read application config: " + ex.Message);
            }
            return false;
        }

        private bool SaveConfig()
        {
            File.WriteAllText(appPath + @"\Application.config", JsonConvert.SerializeObject(ApplicationConfiguration));
            Log("Configuration saved to " + appPath + @"\Application.config");
            return true;
        }

        private void RemoveToken(string user, string token)
        {
            foreach (User u in ApplicationConfiguration.Users)
            {
                if(u.Username.ToLower() == user.ToLower())
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

        private void RegisterToken(string user, string id, string name)
        {
            // remove the token registered anywhere else
            User target = null;
            if(ApplicationConfiguration.Users != null)
            {
                foreach(User u in ApplicationConfiguration.Users)
                {
                    if (u.Tokens.ContainsKey(id))
                        RemoveToken(u.Username, id);
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
            Log("Token registered");
            target.Tokens.Add(id, name);
            SaveConfig();
        }

        private void RegisterCredential(string user, string password, string tokenId, string pluginName)
        {
                // do some work
            if(GetCurrentUsername().Substring(GetCurrentUsername().LastIndexOf('\\')+1).ToLower() == user.ToLower())
            {
                // password is already encoded
                foreach(User u in ApplicationConfiguration.Users)
                {
                    if(u.Username.ToLower() == GetCurrentUsername().ToLower())
                    {
                        u.Events.Add(new Event()
                        {
                            PluginName = pluginName,
                            Token = tokenId,
                            Parameters = new Dictionary<string, object>() {{ "Username", GetCurrentUsername() }, { "Password", password }}
                        });
                        break;
                    }
                }
            }
            // make the registration credential provider not active on the system anymore
            //UseNFCCredential();
        }

        private void LockWorkstation()
        {
            // lock the machine
            ProcessAsUser.Launch(@"C:\WINDOWS\system32\rundll32.exe user32.dll,LockWorkStation");
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

        private void UseRegistrationCredential()
        {
            // this will remove the NFC credential and add the registration one and its filter
            // forcing people to login via the capture one
            
            Process regeditProcess = Process.Start("regedit.exe", "/s \"" + appPath + "\\RegisterUser.reg\"");
            regeditProcess.WaitForExit();
            regeditProcess = Process.Start("regedit.exe", "/s \"" + appPath + "\\DeregisterNFC.reg\"");
            regeditProcess.WaitForExit();
        }

        private void UseNFCCredential()
        {
            // remvoe the registration credential and filter and add the NFC one
            // this will allow people to swipe to login again
            Process regeditProcess = Process.Start("regedit.exe", "/s \"" + appPath + "\\RegisterNFC.reg\"");
            regeditProcess.WaitForExit();
            regeditProcess = Process.Start("regedit.exe", "/s \"" + appPath + "\\DeregisterUser.reg\"");
            regeditProcess.WaitForExit();
        }

        private string GetCurrentUsername()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            return (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
        }

        #region LockWorkstation
        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }



        [StructLayout(LayoutKind.Sequential)]
        internal struct SECURITY_ATTRIBUTES
        {
            public uint nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;

        }

        internal enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        internal enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        public class ProcessAsUser
        {

            [DllImport("advapi32.dll", SetLastError = true)]
            private static extern bool CreateProcessAsUser(
                IntPtr hToken,
                string lpApplicationName,
                string lpCommandLine,
                ref SECURITY_ATTRIBUTES lpProcessAttributes,
                ref SECURITY_ATTRIBUTES lpThreadAttributes,
                bool bInheritHandles,
                uint dwCreationFlags,
                IntPtr lpEnvironment,
                string lpCurrentDirectory,
                ref STARTUPINFO lpStartupInfo,
                out PROCESS_INFORMATION lpProcessInformation);


            [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx", SetLastError = true)]
            private static extern bool DuplicateTokenEx(
                IntPtr hExistingToken,
                uint dwDesiredAccess,
                ref SECURITY_ATTRIBUTES lpThreadAttributes,
                Int32 ImpersonationLevel,
                Int32 dwTokenType,
                ref IntPtr phNewToken);


            [DllImport("advapi32.dll", SetLastError = true)]
            private static extern bool OpenProcessToken(
                IntPtr ProcessHandle,
                UInt32 DesiredAccess,
                ref IntPtr TokenHandle);

            [DllImport("userenv.dll", SetLastError = true)]
            private static extern bool CreateEnvironmentBlock(
                    ref IntPtr lpEnvironment,
                    IntPtr hToken,
                    bool bInherit);


            [DllImport("userenv.dll", SetLastError = true)]
            private static extern bool DestroyEnvironmentBlock(
                    IntPtr lpEnvironment);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool CloseHandle(
                IntPtr hObject);

            private const short SW_SHOW = 5;
            private const uint TOKEN_QUERY = 0x0008;
            private const uint TOKEN_DUPLICATE = 0x0002;
            private const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
            private const int GENERIC_ALL_ACCESS = 0x10000000;
            private const int STARTF_USESHOWWINDOW = 0x00000001;
            private const int STARTF_FORCEONFEEDBACK = 0x00000040;
            private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;


            private static bool LaunchProcessAsUser(string cmdLine, IntPtr token, IntPtr envBlock)
            {
                bool result = false;


                PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
                SECURITY_ATTRIBUTES saProcess = new SECURITY_ATTRIBUTES();
                SECURITY_ATTRIBUTES saThread = new SECURITY_ATTRIBUTES();
                saProcess.nLength = (uint)Marshal.SizeOf(saProcess);
                saThread.nLength = (uint)Marshal.SizeOf(saThread);

                STARTUPINFO si = new STARTUPINFO();
                si.cb = (uint)Marshal.SizeOf(si);


                //if this member is NULL, the new process inherits the desktop 
                //and window station of its parent process. If this member is 
                //an empty string, the process does not inherit the desktop and 
                //window station of its parent process; instead, the system 
                //determines if a new desktop and window station need to be created. 
                //If the impersonated user already has a desktop, the system uses the 
                //existing desktop. 

                si.lpDesktop = @"WinSta0\Default"; //Modify as needed 
                si.dwFlags = STARTF_USESHOWWINDOW | STARTF_FORCEONFEEDBACK;
                si.wShowWindow = SW_SHOW;
                //Set other si properties as required. 

                result = CreateProcessAsUser(
                    token,
                    null,
                    cmdLine,
                    ref saProcess,
                    ref saThread,
                    false,
                    CREATE_UNICODE_ENVIRONMENT,
                    envBlock,
                    null,
                    ref si,
                    out pi);


                if (result == false)
                {
                    int error = Marshal.GetLastWin32Error();
                    string message = String.Format("CreateProcessAsUser Error: {0}", error);
                    Log("ServiceLockWorkstation: Error " + message);
                    //Debug.WriteLine(message);

                }

                return result;
            }


            private static IntPtr GetPrimaryToken(int processId)
            {
                IntPtr token = IntPtr.Zero;
                IntPtr primaryToken = IntPtr.Zero;
                bool retVal = false;
                Process p = null;

                try
                {
                    p = Process.GetProcessById(processId);
                }

                catch (ArgumentException)
                {

                    string details = String.Format("ProcessID {0} Not Available", processId);
                    Log("ServiceLockWorkstation: " + details);

                    //Debug.WriteLine(details);
                    throw;
                }


                //Gets impersonation token 
                retVal = OpenProcessToken(p.Handle, TOKEN_DUPLICATE, ref token);
                if (retVal == true)
                {

                    SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                    sa.nLength = (uint)Marshal.SizeOf(sa);

                    //Convert the impersonation token into Primary token 
                    retVal = DuplicateTokenEx(
                        token,
                        TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_QUERY,
                        ref sa,
                        (int)SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
                        (int)TOKEN_TYPE.TokenPrimary,
                        ref primaryToken);

                    //Close the Token that was previously opened. 
                    CloseHandle(token);
                    if (retVal == false)
                    {
                        string message = String.Format("DuplicateTokenEx Error: {0}", Marshal.GetLastWin32Error());
                        Log("ServiceLockWorkstation: " + message);
                        //Debug.WriteLine(message);
                    }

                }

                else
                {

                    string message = String.Format("OpenProcessToken Error: {0}", Marshal.GetLastWin32Error());
                    Log("ServiceLockWorkstation: " + message);
                    //Debug.WriteLine(message);

                }

                //We'll Close this token after it is used. 
                return primaryToken;

            }

            private static IntPtr GetEnvironmentBlock(IntPtr token)
            {

                IntPtr envBlock = IntPtr.Zero;
                bool retVal = CreateEnvironmentBlock(ref envBlock, token, false);
                if (retVal == false)
                {

                    //Environment Block, things like common paths to My Documents etc. 
                    //Will not be created if "false" 
                    //It should not adversley affect CreateProcessAsUser. 

                    string message = String.Format("CreateEnvironmentBlock Error: {0}", Marshal.GetLastWin32Error());
                    Log("ServiceLockWorkstation: " + message);
                    //Debug.WriteLine(message);

                }
                return envBlock;
            }

            public static bool Launch(string appCmdLine /*,int processId*/)
            {

                bool ret = false;

                //Either specify the processID explicitly 
                //Or try to get it from a process owned by the user. 
                //In this case assuming there is only one explorer.exe 

                Process[] ps = Process.GetProcessesByName("explorer");
                int processId = -1;//=processId 
                if (ps.Length > 0)
                {
                    processId = ps[0].Id;
                }

                if (processId > 1)
                {
                    IntPtr token = GetPrimaryToken(processId);

                    if (token != IntPtr.Zero)
                    {

                        IntPtr envBlock = GetEnvironmentBlock(token);
                        ret = LaunchProcessAsUser(appCmdLine, token, envBlock);
                        if (!ret)
                        {
                            Log("ServiceLockWorkstation: lock failed");
                        }
                        if (envBlock != IntPtr.Zero)
                            DestroyEnvironmentBlock(envBlock);

                        CloseHandle(token);
                    }

                }
                else
                {
                    Log("ServiceLockWorkstation: process not found");
                }
                return ret;
            }

        }
        #endregion
    }
}
