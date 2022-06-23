using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace VKPoster_GUI
{
    /// <summary>
    /// Логика взаимодействия для AuthForm.xaml
    /// </summary>
    public partial class AuthForm : Window
    {
        public MainWindow parent = null;
        public AuthForm()
        {
            InitializeComponent();
        }

        private void enterBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VkApi api = new VkApi();
                api.Authorize(new ApiAuthParams
                {
                    Login = login.Text,
                    Password = password.Password,
                    Settings = Settings.Messages
                });
                parent.Token = api.Token;
                this.Close();
            }
            catch (Exception exp)
            {
                MessageBox.Show($"Не удалось подключиться с этими данными:\t{exp.Message}\n{exp.StackTrace}","VKPoster",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
    }
}
