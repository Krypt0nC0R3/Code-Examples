using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADS_Overlay_Host
{
    public partial class UpdaterForm : Form
    {
        public Form1 MainForm;
        public UpdaterForm()
        {
            InitializeComponent();
        }

        private void UpdaterForm_Load(object sender, EventArgs e)
        {
            label2.Text = String.Empty;
            this.TopMost = true;
            this.Text = "ADS " + MainForm.Version.ToString();
            
            richTextBox1.Text = MainForm.updater.Pathcnote;
            label2.Text = MainForm.updater.FindedVersion.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MainForm.updater.OnDownloadProgressChanged += (p) =>
            {
                progressBar1.Value = p;
            };
            MainForm.updater.OnDownloadCompleted += (f) =>
            {
                if (f == null)
                {
                    MessageBox.Show("An update error has occurred.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }
                MessageBox.Show("The application will be restarted to complete the update.", "Downloading is complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start("updater.exe", f + " " + AppDomain.CurrentDomain.FriendlyName);
                Process.GetCurrentProcess().Kill();
            };
            MainForm.updater.DownloadLastestVersion();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
