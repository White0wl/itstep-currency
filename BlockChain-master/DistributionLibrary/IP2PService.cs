using StepCoin.BaseClasses;
using StepCoin.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionLibrary
{
    
    public interface IP2PService
    {
        void SendTransaction(ITransaction transaction/*, IAccount sender, IAccount recipient*/);
    }
}
