using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace VKPoster_GUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Token = "";
        public const string KEY = @"SECRET_KEY_HERE";

        VkApi api = new VkApi();
        public MainWindow()
        {
            InitializeComponent();
            group.IsEnabled = false;
            if (File.Exists("data.ini"))
            {
                try
                {
                    using(StreamReader sr = new StreamReader("data.ini"))
                    {
                        Token = sr.ReadToEnd().Trim();
                    }
                }
                catch { }
            }
            UpdateForm();
        }


        private void SaveBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateForm()
        {
            if (!String.IsNullOrEmpty(Token))
            {
                group.IsEnabled = true;
                try
                {
                    StreamWriter sw = new StreamWriter("data.ini");
                    sw.WriteLine(Token);
                    sw.Close();
                }
                catch { }
                AuthBTN.Background = Brushes.Green;
            }
        }

        private void CheckBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AuthBTN_Click(object sender, RoutedEventArgs e)
        {
            /*api = new VkApi();
            api.Authorize(new ApiAuthParams
            {
                Login = login,
                Password = password,
                Settings = Settings.Messages
            });*/
            AuthForm authForm = new AuthForm()
            {
                parent = this
            };
            authForm.ShowDialog();
            UpdateForm();
        }
    }
}
