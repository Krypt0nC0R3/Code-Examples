using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kryp0nBots.Twitch;
using Kryp0nBots.OAuth;
using Kryp0nBots.StreamersMarketAPI;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;

namespace ADS_Overlay_Host
{
    public partial class LogIn : Form
    {
        //Localized messages
        public string TwitchOK = ""; 
        public string TwitchError = "";
        public string BotNotConnected = "";
        public string Error = "";

        public string WebError = "";
        public string WebOk = "";
        private OAuthWebServer authWebServer;
        private UriBuilder builder;
#if (DEBUG)
        readonly string webRoot = @"..\..\web\OAuth\";
#else
        string webRoot = @"data\web\OAuth\";
#endif
        //Ref to the Form1 for transmit data
        public Form1 MainForm;

        private const int delayForConnect = 100;
        public LogIn()
        {
            InitializeComponent();
            siteLogInBtn.Enabled = false;
            
            
        }


        private void ApplyLangForControl(object control)
        {
            if (control is MenuStrip)
            {
                foreach (ToolStripMenuItem item in ((MenuStrip)control).Items)
                {
                    ApplyLangForControl(item);
                }
            }
            else if (control is ToolStripMenuItem)
            {
                for (int i = 0; i < ((ToolStripMenuItem)control).DropDownItems.Count; i++)
                {
                    if (((ToolStripMenuItem)control).DropDownItems[i] is ToolStripMenuItem)
                    {
                        ApplyLangForControl(((ToolStripMenuItem)control).DropDownItems[i]);
                    }
                }
                if (((ToolStripMenuItem)control).Tag != null)
                {
                    if (!String.IsNullOrEmpty(((ToolStripMenuItem)control).Tag.ToString()))
                    {
                        ((ToolStripMenuItem)control).Text = MainForm.GetLocalizedMessage(((ToolStripMenuItem)control).Tag.ToString());
                        ((ToolStripMenuItem)control).Width = Convert.ToInt32(MainForm.GetStringLenghtInPx(((ToolStripMenuItem)control).Text).Width) + 10;
                    }
                }
            }
            else if (control is ToolStripDropDownItem)
            {
                foreach (ToolStripMenuItem dropDownItem in ((ToolStripMenuItem)control).DropDownItems)
                {
                    ApplyLangForControl(dropDownItem);
                }
                if (((ToolStripDropDownItem)control).Tag != null)
                {
                    if (!String.IsNullOrEmpty(MainForm.GetSylizedKey(((ToolStripDropDownItem)control).Tag.ToString())))
                    {
                        ((ToolStripDropDownItem)control).Text = MainForm.GetLocalizedMessage(((ToolStripDropDownItem)control).Tag.ToString());
                        ((ToolStripDropDownItem)control).Width = Convert.ToInt32(MainForm.GetStringLenghtInPx(((ToolStripDropDownItem)control).Text).Width) + 10;
                    }
                }
            }
            else if (control is GroupBox)
            {
                foreach (Control ctrl in ((GroupBox)control).Controls)
                {
                    ApplyLangForControl(ctrl);
                }
                if (((Control)control).Tag != null && !String.IsNullOrEmpty(((Control)control).Tag.ToString()))
                {
                    ((Control)control).Text = MainForm.GetLocalizedMessage(((Control)control).Tag.ToString());
                    ((Control)control).Width = Convert.ToInt32(MainForm.GetStringLenghtInPx(((Control)control).Text).Width) + 10;
                }
            }
            else if (control is ContextMenuStrip)
            {
                foreach (ToolStripMenuItem ctrl in ((ContextMenuStrip)control).Items)
                {
                    ApplyLangForControl(ctrl);
                }
            }
            else if (control is NotifyIcon)
            {
                ApplyLangForControl(((NotifyIcon)control).ContextMenuStrip);
            }
            else
            {
                if (((Control)control).Tag != null)
                {
                    if (!String.IsNullOrEmpty(((Control)control).Tag.ToString()))
                    {
                        ((Control)control).Text = MainForm.GetLocalizedMessage(((Control)control).Tag.ToString());
                        ((Control)control).Width = Convert.ToInt32(MainForm.GetStringLenghtInPx(((Control)control).Text).Width) + 10;
                    }

                }
            }
        }
        private void LogIn_Load(object sender, EventArgs e)
        {
            twitchStateLabel.Text += ": ";
            twitchStatusLabel.Text = String.Empty;
            this.Width = GetMaxWidth();
            this.MinimumSize = new Size(this.Width, 125);
            if (!MainForm.smAPI.LoadDataFromRegistry(MainForm.StreamersMarketDataPath)) MainForm.smAPI = new StreamersMarketAPI(textBox1.Text, textBox2.Text);
            MainForm.smAPI.Connect();

            textBox1.Text = MainForm.smAPI.Username;
            textBox2.Text = MainForm.smAPI.Key;

            if (!MainForm.twitchBot.LoadData(MainForm.OAuthDataPath)) MainForm.twitchBot= new Krypt0nTwitchBot();
            MainForm.twitchBot.Connect();

            if(MainForm.smAPI.IsConnected && MainForm.twitchBot.IsConnected)
            {
                MainForm.allConnected = true;
                this.Close();
            }
        }

        private int GetMaxWidth()
        {
            //int width = Math.Max(button1.Width, twitchLogInBtn.Width);
            int width = button1.Width;
            int a = MainForm.GetStringLenghtInPx(twitchStateLabel.Text).Width;
            int b = MainForm.GetStringLenghtInPx(BotNotConnected).Width;
            width = Math.Max(width, a + b + 10);
            width = Math.Max(width, siteLogInBtn.Width);
            return width;
        }

        private void twitchLogInBtn_Click(object sender, EventArgs e)
        {
            MainForm.twitchBot.Clear();
            MainForm.twitchBot.GetOAuth();
            if (!MainForm.twitchBot.Connect())
            {
                
                MessageBox.Show(BotNotConnected, Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                twitchStatusLabel.ForeColor = Color.DarkRed;
                twitchStatusLabel.Text = TwitchError;
            }
            else
            {
                int i = 0;
                while (i < 50 && (!MainForm.twitchBot.IsConnected))
                {
                    i++;
                    Thread.Sleep(delayForConnect);
                }
                if (i == 50 && !MainForm.twitchBot.IsConnected)
                {
                    MessageBox.Show(BotNotConnected, Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    twitchStatusLabel.ForeColor = Color.DarkRed;
                    twitchStatusLabel.Text = TwitchError;
                }
                else
                {
                    MainForm.twitchBot.SaveData(MainForm.OAuthDataPath);
                    twitchStatusLabel.Text = TwitchOK;
                    twitchStatusLabel.ForeColor = Color.Green;
                    siteLogInBtn.Enabled = true;
                    MainForm.twitchBot.SaveData(MainForm.OAuthDataPath);
                }
                
            }
        }

        private void siteLogInBtn_Click(object sender, EventArgs e)
        {
            siteLogInBtn.Text = "Logging In...";
            siteLogInBtn.Update();
            //dummy
            if (GetHostSiteStatus())
            {
                MainForm.smAPI.SaveDataToRegistry(MainForm.StreamersMarketDataPath);
                MainForm.allConnected = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("You do not have any active offers or your username and password are wrong. Please confirm you have an active offer and your username and password are correct.", "Error!",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void twitchStatusLabel_TextChanged(object sender, EventArgs e)
        {
            
        }

        private bool GetHostSiteStatus()
        {
            try
            {
                if (!MainForm.smAPI.LoadDataFromRegistry(MainForm.StreamersMarketDataPath)) MainForm.smAPI = new StreamersMarketAPI(textBox1.Text, textBox2.Text);
                return MainForm.smAPI.Connect();
            }
            catch
            {
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            builder = new UriBuilder("https://id.twitch.tv/oauth2/authorize?client_id=" + MainForm.twitchBot.client_id + "&redirect_uri=http://localhost:6580/index.html&response_type=token&scope=channel:moderate+chat:edit+user_read+chat:read&force_verify=true&state=" + MainForm.twitchBot.client_secret);
            authWebServer = new OAuthWebServer { Root = webRoot };
            authWebServer.OnWSMessage += (param) =>
            {
                try
                {
                    if (MainForm.twitchBot != null)
                    {
                        if (param.ContainsKey("access_token"))
                        {
                            MainForm.twitchBot.OAuthToken = param["access_token"];
                            MainForm.twitchBot.GetUserByOAuth();
                            Thread.Sleep(300);
                            authWebServer.SendWSMessage("o;"+WebOk);
                            this.Invoke(new Action(() =>
                            {
                                this.TopMost = true;
                                Thread.Sleep(10);
                                this.TopMost = false;
                                WebBrowserWasConnected();
                            }));
                        }
                        else
                        {
                            authWebServer.SendWSMessage("e;"+WebError);
                        }
                    }
                }
                catch(Exception exp)
                {
                    Debug.WriteLine(">\tERROR!");
                    Debug.WriteLine(">\t" + exp.Message + Environment.NewLine + ">>\t" + exp.StackTrace);
                    MainForm.ErrorHasOccurred();
                }
            };
            authWebServer.Start();
            Process.Start(builder.Uri.ToString());
        }

        private void WebBrowserWasConnected()
        {
            if (!MainForm.twitchBot.Connect())
            {

                MessageBox.Show(BotNotConnected, Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                twitchStatusLabel.ForeColor = Color.DarkRed;
                twitchStatusLabel.Text = TwitchError;
            }
            else
            {
                int i = 0;
                while (i < 50 && (!MainForm.twitchBot.IsConnected))
                {
                    i++;
                    Thread.Sleep(delayForConnect);
                }
                if (i == 50 && !MainForm.twitchBot.IsConnected)
                {
                    MessageBox.Show(BotNotConnected, Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    twitchStatusLabel.ForeColor = Color.DarkRed;
                    twitchStatusLabel.Text = TwitchError;
                }
                else
                {
                   // MainForm.twitchBot.SaveData(MainForm.OAuthDataPath);
                    twitchStatusLabel.Text = TwitchOK;
                    twitchStatusLabel.ForeColor = Color.Green;
                    //siteLogInBtn.Enabled = true;
                    MainForm.twitchBot.SaveData(MainForm.OAuthDataPath);
                }

            }
        }

        private void LogIn_FormClosing(object sender, FormClosingEventArgs e)
        {
            authWebServer?.Stop();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if(textBox1.Text.Trim()!="" && textBox2.Text.Trim() != "")
            {
                siteLogInBtn.Enabled = true;
            }
        }
    }
}
