using System.Runtime.InteropServices;

namespace PS4Saves
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
            components = new System.ComponentModel.Container();
            ipTextBox = new System.Windows.Forms.TextBox();
            connectButton = new System.Windows.Forms.Button();
            gamesButton = new System.Windows.Forms.Button();
            setupButton = new System.Windows.Forms.Button();
            gamesComboBox = new System.Windows.Forms.ComboBox();
            userComboBox = new System.Windows.Forms.ComboBox();
            dirsComboBox = new System.Windows.Forms.ComboBox();
            searchButton = new System.Windows.Forms.Button();
            mountButton = new System.Windows.Forms.Button();
            unmountButton = new System.Windows.Forms.Button();
            patchButton = new System.Windows.Forms.Button();
            unpatchButton = new System.Windows.Forms.Button();
            connectionGroupBox = new System.Windows.Forms.GroupBox();
            label2 = new System.Windows.Forms.Label();
            ipLabel = new System.Windows.Forms.Label();
            createGroupBox = new System.Windows.Forms.GroupBox();
            sizeSnapCheckbox = new System.Windows.Forms.CheckBox();
            sizeLabel = new System.Windows.Forms.Label();
            sizeTrackBar = new System.Windows.Forms.TrackBar();
            nameLabel = new System.Windows.Forms.Label();
            nameTextBox = new System.Windows.Forms.TextBox();
            createButton = new System.Windows.Forms.Button();
            mountGroupBox = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            infoGroupBox = new System.Windows.Forms.GroupBox();
            dateTextBox = new System.Windows.Forms.TextBox();
            dateLabel = new System.Windows.Forms.Label();
            detailsTextBox = new System.Windows.Forms.TextBox();
            detailsLabel = new System.Windows.Forms.Label();
            subtitleTextBox = new System.Windows.Forms.TextBox();
            subtitleLabel = new System.Windows.Forms.Label();
            titleTextBox = new System.Windows.Forms.TextBox();
            titleLabel = new System.Windows.Forms.Label();
            gameImageBox = new System.Windows.Forms.PictureBox();
            sizeToolTip = new System.Windows.Forms.ToolTip(components);
            statusLabel = new System.Windows.Forms.Label();
            connectionGroupBox.SuspendLayout();
            createGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sizeTrackBar).BeginInit();
            mountGroupBox.SuspendLayout();
            infoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gameImageBox).BeginInit();
            SuspendLayout();
            // 
            // ipTextBox
            // 
            ipTextBox.Location = new System.Drawing.Point(81, 22);
            ipTextBox.Margin = new System.Windows.Forms.Padding(4);
            ipTextBox.Name = "ipTextBox";
            ipTextBox.Size = new System.Drawing.Size(136, 23);
            ipTextBox.TabIndex = 0;
            // 
            // connectButton
            // 
            connectButton.Location = new System.Drawing.Point(224, 22);
            connectButton.Margin = new System.Windows.Forms.Padding(4);
            connectButton.Name = "connectButton";
            connectButton.Size = new System.Drawing.Size(211, 23);
            connectButton.TabIndex = 1;
            connectButton.Text = "Connect";
            connectButton.UseVisualStyleBackColor = true;
            connectButton.Click += connectButton_Click;
            // 
            // gamesButton
            // 
            gamesButton.Enabled = false;
            gamesButton.Location = new System.Drawing.Point(8, 144);
            gamesButton.Margin = new System.Windows.Forms.Padding(4);
            gamesButton.Name = "gamesButton";
            gamesButton.Size = new System.Drawing.Size(211, 24);
            gamesButton.TabIndex = 2;
            gamesButton.Text = "Get Games";
            gamesButton.UseVisualStyleBackColor = true;
            gamesButton.Click += gamesButton_Click;
            // 
            // setupButton
            // 
            setupButton.Enabled = false;
            setupButton.Location = new System.Drawing.Point(8, 112);
            setupButton.Margin = new System.Windows.Forms.Padding(4);
            setupButton.Name = "setupButton";
            setupButton.Size = new System.Drawing.Size(211, 24);
            setupButton.TabIndex = 3;
            setupButton.Text = "Setup";
            setupButton.UseVisualStyleBackColor = true;
            setupButton.Click += setupButton_Click;
            // 
            // gamesComboBox
            // 
            gamesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            gamesComboBox.Enabled = false;
            gamesComboBox.FormattingEnabled = true;
            gamesComboBox.Location = new System.Drawing.Point(224, 144);
            gamesComboBox.Margin = new System.Windows.Forms.Padding(4);
            gamesComboBox.Name = "gamesComboBox";
            gamesComboBox.Size = new System.Drawing.Size(210, 23);
            gamesComboBox.TabIndex = 4;
            gamesComboBox.SelectedIndexChanged += gamesComboBox_SelectedIndexChanged;
            // 
            // userComboBox
            // 
            userComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            userComboBox.Enabled = false;
            userComboBox.FormattingEnabled = true;
            userComboBox.Location = new System.Drawing.Point(224, 112);
            userComboBox.Margin = new System.Windows.Forms.Padding(4);
            userComboBox.Name = "userComboBox";
            userComboBox.Size = new System.Drawing.Size(210, 23);
            userComboBox.TabIndex = 5;
            userComboBox.SelectedIndexChanged += userComboBox_SelectedIndexChanged;
            // 
            // dirsComboBox
            // 
            dirsComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dirsComboBox.Enabled = false;
            dirsComboBox.FormattingEnabled = true;
            dirsComboBox.Location = new System.Drawing.Point(225, 22);
            dirsComboBox.Margin = new System.Windows.Forms.Padding(4);
            dirsComboBox.Name = "dirsComboBox";
            dirsComboBox.Size = new System.Drawing.Size(210, 23);
            dirsComboBox.TabIndex = 7;
            dirsComboBox.SelectedIndexChanged += dirsComboBox_SelectedIndexChanged;
            // 
            // searchButton
            // 
            searchButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            searchButton.Enabled = false;
            searchButton.Location = new System.Drawing.Point(7, 22);
            searchButton.Margin = new System.Windows.Forms.Padding(4);
            searchButton.Name = "searchButton";
            searchButton.Size = new System.Drawing.Size(211, 24);
            searchButton.TabIndex = 6;
            searchButton.Text = "Search";
            searchButton.UseVisualStyleBackColor = true;
            searchButton.Click += searchButton_Click;
            // 
            // mountButton
            // 
            mountButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            mountButton.Enabled = false;
            mountButton.Location = new System.Drawing.Point(7, 55);
            mountButton.Margin = new System.Windows.Forms.Padding(4);
            mountButton.Name = "mountButton";
            mountButton.Size = new System.Drawing.Size(211, 26);
            mountButton.TabIndex = 8;
            mountButton.Text = "Mount";
            mountButton.UseVisualStyleBackColor = true;
            mountButton.Click += mountButton_Click;
            // 
            // unmountButton
            // 
            unmountButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            unmountButton.Enabled = false;
            unmountButton.Location = new System.Drawing.Point(224, 55);
            unmountButton.Margin = new System.Windows.Forms.Padding(4);
            unmountButton.Name = "unmountButton";
            unmountButton.Size = new System.Drawing.Size(211, 26);
            unmountButton.TabIndex = 9;
            unmountButton.Text = "Unmount";
            unmountButton.UseVisualStyleBackColor = true;
            unmountButton.Click += unmountButton_Click;
            // 
            // patchButton
            // 
            patchButton.Enabled = false;
            patchButton.Location = new System.Drawing.Point(8, 80);
            patchButton.Margin = new System.Windows.Forms.Padding(4);
            patchButton.Name = "patchButton";
            patchButton.Size = new System.Drawing.Size(211, 24);
            patchButton.TabIndex = 14;
            patchButton.Text = "Patch";
            patchButton.UseVisualStyleBackColor = true;
            patchButton.Click += patchButton_Click;
            // 
            // unpatchButton
            // 
            unpatchButton.Enabled = false;
            unpatchButton.Location = new System.Drawing.Point(224, 80);
            unpatchButton.Margin = new System.Windows.Forms.Padding(4);
            unpatchButton.Name = "unpatchButton";
            unpatchButton.Size = new System.Drawing.Size(211, 24);
            unpatchButton.TabIndex = 15;
            unpatchButton.Text = "Unpatch";
            unpatchButton.UseVisualStyleBackColor = true;
            unpatchButton.Click += unpatchButton_Click;
            // 
            // connectionGroupBox
            // 
            connectionGroupBox.Controls.Add(patchButton);
            connectionGroupBox.Controls.Add(unpatchButton);
            connectionGroupBox.Controls.Add(label2);
            connectionGroupBox.Controls.Add(ipLabel);
            connectionGroupBox.Controls.Add(ipTextBox);
            connectionGroupBox.Controls.Add(connectButton);
            connectionGroupBox.Controls.Add(gamesButton);
            connectionGroupBox.Controls.Add(setupButton);
            connectionGroupBox.Controls.Add(userComboBox);
            connectionGroupBox.Controls.Add(gamesComboBox);
            connectionGroupBox.Location = new System.Drawing.Point(8, 13);
            connectionGroupBox.Margin = new System.Windows.Forms.Padding(4);
            connectionGroupBox.Name = "connectionGroupBox";
            connectionGroupBox.Padding = new System.Windows.Forms.Padding(4);
            connectionGroupBox.Size = new System.Drawing.Size(442, 178);
            connectionGroupBox.TabIndex = 10;
            connectionGroupBox.TabStop = false;
            connectionGroupBox.Text = "Connection";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(8, 54);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(148, 15);
            label2.TabIndex = 6;
            label2.Text = "Detected firmware version:";
            // 
            // ipLabel
            // 
            ipLabel.AutoSize = true;
            ipLabel.Location = new System.Drawing.Point(7, 25);
            ipLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            ipLabel.Name = "ipLabel";
            ipLabel.Size = new System.Drawing.Size(65, 15);
            ipLabel.TabIndex = 6;
            ipLabel.Text = "IP Address:";
            // 
            // createGroupBox
            // 
            createGroupBox.Controls.Add(sizeSnapCheckbox);
            createGroupBox.Controls.Add(sizeLabel);
            createGroupBox.Controls.Add(sizeTrackBar);
            createGroupBox.Controls.Add(nameLabel);
            createGroupBox.Controls.Add(nameTextBox);
            createGroupBox.Controls.Add(createButton);
            createGroupBox.Location = new System.Drawing.Point(9, 329);
            createGroupBox.Margin = new System.Windows.Forms.Padding(4);
            createGroupBox.Name = "createGroupBox";
            createGroupBox.Padding = new System.Windows.Forms.Padding(4);
            createGroupBox.Size = new System.Drawing.Size(442, 148);
            createGroupBox.TabIndex = 11;
            createGroupBox.TabStop = false;
            createGroupBox.Text = "Create New Saves";
            // 
            // sizeSnapCheckbox
            // 
            sizeSnapCheckbox.AutoSize = true;
            sizeSnapCheckbox.Checked = true;
            sizeSnapCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            sizeSnapCheckbox.Location = new System.Drawing.Point(10, 79);
            sizeSnapCheckbox.Name = "sizeSnapCheckbox";
            sizeSnapCheckbox.Size = new System.Drawing.Size(109, 19);
            sizeSnapCheckbox.TabIndex = 11;
            sizeSnapCheckbox.Text = "Snap size to MB";
            sizeSnapCheckbox.UseVisualStyleBackColor = true;
            // 
            // sizeLabel
            // 
            sizeLabel.AutoSize = true;
            sizeLabel.Location = new System.Drawing.Point(7, 55);
            sizeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            sizeLabel.Name = "sizeLabel";
            sizeLabel.Size = new System.Drawing.Size(80, 15);
            sizeLabel.TabIndex = 9;
            sizeLabel.Text = "Max save size:";
            // 
            // sizeTrackBar
            // 
            sizeTrackBar.Location = new System.Drawing.Point(136, 55);
            sizeTrackBar.Margin = new System.Windows.Forms.Padding(4);
            sizeTrackBar.Maximum = 32768;
            sizeTrackBar.Minimum = 96;
            sizeTrackBar.Name = "sizeTrackBar";
            sizeTrackBar.Size = new System.Drawing.Size(300, 45);
            sizeTrackBar.TabIndex = 8;
            sizeTrackBar.Value = 96;
            sizeTrackBar.Scroll += sizeTrackBar_Scroll;
            // 
            // nameLabel
            // 
            nameLabel.AutoSize = true;
            nameLabel.Location = new System.Drawing.Point(7, 28);
            nameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            nameLabel.Name = "nameLabel";
            nameLabel.Size = new System.Drawing.Size(117, 15);
            nameLabel.TabIndex = 7;
            nameLabel.Text = "Save directory name:";
            // 
            // nameTextBox
            // 
            nameTextBox.Enabled = false;
            nameTextBox.Location = new System.Drawing.Point(136, 25);
            nameTextBox.Margin = new System.Windows.Forms.Padding(4);
            nameTextBox.MaxLength = 31;
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new System.Drawing.Size(298, 23);
            nameTextBox.TabIndex = 6;
            // 
            // createButton
            // 
            createButton.Enabled = false;
            createButton.Location = new System.Drawing.Point(6, 114);
            createButton.Margin = new System.Windows.Forms.Padding(4);
            createButton.Name = "createButton";
            createButton.Size = new System.Drawing.Size(428, 26);
            createButton.TabIndex = 6;
            createButton.Text = "Create Save";
            createButton.UseMnemonic = false;
            createButton.UseVisualStyleBackColor = true;
            createButton.Click += createButton_Click;
            // 
            // mountGroupBox
            // 
            mountGroupBox.Controls.Add(label1);
            mountGroupBox.Controls.Add(searchButton);
            mountGroupBox.Controls.Add(dirsComboBox);
            mountGroupBox.Controls.Add(mountButton);
            mountGroupBox.Controls.Add(unmountButton);
            mountGroupBox.Location = new System.Drawing.Point(8, 199);
            mountGroupBox.Margin = new System.Windows.Forms.Padding(4);
            mountGroupBox.Name = "mountGroupBox";
            mountGroupBox.Padding = new System.Windows.Forms.Padding(4);
            mountGroupBox.Size = new System.Drawing.Size(442, 122);
            mountGroupBox.TabIndex = 12;
            mountGroupBox.TabStop = false;
            mountGroupBox.Text = "Mount Existing Saves";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 85);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.MaximumSize = new System.Drawing.Size(438, 375);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(391, 30);
            label1.TabIndex = 6;
            label1.Text = "Make sure to unmount before closing game, otherwise the save may get corrupted!";
            // 
            // infoGroupBox
            // 
            infoGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            infoGroupBox.AutoSize = true;
            infoGroupBox.Controls.Add(dateTextBox);
            infoGroupBox.Controls.Add(dateLabel);
            infoGroupBox.Controls.Add(detailsTextBox);
            infoGroupBox.Controls.Add(detailsLabel);
            infoGroupBox.Controls.Add(subtitleTextBox);
            infoGroupBox.Controls.Add(subtitleLabel);
            infoGroupBox.Controls.Add(titleTextBox);
            infoGroupBox.Controls.Add(titleLabel);
            infoGroupBox.Controls.Add(gameImageBox);
            infoGroupBox.Location = new System.Drawing.Point(458, 13);
            infoGroupBox.Margin = new System.Windows.Forms.Padding(4);
            infoGroupBox.Name = "infoGroupBox";
            infoGroupBox.Padding = new System.Windows.Forms.Padding(4);
            infoGroupBox.Size = new System.Drawing.Size(462, 489);
            infoGroupBox.TabIndex = 12;
            infoGroupBox.TabStop = false;
            infoGroupBox.Text = "Save Info";
            // 
            // dateTextBox
            // 
            dateTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dateTextBox.Location = new System.Drawing.Point(10, 253);
            dateTextBox.Margin = new System.Windows.Forms.Padding(4);
            dateTextBox.Name = "dateTextBox";
            dateTextBox.ReadOnly = true;
            dateTextBox.Size = new System.Drawing.Size(444, 23);
            dateTextBox.TabIndex = 7;
            // 
            // dateLabel
            // 
            dateLabel.AutoSize = true;
            dateLabel.Location = new System.Drawing.Point(7, 235);
            dateLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            dateLabel.Name = "dateLabel";
            dateLabel.Size = new System.Drawing.Size(34, 15);
            dateLabel.TabIndex = 6;
            dateLabel.Text = "Date:";
            // 
            // detailsTextBox
            // 
            detailsTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            detailsTextBox.BackColor = System.Drawing.SystemColors.Control;
            detailsTextBox.Location = new System.Drawing.Point(10, 158);
            detailsTextBox.Margin = new System.Windows.Forms.Padding(4);
            detailsTextBox.Multiline = true;
            detailsTextBox.Name = "detailsTextBox";
            detailsTextBox.ReadOnly = true;
            detailsTextBox.Size = new System.Drawing.Size(444, 74);
            detailsTextBox.TabIndex = 5;
            // 
            // detailsLabel
            // 
            detailsLabel.AutoSize = true;
            detailsLabel.Location = new System.Drawing.Point(7, 140);
            detailsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            detailsLabel.Name = "detailsLabel";
            detailsLabel.Size = new System.Drawing.Size(45, 15);
            detailsLabel.TabIndex = 4;
            detailsLabel.Text = "Details:";
            // 
            // subtitleTextBox
            // 
            subtitleTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            subtitleTextBox.Location = new System.Drawing.Point(10, 93);
            subtitleTextBox.Margin = new System.Windows.Forms.Padding(4);
            subtitleTextBox.Multiline = true;
            subtitleTextBox.Name = "subtitleTextBox";
            subtitleTextBox.ReadOnly = true;
            subtitleTextBox.Size = new System.Drawing.Size(444, 43);
            subtitleTextBox.TabIndex = 3;
            // 
            // subtitleLabel
            // 
            subtitleLabel.AutoSize = true;
            subtitleLabel.Location = new System.Drawing.Point(7, 75);
            subtitleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Size = new System.Drawing.Size(50, 15);
            subtitleLabel.TabIndex = 2;
            subtitleLabel.Text = "Subtitle:";
            // 
            // titleTextBox
            // 
            titleTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            titleTextBox.BackColor = System.Drawing.SystemColors.Control;
            titleTextBox.Location = new System.Drawing.Point(10, 40);
            titleTextBox.Margin = new System.Windows.Forms.Padding(4);
            titleTextBox.Multiline = true;
            titleTextBox.Name = "titleTextBox";
            titleTextBox.ReadOnly = true;
            titleTextBox.Size = new System.Drawing.Size(444, 29);
            titleTextBox.TabIndex = 1;
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Location = new System.Drawing.Point(7, 22);
            titleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new System.Drawing.Size(33, 15);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Title:";
            // 
            // gameImageBox
            // 
            gameImageBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gameImageBox.BackColor = System.Drawing.SystemColors.Control;
            gameImageBox.Location = new System.Drawing.Point(144, 283);
            gameImageBox.MaximumSize = new System.Drawing.Size(512, 512);
            gameImageBox.MinimumSize = new System.Drawing.Size(175, 175);
            gameImageBox.Name = "gameImageBox";
            gameImageBox.Size = new System.Drawing.Size(175, 183);
            gameImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            gameImageBox.TabIndex = 8;
            gameImageBox.TabStop = false;
            // 
            // statusLabel
            // 
            statusLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            statusLabel.AutoSize = true;
            statusLabel.Location = new System.Drawing.Point(9, 489);
            statusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new System.Drawing.Size(42, 15);
            statusLabel.TabIndex = 13;
            statusLabel.Text = "Status:";
            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            ClientSize = new System.Drawing.Size(934, 511);
            Controls.Add(statusLabel);
            Controls.Add(infoGroupBox);
            Controls.Add(mountGroupBox);
            Controls.Add(createGroupBox);
            Controls.Add(connectionGroupBox);
            Margin = new System.Windows.Forms.Padding(4);
            MinimumSize = new System.Drawing.Size(950, 550);
            Name = "Main";
            Text = "Playstation 5 Save Mounter 1.4.6 [ps5debug]";
            Closing += Main_Closing;
            connectionGroupBox.ResumeLayout(false);
            connectionGroupBox.PerformLayout();
            createGroupBox.ResumeLayout(false);
            createGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)sizeTrackBar).EndInit();
            mountGroupBox.ResumeLayout(false);
            mountGroupBox.PerformLayout();
            infoGroupBox.ResumeLayout(false);
            infoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gameImageBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox ipTextBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button gamesButton;
        private System.Windows.Forms.Button setupButton;
        private System.Windows.Forms.ComboBox gamesComboBox;
        private System.Windows.Forms.ComboBox userComboBox;
        private System.Windows.Forms.ComboBox dirsComboBox;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.Button mountButton;
        private System.Windows.Forms.Button unmountButton;
        private System.Windows.Forms.Button patchButton;
        private System.Windows.Forms.Button unpatchButton;
        private System.Windows.Forms.GroupBox connectionGroupBox;
        private System.Windows.Forms.Label ipLabel;
        private System.Windows.Forms.GroupBox createGroupBox;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.GroupBox mountGroupBox;
        private System.Windows.Forms.GroupBox infoGroupBox;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TrackBar sizeTrackBar;
        private System.Windows.Forms.Label sizeLabel;
        private System.Windows.Forms.ToolTip sizeToolTip;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.TextBox dateTextBox;
        private System.Windows.Forms.Label dateLabel;
        private System.Windows.Forms.TextBox detailsTextBox;
        private System.Windows.Forms.Label detailsLabel;
        private System.Windows.Forms.TextBox subtitleTextBox;
        private System.Windows.Forms.Label subtitleLabel;
        private System.Windows.Forms.TextBox titleTextBox;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox sizeSnapCheckbox;
        private System.Windows.Forms.PictureBox gameImageBox;
    }
}