using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ADS_Overlay_Host;
using System.Threading.Tasks;

namespace Kryp0nBots.StreamersMarketAPI
{
    public class StreamersMarketAPI
    {
        public string Username
        {
            get { return _username; }
            set
            {
                if (!IsConnected) _username = value;
            }
        }
        public string Key
        {
            get { return _password; }
            set
            {
                if (!IsConnected) _password = value;
            }
        }

        public bool IsConnected { get; private set; }



        private string _username;
        private string _password;
        private const byte salt=16;
        public StreamersMarketAPI()
        {
            IsConnected = false;
            Username = "nouser";
            Key = "nopassword";
        }

        public List<Advertisement> GetADSList()
        {
            if (!IsConnected) throw new Exception("Not connected");
            List<Advertisement> result = null;
            try
            {
                var webRequest = WebRequest.Create(@"https://streamersmarket.io/wp-json/wp/v2/campaign_offer/");
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.Timeout = 12000;
                    webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(_username + ":" + _password)));

                    using (Stream s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(s))
                        {
                            string answ = sr.ReadToEnd();
                            JArray json = JArray.Parse(answ);
                            result = new List<Advertisement>();
                            foreach(JObject obj in json)
                            {
                                string link, message,image;
                                link = (string)obj.GetValue("overlay_link");
                                message = (string)obj.GetValue("overlay_txt");
                                int interval = Convert.ToInt32((string)obj.GetValue("interval"));
                                image = (string)obj.GetValue("overlay_img");
                                int id = Convert.ToInt32(obj.GetValue("id"));
                                Advertisement ad = new Advertisement()
                                {
                                    Text = link + "     " + message,
                                    LinkToImage = image,
                                    Interval = interval,
                                    ID = id
                                };
                                result.Add(ad);
                            }
                        }
                    }
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        public void ViewsReport(Advertisement ad,int Count)
        {
            if (!IsConnected) throw new Exception("Not connected");
            string url = @"https://streamersmarket.io/wp-json/wp/v2/campaign_offer/{0}/?sm_timestamp={1}&sm_viewer_count={2}";
            url = String.Format(url,ad.ID, ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString(),Count);
            var webRequest = WebRequest.Create(url);
            if (webRequest != null)
            {
                webRequest.Method = "GET";
                webRequest.Timeout = 12000;
                webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(_username + ":" + _password)));
                //webRequest.GetResponse();
                StreamReader sr = new StreamReader(webRequest.GetResponse().GetResponseStream());
                string str = sr.ReadToEnd();
                sr.Close();
            }

        }

        public StreamersMarketAPI(string Username,string Key)
        {
            IsConnected = false;
            _username = Username;
            _password = Key;
        }
        
        public void SaveDataToRegistry(string Path)
        {
            RegistryKey currentUser = Registry.CurrentUser;
            RegistryKey reg = currentUser.CreateSubKey(Path);
            reg.SetValue("SM_username", _username, RegistryValueKind.String);
            reg.SetValue("SM_password", EncodedString(_password), RegistryValueKind.String);
        }

        public bool LoadDataFromRegistry(string Path)
        {
            try
            {
                if (IsConnected) return false;
                RegistryKey current_user = Registry.CurrentUser;
                RegistryKey currentProgramKey = current_user.OpenSubKey(Path);
                if (currentProgramKey == null) return false;
                if (currentProgramKey.GetValue("SM_username") == null || currentProgramKey.GetValue("SM_password") == null) return false;
                _username = Convert.ToString(currentProgramKey.GetValue("SM_username"));
                _password = DecodeString(Convert.ToString(currentProgramKey.GetValue("SM_password")));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string EncodedString(string InputString)
        {
            byte[] array = Encoding.UTF8.GetBytes(InputString);
            for(int i = 0; i < array.Length; i++)
            {
                array[i] ^= salt;
            }
            return ByteArrayToString(array);
        }

        private string DecodeString(string EncodedString)
        {
            EncodedString = EncodedString.Trim();
            string[] splt = EncodedString.Split(' ');
            byte[] arr = new byte[splt.Length];
            for(int i = 0; i < splt.Length; i++)
            {
                arr[i] = Convert.ToByte(splt[i]);
                arr[i] ^= salt;
            }
            return Encoding.UTF8.GetString(arr);
        }

        private string ByteArrayToString(byte[] Array)
        {
            string result = "";
            for (int i = 0; i < Array.Length; i++)
            {
                result += Array[i].ToString() + " ";
            }
            return result.Trim();
        }

        public bool Connect()
        {

            try
            {
                var webRequest = WebRequest.Create(@"https://streamersmarket.io/wp-json/wp/v2/campaign_offer/");
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.Timeout = 12000;
                    //webRequest.ContentType = "application/json";
                    webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(_username + ":" + _password)));

                    using (Stream s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(s))
                        {
                            string answ = sr.ReadToEnd();
                            JArray json = JArray.Parse(answ);
                            IsConnected = json.Count > 0;
                            return IsConnected;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
