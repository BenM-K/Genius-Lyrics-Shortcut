namespace GeniusShortcut
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
            this.txtBoxLog = new System.Windows.Forms.TextBox();
            this.hotKeyBtn = new System.Windows.Forms.Button();
            this.logLabel = new System.Windows.Forms.Label();
            this.startWithWindows = new System.Windows.Forms.CheckBox();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.minimizeToTray = new System.Windows.Forms.CheckBox();
            this.findSongBtn = new System.Windows.Forms.Button();
            this.aboutBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtBoxLog
            // 
            this.txtBoxLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtBoxLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBoxLog.Location = new System.Drawing.Point(16, 37);
            this.txtBoxLog.Multiline = true;
            this.txtBoxLog.Name = "txtBoxLog";
            this.txtBoxLog.ReadOnly = true;
            this.txtBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBoxLog.ShortcutsEnabled = false;
            this.txtBoxLog.Size = new System.Drawing.Size(481, 170);
            this.txtBoxLog.TabIndex = 0;
            this.txtBoxLog.Enter += new System.EventHandler(this.TextBox_Enter);
            // 
            // hotKeyBtn
            // 
            this.hotKeyBtn.Location = new System.Drawing.Point(302, 234);
            this.hotKeyBtn.Name = "hotKeyBtn";
            this.hotKeyBtn.Size = new System.Drawing.Size(95, 23);
            this.hotKeyBtn.TabIndex = 1;
            this.hotKeyBtn.Text = "Change Hotkey";
            this.hotKeyBtn.UseVisualStyleBackColor = true;
            this.hotKeyBtn.Click += new System.EventHandler(this.HotKeyBtn_Click);
            // 
            // logLabel
            // 
            this.logLabel.AutoSize = true;
            this.logLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logLabel.Location = new System.Drawing.Point(13, 8);
            this.logLabel.Name = "logLabel";
            this.logLabel.Size = new System.Drawing.Size(31, 15);
            this.logLabel.TabIndex = 2;
            this.logLabel.Text = "Log:";
            // 
            // startWithWindows
            // 
            this.startWithWindows.AutoSize = true;
            this.startWithWindows.Location = new System.Drawing.Point(12, 217);
            this.startWithWindows.Name = "startWithWindows";
            this.startWithWindows.Size = new System.Drawing.Size(117, 17);
            this.startWithWindows.TabIndex = 3;
            this.startWithWindows.Text = "Start with Windows";
            this.startWithWindows.UseVisualStyleBackColor = true;
            this.startWithWindows.CheckedChanged += new System.EventHandler(this.startWithWindows_CheckedChanged);
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.BalloonTipText = "Listening for hotkey in the background";
            this.notifyIcon.BalloonTipTitle = "Minimized to system tray";
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Genius Shortcut";
            this.notifyIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseUp);
            // 
            // minimizeToTray
            // 
            this.minimizeToTray.AutoSize = true;
            this.minimizeToTray.Location = new System.Drawing.Point(12, 240);
            this.minimizeToTray.Name = "minimizeToTray";
            this.minimizeToTray.Size = new System.Drawing.Size(161, 17);
            this.minimizeToTray.TabIndex = 4;
            this.minimizeToTray.Text = "Minimize to tray when closed";
            this.minimizeToTray.UseVisualStyleBackColor = true;
            this.minimizeToTray.CheckedChanged += new System.EventHandler(this.minimizeToTray_CheckedChanged);
            // 
            // findSongBtn
            // 
            this.findSongBtn.Location = new System.Drawing.Point(201, 234);
            this.findSongBtn.Name = "findSongBtn";
            this.findSongBtn.Size = new System.Drawing.Size(95, 23);
            this.findSongBtn.TabIndex = 5;
            this.findSongBtn.Text = "Find Song";
            this.findSongBtn.UseVisualStyleBackColor = true;
            this.findSongBtn.Click += new System.EventHandler(this.findSongBtn_Click);
            // 
            // aboutBtn
            // 
            this.aboutBtn.Location = new System.Drawing.Point(403, 234);
            this.aboutBtn.Name = "aboutBtn";
            this.aboutBtn.Size = new System.Drawing.Size(95, 23);
            this.aboutBtn.TabIndex = 6;
            this.aboutBtn.Text = "About";
            this.aboutBtn.UseVisualStyleBackColor = true;
            this.aboutBtn.Click += new System.EventHandler(this.aboutBtn_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(510, 269);
            this.Controls.Add(this.txtBoxLog);
            this.Controls.Add(this.aboutBtn);
            this.Controls.Add(this.findSongBtn);
            this.Controls.Add(this.minimizeToTray);
            this.Controls.Add(this.startWithWindows);
            this.Controls.Add(this.logLabel);
            this.Controls.Add(this.hotKeyBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Genius Shortcut";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Dialog_FormClosing);
            this.Load += new System.EventHandler(this.Dialog_Load);
            this.Resize += new System.EventHandler(this.Dialog_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtBoxLog;
        private System.Windows.Forms.Button hotKeyBtn;
        private System.Windows.Forms.Label logLabel;
        private System.Windows.Forms.CheckBox startWithWindows;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.CheckBox minimizeToTray;
        private System.Windows.Forms.Button findSongBtn;
        private System.Windows.Forms.Button aboutBtn;
    }
}