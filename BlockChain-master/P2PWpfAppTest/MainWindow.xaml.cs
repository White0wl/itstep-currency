using MahApps.Metro.Controls;
using StepCoin.BlockChainClasses;
using StepCoin.Distribution;
using StepCoin.Hash;
using StepCoin.User;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace P2PWpfAppTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        P2PDistribution distribution;
        Node node;
        public MainWindow() => InitializeComponent();

        private void SignIn()
        {
            var wnd = new SignInWindow();
            if (wnd.ShowDialog() is true)
            {
                distribution = new P2PDistribution("P2PDemo", Dispatcher);
                node = new Node(wnd.Account, distribution);
                distribution.RegisterPeer();
                RefreshLists();
                UserPublicKeyTextBlock.Text = node.Account.PublicCode.Code;
            }
            else
                Close();
        }

        private void RefreshLists()
        {
            RefreshAccounts();
            RefreshPendingElements();
            RefreshReadyForMineTransactions();
            RefreshBlockChain();
        }

        private async void RefreshBlockChain() =>
            BlockChain.ItemsSource = await Task.Run(() => node.BlockChain.Blocks);

        private async void RefreshReadyForMineTransactions() =>
            ReadyForMiningElementList.ItemsSource = await Task.Run(() => node.ReadyForMiningElements);

        private async void RefreshPendingElements() =>
            PendingConfirmElements.ItemsSource = await Task.Run(() => node.PendingConfirmElements);

        private async void RefreshAccounts() =>
            Accounts.ItemsSource = await Task.Run(() =>
            {
                distribution.RefreshRemoteEndPoints();
                return AccountList.Accounts;
            });

        private void GenerateTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (Recipient.SelectedItem is null || CountMoney.Value is null) return;
            node.GenerateNewTransaction((HashCode)Recipient.SelectedItem, (decimal)CountMoney.Value);
            RefreshPendingElements();
        }

        private void RefreshListsButton_Click(object sender, RoutedEventArgs e) => RefreshLists();

        private void Accounts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Accounts.SelectedItem is null) return;
            Recipient.SelectedItem = Accounts.SelectedItem;
            CountMoney.Focus();
        }

        private void SendPendingElementButton_Click(object sender, RoutedEventArgs e) =>
            distribution.NotifyAboutPendingElement(PendingConfirmElements.SelectedItem as PendingConfirmChainElement);

        private void FoundConfirmedTransactionsButton_Click(object sender, RoutedEventArgs e) => RefreshReadyForMineTransactions();

        private void StartMineCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e) =>
            e.CanExecute = node?.IsCanMine is true;

        private void StartMineCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) =>
            Task.Run(() => node.StartMine());

        private void StopMineCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e) =>
            e.CanExecute = node?.Miner?.IsMining is true;

        private void StopMineCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) =>
            node.StopMine();

        private void SignOutCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e) =>
            e.CanExecute = node != null && distribution != null;

        private void SignOutCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (CustomMessageBox.Show("Вы уверенны что хотите выйти?", button: MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            distribution.ClosePeer();
            node = null;
            SignIn();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e) => SignIn();

        private void UserPublicKeyTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) =>
            Clipboard.SetText(UserPublicKeyTextBlock.Text);

        private void RemovePendingElementButton_Click(object sender, RoutedEventArgs e)
        {
            node.RemovePendingElelment(PendingConfirmElements.SelectedItem as PendingConfirmChainElement);
            RefreshPendingElements();
        }

        private void ClearPendingElementsButton_Click(object sender, RoutedEventArgs e) =>
            node.ClearPendingElements();

        private void RefreshBlockChainButton_Click(object sender, RoutedEventArgs e) => RefreshBlockChain();
    }
}
