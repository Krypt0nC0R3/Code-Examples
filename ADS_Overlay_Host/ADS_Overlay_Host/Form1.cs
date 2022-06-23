using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kryp0nBots.Twitch;
using Kryp0nBots.StreamersMarketAPI;
using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using Krypt0nWebServer;
using Microsoft.Win32;
using Krypt0n.Tools;

namespace ADS_Overlay_Host
{
    public partial class Form1 : Form
    {
        protected OBSWebsocket obs; // https://github.com/Palakis/obs-websocket
        private const string language_folder = @"lang/"; //folder with language files
        private const string default_language = "en";
        private readonly Dictionary<string, string> current_language = new Dictionary<string, string>();
        private readonly List<string> available_langs = new List<string>();
        private GetSceneListInfo listInfo;
        public System.Version Version = new System.Version("0.4.5");
        private WebServer webServer;
        public readonly int AD_Delay = 60; //sec
        private Advertisement currentAD = null;
        private Target target = null;

        private bool IsRunning = false;

        private Thread ADThread = null;
        private Thread ShowADThread = null;
        private Thread UpdateThread = null;

        public Dictionary<Advertisement, DateTime> advertismentList;

        public Updater updater = null;

        #region [Messages text]
        private string OBSNotRunning = String.Empty;
        private string Error = String.Empty;
        private string Warning = String.Empty;
        private string TextCopied = String.Empty;
        private string StartProgram = String.Empty;
        private string StopProgram = String.Empty;
        private string LoadTargetItemError = String.Empty;
        #endregion

        public Krypt0nTwitchBot twitchBot = new Krypt0nTwitchBot();
        public StreamersMarketAPI smAPI = new StreamersMarketAPI();

        public readonly string OAuthDataPath = @"Krypt0nSoft\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName + @"\OAuth\";
        public readonly string SavedDataPath = @"Krypt0nSoft\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName + @"\PersonalSettings\";
        public readonly string StreamersMarketDataPath = @"Krypt0nSoft\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName+ @"\StreamersMarket\";
#if (DEBUG)
        public readonly string WebDataPath = @"..\..\web\";
#else
        public readonly string WebDataPath = @"data\web\";
#endif

        public bool allConnected = false;

        public int lastAdID = 0;

        public int runCount = 0;
        public Form1()
        {
            InitializeComponent();
            notifyIcon1.Text = "StreamersMarket";
            try
            {
                #region [Copy check]
                    if(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
                    {
                        if (LastLaunchWasError())
                        {
                            //MessageBox.Show("", "Warning", MessageBoxButtons.OK,MessageBoxIcon.Warning);
                            RegistryKey current_user = Registry.CurrentUser;
                            RegistryKey currentProgramKey = current_user.CreateSubKey(@"Krypt0nSoft\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName);
                            currentProgramKey.SetValue("ErrorHasOccurred", false);
                            Process[] _processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location));
                            foreach (Process process in _processes)
                            {
                                if(process.Id!=Process.GetCurrentProcess().Id)
                                    process.Kill();
                            }
                        }
                        else
                        {
                            Exit();
                        }                    
                    }
                #endregion


                #region [Language]
                FileInfo[] files = new DirectoryInfo(language_folder).GetFiles("*.lang");
                    available_langs.Clear();
                    foreach(FileInfo fileInfo in files)
                    {
                        available_langs.Add(fileInfo.Name.Substring(0,fileInfo.Name.IndexOf('.')));
                    }
                    
                    if (available_langs.Contains(default_language))
                    {
                        StreamReader sr = new StreamReader(Environment.CurrentDirectory+ "\\" + language_folder + default_language + ".lang");
                        string file = sr.ReadToEnd();
                        sr.Close();
                        string[] tmp = file.Split('\n');
                        foreach(string arg in tmp)
                        {
                            current_language.Add(arg.Split('=')[0], arg.Split('=')[1].Trim().Replace("\\n","\n").Replace("\\t","\t"));
                        }
                        ApplyLang();
                        TextCopied = GetLocalizedMessage("$TextCopied");
                        StartProgram = GetLocalizedMessage("$StartOverlay");
                        StopProgram = GetLocalizedMessage("$StopOverlay");
                        LoadTargetItemError = GetLocalizedMessage("$LoadTargetItemError");
                        Warning = GetLocalizedMessage("$Warning");
                    }
                    else
                    {
                        throw new Exception("Default language ("+default_language+".lang) not founded!");
                    }
                #endregion

                #region [OBS]
                    Process[] processes = Process.GetProcesses().Where(x => x.ProcessName.ToLower().StartsWith("obs")).ToArray();
                    Error = "$Error";
                    if (current_language.ContainsKey("[$Error]")) Error = current_language["[$Error]"];
                    if (processes.Length != 0)
                    {
                        InitOBS();
                    }
                    else
                    {
                        LockForm(false);
                        string key = "$OBSNotRunning";
                        OBSNotRunning = key;
                        if (current_language.ContainsKey(GetSylizedKey(key))) OBSNotRunning = current_language[GetSylizedKey(key)];
                        /*System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                        timer.Interval = 250;
                        timer.Tick += (sender, e) =>
                        {
                            Process[] process = Process.GetProcesses().Where(x => x.ProcessName.ToLower().StartsWith("obs")).ToArray();
                            if (process.Length != 0)
                            {
                                ((System.Windows.Forms.Timer)sender).Stop();
                                ((System.Windows.Forms.Timer)sender).Enabled = false;
                                InitOBS();
                                ((System.Windows.Forms.Timer)sender).Dispose();

                            }
                        };
                        timer.Enabled = true;
                        timer.Start();*/
                        MessageBox.Show(OBSNotRunning,Error,MessageBoxButtons.OK,MessageBoxIcon.Error);
                        Exit();
                    }
                #endregion



                #region [Twitch bot]
                twitchBot = new Krypt0nTwitchBot();
                    
                #endregion

                #region [LogIn]
                    LogIn logInForm = new LogIn();
                    logInForm.Text = (current_language.ContainsKey(GetSylizedKey("$Title"))) ? current_language[GetSylizedKey("$Title")] : "$Title";
                    logInForm.Text += " " + Version;
                    if (current_language.ContainsKey("[$TwitchOk]")) logInForm.TwitchOK = current_language["[$TwitchOk]"];
                    if (current_language.ContainsKey("[$TwitchError]")) logInForm.TwitchError = current_language["[$TwitchError]"];
                    logInForm.WebOk = GetLocalizedMessage("$WebOk");
                    logInForm.WebError = GetLocalizedMessage("$WebError");
                    logInForm.Error = (String.IsNullOrEmpty(Error)) ? "$Error" : Error;
                    if (current_language.ContainsKey("[$BotNotConnected]")) logInForm.BotNotConnected = current_language["[$BotNotConnected]"];
                    foreach (Control control in logInForm.Controls)
                    {
                        ApplyLangForControl(control);

                        
                    }
                    logInForm.MainForm = this;
                    logInForm.ShowDialog();
                    if (!allConnected) Exit();
                    logInForm.Dispose();
                    CheckADsList();


                #endregion

                #region [Web]
                webServer = new WebServer
                    {
                        Port = 8060,
                        Root = WebDataPath
                    };
                    webServer.Start();
                    webServer.SendWSMessage("Clear");
                #endregion

                #region [Notify]
                    notifyIcon1.Visible = false;
                    notifyIcon1.Text = "StreamersMarket" + Version.ToString();
                    notifyIcon1.BalloonTipTitle = this.Text;
                    notifyIcon1.MouseClick += (sender,e) =>
                    {
                        if(e.Button==MouseButtons.Left)
                            ShowForm();
                    };
                #endregion
            }
            catch (Exception exp)
            {
                Debug.WriteLine(">  ERROR!");
                Debug.WriteLine("> "+exp.Message+Environment.NewLine+">  "+exp.StackTrace);
                MessageBox.Show(exp.Message+Environment.NewLine+exp.StackTrace, Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorHasOccurred();
                Exit();
            }

        }

        private void CheckADsList()
        {
            List<Advertisement> ads = smAPI.GetADSList();
            advertismentList = new Dictionary<Advertisement, DateTime>();
            for(int i = 0; i < ads.Count; i++)
            {
                advertismentList.Add(ads[i], DateTime.Now.AddMinutes(i * 5));
            }
        }

        private bool LastLaunchWasError()
        {
            RegistryKey current_user = Registry.CurrentUser;
            RegistryKey currentProgramKey = current_user.OpenSubKey(@"Krypt0nSoft\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            if (currentProgramKey == null) return false;
            if (currentProgramKey.GetValue("ErrorHasOccurred") == null) return false;
            return Convert.ToBoolean(currentProgramKey.GetValue("ErrorHasOccurred", false));
        }

        public void ErrorHasOccurred()
        {
            RegistryKey current_user = Registry.CurrentUser;
            RegistryKey currentProgramKey = current_user.CreateSubKey(@"Krypt0nSoft\"+ System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            currentProgramKey.SetValue("ErrorHasOccurred", true);
        }

        public string GetLocalizedMessage(string key)
        {
            if (current_language.ContainsKey(GetSylizedKey(key))) return current_language[GetSylizedKey(key)]; else return key;
        }
        
        private void LockForm(bool state)
        {
            mediaLinkGroupBox.Enabled = scenesGroupBox.Enabled = state;
            SceneListBox.SelectedIndices.Clear();
            sceneItemListBox.SelectedIndices.Clear();
        }

        private void SetVisibleTarget(bool visible)
        {
            obs.SetItemVisible(target?.Item.SourceName, target?.Scene.Name, visible);
        }

        private void InitOBS()
        {
            //Thread.Sleep(1000);
            LockForm(true);
            try
            {
                obs = new OBSWebsocket();
                bool isConneced = false;
                obs.Connected+=(sndr,e) =>
                {
                    isConneced = true;
                };
                obs.WSTimeout = new TimeSpan(0, 0, 5);
                obs.OnWSError += (sndr,e) =>
                {
                    MessageBox.Show(e.Exception.Message + "\n" + e.Exception.StackTrace, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw e.Exception;
                };
                obs.Connect("ws://" + IPAddress.Loopback.ToString() + ":4444", String.Empty);
                
                byte delay = 0;
                while (!isConneced && delay < 250)
                {
                    delay++;
                    Thread.Sleep(50);
                }
                if (!isConneced) throw new Exception();
                obs.OBSExit += (object sender, EventArgs e) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        LockForm(false);
                    }));

                };
                UpdateScenes();
                target = LoadData();
            }
            catch
            {
                if(MessageBox.Show("Can't find the obs-websocket server plugin. Please refer to the instructions or to those streamersmarket support. Would you like to open the plugin download page?", "Critical error!", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    Process.Start(@"https://obsproject.com/forum/resources/obs-websocket-remote-control-obs-studio-from-websockets.466/");
                }
                ErrorHasOccurred();
                this.Exit();
            }
        }

        private void UpdateScenes()
        {
            listInfo = obs.GetSceneList();
            SceneListBox.Items.Clear();
            for (int i = 0; i < listInfo.Scenes.Count; i++)
            {
                SceneListBox.Items.Add(listInfo.Scenes[i].Name);
            }
            if (SceneListBox.Items.Count > 0) SceneListBox.Items[0].Selected = true;
        }

        private void Exit()
        {
            webServer?.Stop();
            
            if (System.Windows.Forms.Application.MessageLoop)
            {
                // WinForms app
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                // Console app
                System.Environment.Exit(1);
            }
        }

        public string GetSylizedKey(string key)
        {
            return "[" + key + "]";
        }

        public Size GetStringLenghtInPx(string data)
        {
            Size size = TextRenderer.MeasureText(data, this.Font);
            return size;
        }

        private void ApplyLang()
        {
            //toolStripComboBox1.Items.Clear();
            /*foreach(string language in available_langs)
            {
                toolStripComboBox1.Items.Add(language);
            }
            if (available_langs.Count > 0) toolStripComboBox1.SelectedIndex = 0;*/
            this.Text = GetLocalizedMessage("$Title");
            this.Text += " " + Version;
            foreach (Control control in this.Controls)
            {
                ApplyLangForControl(control);
                control.TextChanged += (object sender, EventArgs e) =>
                {
                    Size sz = TextRenderer.MeasureText(((Control)sender).Text, ((Control)sender).Font);
                    ((Control)sender).Width = sz.Width;
                    ((Control)sender).MinimumSize = sz;
                };
            }
        }

        private void ApplyLangForControl(object control)
        {
            if(control is MenuStrip)
            {
                foreach(ToolStripMenuItem item in ((MenuStrip)control).Items)
                {
                    ApplyLangForControl(item);
                }
            }
            else if(control is ToolStripMenuItem)
            {
                for(int i=0;i< ((ToolStripMenuItem)control).DropDownItems.Count; i++)
                {
                    if(((ToolStripMenuItem)control).DropDownItems[i] is ToolStripMenuItem)
                    {
                        ApplyLangForControl(((ToolStripMenuItem)control).DropDownItems[i]);
                    }
                }
                if (((ToolStripMenuItem)control).Tag != null)
                {
                    if (!String.IsNullOrEmpty(((ToolStripMenuItem)control).Tag.ToString()))
                    {
                        ((ToolStripMenuItem)control).Text = GetLocalizedMessage(((ToolStripMenuItem)control).Tag.ToString());
                        //((ToolStripMenuItem)control).Width = Convert.ToInt32(GetStringLenghtInPx(((ToolStripMenuItem)control).Text).Width) + 10;
                    }
                }
            }
            else if (control is ToolStripDropDownItem)
            {
                foreach (ToolStripMenuItem dropDownItem in ((ToolStripMenuItem)control).DropDownItems)
                {
                    ApplyLangForControl(dropDownItem);
                }
                if(((ToolStripDropDownItem)control).Tag != null)
                {
                    if (!String.IsNullOrEmpty(GetSylizedKey(((ToolStripDropDownItem)control).Tag.ToString())))
                    {
                        ((ToolStripDropDownItem)control).Text = GetLocalizedMessage(((ToolStripDropDownItem)control).Tag.ToString());
                        //((ToolStripDropDownItem)control).Width = Convert.ToInt32(GetStringLenghtInPx(((ToolStripDropDownItem)control).Text).Width) + 10;
                    }
                }
            }
            else if(control is GroupBox)
            {
                foreach(Control ctrl in ((GroupBox)control).Controls)
                {
                    ApplyLangForControl(ctrl);
                }
                if (((Control)control).Tag!=null && !String.IsNullOrEmpty(((Control)control).Tag.ToString()))
                {
                    ((Control)control).Text = GetLocalizedMessage(((Control)control).Tag.ToString());
                    //((Control)control).Width = Convert.ToInt32(GetStringLenghtInPx(((Control)control).Text).Width) + 10;
                }
            }
            else if(control is ContextMenuStrip)
            {
                foreach(ToolStripMenuItem ctrl in ((ContextMenuStrip)control).Items)
                {
                    ApplyLangForControl(ctrl);
                }
            }
            else if(control is NotifyIcon)
            {
                ApplyLangForControl(((NotifyIcon)control).ContextMenuStrip);
            }
            else
            {
                if (((Control)control).Tag != null)
                {
                    if (!String.IsNullOrEmpty(((Control)control).Tag.ToString()))
                    {
                        ((Control)control).Text = GetLocalizedMessage(((Control)control).Tag.ToString());
                        //((Control)control).Width = Convert.ToInt32(GetStringLenghtInPx(((Control)control).Text).Width) + 10;
                    }
                    
                }
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*if (toolStripComboBox1.Text != default_language)
            {
                StreamReader sr = new StreamReader(Environment.CurrentDirectory + "\\" + language_folder + toolStripComboBox1.Text + ".lang");
                string file = sr.ReadToEnd();
                sr.Close();
                string[] tmp = file.Split('\n');
                foreach (string arg in tmp)
                {
                    current_language.Add(arg.Split('=')[0], arg.Split('=')[1].Trim());
                }
                string key = "$Title";
                this.Text = (current_language.ContainsKey(GetSylizedKey(key))) ? current_language[GetSylizedKey(key)] : key;
                this.Text += " " + Version;
                foreach (Control control in this.Controls)
                {
                    ApplyLangForControl(control);
                }
            }*/
        }

        private void UpdateSceneItems()
        {
            OBSScene scene = listInfo.Scenes.Find(x => x.Name == SceneListBox.SelectedItems[0].Text);
            sceneItemListBox.Items.Clear();
            int index = scene.Items.FindIndex(x => x.SourceName.StartsWith("[ADS]"));
            for (int i = 0; i < scene.Items.Count; i++)
            {
                sceneItemListBox.Items.Add(scene.Items[i].SourceName);
                if (scene.Items[i].SourceName.StartsWith("[ADS]"))
                    sceneItemListBox.Items[sceneItemListBox.Items.Count-1].ForeColor = Color.DarkGreen;
            }
            if (index != -1) sceneItemListBox.Items[index].Selected = true; else sceneItemListBox.Items[0].Selected = true;
        }

        private void SceneListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (SceneListBox.SelectedItems.Count > 0)
                UpdateSceneItems();
        }

        private void sceneItemListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(((TextBox)sender).Text);
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipText = TextCopied;
            notifyIcon1.ShowBalloonTip(3000);
            new Thread(() =>
            {
                Thread.Sleep(3100);
                notifyIcon1.Visible = false;
            }).Start();
            
        }

        private void applyTargetBtn_Click(object sender, EventArgs e)
        {
            OBSScene scene = listInfo.Scenes.Find(x => x.Name == SceneListBox.SelectedItems[0].Text);
            target.Scene = scene;
            target.Item = scene.Items[sceneItemListBox.SelectedIndices[0]];
            new Thread(new ThreadStart(()=>
            {
                int delay = 250;
                SetVisibleTarget(true);
                Thread.Sleep(delay);
                webServer.SendWSMessage("success.png");
                Thread.Sleep(3500 - (2 * delay));
                webServer.SendWSMessage("Clear");
                Thread.Sleep(delay);
                SetVisibleTarget(false);
            })).Start();
            SaveData();
            applyTargetBtn.ForeColor = Color.DarkGreen;
        }

        private void SaveData()
        {
            try
            {
                if (target != null && target.Scene != null && target.Item != null)
                {
                    RegistryKey currentUser = Registry.CurrentUser;
                    RegistryKey key = currentUser.CreateSubKey(SavedDataPath);
                    key.SetValue("sceneName", target.Scene.Name, RegistryValueKind.String);
                    key.SetValue("sourceName", target.Item.SourceName, RegistryValueKind.String);
                }
            }
            catch
            {
                MessageBox.Show(String.Format("The program cannot access registry key {0}.\nTry to run the program as administrator or move it to another location.","HKEY_CURRENT_USER\\"+SavedDataPath),"Error!",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                ErrorHasOccurred();
            }
        }

        private Target LoadData()
        {
            try
            {
                applyTargetBtn.ForeColor = Color.DarkRed;
                Target result = new Target(); ;

                RegistryKey currentUser = Registry.CurrentUser;
                RegistryKey key = currentUser.OpenSubKey(SavedDataPath);
                if (key == null) return result;
                if (key.GetValue("sceneName") == null || key.GetValue("sourceName") == null) return result;

                string scene = Convert.ToString(key.GetValue("sceneName")).Trim();
                string item = Convert.ToString(key.GetValue("sourceName")).Trim();

                if (obs.GetSceneList().Scenes.FindIndex(x => x.Name == scene) != -1)
                {
                    result.Scene = obs.GetSceneList().Scenes.Find(x => x.Name == scene);
                    if (result.Scene.Items.FindIndex(x => x.SourceName == item) != -1)
                    {
                        result.Item = result.Scene.Items.Find(x => x.SourceName == item);
                        applyTargetBtn.ForeColor = Color.DarkGreen;
                    }
                }
                else
                {
                    MessageBox.Show(String.Format(LoadTargetItemError, scene), Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }



                /*if (File.Exists(SavedDataPath))
                {
                    StreamReader sr = new StreamReader(SavedDataPath);
                    string scene = sr.ReadLine().Trim();
                    string item = sr.ReadLine().Trim();
                    if (obs.GetSceneList().Scenes.FindIndex(x => x.Name == scene) != -1)
                    {
                        result.Scene = obs.GetSceneList().Scenes.Find(x => x.Name == scene);
                        if (result.Scene.Items.FindIndex(x => x.SourceName == item) != -1)
                        {
                            result.Item = result.Scene.Items.Find(x => x.SourceName == item);
                            applyTargetBtn.ForeColor = Color.DarkGreen;
                        }
                    }
                    else
                    {
                        MessageBox.Show(String.Format(LoadTargetItemError,scene), Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    
                    result.targetScene.Name = ;
                    result.targetItem.SourceName = sr.ReadLine().Trim();
                    
                }*/
                return result;
            }
            catch (Exception exp)
            {
                Debug.WriteLine(">  ERROR!");
                Debug.WriteLine("> " + exp.Message + Environment.NewLine + ">  " + exp.StackTrace);
                MessageBox.Show(exp.Message + Environment.NewLine + exp.StackTrace,Error,MessageBoxButtons.OK,MessageBoxIcon.Error);

                ErrorHasOccurred();
                Exit();
                return new Target(); ;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!IsRunning)
            {
                this.Hide();
                notifyIcon1.Visible = true;
                startADS.Text = StopProgram;
                IsRunning = true;

                ADThread = new Thread(() =>
                {
                    while (true) {


                        int id = lastAdID++;
                        Advertisement ad = advertismentList.Keys.ElementAt(id++);
                        int timeToSleep = 0;
                        Debug.WriteLine("\n\n\nRunCount: "+runCount.ToString());
                        if (runCount == 0)
                        {

                            timeToSleep = 5;
                        }
                        else
                        {
                            Debug.WriteLine("\n\n\nIs counted: " + (ad.Interval >= ((advertismentList.Count + 1) * 5)).ToString());
                            if (ad.Interval >= ((advertismentList.Count + 1) * 5))
                            {
                                TimeSpan offset = advertismentList.Values.ElementAt(id).AddMinutes(advertismentList.Keys.ElementAt(id).Interval) - DateTime.Now;
                                int mins = (int)Math.Truncate(offset.TotalMinutes);
                                //Debug.WriteLine("\n\n\nMins: " + mins );
                                if (mins < 0) timeToSleep = 5;
                                else timeToSleep = mins-5;
                            }
                            else
                            {
                                timeToSleep = 5;
                            }
                            
                        }

                        Debug.WriteLine("Time to sleep: "+timeToSleep);
                        Thread.Sleep(timeToSleep * 60000);
                        ShowAD(ad);
                        advertismentList[ad] = DateTime.Now;



                        if (lastAdID == advertismentList.Count)
                        {
                            runCount++;
                            lastAdID = 0;
                        }
                        
                    }
                });
                CheckADsList();
                ADThread.Start();
            }
            else
            {
                notifyIcon1.Visible = false;
                startADS.Text = StartProgram;
                ADThread?.Abort();
                ADThread = null;
                SetVisibleTarget(false);
                webServer.SendWSMessage("Clear");
                twitchBot.SubModChat(false);
                twitchBot.SlowChat(false);
                this.Invoke(new Action(() =>
                {
                    timer1.Stop();
                }));
                IsRunning = false;
            }
            
        }

        private void ShowForm()
        {
            this.Show();
            notifyIcon1.Visible = false;
        }
        

        private void ShowAD(Advertisement AD)
        {
            try
            {
                if (String.IsNullOrEmpty(AD.LinkToImage)) throw new Exception("AD's URL is empty!");

                currentAD = AD;

                ShowADThread = new Thread(() =>
                {


                    int viewers = twitchBot.GetVieversCount();
                    twitchBot.SlowChat(true, AD_Delay);
                    twitchBot.SubModChat(true);
                    twitchBot.ClearChat();
                    SetVisibleTarget(true);
                    Thread.Sleep(500);
                    this.Invoke(new Action(() =>
                    {
                        timer1.Start();
                    }));

                    webServer.SendWSMessage(AD.LinkToImage);


                    twitchBot.SendMessage(AD.Text);

                    Thread.Sleep((AD_Delay - 1) * 500);

                    twitchBot.ClearChat();
                    twitchBot.SendMessage(AD.Text);

                    Thread.Sleep(AD_Delay * 500);

                    SetVisibleTarget(false);

                    twitchBot.SubModChat(false);
                    twitchBot.SlowChat(false);

                    this.Invoke(new Action(() =>
                    {
                        timer1.Stop();
                    }));

                    webServer.SendWSMessage("Clear");

                    //dummy for test
                    //MessageBox.Show("Viewers count: " + viewers.ToString());
#if (DEBUG)
                    Random rnd = new Random();
                    smAPI.ViewsReport(currentAD, rnd.Next(100,1000000000));
#else
                    smAPI.ViewsReport(currentAD, viewers);
#endif
                    //smAPI.ViewsReport(currentAD, viewers);
                    //File.Delete(WebDataPath + AD.LinkFileName);
                    currentAD = null;

                });
                ShowADThread.Start();
            }
            catch (Exception exp)
            {
                Debug.WriteLine(">  ERROR!");
                Debug.WriteLine("> " + exp.Message + Environment.NewLine + ">  " + exp.StackTrace);
                MessageBox.Show(exp.Message + Environment.NewLine + exp.StackTrace, Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorHasOccurred();
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Visible = false;
            startADS.Text = StartProgram;
            ShowADThread?.Abort();
            UpdateThread?.Abort();
            ADThread?.Abort();
            ADThread = null;
            SetVisibleTarget(false);
            webServer.SendWSMessage("Clear");
            twitchBot.SubModChat(false);
            twitchBot.SlowChat(false);
            this.Invoke(new Action(() =>
            {
                timer1.Stop();
            }));
            IsRunning = false;
            Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!(currentAD is null))
            {
                webServer?.SendWSMessage(currentAD.LinkToImage);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                RegistryKey currentUser = Registry.CurrentUser;
                if (currentUser.OpenSubKey(OAuthDataPath) != null) currentUser.DeleteSubKey(OAuthDataPath);
                if (currentUser.OpenSubKey(StreamersMarketDataPath) != null) currentUser.DeleteSubKey(StreamersMarketDataPath);
                MessageBox.Show("The application need to be restarted!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                ErrorHasOccurred();
                Exit();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
            foreach(string file in files)
            {
                if (file.EndsWith(".update")) File.Delete(file);
            }
            UpdateThread = new Thread(new ThreadStart(() =>
            {
                string updateURL = @"http://krypt0n.stream/projects/files/ADSOH/versions.json";
                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(updateURL);
                httpReq.AllowAutoRedirect = false;

                try
                {
                    HttpWebResponse httpRes = (HttpWebResponse)httpReq.GetResponse();

                    if (httpRes.StatusCode == HttpStatusCode.OK)
                    {
                        updater = new Updater(updateURL, Version);
                        if (updater.UpdatesAvailable())
                        {
                            UpdaterForm updaterForm = new UpdaterForm();
                            updaterForm.Icon = this.Icon;
                            updaterForm.MainForm = this;
                            updaterForm.ShowDialog();
                        }
                    }

                    // Close the response.
                    httpRes.Close();
                }
                catch { }
                
                
            }));
            UpdateThread.Start();
        }
    }
}
