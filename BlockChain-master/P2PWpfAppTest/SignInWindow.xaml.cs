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
            var account = new Account(UserLoginTextBox.Text, UserPasswordPasswordBox.Password);
            try
            {
                var distribution = new P2PDistribution("P2PDemo", Dispatcher);
                await Task.Run(() =>
                {
                    DateTime start = DateTime.Now;
                    distribution.RegisterPeer();
                    while (!AccountList.Accounts.Contains(account.PublicAddress) && DateTime.Now - start < TimeSpan.FromSeconds(5)) { }
                });
                var accountCode = AccountList.Accounts.FirstOrDefault(a => a == account.PublicAddress);
                if (accountCode != null)
                {
                    if (account.Password != Account.GetPassword(accountCode, UserPasswordPasswordBox.Password))
                        throw new Exception("Неверный логин или пароль");
                    Account = account;
                    distribution.ClosePeer();
                    DialogResult = true;
                }
                else
                {
                    if (CustomMessageBox.Show("Уч. запись не найдена, хотите зарегестрироватся?", button: MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        distribution.RegisterNode(account.PublicAddress);
                        Account = account;
                        distribution.ClosePeer();
                        DialogResult = true;
                    }
                }
            }
            catch (Exception ex) { CustomMessageBox.Show(ex.Message); }
            HideProggres();
        }

        private void HideProggres() => Progress.Visibility = Visibility.Collapsed;

        private void ShowProgress() => Progress.Visibility = Visibility.Visible;

        private void UserLoginTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
            UserPublicKeyTextBox.Text = new Account(UserLoginTextBox.Text ?? "", UserPasswordPasswordBox.Password ?? "").PublicAddress.Code;
    }
}
