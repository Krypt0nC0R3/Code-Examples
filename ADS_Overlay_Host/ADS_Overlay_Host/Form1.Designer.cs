namespace ADS_Overlay_Host
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scenesGroupBox = new System.Windows.Forms.GroupBox();
            this.applyTargetBtn = new System.Windows.Forms.Button();
            this.sceneItemListBox = new System.Windows.Forms.ListView();
            this.SceneListBox = new System.Windows.Forms.ListView();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.mediaLinkGroupBox = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.startADS = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.scenesGroupBox.SuspendLayout();
            this.mediaLinkGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(301, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.settingsToolStripMenuItem.Tag = "$Settings";
            this.settingsToolStripMenuItem.Text = "Params";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Tag = "$Exit";
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // scenesGroupBox
            // 
            this.scenesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scenesGroupBox.Controls.Add(this.applyTargetBtn);
            this.scenesGroupBox.Controls.Add(this.sceneItemListBox);
            this.scenesGroupBox.Controls.Add(this.SceneListBox);
            this.scenesGroupBox.Location = new System.Drawing.Point(12, 27);
            this.scenesGroupBox.Name = "scenesGroupBox";
            this.scenesGroupBox.Size = new System.Drawing.Size(277, 257);
            this.scenesGroupBox.TabIndex = 1;
            this.scenesGroupBox.TabStop = false;
            this.scenesGroupBox.Tag = "$TargetScene";
            this.scenesGroupBox.Text = "scenesGroupBox";
            // 
            // applyTargetBtn
            // 
            this.applyTargetBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.applyTargetBtn.Location = new System.Drawing.Point(6, 225);
            this.applyTargetBtn.Name = "applyTargetBtn";
            this.applyTargetBtn.Size = new System.Drawing.Size(265, 23);
            this.applyTargetBtn.TabIndex = 2;
            this.applyTargetBtn.Tag = "$ApplyTargetScene";
            this.applyTargetBtn.Text = "button1";
            this.applyTargetBtn.UseVisualStyleBackColor = true;
            this.applyTargetBtn.Click += new System.EventHandler(this.applyTargetBtn_Click);
            // 
            // sceneItemListBox
            // 
            this.sceneItemListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sceneItemListBox.HideSelection = false;
            this.sceneItemListBox.Location = new System.Drawing.Point(6, 122);
            this.sceneItemListBox.Name = "sceneItemListBox";
            this.sceneItemListBox.Size = new System.Drawing.Size(265, 97);
            this.sceneItemListBox.TabIndex = 1;
            this.sceneItemListBox.UseCompatibleStateImageBehavior = false;
            this.sceneItemListBox.View = System.Windows.Forms.View.List;
            this.sceneItemListBox.SelectedIndexChanged += new System.EventHandler(this.sceneItemListBox_SelectedIndexChanged);
            // 
            // SceneListBox
            // 
            this.SceneListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SceneListBox.HideSelection = false;
            this.SceneListBox.Location = new System.Drawing.Point(6, 19);
            this.SceneListBox.Name = "SceneListBox";
            this.SceneListBox.Size = new System.Drawing.Size(265, 97);
            this.SceneListBox.TabIndex = 0;
            this.SceneListBox.UseCompatibleStateImageBehavior = false;
            this.SceneListBox.View = System.Windows.Forms.View.List;
            this.SceneListBox.SelectedIndexChanged += new System.EventHandler(this.SceneListBox_SelectedIndexChanged);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // mediaLinkGroupBox
            // 
            this.mediaLinkGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mediaLinkGroupBox.Controls.Add(this.textBox1);
            this.mediaLinkGroupBox.Location = new System.Drawing.Point(12, 290);
            this.mediaLinkGroupBox.Name = "mediaLinkGroupBox";
            this.mediaLinkGroupBox.Size = new System.Drawing.Size(277, 47);
            this.mediaLinkGroupBox.TabIndex = 2;
            this.mediaLinkGroupBox.TabStop = false;
            this.mediaLinkGroupBox.Tag = "$MediaLink";
            this.mediaLinkGroupBox.Text = "groupBox1";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(6, 19);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(265, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "http://127.0.0.1:8060/index.html";
            this.textBox1.Click += new System.EventHandler(this.textBox1_Click);
            // 
            // startADS
            // 
            this.startADS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.startADS.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.startADS.Location = new System.Drawing.Point(12, 344);
            this.startADS.Name = "startADS";
            this.startADS.Size = new System.Drawing.Size(277, 23);
            this.startADS.TabIndex = 3;
            this.startADS.Tag = "$StartOverlay";
            this.startADS.Text = "button1";
            this.startADS.UseVisualStyleBackColor = true;
            this.startADS.Click += new System.EventHandler(this.button2_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(301, 375);
            this.Controls.Add(this.startADS);
            this.Controls.Add(this.mediaLinkGroupBox);
            this.Controls.Add(this.scenesGroupBox);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.scenesGroupBox.ResumeLayout(false);
            this.mediaLinkGroupBox.ResumeLayout(false);
            this.mediaLinkGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.GroupBox scenesGroupBox;
        private System.Windows.Forms.Button applyTargetBtn;
        private System.Windows.Forms.ListView sceneItemListBox;
        private System.Windows.Forms.ListView SceneListBox;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.GroupBox mediaLinkGroupBox;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button startADS;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}

