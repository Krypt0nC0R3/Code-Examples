using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CefSharp;
using CefSharp.WinForms;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Globalization;

namespace NewsAssistant
{
    public class VK_API : IDisposable
    {

        private class OAuthForm : Form
        {
            private ChromiumWebBrowser webBrowser1;
            public VK_API parent = null;
            public string URL = "";
            public OAuthForm(VK_API parent)
            {
                this.parent = parent;
                Init();
                webBrowser1.Focus();
                try
                {
                    var appName = Process.GetCurrentProcess().ProcessName + ".exe";

                    using (var Key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                        Key.SetValue(appName, 99999, RegistryValueKind.DWord);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(">\tERROR!");
                    Debug.WriteLine(">\t" + e.Message + Environment.NewLine + ">>\t" + e.StackTrace);
                }
            }

            private void OnFormLoad(object sender,EventArgs e)
            {
                webBrowser1.Load(URL);
            }

            private void OnWebBrowserLocationChanged(object sender,EventArgs e)
            {
                if (!webBrowser1.Address.ToString().Contains("authorize"))
                {
                    List<string> param_list = webBrowser1.Address.ToString().Split('#')[1].Split('&').ToList();
                    Dictionary<string, string> param = new Dictionary<string, string>();
                    for(int i = 0; i < param_list.Count; i++)
                    {
                        param.Add(param_list[i].Split('=')[0], param_list[i].Split('=')[1]);
                        
                        
                    }
                    //Cef.Shutdown();*/

                    parent.OnServerGet(param);
                    this.Invoke(new Action(() =>
                    {
                        this.Hide();
                    }));
                }
            }

            private void Init()
            {
                this.Size = this.MinimumSize = new System.Drawing.Size(816, 485);
                this.Load += OnFormLoad;
                this.Icon = parent.formIcon;
                this.Text = "NewsAssistant - Авторизация";
                this.FormClosing+= (s, e) =>
                {
                    Cef.Shutdown();
                };

                CefSettings settings = new CefSettings();
                Cef.Initialize(settings);

                webBrowser1 = new ChromiumWebBrowser();
                webBrowser1.Location = new System.Drawing.Point(0, 0);
                webBrowser1.Dock = DockStyle.Fill;
                webBrowser1.Name = "webBrowser1";
                webBrowser1.AddressChanged += OnWebBrowserLocationChanged;
                this.Controls.Add(webBrowser1);
            }
        }

        public enum SCOPE : int
        {
            NOTIFY = 1,
            FRIENDS = 2,
            PHOTOS = 4,
            AUDIO = 8,
            VIDE0 = 16,
            STORIES = 64,
            PAGES = 128,
            LINKS = 256,
            STATUS = 1024,
            NOTES = 2048,
            MESSAGES = 4096,
            WALL = 8192,
            ADS = 32768,
            OFFLINE = 65536,
            DOCS = 131072,
            GROUPS = 262144,
            NOTIFICATIONS = 524288,
            STATS = 1048576,
            EMAIL = 4194304,
            MARKET = 1342177728
        }

        public enum DISPLAY : ushort
        {
            PAGE = 0,
            POPUP = 1
        }

        public Icon formIcon;

        private string _app_id = "";
        private string _oauth_string = "";
        private int _user_id = -1;
        private string _version = "5.126";

        public bool IsDisposable { get { return _form != null; } }

        private OAuthForm _form = null;

        public string OAuth { get { return _oauth_string; } }

        public event Authenticated OnAuthenticated;

        public delegate void Authenticated(int User_ID);

        public bool IsConnected { get; private set; } = false;

        public VK_API(string App_ID)
        {
            _app_id = App_ID;
        }

        public void OfflineAuth(string OAuth)
        {
            _oauth_string = OAuth;
            IsConnected = true;
            OnAuthenticated?.Invoke(0);
        }

        public void Authenticate(DISPLAY Display,int Scope)
        {

            string d = (Display == DISPLAY.PAGE) ? "page" : "popup";
            string URL = @"https://oauth.vk.com/authorize?client_id={0}&display={1}&lang=ru&redirect_uri=https://oauth.vk.com/blank.html&scope={2}&response_type=token&revoke=1&v=" + _version;
            URL = String.Format(URL, _app_id, d, Scope);
            _form = new OAuthForm(this);
            _form.URL = URL;
            _form.Show();
        }

        public bool CreatePostOnWall(int To,string Text,string Link,string photo,DateTime How)
        {
            string url = @"https://api.vk.com/method/wall.post?{0}access_token={1}&v="+_version;
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("owner_id", To.ToString()); ;
            param.Add("from_group", "1");
            param.Add("attachments", photo+","+Link);
            param.Add("message", Text);
            param.Add("publish_date", ((DateTimeOffset)How).ToUnixTimeSeconds().ToString());

            string params_string = "";
            for(int i = 0; i < param.Count; i++)
            {
                params_string += param.Keys.ElementAt(i) + "=" + param.Values.ElementAt(i) + "&";
            }
            url = String.Format(url, params_string, _oauth_string);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.Accept = "application/json";
            request.UserAgent = "Mozilla/5.0 ....";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            StringBuilder output = new StringBuilder();
            output.Append(reader.ReadToEnd());
            response.Close();
            JObject json = JObject.Parse(output.ToString());
            return !json.ContainsKey("error");
        }

        private void OnServerGet(Dictionary<string, string> param)
        {
            try
            {
                if (param.ContainsKey("access_token") && param.ContainsKey("user_id"))
                {
                    _user_id = Convert.ToInt32(param["user_id"]);
                    _oauth_string = param["access_token"];
                    IsConnected = true;
                    OnAuthenticated?.Invoke(_user_id);
                    
                }
                else
                {
                    IsConnected = false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(">\tERROR!");
                Debug.WriteLine(">\t" + e.Message + Environment.NewLine + ">>\t" + e.StackTrace);
            }
        }

        public DateTime GetLastPostponed(int To)
        {
            string url = @"https://api.vk.com/method/wall.get?{0}access_token={1}&v=" + _version;
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("owner_id", To.ToString());
            param.Add("count", 20.ToString());
            param.Add("filter", "postponed");


            string params_string = "";
            for (int i = 0; i < param.Count; i++)
            {
                params_string += param.Keys.ElementAt(i) + "=" + param.Values.ElementAt(i) + "&";
            }
            url = String.Format(url, params_string, _oauth_string);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.Accept = "application/json";
            request.UserAgent = "Mozilla/5.0 ....";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            StringBuilder output = new StringBuilder();
            output.Append(reader.ReadToEnd());
            response.Close();
            JObject json = JObject.Parse(output.ToString());
            JArray array = (JArray)(((JObject)json.GetValue("response")).GetValue("items"));
            DateTime result = DateTime.Parse("1.1.1970 00:00");
            foreach(JObject item in array)
            {
                DateTime time = UnixTimeStampToDateTime((double)item.GetValue("date"));
                if (time > result) result = time;
            }
            return result;
        }

        public int GetUserID()
        {
            string url = @"https://api.vk.com/method/account.getProfileInfo?{0}access_token={1}&v=" + _version;
            Dictionary<string, string> param = new Dictionary<string, string>();

            string params_string = "";
            url = String.Format(url, params_string, _oauth_string);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.Accept = "application/json";
            request.UserAgent = "Mozilla/5.0 ....";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            StringBuilder output = new StringBuilder();
            output.Append(reader.ReadToEnd());
            response.Close();
            JObject json = JObject.Parse(output.ToString());

            return (int)((JObject)json.GetValue("response")).GetValue("id");
        }

        public string UploadBitmap(int Owner_ID,Bitmap Image)
        {
            string url = @"https://api.vk.com/method/photos.getWallUploadServer?{0}access_token={1}&lang=ru&v=" + _version;
            Dictionary<string, string> param = new Dictionary<string, string>();
            int owner_id = (Owner_ID > 0) ? Owner_ID : -Owner_ID;
            param.Add("owner_id", (owner_id).ToString());

            string params_string = "";
            for (int i = 0; i < param.Count; i++)
            {
                params_string += param.Keys.ElementAt(i) + "=" + param.Values.ElementAt(i) + "&";
            }
            url = String.Format(url, params_string, _oauth_string);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.Accept = "application/json";
            request.UserAgent = "Mozilla/5.0 ....";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            StringBuilder output = new StringBuilder();
            output.Append(reader.ReadToEnd());
            reader.Close();
            response.Close();
            JObject json = JObject.Parse(output.ToString());

            url = (string)((JObject)json.GetValue("response")).GetValue("upload_url");










            MemoryStream ms = new MemoryStream();
            Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new ByteArrayContent(ms.ToArray(), 0, ms.ToArray().Length), "photo", "I_HATE_THAT_API.jpg");
            HttpResponseMessage responsee = httpClient.PostAsync(url, form).Result;

            httpClient.Dispose();
            string str = responsee.Content.ReadAsStringAsync().Result;





            json = JObject.Parse(str);




            url = @"https://api.vk.com/method/photos.saveWallPhoto?{0}access_token={1}&lang=ru&v=" + _version;
            param = new Dictionary<string, string>();
            //param.Add("group_id", (-owner_id).ToString());
            string sss = json.GetValue("photo").ToString();
            param.Add("photo", sss);
            param.Add("server", (string)json.GetValue("server"));
            param.Add("hash", (string)json.GetValue("hash"));

            params_string = "";
            for (int i = 0; i < param.Count; i++)
            {
                params_string += param.Keys.ElementAt(i) + "=" + param.Values.ElementAt(i) + "&";
            }
            url = String.Format(url, params_string, _oauth_string);

            request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.Accept = "application/json";
            request.UserAgent = "Mozilla/5.0 ....";

            response = (HttpWebResponse)request.GetResponse();
            reader = new StreamReader(response.GetResponseStream());
            output = new StringBuilder();
            output.Append(reader.ReadToEnd());
            response.Close();
            json = JObject.Parse(output.ToString());
            JArray ar = (JArray)json.GetValue("response");

            int pID = (int)(ar.ElementAt(0).Value<int>("id"));
            int oID = (int)(ar.ElementAt(0).Value<int>("owner_id"));

            return "photo"+oID+"_"+pID;
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public void Dispose()
        {
            _form?.Close();
            _form?.Dispose();
            _form = null;
        }
    }
}
