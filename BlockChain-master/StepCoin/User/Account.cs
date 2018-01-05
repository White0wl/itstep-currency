using StepCoin.Hash;
using System.Security.Cryptography;
using System.Text;

namespace StepCoin.User
{
    /// <summary>
    /// Класс Account, предназначен для создания счета пользователем
    /// Хранит адрес (строка, по которой данный Account ищется в списке всех счетов, 
    /// которая служит адресом отправителя или получателя в транзакции.
    /// SecretKey - строка, которая служит паролем при совершении транзакции.
    /// Класс имеет список всех исходящих и входящих транзакций и метод, который вычисляет баланс
    /// по истории всех транзакций
    /// </summary>
    public class Account
    {
        public HashCode PublicAddress { get; set; }//Address
        public HashCode Password { get; set; }//SecretKey

        public Account(string name)
        {
            int id = AccountList.ListOfAllAccounts.Count;
            PublicAddress = new HashCode(HashGenerator.GenerateString(MD5.Create(),Encoding.Unicode.GetBytes(name)));
            Password = new HashCode(HashGenerator.GenerateString(MD5.Create(), Encoding.Unicode.GetBytes(PublicAddress.Code)));
        }
    }
}