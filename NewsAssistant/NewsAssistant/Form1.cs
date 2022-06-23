using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace NewsAssistant
{
    public partial class Form1 : Form
    {
        VK_API api;
        NewsParser parser;
        List<string> urls = new List<string>() { "http://info-vb.ru/category/novosti/blagoustroistvo/", "http://info-vb.ru/category/novosti/zdravoohranenie/",
        "http://info-vb.ru/category/novosti/kultura/", "http://info-vb.ru/category/novosti/obrazovanie/", "http://info-vb.ru/category/novosti/obshestvo/",
        "http://info-vb.ru/category/novosti/sport/", "http://info-vb.ru/category/novosti/chp/", "http://info-vb.ru/category/novosti/ekonomika/"};
        DateTime lastPost = DateTime.Parse("1.1.1970 00:00");
        DateTime currentPost;

        Version version = new Version("1.0.3");

        string En = "Войти в VK";
        string Ex = "Выйти из аккаунта";
        Thread parserThread = null;

        bool authed = false;

#if (DEBUG)
        int owner_id = -173718904;
#else
        int owner_id = -48722789;
#endif
        //int user_id = 0;

        Bitmap photo;
        List<NewsParser.News> news;

        private string OAuth_data = @"OAuth\key.data";
        public Form1()
        {
            InitializeComponent();
            this.Text += " " + version;
            api = new VK_API("API+CODE_HERE")
            {
                formIcon = this.Icon
            };
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Text = En;
            button1.ForeColor = Color.DarkRed;
            this.groupBox1.Enabled = false;
            if (File.Exists(OAuth_data))
            {
                StreamReader sr = new StreamReader(OAuth_data);
                string data = sr.ReadToEnd().Trim();
                if (!String.IsNullOrEmpty(data))
                {
                    api.OfflineAuth(data);
                    SuccessfulyAuth();
                    button1.Text = Ex;
                }
            }
            
            ParseSite();
            
        }

        private void UpdateLastPostTime()
        {
            lastPost = api.GetLastPostponed(owner_id);
        }

        private void ParseSite()
        {
            parserThread = new Thread(() =>
            {
                parser = new NewsParser(urls);
                TaskbarManager taskbarInstance = null;
                if (TaskbarManager.IsPlatformSupported)
                    taskbarInstance = TaskbarManager.Instance;
                parser.OnPageParsed += (a) =>
                {
                    this.Invoke(new Action(()=>
                    {
                        toolStripProgressBar1.Maximum = a.Max;
                        toolStripProgressBar1.Value = a.Value;
                        toolStripStatusLabel4.Text = a.NewsCount.ToString();
                        toolStripStatusLabel6.Text = a.PagesCount.ToString();
                        taskbarInstance?.SetProgressState(TaskbarProgressBarState.Normal);
                        taskbarInstance?.SetProgressValue(a.Value, a.Max);
                        if (a.Value == a.Max)
                        {
                            new Thread(() =>
                            {
                                Thread.Sleep(3000);
                                this.Invoke(new Action(() =>
                                {
                                    toolStripProgressBar1.Maximum = 1;
                                    toolStripProgressBar1.Value = 0;
                                    taskbarInstance?.SetProgressState(TaskbarProgressBarState.NoProgress);
                                    statusStrip1.Update();
                                }));
                                parser.Optimize();
                            }).Start();
                        }
                        statusStrip1.Update();
                    }));
                };
                if (!parser.SearchNews())
                {
                    MessageBox.Show("Сайт недоступен!","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                else
                {
                    this.Invoke(new Action(() =>
                    {
                        listBox1.Items.Clear();
                        news = new List<NewsParser.News>();
                        foreach (NewsParser.News ns in parser.news)
                        {
                            if (ns.Date.Date == DateTime.Today)
                            {
                                listBox1.Items.Add(ns.Title);
                                news.Add(ns);
                            }
                            
                        }
                        listBox1.Update();
                        //button2.Enabled = true;
                    }));
                    
                }
            });
            parserThread.Start();
        }

        private void SuccessfulyAuth()
        {
            button1.ForeColor = Color.DarkGreen;
            this.groupBox1.Enabled = true;
            button1.Text = Ex;
            authed = true;
            UpdateLastPostTime();
            GenerateRandomTime();
            //user_id = api.GetUserID();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!authed)
            {
                api.Dispose();
                int scope = 0;
                scope += Convert.ToInt32(VK_API.SCOPE.WALL);
                scope += Convert.ToInt32(VK_API.SCOPE.OFFLINE);
                scope += Convert.ToInt32(VK_API.SCOPE.PHOTOS);
                api.OnAuthenticated += (id) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        SuccessfulyAuth();
                        try
                        {
                            string dir = OAuth_data.Substring(0, OAuth_data.LastIndexOf('\\'));
                            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                            StreamWriter sw = new StreamWriter(OAuth_data);
                            sw.WriteLine(api.OAuth);
                            sw.Close();
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show("Не удалось сохранить данные. При следующем запуске потребуется повторная аутентификация.", "Предупреждение!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            System.Diagnostics.Debug.WriteLine(exp.Message + "\n\t" + exp.StackTrace);
                        }
                    }));

                    button1.ForeColor = Color.DarkGreen;
                    this.TopMost = true;
                    this.Update();
                    Thread.Sleep(100);
                    this.TopMost = false; ;
                };
                api.Authenticate(VK_API.DISPLAY.PAGE, scope);
            }
            else
            {
                button1.Text = En;
                button1.ForeColor = Color.DarkRed;
                authed = false;
                api.Dispose();
                this.groupBox1.Enabled = false;
                if (File.Exists(OAuth_data))
                {
                    File.Delete(OAuth_data);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            parserThread.Abort();
            api.Dispose();
        }

        private void переподключитьсяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParseSite();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (api.IsDisposable) api.Dispose();
            button2.Enabled = (listBox1.SelectedIndex>=0);
            if (listBox1.SelectedIndex < 0)
            {
                richTextBox1.Text = richTextBox2.Text = linkLabel1.Text = String.Empty;
                pictureBox1.Image = null;
                photo = new Bitmap(0, 0);
            }
            else
            {
                richTextBox1.Text = news[listBox1.SelectedIndex].Title;
                richTextBox2.Text = news[listBox1.SelectedIndex].Text;
                linkLabel1.Text = news[listBox1.SelectedIndex].News_URL;
                pictureBox1.Image = news[listBox1.SelectedIndex].Picture;
                photo = news[listBox1.SelectedIndex].Picture;
            }
            
        }

        private void GenerateRandomTime()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int minutes = rnd.Next(30, 91);
            if (lastPost != DateTime.Parse("1.1.1970 00:00"))
                currentPost = lastPost;
            else
                currentPost = DateTime.Now;
            currentPost = currentPost.AddMinutes(minutes);
            string str = String.Format("{0:HH:mm}", currentPost);
            maskedTextBox1.Text = str;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!String.IsNullOrEmpty(linkLabel1.Text)) Process.Start(linkLabel1.Text);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            maskedTextBox1.Enabled = !checkBox1.Checked;
            if(checkBox1.Checked) GenerateRandomTime();
        }

        private void maskedTextBox1_Click(object sender, EventArgs e)
        {
            maskedTextBox1.Select(0, 0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string ph = api.UploadBitmap(owner_id, photo);
            DateTime postTime = DateTime.Now;
            if (checkBox1.Checked)
            {
                postTime = currentPost;
            }
            else
            {
                postTime = DateTime.Parse(postTime.Day+"."+postTime.Month+"."+ postTime.Year+" "+maskedTextBox1.Text);
            }
            api.CreatePostOnWall(owner_id,richTextBox1.Text+"\n\n"+richTextBox2.Text,linkLabel1.Text, ph, postTime);
            UpdateLastPostTime();
            MessageBox.Show("Запись успешно добавлена!","Успешно",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Укажите фото";
            dialog.Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                photo = (Bitmap)Bitmap.FromFile(dialog.FileName);
                pictureBox1.Image = photo;
            }
        }
    }
}
