using MahApps.Metro.Controls;
using StepCoin.Distribution;
using StepCoin.User;
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
using StepCoin.BaseClasses;

namespace P2PWpfAppTest
{
    /// <summary>
    /// Interaction logic for SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : MetroWindow
    {
        public Account Account { get; set; }
        public SignInWindow()
        {
            InitializeComponent();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = !string.IsNullOrWhiteSpace(UserLoginTextBox.Text) && !string.IsNullOrWhiteSpace(UserPasswordPasswordBox.Password);

        private async void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowProgress();
            await RegisterAccount();
            HideProggres();
        }

        private async Task RegisterAccount()
        {
            var account = new Account(UserLoginTextBox.Text, UserPasswordPasswordBox.Password);
            var distribution = new P2PDistribution("P2PDemo", Dispatcher);
            try
            {
                await Task.Run(() =>
                {
                    var start = DateTime.Now;
                    distribution.RegisterPeer();
                    distribution.SynchronizeRequestAccounts();
                    const int timeOutSeconds = 5;
                    while (true)
                    {
                        if (AccountList.Accounts.Contains(account) || DateTime.Now - start > TimeSpan.FromSeconds(timeOutSeconds)) break;
                    }
                });
                var returnResult = false;
                if (AccountList.Contains(account))
                {
                    returnResult = true;
                }
                else
                {
                    if (AccountList.Contains(account.PublicCode))
                    {
                        CustomMessageBox.Show("Неверный логин или пароль");
                    }
                    else if (CustomMessageBox.Show("Уч. запись не найдена.\r\nХотите зарегестрироватся?",
                            button: MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        distribution.RegisterNode(account);
                        returnResult = true;
                    }
                }

                Account = account;
                if (returnResult)
                    DialogResult = true;
            }
            catch (Exception ex) { CustomMessageBox.Show(ex.Message); }
            finally { distribution.ClosePeer(); }
        }

        private void HideProggres() => Progress.Visibility = Visibility.Collapsed;

        private void ShowProgress() => Progress.Visibility = Visibility.Visible;

        private void UserLoginTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
            UserPublicKeyTextBox.Text = new Account(UserLoginTextBox.Text ?? "", UserPasswordPasswordBox.Password ?? "").PublicCode.Code;
    }
}
