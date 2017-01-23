using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NFCRing.Service.Common;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace CredentialRegistration
{
    public partial class frmEventRegistration : Form
    {
        List<PluginInfo> plugins = new List<PluginInfo>();
        TcpClient client;
        Dictionary<string, string> tokens = new Dictionary<string, string>();

        public frmEventRegistration(ref TcpClient client, Dictionary<string, string> tokens, List<PluginInfo> plugins)
        {
            InitializeComponent();
            this.plugins = plugins;
            this.client = client;
            foreach(KeyValuePair<string, string> token in tokens)
            {
                cboTokens.Items.Add(token.Value + " - " + token.Key);
            }
            if(tokens.Count == 1)
            {
                cboTokens.SelectedIndex = 0;
            }
            foreach(PluginInfo pi in plugins)
            {
                cboPlugins.Items.Add(pi.Name);
            }
            if(plugins.Count == 1)
            {
                cboPlugins.SelectedItem = 0;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cboPlugins_SelectedValueChanged(object sender, EventArgs e)
        {
            dgvParameters.Rows.Clear();
            foreach (PluginInfo pi in plugins)
            {
                if (pi.Name == (string)cboPlugins.SelectedItem)
                {
                    foreach (Parameter p in pi.Parameters)
                    {
                        dgvParameters.Rows.Add(new object[] { p.Name, p.IsOptional, p.Default });
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            NetworkMessage nm = new NetworkMessage(MessageType.AssociatePluginToToken);
            if (cboTokens.SelectedItem.ToString() == "")
            {
                MessageBox.Show("Select a token to register");
                return;
            }
            else
            {
                nm.Token = cboTokens.SelectedItem.ToString().Split('-')[1].Trim();
            }
            if (cboPlugins.SelectedItem.ToString() == "")
            {
                MessageBox.Show("Select a plugin to register an event for.");
                return;
            }
            else
            {
                nm.PluginName = cboPlugins.SelectedItem.ToString();
            }
            foreach (DataGridViewRow dgvr in dgvParameters.Rows)
            {
                if((bool)dgvr.Cells["dgcIsOptional"].Value == false)
                {
                    if(dgvr.Cells["dgcValue"].Value.ToString() == "")
                    {
                        MessageBox.Show("Fill in the required paramters or go home");
                        return;
                    }
                    else
                    {
                        if(dgvr.Cells["dgcName"].Value.ToString() == "Username")
                        {
                            nm.Username = dgvr.Cells["dgcValue"].Value.ToString(); // this should be a list instead of specific properties
                        }
                        else if (dgvr.Cells["dgcName"].Value.ToString() == "Password")
                        {
                            nm.Password = NFCRing.Service.Common.Crypto.Encrypt(dgvr.Cells["dgcValue"].Value.ToString(), nm.Token);
                        }
                    }
                }
            }
            if (ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(nm)) > 0)
                this.Close();
        }

        private void btnGetPassword_Click(object sender, EventArgs e)
        {
            ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.GetUserCredential)));
            string result = ServiceCommunication.ReadNetworkMessage(ref client);
            NetworkMessage nm = JsonConvert.DeserializeObject<NetworkMessage>(result);
            if (result == "")
            {
                MessageBox.Show("Failed to get credential");
            }
            else
            {
                if (nm.Type == MessageType.UserCredential)
                {
                    // set the parameter values in the data grid view
                    foreach (DataGridViewRow dgvr in dgvParameters.Rows)
                    {
                        if (dgvr.Cells["dgcName"].Value.ToString() == "Username")
                        {
                            dgvr.Cells["dgcValue"].Value = nm.Username;
                        }
                        else if (dgvr.Cells["dgcName"].Value.ToString() == "Password")
                        {
                            dgvr.Cells["dgcValue"].Value = nm.Password;
                        }
                    }
                }
            }
        }
    }
}
