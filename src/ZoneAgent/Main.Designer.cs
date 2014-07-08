namespace ZoneAgent
{
    partial class Main
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.label4 = new System.Windows.Forms.Label();
            this.btnclose = new System.Windows.Forms.Button();
            this.zonelog = new System.Windows.Forms.RichTextBox();
            this.lbllssockstatus = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.lstzone = new System.Windows.Forms.ListBox();
            this.lblconnectedzonecount = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lblzoneport = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblagentid = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblserverid = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblpreparedconnectioncount = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblmaxconnectioncount = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblconnectioncount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.refreshzonestatus = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 201);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(268, 13);
            this.label4.TabIndex = 81;
            this.label4.Text = "---------------------------------------------------------------------------------" +
    "------";
            // 
            // btnclose
            // 
            this.btnclose.Location = new System.Drawing.Point(178, 323);
            this.btnclose.Name = "btnclose";
            this.btnclose.Size = new System.Drawing.Size(75, 23);
            this.btnclose.TabIndex = 80;
            this.btnclose.Text = "Close";
            this.btnclose.UseVisualStyleBackColor = true;
            this.btnclose.Click += new System.EventHandler(this.btnclose_Click);
            // 
            // zonelog
            // 
            this.zonelog.Location = new System.Drawing.Point(14, 234);
            this.zonelog.Name = "zonelog";
            this.zonelog.Size = new System.Drawing.Size(239, 82);
            this.zonelog.TabIndex = 79;
            this.zonelog.Text = "";
            // 
            // lbllssockstatus
            // 
            this.lbllssockstatus.AutoSize = true;
            this.lbllssockstatus.Location = new System.Drawing.Point(111, 217);
            this.lbllssockstatus.Name = "lbllssockstatus";
            this.lbllssockstatus.Size = new System.Drawing.Size(142, 13);
            this.lbllssockstatus.TabIndex = 78;
            this.lbllssockstatus.Text = "Login Server : Disconnected";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(14, 217);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(74, 13);
            this.label15.TabIndex = 77;
            this.label15.Text = "Log Message:";
            // 
            // lstzone
            // 
            this.lstzone.FormattingEnabled = true;
            this.lstzone.Location = new System.Drawing.Point(16, 129);
            this.lstzone.Name = "lstzone";
            this.lstzone.Size = new System.Drawing.Size(237, 69);
            this.lstzone.TabIndex = 76;
            // 
            // lblconnectedzonecount
            // 
            this.lblconnectedzonecount.AutoSize = true;
            this.lblconnectedzonecount.Location = new System.Drawing.Point(121, 113);
            this.lblconnectedzonecount.Name = "lblconnectedzonecount";
            this.lblconnectedzonecount.Size = new System.Drawing.Size(13, 13);
            this.lblconnectedzonecount.TabIndex = 75;
            this.lblconnectedzonecount.Text = "0";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(13, 113);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(107, 13);
            this.label12.TabIndex = 74;
            this.label12.Text = "Connected Servers  :";
            // 
            // lblzoneport
            // 
            this.lblzoneport.AutoSize = true;
            this.lblzoneport.Location = new System.Drawing.Point(95, 91);
            this.lblzoneport.Name = "lblzoneport";
            this.lblzoneport.Size = new System.Drawing.Size(13, 13);
            this.lblzoneport.TabIndex = 73;
            this.lblzoneport.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 91);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 13);
            this.label6.TabIndex = 72;
            this.label6.Text = "Listening Port  :";
            // 
            // lblagentid
            // 
            this.lblagentid.AutoSize = true;
            this.lblagentid.Location = new System.Drawing.Point(190, 68);
            this.lblagentid.Name = "lblagentid";
            this.lblagentid.Size = new System.Drawing.Size(13, 13);
            this.lblagentid.TabIndex = 71;
            this.lblagentid.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(129, 68);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 13);
            this.label8.TabIndex = 70;
            this.label8.Text = "Agent ID  :";
            // 
            // lblserverid
            // 
            this.lblserverid.AutoSize = true;
            this.lblserverid.Location = new System.Drawing.Point(76, 68);
            this.lblserverid.Name = "lblserverid";
            this.lblserverid.Size = new System.Drawing.Size(13, 13);
            this.lblserverid.TabIndex = 69;
            this.lblserverid.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 68);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(64, 13);
            this.label10.TabIndex = 68;
            this.label10.Text = "Server ID   :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(268, 13);
            this.label2.TabIndex = 67;
            this.label2.Text = "---------------------------------------------------------------------------------" +
    "------";
            // 
            // lblpreparedconnectioncount
            // 
            this.lblpreparedconnectioncount.AutoSize = true;
            this.lblpreparedconnectioncount.Location = new System.Drawing.Point(174, 42);
            this.lblpreparedconnectioncount.Name = "lblpreparedconnectioncount";
            this.lblpreparedconnectioncount.Size = new System.Drawing.Size(13, 13);
            this.lblpreparedconnectioncount.TabIndex = 87;
            this.lblpreparedconnectioncount.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 42);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(147, 13);
            this.label5.TabIndex = 86;
            this.label5.Text = "Prepared Connection Count  :";
            // 
            // lblmaxconnectioncount
            // 
            this.lblmaxconnectioncount.AutoSize = true;
            this.lblmaxconnectioncount.Location = new System.Drawing.Point(174, 24);
            this.lblmaxconnectioncount.Name = "lblmaxconnectioncount";
            this.lblmaxconnectioncount.Size = new System.Drawing.Size(13, 13);
            this.lblmaxconnectioncount.TabIndex = 85;
            this.lblmaxconnectioncount.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(151, 13);
            this.label3.TabIndex = 84;
            this.label3.Text = "Max Connection Count          : ";
            // 
            // lblconnectioncount
            // 
            this.lblconnectioncount.AutoSize = true;
            this.lblconnectioncount.Location = new System.Drawing.Point(174, 6);
            this.lblconnectioncount.Name = "lblconnectioncount";
            this.lblconnectioncount.Size = new System.Drawing.Size(13, 13);
            this.lblconnectioncount.TabIndex = 83;
            this.lblconnectioncount.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 13);
            this.label1.TabIndex = 82;
            this.label1.Text = "Connection Count                 : ";
            // 
            // refreshzonestatus
            // 
            this.refreshzonestatus.Enabled = true;
            this.refreshzonestatus.Interval = 2000;
            this.refreshzonestatus.Tick += new System.EventHandler(this.refreshzonestatus_Tick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(267, 354);
            this.Controls.Add(this.lblpreparedconnectioncount);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblmaxconnectioncount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblconnectioncount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnclose);
            this.Controls.Add(this.zonelog);
            this.Controls.Add(this.lbllssockstatus);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.lstzone);
            this.Controls.Add(this.lblconnectedzonecount);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.lblzoneport);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblagentid);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lblserverid);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "ZoneAgent";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnclose;
        private System.Windows.Forms.RichTextBox zonelog;
        public System.Windows.Forms.Label lbllssockstatus;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ListBox lstzone;
        private System.Windows.Forms.Label lblconnectedzonecount;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lblzoneport;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblagentid;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblserverid;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblpreparedconnectioncount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblmaxconnectioncount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblconnectioncount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer refreshzonestatus;
    }
}

