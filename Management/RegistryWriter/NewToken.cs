using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NFCRing.Service.Common;
using Newtonsoft.Json;
using System.Reflection;

namespace CredentialRegistration
{

    public partial class frmNewToken : Form
    {
        TcpClient client;
        System.Threading.Timer t;
        public frmNewToken(ref TcpClient client)
        {
            this.client = client;
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if(t != null)
                t.Change(Timeout.Infinite, Timeout.Infinite); // stop the timer

            // tell the service we dont care anymore
            ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.CancelRegistration)));
            pgbAwaitingToken.Visible = false;
            pgbAwaitingToken.Value = 0;
            this.Close();
        }

        private void frmNewToken_Load(object sender, EventArgs e)
        {
            int value = 0;
            var task = Task<string>.Factory.StartNew(() => { return ServiceCommunication.ReadNetworkMessage(ref client); });
            t = new System.Threading.Timer((o) =>
            {
                Task<string> tsk = (Task<string>)o;
                if (tsk.IsCompleted)
                {
                    t.Change(Timeout.Infinite, Timeout.Infinite); // stop the timer
                    ClientCommon.SetControlPropertyThreadSafe(pgbAwaitingToken, "Visible", false);
                    if (tsk.Result != "")
                    {
                        ClientCommon.SetControlPropertyThreadSafe(txtToken, "Text", JsonConvert.DeserializeObject<NetworkMessage>(tsk.Result).Token);
                    }
                    else
                    {
                        // failed to scan a ring. 
                        Invoke(new Action(Close));
                    }
                }
                else
                {
                    // still waiting for it to be scanned
                    value += 7;
                    ClientCommon.SetControlPropertyThreadSafe(pgbAwaitingToken, "Value", value);
                }
            }, task, 1000, 1000);
            pgbAwaitingToken.Visible = true;
        }


        private void btnRegister_Click(object sender, EventArgs e)
        {
            // this should take the token ID, friendly name, and the current username (with domain) and send it to the service
            int res = ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.RegisterToken) { Token = txtToken.Text, TokenFriendlyName = txtFriendlyName.Text, Username = ClientCommon.GetCurrentUsername() }));
            if (res > 0)
                this.Close(); // we might have registered a token now?
        }
    }
}
