using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using StepCoin;
using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using StepCoin.Distribution;
using StepCoin.Hash;
using StepCoin.User;
using StepCoin.Validators;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
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
        private Timer timerRefresh;

        public MainWindow() => InitializeComponent();

        private void SignIn()
        {
            var wnd = new SignInWindow();
            if (wnd.ShowDialog() is true)
            {
                distribution = new P2PDistribution("P2PDemo", Dispatcher);
                node = new Node(wnd.Account, distribution);
                distribution.Client = node.Account.PublicCode;
                FirstLoad();
                timerRefresh = new Timer(5000);
                timerRefresh.Elapsed += (sender, arg) => { Dispatcher.BeginInvoke(new Action(() => RefreshLists())); };
                timerRefresh.Start();
            }
            else
                Close();
        }

        private async void FirstLoad()
        {
            var controller = await this.ShowProgressAsync("Загрузка данных", "Получение удалленых узлов...");
            await Task.Run(() =>
            {
                distribution.RegisterPeer();
            });
            RefreshLists();
            await controller.CloseAsync();
        }

        private void RefreshLists()
        {
            RefreshRemoteEndPoints();
            RefreshAccounts();
            RefreshPendingElements();
            RefreshReadyForMineTransactions();
            RefreshBlockChain();
        }

        private async void RefreshRemoteEndPoints()
        => await Task.Run(() =>
            {
                distribution.RefreshRemoteEndPoints();
                distribution.SynchronizeRequstFull();
                return true;
            });

        private async void RefreshBlockChain()
        {
            var index = BlockChain.SelectedIndex;
            BlockChain.ItemsSource = await Task.Run(() =>
            {
                distribution.SynchronizeRequestBlocks();
                return node.BlockChain.Blocks;
            });

            UserPublicKeyTextBlock.Text = $"{node.Account.PublicCode.Code} ({node.BlockChain.GetBalance(node.Account.PublicCode)}$)";
            BlockChain.SelectedIndex = index;
        }

        private async void RefreshReadyForMineTransactions()
        {
            var index = ReadyForMiningElementList.SelectedIndex;
            ReadyForMiningElementList.ItemsSource = await Task.Run(() => node.ReadyForMiningElements);
            ReadyForMiningElementList.SelectedIndex = index;
        }

        private async void RefreshPendingElements()
        {
            var index = PendingConfirmElements.SelectedIndex;
            PendingConfirmElements.ItemsSource = await Task.Run(() =>
            {
                distribution.SynchronizeRequestPendingElements();
                return node.PendingConfirmElements;
            });
            PendingConfirmElements.SelectedIndex = index;
        }

        private async void RefreshAccounts()
        {
            var index = Accounts.SelectedIndex;
            Accounts.ItemsSource = await Task.Run(() =>
                {
                    RefreshRemoteEndPoints();
                    distribution.SynchronizeRequestAccounts();
                    return AccountList.Accounts.Select(a => new { PublicCode = a.PublicCode, IsOnline = BlockChainConfigurations.ActiveUserKeys.Contains(a.PublicCode.Code) });
                });
            ;
            Accounts.SelectedIndex = index;
        }

        private void GenerateTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (Recipient.SelectedItem is null || CountMoney.Value is null) return;
            node.GenerateNewTransaction(new HashCode(Recipient.Text), (decimal)CountMoney.Value);
        }

        private void RefreshTransactionsToMineButton_Click(object sender, RoutedEventArgs e) =>
            RefreshReadyForMineTransactions();

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
            timerRefresh.Stop();
            timerRefresh.Dispose();
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

        private void RefreshAccountsButton_Click(object sender, RoutedEventArgs e) => RefreshAccounts();
    }
}
