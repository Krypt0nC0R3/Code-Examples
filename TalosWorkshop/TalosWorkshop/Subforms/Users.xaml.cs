using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TalosWorkshop.Core;
using Microsoft.Win32;

namespace TalosWorkshop.Subforms
{
    /// <summary>
    /// Логика взаимодействия для Users.xaml
    /// </summary>
    public partial class Users : Window
    {
        private List<KUser> users = null;

        private Thread load_thread;

        public Users()
        {
            InitializeComponent();
            mailAllowedCheckBox.Content = "Почта\nдоступна";
            saveBtn.Click += SaveBtn_Click;
            usersListBox.SelectionChanged += UsersListBox_SelectionChanged;

            addBtn.Click += AddBtn_Click;
            removeBtn.Click += RemoveBtn_Click;

            pathSelectorBtn.Click += PathSelectorBtn_Click;

            this.Closing += Users_Closing;

            load_thread = new(() =>
            {
                int k = 0;
                while (!DB.Connect())
                {
                    if (k++ == 10) break;

                    Thread.Sleep(100);
                }
                if (k == 11)
                {
                    MessageBox.Show("Ошибка загрузки списка пользователей.\nРабота невозможна!", "TalosWorkshop", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                k = 0;
                while (users is null)
                {
                    users = DB.LoadUsers();
                    if (k++ == 10) break;
                    Thread.Sleep(100);
                }
                if (k == 11)
                {
                    MessageBox.Show("Ошибка загрузки списка пользователей.\nРабота невозможна!", "TalosWorkshop", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    UpdateListbox();
                });
            });
            load_thread.Start();
        }

        private void PathSelectorBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.InitialDirectory = Environment.CurrentDirectory;
            dialog.Filter = "Изображения (*.png, *.jpg, *.jpeg, *.gif, *.webp)|*.png;*.jpg;*.jpeg;*.gif;*.webp";
            if(dialog.ShowDialog() == true)
            {
                imagePathTextBox.Text = dialog.FileName;
                wallpaperBlockImage.Source = new BitmapImage(new Uri(dialog.FileName));
            }
        }

        private void Users_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DB.UpdateUsers(users);
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (usersListBox.SelectedIndex != -1)
            {
                users.RemoveAt(usersListBox.SelectedIndex);
            }
            UpdateListbox();
        }

        private void UpdateListbox()
        {
            usersListBox.Items.Clear();
            for (int i = 0; i < users.Count; i++)
            {
                usersListBox.Items.Add(users[i].HumanReadableUsername);
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            users.Add(new KUser());
            UpdateListbox();
        }

        

        private void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (usersListBox.SelectedIndex != -1)
            {
                ApplyUserOnForm(users[usersListBox.SelectedIndex]);
            }
            else
            {
                ApplyUserOnForm();
            }
        }
        private void ApplyUserOnForm(KUser user = null)
        {
            if(user is null)
            {
                loginTextBox.Text = "";
                personalIdTextBox.Text = "";
                passwordTextBox.Text = "";
                imagePathTextBox.Text = "";
                mailAllowedCheckBox.IsChecked = false;
                wallpaperBlockImage.Source = new BitmapImage();
            }
            else
            {
                loginTextBox.Text = user.Username;
                personalIdTextBox.Text = user.PersoanlID;
                passwordTextBox.Text = user.Password;
                imagePathTextBox.Text = user.Wallpaper;
                mailAllowedCheckBox.IsChecked = user.IsMessagesAllowed;
                string url = user.Wallpaper;
                if (!String.IsNullOrEmpty(url))
                {
                    wallpaperBlockImage.Source = new BitmapImage(new Uri(url));
                }
                else
                {
                    wallpaperBlockImage.Source = new BitmapImage();
                }
            }
        }


        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (usersListBox.SelectedIndex != -1)
            {
                users[usersListBox.SelectedIndex].Username = loginTextBox.Text;
                users[usersListBox.SelectedIndex].PersoanlID = personalIdTextBox.Text;
                users[usersListBox.SelectedIndex].Password = passwordTextBox.Text;
                try
                {
                    users[usersListBox.SelectedIndex].Wallpaper = imagePathTextBox.Text;
                }
                catch (System.IO.FileNotFoundException)
                {
                    MessageBox.Show("Обои рабочего стола не найдены. Проверьте путь или очистите путь к картинке","TalosWorkshop",MessageBoxButton.OK,MessageBoxImage.Warning);
                }
            }
            UpdateListbox();
        }
    }
}
