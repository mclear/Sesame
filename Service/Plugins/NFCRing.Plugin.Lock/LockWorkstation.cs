using Microsoft.Win32;
using NFCRing.Service.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NFCRing.Plugin.Lock
{
    [Export(typeof(INFCRingServicePlugin))]
    public class LockWorkstation : INFCRingServicePlugin
    {
        public void NCFRingDown(string id)
        {
            try
            {
                // check that this ID is registered for the credential provider
                RegistryKey key = OpenKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}");

                // i guess the credential provider isn't installed or we're not running as admin
                if (key == null)
                    return;

                SHA1Managed sm = new SHA1Managed();
                // add salt. this is dumb
                byte[] hash = sm.ComputeHash(System.Text.Encoding.ASCII.GetBytes(id + "02164873"));
                string hash1 = HashToHex(hash);
                string newKeyName = HashToHex(sm.ComputeHash(System.Text.Encoding.ASCII.GetBytes(hash1)));

                if (key.OpenSubKey(newKeyName) == null)
                {
                    NFCRing.Service.Core.ServiceCore.Log("LockWorkstationPlugin: Unknown token");
                    return;
                }
                NFCRing.Service.Core.ServiceCore.Log("LockWorkstationPlugin: Found token");
                ProcessAsUser.Launch(@"C:\WINDOWS\system32\rundll32.exe user32.dll,LockWorkStation");
            }
            catch(Exception ex)
            {
                NFCRing.Service.Core.ServiceCore.Log("LockWorkstationPlugin: Exception thrown: " + ex.Message);
            }
        }

        public void NCFRingUp(string id)
        {
            // we just want ring down for now
            return;
        }

        public void NFCRingDataRead(string id, byte[] data)
        {
            // not using data at this stage
            return;
        }

        public string HashToHex(byte[] hash)
        {
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sBuilder.Append(hash[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public RegistryKey OpenKey(string path)
        {
            // should we accept HKLM or only HKEY_LOCAL_MACHINE?
            string[] parts = path.Split('\\');
            if (parts.Length == 0)
            {
                return null;
            }
            RegistryKey hive;
            switch (parts[0].ToUpper())
            {
                case "HKEY_LOCAL_MACHINE":
                    hive = Registry.LocalMachine;
                    break;
                case "HKEY_CURRENT_USER":
                    hive = Registry.CurrentUser;
                    break;
                case "HKEY_USERS":
                    hive = Registry.Users;
                    break;
                default:
                    return null;
            }
            bool skip = true;
            foreach (string name in parts)
            {
                if (skip)
                {
                    skip = false;
                    continue;
                }
                hive = hive.OpenSubKey(name, true);
                if (hive == null)
                    return null;
            }
            return hive;
        }

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
                    NFCRing.Service.Core.ServiceCore.Log("LockWorkstationPlugin: Error " + message);
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
                    NFCRing.Service.Core.ServiceCore.Log("LockWorkstationPlugin: " + details);

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
                        NFCRing.Service.Core.ServiceCore.Log("LockWorkstationPlugin: " + message);
                        //Debug.WriteLine(message);
                    }

                }

                else
                {

                    string message = String.Format("OpenProcessToken Error: {0}", Marshal.GetLastWin32Error());
                    NFCRing.Service.Core.ServiceCore.Log("LockWorkstationPlugin: " + message);
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
                    NFCRing.Service.Core.ServiceCore.Log("LockWorkstationPlugin: " + message);
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
                        if(!ret)
                        {
                            NFCRing.Service.Core.ServiceCore.Log("LockWorkstationPlugin: lock failed");
                        }
                        if (envBlock != IntPtr.Zero)
                            DestroyEnvironmentBlock(envBlock);

                        CloseHandle(token);
                    }

                }
                else
                {
                    NFCRing.Service.Core.ServiceCore.Log("LockWorkstationPlugin: process not found");
                }
                return ret;
            }

        } 
    }
}


