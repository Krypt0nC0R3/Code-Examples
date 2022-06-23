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
using System.Windows.Navigation;
using System.Windows.Shapes;

using TalosWorkshop.Subforms;
using TalosWorkshop.Core;

namespace TalosWorkshop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            usersBtn.Click += UsersBtn_Click;
            filesBtn.Click += FilesBtn_Click;
        }

        private void FilesBtn_Click(object sender, RoutedEventArgs e)
        {
            Files frm1 = new();
            this.Hide();
            frm1.ShowDialog();
            this.Show();
        }

        private void UsersBtn_Click(object sender, RoutedEventArgs e)
        {
            Users frm1 = new();
            this.Hide();
            frm1.ShowDialog();
            this.Show();
        }
    }
}
