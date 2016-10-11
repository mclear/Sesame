namespace CredentialRegistration
{
    partial class frmRegistrationApp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnAddToken = new System.Windows.Forms.Button();
            this.tvwConfig = new System.Windows.Forms.TreeView();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnAddEvent = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(450, 416);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(145, 21);
            this.button1.TabIndex = 0;
            this.button1.Text = "Read NFC Tag";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(60, 417);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(382, 20);
            this.textBox1.TabIndex = 4;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(448, 472);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(147, 21);
            this.button2.TabIndex = 2;
            this.button2.Text = "Encrypt Password";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(60, 472);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(382, 21);
            this.textBox2.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-8, 420);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "NFC Tag ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-1, 447);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Username";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(60, 444);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(382, 20);
            this.textBox3.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1, 476);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Password";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(60, 508);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(95, 33);
            this.button3.TabIndex = 3;
            this.button3.Text = "Enrol";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(99, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 13);
            this.label4.TabIndex = 8;
            // 
            // btnAddToken
            // 
            this.btnAddToken.Location = new System.Drawing.Point(491, 20);
            this.btnAddToken.Name = "btnAddToken";
            this.btnAddToken.Size = new System.Drawing.Size(128, 23);
            this.btnAddToken.TabIndex = 9;
            this.btnAddToken.Text = "Add Token";
            this.btnAddToken.UseVisualStyleBackColor = true;
            this.btnAddToken.Click += new System.EventHandler(this.btnNetworkRegistrationStart_Click);
            // 
            // tvwConfig
            // 
            this.tvwConfig.Location = new System.Drawing.Point(12, 12);
            this.tvwConfig.Name = "tvwConfig";
            this.tvwConfig.Size = new System.Drawing.Size(462, 286);
            this.tvwConfig.TabIndex = 10;
            this.tvwConfig.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwConfig_AfterSelect);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 304);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(625, 101);
            this.txtLog.TabIndex = 11;
            // 
            // btnAddEvent
            // 
            this.btnAddEvent.Enabled = false;
            this.btnAddEvent.Location = new System.Drawing.Point(491, 49);
            this.btnAddEvent.Name = "btnAddEvent";
            this.btnAddEvent.Size = new System.Drawing.Size(128, 23);
            this.btnAddEvent.TabIndex = 12;
            this.btnAddEvent.Text = "Add Event";
            this.btnAddEvent.UseVisualStyleBackColor = true;
            this.btnAddEvent.Click += new System.EventHandler(this.btnAddEvent_Click);
            // 
            // frmRegistrationApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(649, 411);
            this.Controls.Add(this.btnAddEvent);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.tvwConfig);
            this.Controls.Add(this.btnAddToken);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Name = "frmRegistrationApp";
            this.Text = "NFC Ring Credential Provider Registration";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnAddToken;
        private System.Windows.Forms.TreeView tvwConfig;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnAddEvent;
    }
}

