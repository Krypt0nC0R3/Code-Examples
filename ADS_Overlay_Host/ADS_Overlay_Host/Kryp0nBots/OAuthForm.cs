using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kryp0nBots.OAuth;
using Kryp0nBots.Twitch;
using Microsoft.Win32;

namespace Kryp0nBots.OAuth
{
    public partial class OAuthForm : Form
    {
        public Krypt0nTwitchBot twitchBot = null;


#if (DEBUG)
        readonly string webRoot = @"..\..\web\OAuth\";
#else
        string webRoot = @"data\web\OAuth\";
#endif
        private UriBuilder builder;
        private OAuthWebServer server;
        public OAuthForm()
        {
            InitializeComponent();
            webBrowser.Focus();
            try
            {
                var appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";

                using (var Key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                    Key.SetValue(appName, 99999, RegistryValueKind.DWord);
            }
            catch (Exception e)
            {
                Debug.WriteLine(">\tERROR!");
                Debug.WriteLine(">\t" + e.Message + Environment.NewLine + ">>\t" + e.StackTrace);
            }
        }

        private void OnServerGet(Dictionary<string, string> param)
        {
            try
            {
                if (twitchBot != null)
                {
                    if (param.ContainsKey("access_token"))
                    {
                        twitchBot.OAuthToken = param["access_token"];
                        //twitchBot.OAuthRefreshToken = param["refresh_token"];
                        this.Invoke(new Action(() =>
                        {
                            this.Close();
                        }));
                    }
                    else
                    {

                        webBrowser.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Авторизация не удалась, попробуйте ещё раз.\n" + param["error_description"].Replace('+', ' '), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            webBrowser.Url = builder.Uri;
                            webBrowser.Refresh();
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(">\tERROR!");
                Debug.WriteLine(">\t" + e.Message + Environment.NewLine + ">>\t" + e.StackTrace);
            }
        }

        private void OAuthForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            server?.Stop();
        }

        private void OAuthForm_Load(object sender, EventArgs e)
        {
            server = new OAuthWebServer
            {
                Root = webRoot
            };
            server.OnWSMessage += OnServerGet;
            server.Start();
            if (twitchBot != null)
            {
                builder = new UriBuilder("https://id.twitch.tv/oauth2/authorize?client_id=" + twitchBot.client_id + "&redirect_uri=http://localhost:6580/index.html&response_type=token&scope=channel:moderate+chat:edit+user_read+chat:read&force_verify=true&state=" + twitchBot.client_secret);
                webBrowser.Url = builder.Uri;
                webBrowser.Refresh();

            }
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            UrlBox.Text = webBrowser.Url.ToString();
        }
    }
}
