using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;
using System.Net.Sockets;
using System.Net;
using NFCRing.Service.Common;
using Newtonsoft.Json;

namespace CredentialRegistration
{
    public partial class frmRegistrationApp : Form
    {

        [DllImport("WinAPIWrapper", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PCSC_GetID([In, Out] IntPtr id, [In, Out] IntPtr err);
        //[DllImport("WinAPIWrapper", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int CredProtectWrapper(IntPtr inputBuffer, int inputLength, [In, Out]IntPtr outputBuffer);

        TcpClient client;
        UserServerState uss;

        public frmRegistrationApp()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label4.Text = "";
            IntPtr idloc = Marshal.AllocHGlobal(100);
            IntPtr errloc = Marshal.AllocHGlobal(100);
            int len = PCSC_GetID(idloc, errloc);
            switch(len)
            {
                case 0: label4.Text = "No data read " + Marshal.PtrToStringAnsi(errloc);
                    break;
                case -1: label4.Text = "context could not be established " + Marshal.PtrToStringAnsi(errloc);
                    break;
                case -2: label4.Text = "could not list readers " + Marshal.PtrToStringAnsi(errloc);
                    break;
                case -3: label4.Text = "could not connect to card " + Marshal.PtrToStringAnsi(errloc);
                    break;
                case -4: label4.Text = "could not read from card " + Marshal.PtrToStringAnsi(errloc);
                    break;
                case -5: label4.Text = "reader unavailable " + Marshal.PtrToStringAnsi(errloc);
                    break;
                case -6: label4.Text = "no readers available " + Marshal.PtrToStringAnsi(errloc);
                    break;
                case -111: label4.Text = "a reader threw a critical exception";
                    break;
                default:
                    {
                        string id = Marshal.PtrToStringAnsi(idloc, len);
                        textBox1.Text = id;
                    }
                    break;
            }
            Marshal.FreeHGlobal(idloc);
            Marshal.FreeHGlobal(errloc);
        }

        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream. 
            return encrypted;

        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold 
            // the decrypted text. 
            string plaintext = null;

            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream 
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //using (RijndaelManaged myRijndael = new RijndaelManaged())
            //{
            //    myRijndael.BlockSize = 128;
            //    myRijndael.FeedbackSize = 128;
            //    myRijndael.Padding = PaddingMode.ANSIX923;
            //    myRijndael.Mode = CipherMode.CBC;
            //    myRijndael.Key = System.Text.Encoding.ASCII.GetBytes(textBox1.Text);
            //    myRijndael.GenerateIV();
            //    // Encrypt the string to an array of bytes. 
            //    byte[] encrypted = EncryptStringToBytes(textBox2.Text, myRijndael.Key, myRijndael.IV);
            //    string encoded = Convert.ToBase64String(encrypted);
            //    // Decrypt the bytes to a string. 
            //    string roundtrip = DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);

            //    //Display the original data and the decrypted data.
            //    Console.WriteLine("Original:   {0}", textBox2.Text);
            //    Console.WriteLine("Round Trip: {0}", roundtrip);
            //}

            //using (NFCRingRPC.RPCClient client = new NFCRingRPC.RPCClient())
            //{
            //    textBox2.Text = client.CredProtect(textBox2.Text);
            //}

            //IntPtr result = Marshal.AllocHGlobal(500);
            //IntPtr instring = Marshal.StringToHGlobalUni(textBox1.Text);
            //int res = CredProtectWrapper(instring, textBox1.Text.Length, result);
            //textBox2.Text = Marshal.PtrToStringUni(result);
            //Marshal.FreeHGlobal(instring);
            //Marshal.FreeHGlobal(result);
        }

        public bool WriteCredToRegistry(string id, string username, string password, string description = "")
        {
            RegistryKey key = OpenKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}");

            // i guess the credential provider isn't installed or we're not running as admin
            if (key == null)
            {
                MessageBox.Show("This tool needs to be run as an administrator.");
                return false;
            }
            
            SHA1Managed sm = new SHA1Managed();
            // add salt. this is dumb
            byte[] hash = sm.ComputeHash(System.Text.Encoding.ASCII.GetBytes(id + "02164873"));
            string hash1 = HashToHex(hash);
            string newKeyName = HashToHex(sm.ComputeHash(System.Text.Encoding.ASCII.GetBytes(hash1)));
            key = key.CreateSubKey(newKeyName);
            if (key == null)
                return false;
            key.SetValue("User", username, RegistryValueKind.String);
            key.SetValue("Data", password, RegistryValueKind.String);
            if (description != "")
                key.SetValue("Name", description, RegistryValueKind.String);
            return true;
        }

        public RegistryKey OpenKey(string path)
        {
            try
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
            catch(Exception ex)
            {
                return null;
            }
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

        private void button3_Click(object sender, EventArgs e)
        {
            if (WriteCredToRegistry(textBox1.Text, textBox3.Text, textBox2.Text))
            {
                textBox1.Text = "";
                textBox3.Text = "";
                textBox2.Text = "";
                MessageBox.Show("Credential saved");
            }
            else
                MessageBox.Show("Enrolment failed");
        }

        private void btnNetworkRegistrationStart_Click(object sender, EventArgs e)
        {
            if(ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.GetToken))) > 0)
            {
                frmNewToken nt = new frmNewToken(ref client);
                nt.ShowDialog();
                LoadConfig();
            }
        }

        private void Log(string message)
        {
            txtLog.Text = DateTime.Now.ToShortTimeString() + " - " + message + Environment.NewLine + txtLog.Text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }

        private void tvwConfig_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string tag = (string)tvwConfig.SelectedNode.Tag;
            if(tag == "Event")
            {
                btnDeleteEvent.Enabled = true;
            }
            else
            {
                btnDeleteEvent.Enabled = false;
            }
            if (tag == "Token")
            {
                btnDeleteToken.Enabled = true;
            }
            else
            {
                btnDeleteToken.Enabled = false;
            }
        }

        private void LoadConfig()
        {
            ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.GetState) { Username = ClientCommon.GetCurrentUsername() }));
            var task = Task<string>.Factory.StartNew(() =>
            {
                return ServiceCommunication.ReadNetworkMessage(ref client);
            });
            if (task.Result == "")
            {
                if (MessageBox.Show("Unable to connect to service. Please make sure its running", "NFC Ring Service Registration App", MessageBoxButtons.RetryCancel) == DialogResult.Cancel)
                {
                    Application.Exit();
                }
                // RETRY ISNT GOING TO DO ANYTHING YET
            }
            else
            {
                tvwConfig.Nodes.Clear();
                uss = JsonConvert.DeserializeObject<UserServerState>(task.Result);
                TreeNode user = tvwConfig.Nodes.Add(uss.UserConfiguration.Username);
                TreeNode tokens = user.Nodes.Add("Tokens");
                if(uss.UserConfiguration.Tokens != null)
                {
                    foreach (KeyValuePair<string, string> tok in uss.UserConfiguration.Tokens)
                    {

                        TreeNode tokenNode = tokens.Nodes.Add(tok.Value + " - " + tok.Key);
                        tokenNode.Tag = "Token";
                    }
                }
                TreeNode plugins = user.Nodes.Add("Plugins");
                if (uss.Plugins != null)
                {
                    foreach (PluginInfo p in uss.Plugins)
                    {
                        TreeNode aPlugin = plugins.Nodes.Add(p.Name);
                        aPlugin.Tag = "Plugin";
                    }
                }
                TreeNode events = user.Nodes.Add("Events");
                if (uss.UserConfiguration.Events != null)
                {
                    foreach (Event ev in uss.UserConfiguration.Events)
                    {
                        TreeNode eventNode = events.Nodes.Add(ev.PluginName + " - " + ev.Token);
                        eventNode.Tag = "Event";
                        foreach (KeyValuePair<string, object> param in ev.Parameters)
                        {
                            if (param.Value != null)
                            {
                                TreeNode e = eventNode.Nodes.Add(param.Key + " = " + param.Value.ToString());
                                //e.Tag = "Event";
                            }
                        }
                    }
                }
                if((uss.UserConfiguration.Tokens != null && uss.UserConfiguration.Tokens.Count > 0) && (uss.Plugins != null && uss.Plugins.Count > 0))
                {
                    btnAddEvent.Enabled = true;
                }
                tvwConfig.ExpandAll();
            }
        }

        private void btnAddEvent_Click(object sender, EventArgs e)
        {
            frmEventRegistration er = new frmEventRegistration(ref client, uss.UserConfiguration.Tokens, uss.Plugins);
            er.ShowDialog();
            LoadConfig();
        }

        private void btnDeleteToken_Click(object sender, EventArgs e)
        {
            if(tvwConfig.SelectedNode.Tag.ToString() == "Token")
            {
                string tokenId = uss.UserConfiguration.Tokens.Keys.ToList()[tvwConfig.SelectedNode.Index];
                NetworkMessage nm = new NetworkMessage(MessageType.Delete) { Username = uss.UserConfiguration.Username, Token = tokenId };
                ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(nm));
                LoadConfig();
            }
            btnDeleteToken.Enabled = false;
        }

        private void btnDeleteEvent_Click(object sender, EventArgs e)
        {
            if (tvwConfig.SelectedNode.Tag.ToString() == "Event")
            {
                Event ev = uss.UserConfiguration.Events[tvwConfig.SelectedNode.Index];
                NetworkMessage nm = new NetworkMessage(MessageType.Delete) { Username = uss.UserConfiguration.Username, Token = ev.Token, PluginName = ev.PluginName };
                ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(nm));
                LoadConfig();
            }
            btnDeleteEvent.Enabled = false;
        }
    }
}
