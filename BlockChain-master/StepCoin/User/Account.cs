using System.Runtime.Serialization;
using StepCoin.BaseClasses;
using StepCoin.Hash;
using System.Text;

namespace StepCoin.User
{
    [DataContract]
    public class Account : BaseAccount
    {
        public Account() { }
        public Account(string name, string password)
        {
            PublicCode = new HashCode(HashGenerator.GenerateString(BlockChainConfigurations.HashAlgorithmPublicCode, Encoding.Unicode.GetBytes(name)));
            SecretCode = GenerateSecretCode(PublicCode, password);
        }

        public static HashCode GenerateSecretCode(HashCode publicAddress, string password) =>
            new HashCode(HashGenerator.GenerateString(BlockChainConfigurations.HashAlgorithmSecretCode, Encoding.Unicode.GetBytes(publicAddress.Code + password)));

    }
}