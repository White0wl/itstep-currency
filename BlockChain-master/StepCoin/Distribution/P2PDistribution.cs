
using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Net.PeerToPeer;
using System.ServiceModel;
using System;
using System.Text;
using LoggerLibrary;
using StepCoin.Hash;
using StepCoin.User;
using System.Windows.Threading;
using StepCoin.Validators;

namespace StepCoin.Distribution
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class P2PDistribution : IDistribution, IP2PStepCoin
    {

        #region IDistribution
        public event TakeBlock BlockNotification;
        public event TakePendingElement PendingElementNotification;
        public event RequestBlocks RequestBlocks;
        public event RequestPendingElements RequestPendingElements;

        public void NotifyAboutBlock(BaseBlock newBlock)
        {
            if (!CollectionIsNotEmpty(_remoteEndPoints)) RefreshRemoteEndPoints();
            SendAllProxyBlock(newBlock);
        }
        private void SendAllProxyBlock(BaseBlock block)
        {
            foreach (var endPoint in _remoteEndPoints)
            {
                try { GetProxy(endPoint).ObtainBlock(block); }
                catch { }
            }
        }
        public void NotifyAboutPendingElement(PendingConfirmChainElement pendingElement)
        {
            if (!CollectionIsNotEmpty(_remoteEndPoints)) RefreshRemoteEndPoints();
            //Logger.Instance.LogMessage($"Sent item {pendingElement.Element.Hash}");
            SendAllProxyPendingElement(pendingElement);
        }
        private void SendAllProxyPendingElement(PendingConfirmChainElement pendingElement)
        {
            foreach (var endPoint in _remoteEndPoints)
            {
                try { GetProxy(endPoint).ObtainPendingElement(pendingElement); }
                catch { }
            }
        }


        public void RequestBlocksFromProxy() => RequestBlocks?.Invoke();
        public void RequestPendingElementsFromProxy() => RequestPendingElements?.Invoke();

        #endregion
        #region IP2PStepCoint
        /// <summary>
        /// Получение блока от другого учасника сети
        /// </summary>
        /// <param name="block"></param>
        public void ObtainBlock(BaseBlock block) => BlockNotification?.Invoke(block);

        /// <summary>
        /// Получение ожидающего подтверждения элемента от учасника сети
        /// </summary>
        /// <param name="element"></param>
        public void ObtainPendingElement(PendingConfirmChainElement element)
        {
            if (element.Element is BaseTransaction)
            {
                var v = element.Element as BaseTransaction;
                if (!TransactionValidator.IsValidAddresses(v.Sender, v.Recipient))
                {
                    RefreshRemoteEndPoints();
                    SynchronizeRequestAccounts();
                }
            }
            PendingElementNotification?.Invoke(element);
        }

        public void ObtainAccount(BaseAccount account)
        {
            var refreshAccountsAction = new Action(() =>
            {
                try
                {
                    AccountList.AddAccount(account);
                }
                catch
                {
                    // ignored
                }
            });
            if (_dispatcher != null)
                _dispatcher.BeginInvoke(refreshAccountsAction);
            else
                refreshAccountsAction.Invoke();
        }

        public void RequestAccountsFromProxy()
        {
            if (!CollectionIsNotEmpty(_remoteEndPoints)) RefreshRemoteEndPoints();
            foreach (var account in AccountList.Accounts)
            {
                SendAllProxyAccount(account);
            }
        }
        private void SendAllProxyAccount(BaseAccount account)
        {
            foreach (var endPoint in _remoteEndPoints)
            {
                try { GetProxy(endPoint).ObtainAccount(account); }
                catch { }
            }
        }
        #endregion

        public HashCode Client { get; private set; }
        //Локальные адреса V6 и V4
        private readonly List<IPAddress> _localAddresses = new List<IPAddress>(Dns.GetHostEntry(Dns.GetHostName()).AddressList);
        private PeerNameRegistration _peerNameRegistration;
        private readonly NetTcpBinding _binding = new NetTcpBinding(SecurityMode.None);
        private const int Port = 8080;
        private const string BaseUrl = "net.tcp://%ip:%port/stepCoin";
        private ServiceHost _host;
        private IPEndPoint[] _remoteEndPoints;
        public PeerNameRecordCollection AvailablePeers { get; set; }
        public string Classifier { get; }

        private readonly Dispatcher _dispatcher;

        public P2PDistribution() { }
        public P2PDistribution(string classifier, Dispatcher dispatcher = null)
        {
            Classifier = classifier;
            _dispatcher = dispatcher;
        }
        public void RegisterPeer(byte[] data = null)
        {
            if (string.IsNullOrWhiteSpace(Classifier)) throw new ArgumentNullException(nameof(Classifier));
            if (IsRegistered())
                throw new Exception("Previous peer is not closed");
            _peerNameRegistration =
                 new PeerNameRegistration(new PeerName(Classifier, PeerNameType.Unsecured), 8080, Cloud.AllLinkLocal)
                 {
                     Comment = Client is null ? " " : Client.Code,
                     Data = data ?? new byte[] { 0 }
                 };
            _peerNameRegistration.Start();
            _host = new ServiceHost(this);
            _host.AddServiceEndpoint(typeof(IP2PStepCoin), _binding, GetUri(_localAddresses.Last(), Port));
            _host.Open();
            RefreshRemoteEndPoints();
        }

        private void SynchronizeRequstFull()
        {
            SynchronizeRequestAccounts();
            SynchronizeRequestBlocks();
            SynchronizeRequestPendingElements();
        }

        public void RegisterNode(BaseAccount account)
        {
            var refreshAccountsAction = new Action(() =>
            {
                SendAllProxyAccount(account);
            });
            if (_dispatcher != null)
                _dispatcher.BeginInvoke(refreshAccountsAction);
            else
                refreshAccountsAction.Invoke();
            RefreshRemoteEndPoints();
        }
        public void RefreshRemoteEndPoints()
        {
            AvailablePeers = PeerNameResolve();
            _remoteEndPoints = GetRemoteEndPointsIPv4().ToArray();
            BlockChainConfigurations.ActiveUsers = AvailablePeers.Count;
        }

        private void SynchronizeRequestAccounts()
        {
            foreach (var remoteEndPoint in _remoteEndPoints)
            {
                try
                { GetProxy(remoteEndPoint).RequestAccountsFromProxy(); }
                catch {/*ignored*/}
            }
        }

        private void SynchronizeRequestBlocks()
        {
            foreach (var remoteEndPoint in _remoteEndPoints)
            {
                try
                { GetProxy(remoteEndPoint).RequestBlocksFromProxy(); }
                catch {/*ignored*/}
            }
        }

        private void SynchronizeRequestPendingElements()
        {
            foreach (var remoteEndPoint in _remoteEndPoints)
            {
                try
                { GetProxy(remoteEndPoint).RequestPendingElementsFromProxy(); }
                catch {/*ignored*/}
            }
        }

        private bool IsRegistered() =>
            _peerNameRegistration?.IsRegistered() is true;
        private static string GetUri(IPAddress iPAddress, int port) =>
            BaseUrl.Replace("%ip", iPAddress.ToString())
                  .Replace("%port", port.ToString());

        private bool IsLocalAddress(IPEndPoint ep) =>
            _localAddresses.Select(a => a.ToString()).Contains(ep.Address.ToString());
        private IEnumerable<IPEndPoint> GetRemoteEndPointsIPv4() => GetRemoteEndPoints().Where(ep => ep.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        private IEnumerable<IPEndPoint> GetRemoteEndPoints()
        {
            if (!IsRegistered()) throw new Exception("Peer is not registered");
            var endPoints = new List<IPEndPoint>();
            //Получение адресов друих учасников сети
            AvailablePeers.ToList().ForEach(record => endPoints.AddRange(record.EndPointCollection.Where(ep => !IsLocalAddress(ep))));
            return endPoints;
        }

        public PeerNameRecordCollection PeerNameResolve() => IsRegistered() ?
            new PeerNameResolver().Resolve(_peerNameRegistration.PeerName, Cloud.AllLinkLocal) : throw new Exception("Peer is not registered");

        private IP2PStepCoin GetProxy(IPEndPoint ipEndPoint) =>
            ChannelFactory<IP2PStepCoin>.CreateChannel(_binding, new EndpointAddress(GetUri(ipEndPoint.Address, ipEndPoint.Port)));

        private static bool CollectionIsNotEmpty(IEnumerable<object> enumerable) => enumerable != null && enumerable.Any();
        ~P2PDistribution() => ClosePeer();
        public void ClosePeer()
        {
            try
            {
                _peerNameRegistration.Stop();
                _peerNameRegistration.Dispose();
            }
            catch {/*ignored*/}

            try
            {
                if (_host is null) return;
                _host.Close();
            }
            catch {/*ignored*/}

        }
    }
}
