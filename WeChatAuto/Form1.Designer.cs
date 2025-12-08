
namespace WeChatAuto
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.buttonOut = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.textBoxAdd = new System.Windows.Forms.TextBox();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtServerPort = new System.Windows.Forms.TextBox();
            this.lblServerPort = new System.Windows.Forms.Label();
            this.txtServerIP = new System.Windows.Forms.TextBox();
            this.lblServerIP = new System.Windows.Forms.Label();
            this.lblHeartbeat = new System.Windows.Forms.Label();
            this.txtHeartbeat = new System.Windows.Forms.TextBox();
            this.lblHeartbeatUnit = new System.Windows.Forms.Label();
            this.lstInfo = new System.Windows.Forms.ListBox();
            this.txtMoney = new System.Windows.Forms.TextBox();
            this.lstName = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAmount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lstNet = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.label1.Location = new System.Drawing.Point(32, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(205, 29);
            this.label1.TabIndex = 6;
            this.label1.Text = "发送联系人列表：";
            // 
            // buttonOut
            // 
            this.buttonOut.Location = new System.Drawing.Point(189, 402);
            this.buttonOut.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonOut.Name = "buttonOut";
            this.buttonOut.Size = new System.Drawing.Size(99, 46);
            this.buttonOut.TabIndex = 10;
            this.buttonOut.Text = "移出列表";
            this.buttonOut.UseVisualStyleBackColor = true;
            this.buttonOut.Click += new System.EventHandler(this.buttonOut_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(37, 402);
            this.buttonAdd.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(99, 46);
            this.buttonAdd.TabIndex = 9;
            this.buttonAdd.Text = "加入列表";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // textBoxAdd
            // 
            this.textBoxAdd.Location = new System.Drawing.Point(37, 355);
            this.textBoxAdd.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxAdd.Name = "textBoxAdd";
            this.textBoxAdd.Size = new System.Drawing.Size(132, 25);
            this.textBoxAdd.TabIndex = 11;
            // 
            // txtMsg
            // 
            this.txtMsg.Location = new System.Drawing.Point(37, 469);
            this.txtMsg.Margin = new System.Windows.Forms.Padding(4);
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.Size = new System.Drawing.Size(320, 25);
            this.txtMsg.TabIndex = 12;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(37, 514);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(99, 46);
            this.button1.TabIndex = 13;
            this.button1.Text = "发送";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lblConnectionStatus);
            this.panel1.Controls.Add(this.btnConnect);
            this.panel1.Controls.Add(this.txtServerPort);
            this.panel1.Controls.Add(this.lblServerPort);
            this.panel1.Controls.Add(this.txtServerIP);
            this.panel1.Controls.Add(this.lblServerIP);
            this.panel1.Controls.Add(this.lblHeartbeat);
            this.panel1.Controls.Add(this.txtHeartbeat);
            this.panel1.Controls.Add(this.lblHeartbeatUnit);
            this.panel1.Location = new System.Drawing.Point(907, 15);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(359, 156);
            this.panel1.TabIndex = 16;
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.AutoSize = true;
            this.lblConnectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblConnectionStatus.ForeColor = System.Drawing.Color.Red;
            this.lblConnectionStatus.Location = new System.Drawing.Point(13, 119);
            this.lblConnectionStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(56, 18);
            this.lblConnectionStatus.TabIndex = 5;
            this.lblConnectionStatus.Text = "未连接";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(240, 8);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(100, 31);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            // 
            // txtServerPort
            // 
            this.txtServerPort.Location = new System.Drawing.Point(73, 51);
            this.txtServerPort.Margin = new System.Windows.Forms.Padding(4);
            this.txtServerPort.Name = "txtServerPort";
            this.txtServerPort.Size = new System.Drawing.Size(59, 25);
            this.txtServerPort.TabIndex = 3;
            this.txtServerPort.Text = "9000";
            // 
            // lblServerPort
            // 
            this.lblServerPort.AutoSize = true;
            this.lblServerPort.Location = new System.Drawing.Point(20, 54);
            this.lblServerPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblServerPort.Name = "lblServerPort";
            this.lblServerPort.Size = new System.Drawing.Size(45, 15);
            this.lblServerPort.TabIndex = 2;
            this.lblServerPort.Text = "端口:";
            // 
            // txtServerIP
            // 
            this.txtServerIP.Location = new System.Drawing.Point(73, 12);
            this.txtServerIP.Margin = new System.Windows.Forms.Padding(4);
            this.txtServerIP.Name = "txtServerIP";
            this.txtServerIP.Size = new System.Drawing.Size(159, 25);
            this.txtServerIP.TabIndex = 1;
            this.txtServerIP.Text = "127.0.0.1";
            // 
            // lblServerIP
            // 
            this.lblServerIP.AutoSize = true;
            this.lblServerIP.Location = new System.Drawing.Point(5, 16);
            this.lblServerIP.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblServerIP.Name = "lblServerIP";
            this.lblServerIP.Size = new System.Drawing.Size(60, 15);
            this.lblServerIP.TabIndex = 0;
            this.lblServerIP.Text = "服务器:";
            // 
            // lblHeartbeat
            // 
            this.lblHeartbeat.AutoSize = true;
            this.lblHeartbeat.Location = new System.Drawing.Point(21, 88);
            this.lblHeartbeat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHeartbeat.Name = "lblHeartbeat";
            this.lblHeartbeat.Size = new System.Drawing.Size(52, 15);
            this.lblHeartbeat.TabIndex = 6;
            this.lblHeartbeat.Text = "心间：";
            this.lblHeartbeat.Click += new System.EventHandler(this.lblHeartbeat_Click);
            // 
            // txtHeartbeat
            // 
            this.txtHeartbeat.Location = new System.Drawing.Point(73, 85);
            this.txtHeartbeat.Margin = new System.Windows.Forms.Padding(4);
            this.txtHeartbeat.Name = "txtHeartbeat";
            this.txtHeartbeat.Size = new System.Drawing.Size(52, 25);
            this.txtHeartbeat.TabIndex = 7;
            this.txtHeartbeat.Text = "5";
            // 
            // lblHeartbeatUnit
            // 
            this.lblHeartbeatUnit.AutoSize = true;
            this.lblHeartbeatUnit.Location = new System.Drawing.Point(133, 88);
            this.lblHeartbeatUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHeartbeatUnit.Name = "lblHeartbeatUnit";
            this.lblHeartbeatUnit.Size = new System.Drawing.Size(22, 15);
            this.lblHeartbeatUnit.TabIndex = 8;
            this.lblHeartbeatUnit.Text = "秒";
            // 
            // lstInfo
            // 
            this.lstInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstInfo.FormattingEnabled = true;
            this.lstInfo.ItemHeight = 15;
            this.lstInfo.Location = new System.Drawing.Point(4, 4);
            this.lstInfo.Margin = new System.Windows.Forms.Padding(4);
            this.lstInfo.Name = "lstInfo";
            this.lstInfo.Size = new System.Drawing.Size(479, 468);
            this.lstInfo.TabIndex = 14;
            // 
            // txtMoney
            // 
            this.txtMoney.Location = new System.Drawing.Point(189, 354);
            this.txtMoney.Margin = new System.Windows.Forms.Padding(4);
            this.txtMoney.Name = "txtMoney";
            this.txtMoney.Size = new System.Drawing.Size(132, 25);
            this.txtMoney.TabIndex = 17;
            // 
            // lstName
            // 
            this.lstName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstName.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colAmount});
            this.lstName.FullRowSelect = true;
            this.lstName.GridLines = true;
            this.lstName.HideSelection = false;
            this.lstName.Location = new System.Drawing.Point(37, 79);
            this.lstName.Margin = new System.Windows.Forms.Padding(4);
            this.lstName.MultiSelect = false;
            this.lstName.Name = "lstName";
            this.lstName.Size = new System.Drawing.Size(320, 252);
            this.lstName.TabIndex = 18;
            this.lstName.UseCompatibleStateImageBehavior = false;
            this.lstName.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "姓名";
            this.colName.Width = 100;
            // 
            // colAmount
            // 
            this.colAmount.Text = "金额";
            this.colAmount.Width = 100;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(385, 14);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(495, 505);
            this.tabControl1.TabIndex = 19;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lstInfo);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(487, 476);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "操作信息";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lstNet);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(487, 476);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "连接信息";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lstNet
            // 
            this.lstNet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstNet.FormattingEnabled = true;
            this.lstNet.ItemHeight = 15;
            this.lstNet.Location = new System.Drawing.Point(4, 4);
            this.lstNet.Margin = new System.Windows.Forms.Padding(4);
            this.lstNet.Name = "lstNet";
            this.lstNet.Size = new System.Drawing.Size(479, 468);
            this.lstNet.TabIndex = 15;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 562);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lstName);
            this.Controls.Add(this.txtMoney);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtMsg);
            this.Controls.Add(this.textBoxAdd);
            this.Controls.Add(this.buttonOut);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WeChatAuto";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonOut;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.TextBox textBoxAdd;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtServerPort;
        private System.Windows.Forms.Label lblServerPort;
        private System.Windows.Forms.TextBox txtServerIP;
        private System.Windows.Forms.Label lblServerIP;
        private System.Windows.Forms.ListBox lstInfo;
        private System.Windows.Forms.TextBox txtMoney;
        private System.Windows.Forms.ListView lstName;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colAmount;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox lstNet;
        private System.Windows.Forms.Label lblHeartbeat;
        private System.Windows.Forms.TextBox txtHeartbeat;
        private System.Windows.Forms.Label lblHeartbeatUnit;
    }
}