using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kryp0nBots.OAuth;
using WebSocketSharp;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;
using Microsoft.Win32;

namespace Kryp0nBots.Twitch
{
    public class Krypt0nTwitchBot
    {
        /// <summary>
        /// Twitch API Client ID
        /// </summary>
        public string client_id = "u61ewzlqe0aaooi84qutm2tue68sw9";
        /// <summary>
        /// Twitch API chat WebSoket Server
        /// </summary>
        public string wsServer = "wss://irc-ws.chat.twitch.tv:443";
        /// <summary>
        /// Twitch username
        /// </summary>
        public string username = null;

        public long userID = 0;

        public string client_secret = "liano8m8ggl5ian2i7me30u1to54h8";


        private OAuthForm oAuth = null;
        private WebSocket webSocket = null;
        /// <summary>
        /// Twitch API OAuth token
        /// </summary>
        public string OAuthToken = "";

        public string OAuthRefreshToken = "";
        /// <summary>
        /// Default chat channel
        /// </summary>
        private string defaultChannel = null;

        public bool IsConnected { get; private set; }
        public Krypt0nTwitchBot(string channel = null)
        {
            defaultChannel = channel;
            IsConnected = false;
        }

        /// <summary>
        /// Connect to Twitch chat Websoket server and connect to default channel, if exist
        /// </summary>
        /// <returns>Return true if success</returns>
        public bool Connect()
        {
            try
            {
                if (username == null) return false;
                webSocket = new WebSocket(wsServer);
                webSocket.OnMessage += (sender, e) =>
                {
                    OnWsMessage(e.Data);
                };
                webSocket.Connect();
                SendRaw("PASS oauth:" + OAuthToken);
                SendRaw("NICK " + username.ToLower());
                if (defaultChannel != null)
                {
                    JoinChannel(defaultChannel.ToLower());
                }
                else
                {
                    JoinChannel(username);
                }
                IsConnected = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SendRaw(string message)
        {
            Debug.WriteLine("> "+message);
            webSocket.Send(message);
        }

        private void OnWsMessage(string data)
        {
            Debug.WriteLine("< " + data);
            if(data.StartsWith("PING"))
            {
                SendRaw("PONG :tmi.twitch.tv");
            }
            if(data.Contains("NOTICE * :Login unsuccessful"))
            {
                IsConnected = false;
            }
            if(data.Contains(" :Welcome, GLHF!"))
            {
                IsConnected = true;
            }
        }


        public bool RefreshOAuth()
        {
            try
            {
                WebRequest request = WebRequest.Create("https://id.twitch.tv/oauth2/token");
                request.Method = "POST"; // для отправки используется метод Post
                                         // данные для отправки
                string data = "grant_type=refresh_token&refresh_token=" + OAuthRefreshToken + "&client_id=" + client_id + "&client_secret=" + client_secret;
                // преобразуем данные в массив байтов
                byte[] byteArray = Encoding.UTF8.GetBytes(data);
                // устанавливаем тип содержимого - параметр ContentType
                request.ContentType = "application/x-www-form-urlencoded";
                // Устанавливаем заголовок Content-Length запроса - свойство ContentLength
                request.ContentLength = byteArray.Length;

                //записываем данные в поток запроса
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
                JObject answer;
                WebResponse response = request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        answer = JObject.Parse(reader.ReadToEnd());
                    }
                }
                response.Close();
                if (answer.ContainsKey("error")) return false;
                OAuthRefreshToken = answer["refresh_token"].ToString();
                OAuthToken = answer["access_token"].ToString();
                return true;
            }
            catch(Exception exp)
            {
                Debug.WriteLine(">  ERROR!");
                Debug.WriteLine("> "+exp.Message+Environment.NewLine+"> "+exp.StackTrace);
                return false;
            }
        }
        /// <summary>
        /// Change current chat channel
        /// </summary>
        /// <param name="name">Channel name</param>
        public void JoinChannel(string name)
        {
            if (webSocket == null) return;
            if (defaultChannel != null)
            {
                SendRaw("PART #" + defaultChannel);
            }
            defaultChannel = name;
            SendRaw("JOIN #" + defaultChannel);
        }

        /// <summary>
        /// Disconnect to Twithc chat Websoket server
        /// </summary>
        public void Disconnect()
        {
            if (defaultChannel != null)
            {
                SendRaw("PART #" + defaultChannel);
            }
            webSocket.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(string message)
        {
            if (webSocket == null) return false;
            if (defaultChannel != null)
            {
                message = (!String.IsNullOrEmpty(message)) ? message : "KappaPride";
                SendRaw("PRIVMSG #"+defaultChannel+" :"+message);
            }
            return false;
        }

        public void Clear()
        {
            if(IsConnected) Disconnect();
            username = null;
            defaultChannel = null;
            webSocket = null;
            IsConnected = true;
            oAuth = null;
        }
        public bool ClearChat()
        {
            if(webSocket == null) return false;
            if (defaultChannel != null)
            {
                SendRaw("PRIVMSG #" + defaultChannel + " :/clear");
            }
            return false;
        }
        public bool SlowChat(bool on, int time = 60)
        {
            if (webSocket == null) return false;
            if (defaultChannel != null)
            {
                if(on)
                    SendRaw("PRIVMSG #" + defaultChannel + " :/slow "+time.ToString());
                else
                    SendRaw("PRIVMSG #" + defaultChannel + " :/slowoff");
            }
            return false;
        }
        public bool SubModChat(bool on)
        {
            if (webSocket == null) return false;
            if (defaultChannel != null)
            {
                if (on)
                    SendRaw("PRIVMSG #" + defaultChannel + " :/subscribers");
                else
                    SendRaw("PRIVMSG #" + defaultChannel + " :/subscribersoff");
            }
            return false;
        }

        public void SaveData(string path)
        {
            RegistryKey currentUser = Registry.CurrentUser;
            RegistryKey key = currentUser.CreateSubKey(path);
            key.SetValue("OAuthToken", OAuthToken, RegistryValueKind.String);
            key.SetValue("OAuthRefreshToken", OAuthRefreshToken, RegistryValueKind.String);
            key.SetValue("defaultChannel", defaultChannel ?? "-1", RegistryValueKind.String);

            /*string dir = path.Substring(0, path.LastIndexOf('\\'));

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (File.Exists(path)) File.Delete(path);
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine(OAuthToken);
            sw.WriteLine(OAuthRefreshToken);
            sw.WriteLine(defaultChannel ?? "-1");
            sw.Close();*/
        }

        public bool LoadData(string path)
        {
            RegistryKey currentuser = Registry.CurrentUser;
            RegistryKey key = currentuser.OpenSubKey(path);
            if (key == null) return false;
            if (key.GetValue("OAuthToken") == null || key.GetValue("OAuthRefreshToken") == null || key.GetValue("defaultChannel") == null) return false;
            OAuthToken = Convert.ToString(key.GetValue("OAuthToken"));
            OAuthRefreshToken = Convert.ToString(key.GetValue("OAuthRefreshToken"));
            defaultChannel = Convert.ToString(key.GetValue("defaultChannel"));
            defaultChannel = (!defaultChannel.Equals("-1")) ? defaultChannel : null;
            GetUserByOAuth();
            JoinChannel(defaultChannel);
            return true;
        }

        public void GetOAuth()
        {
            oAuth = new OAuthForm
            {
                twitchBot = this
            };
            oAuth.ShowDialog();
            GetUserByOAuth();
        }

        public int GetVieversCount()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (HttpClient httpClient = new HttpClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.twitch.tv/kraken/streams/"+userID.ToString()))
                    {
                        request.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
                        request.Headers.Add("Client-ID", client_id);

                        HttpResponseMessage response = httpClient.SendAsync(request).Result;
                        JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        int result = -1;
                        if (json["stream"].HasValues)
                        {
                            result = (int)json["stream"]["viewers"];
                        }
                        return result;
                    }
                }
            }
            catch (AggregateException err)
            {
                foreach (var errInner in err.InnerExceptions)
                {
                    Debug.WriteLine(">\tERROR!");
                    Debug.WriteLine(">\t" + errInner.InnerException.Message + Environment.NewLine + ">>\t" + errInner.InnerException.StackTrace); //this will call ToString() on the inner execption and get you message, stacktrace and you could perhaps drill down further into the inner exception of it if necessary 
                }
                return -1;
            }
        }

        public void GetUserByOAuth()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (HttpClient httpClient = new HttpClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.twitch.tv/kraken/user"))
                    {
                        request.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
                        request.Headers.Add("Client-ID", client_id);
                        request.Headers.Add("Authorization", "OAuth " + OAuthToken);
                        
                        HttpResponseMessage response = httpClient.SendAsync(request).Result;
                        JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        username = (string)json.SelectToken("name");
                        userID = (long)json.SelectToken("_id");
                    }
                }
            }
            catch (AggregateException err)
            {
                foreach (var errInner in err.InnerExceptions)
                {
                    Debug.WriteLine(">\tERROR!");
                    Debug.WriteLine(">\t"+errInner.InnerException.Message + Environment.NewLine + ">>\t"+errInner.InnerException.StackTrace); //this will call ToString() on the inner execption and get you message, stacktrace and you could perhaps drill down further into the inner exception of it if necessary 
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(exp.Message + Environment.NewLine + exp.StackTrace, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
