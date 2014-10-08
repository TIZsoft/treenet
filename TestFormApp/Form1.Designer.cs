namespace TestFormApp
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
            this.components = new System.ComponentModel.Container();
            this.StartBtn = new System.Windows.Forms.Button();
            this.LogMsgrichTextBox = new System.Windows.Forms.RichTextBox();
            this.ConnectionPanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.StatusprogressBar = new System.Windows.Forms.ProgressBar();
            this.ListenNoTextBox = new System.Windows.Forms.TextBox();
            this.ListenNoLabel = new System.Windows.Forms.Label();
            this.BufferSizeTextBox = new System.Windows.Forms.TextBox();
            this.BufferSizeLabel = new System.Windows.Forms.Label();
            this.MaxConnsTextBox = new System.Windows.Forms.TextBox();
            this.MaxConnLabel = new System.Windows.Forms.Label();
            this.PortTextBox = new System.Windows.Forms.TextBox();
            this.PortLabel = new System.Windows.Forms.Label();
            this.AddressLabel = new System.Windows.Forms.Label();
            this.AddressTextBox = new System.Windows.Forms.TextBox();
            this.statusTimer = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.DBPwdtextBox = new System.Windows.Forms.TextBox();
            this.NewGuidBtn = new System.Windows.Forms.Button();
            this.DBPwdlabel = new System.Windows.Forms.Label();
            this.DBUsertextBox = new System.Windows.Forms.TextBox();
            this.DBUserlabel = new System.Windows.Forms.Label();
            this.DBHosttextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ConnectionCountLabel = new System.Windows.Forms.Label();
            this.GameUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.IsClientCheckBox = new System.Windows.Forms.CheckBox();
            this.QueryGuidBtn = new System.Windows.Forms.Button();
            this.SetLevelBtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.ConnectionPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // StartBtn
            // 
            this.StartBtn.Location = new System.Drawing.Point(17, 181);
            this.StartBtn.Name = "StartBtn";
            this.StartBtn.Size = new System.Drawing.Size(196, 31);
            this.StartBtn.TabIndex = 5;
            this.StartBtn.Text = "Start";
            this.StartBtn.UseVisualStyleBackColor = true;
            this.StartBtn.Click += new System.EventHandler(this.StartBtn_Click);
            // 
            // LogMsgrichTextBox
            // 
            this.LogMsgrichTextBox.BackColor = System.Drawing.SystemColors.WindowText;
            this.LogMsgrichTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LogMsgrichTextBox.ForeColor = System.Drawing.Color.White;
            this.LogMsgrichTextBox.Location = new System.Drawing.Point(0, 300);
            this.LogMsgrichTextBox.Name = "LogMsgrichTextBox";
            this.LogMsgrichTextBox.ReadOnly = true;
            this.LogMsgrichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.LogMsgrichTextBox.Size = new System.Drawing.Size(784, 262);
            this.LogMsgrichTextBox.TabIndex = 2;
            this.LogMsgrichTextBox.Text = "";
            // 
            // ConnectionPanel
            // 
            this.ConnectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ConnectionPanel.Controls.Add(this.label1);
            this.ConnectionPanel.Controls.Add(this.StatusprogressBar);
            this.ConnectionPanel.Controls.Add(this.ListenNoTextBox);
            this.ConnectionPanel.Controls.Add(this.ListenNoLabel);
            this.ConnectionPanel.Controls.Add(this.BufferSizeTextBox);
            this.ConnectionPanel.Controls.Add(this.BufferSizeLabel);
            this.ConnectionPanel.Controls.Add(this.MaxConnsTextBox);
            this.ConnectionPanel.Controls.Add(this.MaxConnLabel);
            this.ConnectionPanel.Controls.Add(this.PortTextBox);
            this.ConnectionPanel.Controls.Add(this.PortLabel);
            this.ConnectionPanel.Controls.Add(this.AddressLabel);
            this.ConnectionPanel.Controls.Add(this.AddressTextBox);
            this.ConnectionPanel.Controls.Add(this.StartBtn);
            this.ConnectionPanel.Location = new System.Drawing.Point(12, 12);
            this.ConnectionPanel.Name = "ConnectionPanel";
            this.ConnectionPanel.Size = new System.Drawing.Size(230, 257);
            this.ConnectionPanel.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(81, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 12);
            this.label1.TabIndex = 15;
            this.label1.Text = "Server 設定";
            // 
            // StatusprogressBar
            // 
            this.StatusprogressBar.Location = new System.Drawing.Point(17, 229);
            this.StatusprogressBar.MarqueeAnimationSpeed = 0;
            this.StatusprogressBar.Name = "StatusprogressBar";
            this.StatusprogressBar.Size = new System.Drawing.Size(196, 23);
            this.StatusprogressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.StatusprogressBar.TabIndex = 5;
            // 
            // ListenNoTextBox
            // 
            this.ListenNoTextBox.AcceptsReturn = true;
            this.ListenNoTextBox.Location = new System.Drawing.Point(83, 139);
            this.ListenNoTextBox.Name = "ListenNoTextBox";
            this.ListenNoTextBox.Size = new System.Drawing.Size(130, 22);
            this.ListenNoTextBox.TabIndex = 4;
            this.ListenNoTextBox.Text = "200";
            this.ListenNoTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ListenNoLabel
            // 
            this.ListenNoLabel.AutoSize = true;
            this.ListenNoLabel.Location = new System.Drawing.Point(15, 142);
            this.ListenNoLabel.Name = "ListenNoLabel";
            this.ListenNoLabel.Size = new System.Drawing.Size(53, 12);
            this.ListenNoLabel.TabIndex = 14;
            this.ListenNoLabel.Text = "Listen No.";
            // 
            // BufferSizeTextBox
            // 
            this.BufferSizeTextBox.AcceptsReturn = true;
            this.BufferSizeTextBox.Location = new System.Drawing.Point(83, 111);
            this.BufferSizeTextBox.Name = "BufferSizeTextBox";
            this.BufferSizeTextBox.Size = new System.Drawing.Size(130, 22);
            this.BufferSizeTextBox.TabIndex = 3;
            this.BufferSizeTextBox.Text = "1024";
            this.BufferSizeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // BufferSizeLabel
            // 
            this.BufferSizeLabel.AutoSize = true;
            this.BufferSizeLabel.Location = new System.Drawing.Point(15, 114);
            this.BufferSizeLabel.Name = "BufferSizeLabel";
            this.BufferSizeLabel.Size = new System.Drawing.Size(64, 12);
            this.BufferSizeLabel.TabIndex = 13;
            this.BufferSizeLabel.Text = "Content Size";
            // 
            // MaxConnsTextBox
            // 
            this.MaxConnsTextBox.AcceptsReturn = true;
            this.MaxConnsTextBox.Location = new System.Drawing.Point(83, 83);
            this.MaxConnsTextBox.Name = "MaxConnsTextBox";
            this.MaxConnsTextBox.Size = new System.Drawing.Size(130, 22);
            this.MaxConnsTextBox.TabIndex = 2;
            this.MaxConnsTextBox.Text = "200";
            this.MaxConnsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MaxConnLabel
            // 
            this.MaxConnLabel.AutoSize = true;
            this.MaxConnLabel.Location = new System.Drawing.Point(15, 86);
            this.MaxConnLabel.Name = "MaxConnLabel";
            this.MaxConnLabel.Size = new System.Drawing.Size(62, 12);
            this.MaxConnLabel.TabIndex = 12;
            this.MaxConnLabel.Text = "Max Conns.";
            // 
            // PortTextBox
            // 
            this.PortTextBox.AcceptsReturn = true;
            this.PortTextBox.Location = new System.Drawing.Point(83, 55);
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(130, 22);
            this.PortTextBox.TabIndex = 1;
            this.PortTextBox.Text = "7788";
            this.PortTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PortTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PortTextBox_KeyPress);
            // 
            // PortLabel
            // 
            this.PortLabel.AutoSize = true;
            this.PortLabel.Location = new System.Drawing.Point(15, 58);
            this.PortLabel.Name = "PortLabel";
            this.PortLabel.Size = new System.Drawing.Size(24, 12);
            this.PortLabel.TabIndex = 11;
            this.PortLabel.Text = "Port";
            // 
            // AddressLabel
            // 
            this.AddressLabel.AutoSize = true;
            this.AddressLabel.Location = new System.Drawing.Point(15, 30);
            this.AddressLabel.Name = "AddressLabel";
            this.AddressLabel.Size = new System.Drawing.Size(42, 12);
            this.AddressLabel.TabIndex = 10;
            this.AddressLabel.Text = "Address";
            // 
            // AddressTextBox
            // 
            this.AddressTextBox.AcceptsReturn = true;
            this.AddressTextBox.Location = new System.Drawing.Point(83, 27);
            this.AddressTextBox.MaxLength = 20;
            this.AddressTextBox.Name = "AddressTextBox";
            this.AddressTextBox.Size = new System.Drawing.Size(130, 22);
            this.AddressTextBox.TabIndex = 0;
            this.AddressTextBox.Text = "192.168.7.121";
            this.AddressTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // statusTimer
            // 
            this.statusTimer.Enabled = true;
            this.statusTimer.Interval = 33;
            this.statusTimer.Tick += new System.EventHandler(this.statusTimer_Tick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.SetLevelBtn);
            this.panel1.Controls.Add(this.QueryGuidBtn);
            this.panel1.Controls.Add(this.DBPwdtextBox);
            this.panel1.Controls.Add(this.NewGuidBtn);
            this.panel1.Controls.Add(this.DBPwdlabel);
            this.panel1.Controls.Add(this.DBUsertextBox);
            this.panel1.Controls.Add(this.DBUserlabel);
            this.panel1.Controls.Add(this.DBHosttextBox);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.ConnectionCountLabel);
            this.panel1.Location = new System.Drawing.Point(276, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(240, 256);
            this.panel1.TabIndex = 4;
            // 
            // DBPwdtextBox
            // 
            this.DBPwdtextBox.AcceptsReturn = true;
            this.DBPwdtextBox.Location = new System.Drawing.Point(98, 83);
            this.DBPwdtextBox.MaxLength = 20;
            this.DBPwdtextBox.Name = "DBPwdtextBox";
            this.DBPwdtextBox.PasswordChar = '*';
            this.DBPwdtextBox.Size = new System.Drawing.Size(130, 22);
            this.DBPwdtextBox.TabIndex = 20;
            this.DBPwdtextBox.Text = "Treenet";
            this.DBPwdtextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // NewGuidBtn
            // 
            this.NewGuidBtn.Location = new System.Drawing.Point(17, 114);
            this.NewGuidBtn.Name = "NewGuidBtn";
            this.NewGuidBtn.Size = new System.Drawing.Size(75, 23);
            this.NewGuidBtn.TabIndex = 5;
            this.NewGuidBtn.Text = "New Guid";
            this.NewGuidBtn.UseVisualStyleBackColor = true;
            this.NewGuidBtn.Click += new System.EventHandler(this.NewGuidBtn_Click);
            // 
            // DBPwdlabel
            // 
            this.DBPwdlabel.AutoSize = true;
            this.DBPwdlabel.Location = new System.Drawing.Point(15, 86);
            this.DBPwdlabel.Name = "DBPwdlabel";
            this.DBPwdlabel.Size = new System.Drawing.Size(48, 12);
            this.DBPwdlabel.TabIndex = 19;
            this.DBPwdlabel.Text = "Password";
            // 
            // DBUsertextBox
            // 
            this.DBUsertextBox.AcceptsReturn = true;
            this.DBUsertextBox.Location = new System.Drawing.Point(98, 55);
            this.DBUsertextBox.MaxLength = 20;
            this.DBUsertextBox.Name = "DBUsertextBox";
            this.DBUsertextBox.Size = new System.Drawing.Size(130, 22);
            this.DBUsertextBox.TabIndex = 18;
            this.DBUsertextBox.Text = "test";
            this.DBUsertextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // DBUserlabel
            // 
            this.DBUserlabel.AutoSize = true;
            this.DBUserlabel.Location = new System.Drawing.Point(15, 58);
            this.DBUserlabel.Name = "DBUserlabel";
            this.DBUserlabel.Size = new System.Drawing.Size(26, 12);
            this.DBUserlabel.TabIndex = 17;
            this.DBUserlabel.Text = "User";
            // 
            // DBHosttextBox
            // 
            this.DBHosttextBox.AcceptsReturn = true;
            this.DBHosttextBox.Location = new System.Drawing.Point(98, 27);
            this.DBHosttextBox.MaxLength = 20;
            this.DBHosttextBox.Name = "DBHosttextBox";
            this.DBHosttextBox.Size = new System.Drawing.Size(130, 22);
            this.DBHosttextBox.TabIndex = 16;
            this.DBHosttextBox.Text = "1.34.115.165";
            this.DBHosttextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "Host";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(96, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "DB 設定";
            // 
            // ConnectionCountLabel
            // 
            this.ConnectionCountLabel.AutoSize = true;
            this.ConnectionCountLabel.Location = new System.Drawing.Point(27, 18);
            this.ConnectionCountLabel.Name = "ConnectionCountLabel";
            this.ConnectionCountLabel.Size = new System.Drawing.Size(0, 12);
            this.ConnectionCountLabel.TabIndex = 0;
            // 
            // GameUpdateTimer
            // 
            this.GameUpdateTimer.Enabled = true;
            this.GameUpdateTimer.Interval = 33;
            this.GameUpdateTimer.Tick += new System.EventHandler(this.GameUpdateTimer_Tick);
            // 
            // IsClientCheckBox
            // 
            this.IsClientCheckBox.AutoSize = true;
            this.IsClientCheckBox.Location = new System.Drawing.Point(537, 14);
            this.IsClientCheckBox.Name = "IsClientCheckBox";
            this.IsClientCheckBox.Size = new System.Drawing.Size(60, 16);
            this.IsClientCheckBox.TabIndex = 6;
            this.IsClientCheckBox.Text = "IsClient";
            this.IsClientCheckBox.UseVisualStyleBackColor = true;
            // 
            // QueryGuidBtn
            // 
            this.QueryGuidBtn.Enabled = false;
            this.QueryGuidBtn.Location = new System.Drawing.Point(17, 143);
            this.QueryGuidBtn.Name = "QueryGuidBtn";
            this.QueryGuidBtn.Size = new System.Drawing.Size(75, 69);
            this.QueryGuidBtn.TabIndex = 21;
            this.QueryGuidBtn.UseVisualStyleBackColor = true;
            this.QueryGuidBtn.Click += new System.EventHandler(this.QueryGuidBtn_Click);
            // 
            // SetLevelBtn
            // 
            this.SetLevelBtn.Location = new System.Drawing.Point(98, 114);
            this.SetLevelBtn.Name = "SetLevelBtn";
            this.SetLevelBtn.Size = new System.Drawing.Size(75, 23);
            this.SetLevelBtn.TabIndex = 22;
            this.SetLevelBtn.Text = "Level = 10";
            this.SetLevelBtn.UseVisualStyleBackColor = true;
            this.SetLevelBtn.Click += new System.EventHandler(this.SetLevelBtn_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(537, 40);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 23;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.IsClientCheckBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.ConnectionPanel);
            this.Controls.Add(this.LogMsgrichTextBox);
            this.Name = "Form1";
            this.Text = "Puzzle Battle Server";
            this.ConnectionPanel.ResumeLayout(false);
            this.ConnectionPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartBtn;
        private System.Windows.Forms.RichTextBox LogMsgrichTextBox;
        private System.Windows.Forms.Panel ConnectionPanel;
        private System.Windows.Forms.Label AddressLabel;
        private System.Windows.Forms.TextBox AddressTextBox;
        private System.Windows.Forms.TextBox PortTextBox;
        private System.Windows.Forms.Label PortLabel;
        private System.Windows.Forms.TextBox MaxConnsTextBox;
        private System.Windows.Forms.Label MaxConnLabel;
        private System.Windows.Forms.TextBox ListenNoTextBox;
        private System.Windows.Forms.Label ListenNoLabel;
        private System.Windows.Forms.TextBox BufferSizeTextBox;
        private System.Windows.Forms.Label BufferSizeLabel;
        private System.Windows.Forms.Timer statusTimer;
        private System.Windows.Forms.ProgressBar StatusprogressBar;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label ConnectionCountLabel;
        private System.Windows.Forms.Button NewGuidBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox DBPwdtextBox;
        private System.Windows.Forms.Label DBPwdlabel;
        private System.Windows.Forms.TextBox DBUsertextBox;
        private System.Windows.Forms.Label DBUserlabel;
        private System.Windows.Forms.TextBox DBHosttextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer GameUpdateTimer;
        private System.Windows.Forms.CheckBox IsClientCheckBox;
        private System.Windows.Forms.Button QueryGuidBtn;
        private System.Windows.Forms.Button SetLevelBtn;
        private System.Windows.Forms.Button button1;
    }
}

