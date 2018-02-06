
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

namespace StepCoin.Distribution
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class P2PDistribution : IDistribution, IP2PStepCoin
    {

        #region IDistribution
        public event GetBlock BlockNotification;
        public event GetPendingElement PendingElementNotification;
        public HashCode ClientCode { get; set; }
        public void NotifyAboutBlock(BaseBlock newBlock)
        {
            if (_remoteEndPoints is null) return;
            if (!_remoteEndPoints.Any()) return;
            foreach (var endPoint in _remoteEndPoints)
            {
                var proxy = GetProxy(endPoint);
                proxy.ObtainBlock(newBlock);
            }
        }


        public void NotifyAboutPendingElement(PendingConfirmChainElement pendingElement)
        {
            if (_remoteEndPoints is null) return;
            _remoteEndPoints = GetRemoteEndPointsIPv4().ToArray();

            if (_remoteEndPoints.Any()) return;
            Logger.Instance.LogMessage($"Sent item {pendingElement.Element.Hash}");
            foreach (var endPoint in _remoteEndPoints)
            {
                var proxy = GetProxy(endPoint);
                proxy.ObtainPendingElement(pendingElement);
            }
        }
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
        public void ObtainPendingElement(PendingConfirmChainElement element) => PendingElementNotification?.Invoke(element);

        #endregion

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

        public P2PDistribution() { }
        public P2PDistribution(string classifier)
        {
            Classifier = classifier;
        }
        public void RegisterPeer(byte[] data = null)
        {
            if (string.IsNullOrWhiteSpace(Classifier)) throw new ArgumentNullException(nameof(Classifier));
            if (IsRegistered())
                throw new Exception("Previous peer is not closed");
            _peerNameRegistration =
                 new PeerNameRegistration(new PeerName(Classifier, PeerNameType.Unsecured), 8080, Cloud.AllLinkLocal)
                 {
                     Comment = ClientCode?.Code,
                     Data = data
                 };
            _peerNameRegistration.Start();
            _host = new ServiceHost(this);
            _host.AddServiceEndpoint(typeof(IP2PStepCoin), _binding, GetUri(_localAddresses.Last(), Port));
            _host.Open();
            RefreshRemoteEndPoints();
        }
        public void RefreshRemoteEndPoints()
        {
            AvailablePeers = PeerNameResolve();
            _remoteEndPoints = GetRemoteEndPointsIPv4().ToArray();
        }
        private bool IsRegistered() => _peerNameRegistration?.IsRegistered() == true;
        private static string GetUri(IPAddress iPAddress, int port) =>
            BaseUrl.Replace("%ip", iPAddress.ToString())
                  .Replace("%port", port.ToString());

        private bool IsLocalAddress(IPEndPoint ep) => _localAddresses.Select(a => a.ToString()).Contains(ep.Address.ToString());

        private IEnumerable<IPEndPoint> GetRemoteEndPointsIPv4() => GetRemoteEndPoints().Where(ep => ep.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        private IEnumerable<IPEndPoint> GetRemoteEndPoints()
        {
            if (!IsRegistered()) throw new Exception("Peer is not registered");
            List<IPEndPoint> endPoints = new List<IPEndPoint>();
            //Получение адресов друих учасников сети
            AvailablePeers.ToList().ForEach(record => endPoints.AddRange(record.EndPointCollection.Where(ep => !IsLocalAddress(ep))));
            return endPoints;
        }

        public PeerNameRecordCollection PeerNameResolve() => IsRegistered() ? new PeerNameResolver().Resolve(_peerNameRegistration.PeerName, Cloud.AllLinkLocal) : throw new Exception("Peer is not registered");

        private IP2PStepCoin GetProxy(IPEndPoint ipEndPoint) => ChannelFactory<IP2PStepCoin>.CreateChannel(_binding, new EndpointAddress(GetUri(ipEndPoint.Address, ipEndPoint.Port)));

        ~P2PDistribution() => ClosePeer();
        private void ClosePeer()
        {
            try
            {
                _peerNameRegistration.Stop();
                _peerNameRegistration.Dispose();
            }
            catch
            {
                // ignored
            }

            try
            {
                if (_host is null) return;
                _host.Close();
            }
            catch
            {
                // ignored
            }
        }
    }
}
