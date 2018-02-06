using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using StepCoin.BaseClasses;

namespace DistributionLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class P2PService : IP2PService
    {
        public void SendTransaction(ITransaction transaction/*, IAccount sender, IAccount recipient*/)
        {
            throw new NotImplementedException();   
        }
    }
}
