using MahApps.Metro.Controls;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace P2PWpfAppTest
{
    /// <summary>
    /// Логика взаимодействия для CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : MetroWindow
    {
        private CustomMessageBox()
        {
            InitializeComponent();
        }
        static CustomMessageBox _messageBox;
        static MessageBoxResult _result = MessageBoxResult.None;

        public static MessageBoxResult Show(string messageBoxText, string caption = "Уведомление", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            _messageBox = new CustomMessageBox
            { MessageTextBox = { Text = messageBoxText }, Title = caption };
            SetVisibilityOfButtons(button);
            SetImageOfMessageBox(icon);
            _messageBox.ShowDialog();
            return _result;
        }

        private static void SetVisibilityOfButtons(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OK:
                    _messageBox.ButtonCancel.Visibility = Visibility.Collapsed;
                    _messageBox.ButtonNo.Visibility = Visibility.Collapsed;
                    _messageBox.ButtonYes.Visibility = Visibility.Collapsed;
                    _messageBox.ButtonOk.Focus();
                    break;
                case MessageBoxButton.OKCancel:
                    _messageBox.ButtonNo.Visibility = Visibility.Collapsed;
                    _messageBox.ButtonYes.Visibility = Visibility.Collapsed;
                    _messageBox.ButtonCancel.Focus();
                    break;
                case MessageBoxButton.YesNo:
                    _messageBox.ButtonOk.Visibility = Visibility.Collapsed;
                    _messageBox.ButtonCancel.Visibility = Visibility.Collapsed;
                    _messageBox.ButtonNo.Focus();
                    break;
                case MessageBoxButton.YesNoCancel:
                    _messageBox.ButtonOk.Visibility = Visibility.Collapsed;
                    _messageBox.ButtonCancel.Focus();
                    break;
                default:
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == ButtonOk)
                _result = MessageBoxResult.OK;
            else if (sender == ButtonYes)
                _result = MessageBoxResult.Yes;
            else if (sender == ButtonNo)
                _result = MessageBoxResult.No;
            else if (sender == ButtonCancel)
                _result = MessageBoxResult.Cancel;
            else
                _result = MessageBoxResult.None;
            _messageBox.Close();
            _messageBox = null;
        }

        private static void SetImageOfMessageBox(MessageBoxImage image)
        {
            switch (image)
            {
                case MessageBoxImage.Warning:
                    _messageBox.SetImage("Warning.png");
                    break;
                case MessageBoxImage.Question:
                    _messageBox.SetImage("Question.png");
                    break;
                case MessageBoxImage.Information:
                    _messageBox.SetImage("Information.png");
                    break;
                case MessageBoxImage.Error:
                    _messageBox.SetImage("Error.png");
                    break;
                default:
                    _messageBox.IconImage.Source = null;
                    break;
            }
        }

        private void SetImage(string imageName)
        {
            string uri = string.Format($"/Resources/Images/{imageName}");
            var uriSource = new Uri(Path.GetFullPath(uri));
            IconImage.Source = new BitmapImage(uriSource);
        }
    }

}
