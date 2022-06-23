using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TalosWorkshop.Core;

namespace TalosWorkshop.Subforms
{
    /// <summary>
    /// Логика взаимодействия для Files.xaml
    /// </summary>
    public partial class Files : Window
    {
        private List<KFile> files = null;
        private List<KUser> users = null;
        private Thread load_thread;

        public Files()
        {
            InitializeComponent();
            /*string str = GetHTMText(rtbox);
            rtbox.Document.Blocks.Clear();
            rtbox.Document.Blocks.Add(new Paragraph(new Run(str)));*/
            boldBtn.Click += (_, _) =>
            {
                Bold();

            };
            italicBtn.Click += (_, _) =>
            {
                Italic();
            };
            underlineBtn.Click += (_, _) =>
            {
                Underline();
            };
            strikeBtn.Click += (_, _) =>
            {
                Strike();
            };
            redBtn.Click += (_, _) =>
            {
                Color(Brushes.Red);
            };
            orangeBtn.Click += (_, _) =>
            {
                Color(Brushes.Orange);
            };
            greenBtn.Click += (_, _) =>
            {
                Color(Brushes.Green);
            };
            glitchBtn.Click += (_, _) =>
            {
                Color(Brushes.DeepPink);
            };
            usersListComboBox.SelectionChanged += UsersListComboBox_SelectionChanged;
            KeyBinding binding = new(ApplicationCommands.Find, Key.G, ModifierKeys.Control);
            fileContentRTB.InputBindings.Add(binding);


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
                    UpdateComboBox();
                });
            });
            load_thread.Start();
        }

        private void UsersListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            files = null;
            if (usersListComboBox.SelectedIndex != -1)
            {
                int index = usersListComboBox.SelectedIndex;
                usersListComboBox.IsEnabled = false;
                load_thread = new(()=>
                {
                    int k = 0;
                    while (files is null)
                    {
                        files = DB.LoadFiles(users[index]);
                        if (k++ == 10) break;
                        Thread.Sleep(100);
                    }
                    if (k == 11)
                    {
                        MessageBox.Show("Ошибка загрузки списка файлов.\nРабота невозможна!", "TalosWorkshop", MessageBoxButton.OK, MessageBoxImage.Error);
                        UpdateListBox();
                        return;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        UpdateListBox();
                        usersListComboBox.IsEnabled = true;
                    });
                });
                load_thread.Start();
            }
            else
            {
                
                UpdateListBox();
            }
        }

        protected void BlockTheCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
            if(e.Command == EditingCommands.AlignRight)
            {
                Color(Brushes.Red);
            }
            if (e.Command == ApplicationCommands.Open)
            {
                Color(Brushes.Orange);
            }
            if (e.Command == ApplicationCommands.Find)
            {
                Color(Brushes.Green);
            }
            if (e.Command == ApplicationCommands.Print)
            {
                Color(Brushes.DeepPink);
            }
            if (e.Command == ApplicationCommands.Save)
            {
                Strike();
            }
        }

        private void UpdateListBox()
        {
            filesListBox.Items.Clear();
            if (files != null)
            {
                var ss = TextExtensions.ParseHTMLText(files[0].Content);
                foreach (var file in files)
                {
                    filesListBox.Items.Add(file.Title);
                }
            }
        }

        private void UpdateComboBox()
        {
            usersListComboBox.Items.Clear();
            foreach (var user in users)
            {
                usersListComboBox.Items.Add(user.HumanReadableUsername);
            }
        }

        private void Bold()
        {
            if ((FontWeight)fileContentRTB.Selection.GetPropertyValue(TextElement.FontWeightProperty) != FontWeights.Bold)
                fileContentRTB.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            else
                fileContentRTB.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
        }

        private void Italic()
        {
            if ((FontStyle)fileContentRTB.Selection.GetPropertyValue(TextElement.FontStyleProperty) != FontStyles.Italic)
                fileContentRTB.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
            else
                fileContentRTB.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
        }

        private void Underline()
        {
            if ((TextDecorationCollection)fileContentRTB.Selection.GetPropertyValue(Inline.TextDecorationsProperty) != TextDecorations.Underline)
                fileContentRTB.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            else
                fileContentRTB.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
        }

        private void Strike()
        {
            if ((TextDecorationCollection)fileContentRTB.Selection.GetPropertyValue(Inline.TextDecorationsProperty) != TextDecorations.Strikethrough)
                fileContentRTB.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
            else
                fileContentRTB.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
        }

        private void Color(Brush c)
        {
            if ((Brush)fileContentRTB.Selection.GetPropertyValue(TextElement.ForegroundProperty) != c)
                fileContentRTB.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, c);
            else
                fileContentRTB.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
        }

        
    }

}
