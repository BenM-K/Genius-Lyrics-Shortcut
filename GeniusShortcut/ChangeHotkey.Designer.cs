namespace GeniusShortcut
{
    partial class ChangeHotKey
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangeHotKey));
            this.keysTxtBox = new System.Windows.Forms.TextBox();
            this.setBtn = new System.Windows.Forms.Button();
            this.promptLabel = new System.Windows.Forms.Label();
            this.clearBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // keysTxtBox
            // 
            this.keysTxtBox.Location = new System.Drawing.Point(12, 154);
            this.keysTxtBox.Name = "keysTxtBox";
            this.keysTxtBox.ReadOnly = true;
            this.keysTxtBox.ShortcutsEnabled = false;
            this.keysTxtBox.Size = new System.Drawing.Size(356, 20);
            this.keysTxtBox.TabIndex = 0;
            this.keysTxtBox.TextChanged += new System.EventHandler(this.keysTxtBox_TextChanged);
            this.keysTxtBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeysTextBox_KeyDown);
            // 
            // setBtn
            // 
            this.setBtn.Enabled = false;
            this.setBtn.Location = new System.Drawing.Point(113, 187);
            this.setBtn.Name = "setBtn";
            this.setBtn.Size = new System.Drawing.Size(75, 23);
            this.setBtn.TabIndex = 1;
            this.setBtn.Text = "Set";
            this.setBtn.UseVisualStyleBackColor = true;
            this.setBtn.Click += new System.EventHandler(this.SetBtn_Click);
            // 
            // promptLabel
            // 
            this.promptLabel.AutoSize = true;
            this.promptLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.promptLabel.Location = new System.Drawing.Point(8, 13);
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Size = new System.Drawing.Size(346, 120);
            this.promptLabel.TabIndex = 2;
            this.promptLabel.Text = resources.GetString("promptLabel.Text");
            // 
            // clearBtn
            // 
            this.clearBtn.Location = new System.Drawing.Point(194, 187);
            this.clearBtn.Name = "clearBtn";
            this.clearBtn.Size = new System.Drawing.Size(75, 23);
            this.clearBtn.TabIndex = 3;
            this.clearBtn.Text = "Clear";
            this.clearBtn.UseVisualStyleBackColor = true;
            this.clearBtn.Click += new System.EventHandler(this.ClearBtn_Click);
            // 
            // ChangeHotKey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 223);
            this.Controls.Add(this.clearBtn);
            this.Controls.Add(this.promptLabel);
            this.Controls.Add(this.setBtn);
            this.Controls.Add(this.keysTxtBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangeHotKey";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Change Hotkey";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox keysTxtBox;
        private System.Windows.Forms.Button setBtn;
        private System.Windows.Forms.Label promptLabel;
        private System.Windows.Forms.Button clearBtn;
    }
}