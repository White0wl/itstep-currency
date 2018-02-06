
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

namespace StepCoin.Distribution
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PTPDistribution : IDistribution, IP2PStepCoin
    {

        #region IDistribution
        public event GetBlock BlockNotification;
        public event GetPendingElement PendingElementNotification;
        public void NotifyAboutBlock(BaseBlock newBlock)
        {
            if (_remoteEndPoints is null) return;
            if (_remoteEndPoints.Count() < 1) return;
            foreach (IPEndPoint endPoint in _remoteEndPoints)
            {
                IP2PStepCoin proxy = GetProxy(endPoint);
                proxy.ObtainBlock(newBlock);
            }
        }
        public void NotifyAboutPendingElement(PendingConfirmChainElement pendingElement)
        {
            if (_remoteEndPoints is null) return;
            _remoteEndPoints = GetRemoteEndPointsIPv4();

            //PeerNameResolver peerNameResolver = new PeerNameResolver();
            //var peersRecord = peerNameResolver.Resolve(_peerNameRegistration.PeerName, Cloud.AllLinkLocal);
            //foreach (PeerNameRecord pnr in peersRecord)
            //{
            //    foreach (var ep in pnr.EndPointCollection)
            //    {
            //        if (ep.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            //        {
            //            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            //            EndpointAddress endpoint = new EndpointAddress($"net.tcp://{ep.Address}:{ep.Port}/stepCoin");

            //            IP2PStepCoin proxy = ChannelFactory<IP2PStepCoin>.CreateChannel(binding, endpoint);
            //        }
            //    }
            //}



            if (_remoteEndPoints.Count() < 1) return;
            Logger.Instance.LogMessage("Отправлен элемент");
            foreach (IPEndPoint endPoint in _remoteEndPoints)
            {
                IP2PStepCoin proxy = GetProxy(endPoint);
                proxy.ObtainPendingElement(pendingElement);
            }
        }
        #endregion
        #region IP2PStepCoint
        /// <summary>
        /// Получение блока от другого учасника сети
        /// </summary>
        /// <param name="block"></param>
        public void ObtainBlock(BaseBlock block)
        {
            BlockNotification?.Invoke(block);
        }
        /// <summary>
        /// Получение ожидающего подтверждения элемента от учасника сети
        /// </summary>
        /// <param name="element"></param>
        public void ObtainPendingElement(PendingConfirmChainElement element)
        {
            Logger.Instance.LogMessage("Получен элемент");
            PendingElementNotification?.Invoke(element);
        }
        #endregion

        private readonly List<IPAddress> _localAddresses = new List<IPAddress>(Dns.GetHostEntry(Dns.GetHostName()).AddressList);
        private PeerName _peerName;
        private PeerNameRegistration _peerNameRegistration;
        private NetTcpBinding _binding = new NetTcpBinding(SecurityMode.None);
        private readonly int Port = 8080;
        private string _baseUri = $"net.tcp://%ip:%port/stepCoin";
        private ServiceHost host;
        private IEnumerable<IPEndPoint> _remoteEndPoints;

        public PTPDistribution(string classifier)
        {
            StartListening(classifier);
            _remoteEndPoints = GetRemoteEndPointsIPv4();
        }

        private void StartListening(string classifier)
        {
            _peerName = new PeerName(classifier,PeerNameType.Unsecured);
            _peerNameRegistration =
                 new PeerNameRegistration(_peerName, 8080, Cloud.AllLinkLocal)
                 {
                     Comment = "комментарий",
                     Data = Encoding.UTF8.GetBytes("данные")
                 };
            _peerNameRegistration.Start();

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;

            host = new ServiceHost(this);
            host.AddServiceEndpoint(typeof(IP2PStepCoin), binding, GetUri(_localAddresses.Last(), Port));
            host.Open();
        }

        private string GetUri(IPAddress iPAddress, int port) =>
            _baseUri.ToString()
                  .Replace("%ip", iPAddress.ToString())
                  .Replace("%port", port.ToString());

        private bool IsLocalAddress(IPEndPoint ep) => _localAddresses.Select(a => a.ToString()).Contains(ep.Address.ToString());

        private IEnumerable<IPEndPoint> GetRemoteEndPointsIPv4() => GetRemoteEndPoints().Where(ep => ep.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        private IEnumerable<IPEndPoint> GetRemoteEndPoints()
        {
            List<IPEndPoint> endPoints = new List<IPEndPoint>();
            new PeerNameResolver().Resolve(_peerNameRegistration.PeerName, Cloud.AllLinkLocal).ToList().ForEach(record => endPoints.AddRange(record.EndPointCollection.Where(ep => !IsLocalAddress(ep))));
            return endPoints;
        }
        private IP2PStepCoin GetProxy(IPEndPoint ipEndPoint)
        {
            EndpointAddress endpoint = new EndpointAddress(GetUri(ipEndPoint.Address, ipEndPoint.Port));
            return ChannelFactory<IP2PStepCoin>.CreateChannel(_binding, endpoint);
        }

        ~PTPDistribution()
        {
            try
            {
                _peerNameRegistration.Stop();
                _peerNameRegistration.Dispose();
            }
            catch { }
            try
            {
                if (host is null) return;
                host.Close();
            }
            catch { }
        }
    }
}
