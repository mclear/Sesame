namespace CredentialRegistration
{
    partial class frmEventRegistration
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
            this.cboTokens = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboPlugins = new System.Windows.Forms.ComboBox();
            this.dgvParameters = new System.Windows.Forms.DataGridView();
            this.dgcName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgcIsOptional = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgcValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pgbAwaitingToken = new System.Windows.Forms.ProgressBar();
            this.lblSwipeEncrypt = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParameters)).BeginInit();
            this.SuspendLayout();
            // 
            // cboTokens
            // 
            this.cboTokens.FormattingEnabled = true;
            this.cboTokens.Location = new System.Drawing.Point(100, 22);
            this.cboTokens.Name = "cboTokens";
            this.cboTokens.Size = new System.Drawing.Size(427, 21);
            this.cboTokens.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select Token:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Select Plugin:";
            // 
            // cboPlugins
            // 
            this.cboPlugins.FormattingEnabled = true;
            this.cboPlugins.Location = new System.Drawing.Point(100, 49);
            this.cboPlugins.Name = "cboPlugins";
            this.cboPlugins.Size = new System.Drawing.Size(427, 21);
            this.cboPlugins.TabIndex = 2;
            this.cboPlugins.SelectedValueChanged += new System.EventHandler(this.cboPlugins_SelectedValueChanged);
            // 
            // dgvParameters
            // 
            this.dgvParameters.AllowUserToAddRows = false;
            this.dgvParameters.AllowUserToDeleteRows = false;
            this.dgvParameters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvParameters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgcName,
            this.dgcIsOptional,
            this.dgcValue});
            this.dgvParameters.Location = new System.Drawing.Point(12, 76);
            this.dgvParameters.MultiSelect = false;
            this.dgvParameters.Name = "dgvParameters";
            this.dgvParameters.Size = new System.Drawing.Size(515, 246);
            this.dgvParameters.TabIndex = 4;
            this.dgvParameters.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgvParameters_EditingControlShowing);
            // 
            // dgcName
            // 
            this.dgcName.HeaderText = "Name";
            this.dgcName.Name = "dgcName";
            this.dgcName.ReadOnly = true;
            // 
            // dgcIsOptional
            // 
            this.dgcIsOptional.HeaderText = "Is Optional";
            this.dgcIsOptional.Name = "dgcIsOptional";
            this.dgcIsOptional.ReadOnly = true;
            // 
            // dgcValue
            // 
            this.dgcValue.HeaderText = "Value";
            this.dgcValue.Name = "dgcValue";
            this.dgcValue.Width = 300;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(371, 328);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(452, 328);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // pgbAwaitingToken
            // 
            this.pgbAwaitingToken.Location = new System.Drawing.Point(12, 328);
            this.pgbAwaitingToken.Name = "pgbAwaitingToken";
            this.pgbAwaitingToken.Size = new System.Drawing.Size(316, 23);
            this.pgbAwaitingToken.TabIndex = 7;
            this.pgbAwaitingToken.Visible = false;
            // 
            // lblSwipeEncrypt
            // 
            this.lblSwipeEncrypt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSwipeEncrypt.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSwipeEncrypt.Location = new System.Drawing.Point(12, 76);
            this.lblSwipeEncrypt.Name = "lblSwipeEncrypt";
            this.lblSwipeEncrypt.Size = new System.Drawing.Size(515, 246);
            this.lblSwipeEncrypt.TabIndex = 8;
            this.lblSwipeEncrypt.Text = "Swipe your token again to encrypt password";
            this.lblSwipeEncrypt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSwipeEncrypt.Visible = false;
            // 
            // frmEventRegistration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 357);
            this.Controls.Add(this.pgbAwaitingToken);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboPlugins);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboTokens);
            this.Controls.Add(this.lblSwipeEncrypt);
            this.Controls.Add(this.dgvParameters);
            this.Name = "frmEventRegistration";
            this.Text = "EventRegistration";
            ((System.ComponentModel.ISupportInitialize)(this.dgvParameters)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboTokens;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboPlugins;
        private System.Windows.Forms.DataGridView dgvParameters;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgcName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgcIsOptional;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgcValue;
        private System.Windows.Forms.ProgressBar pgbAwaitingToken;
        private System.Windows.Forms.Label lblSwipeEncrypt;
    }
}