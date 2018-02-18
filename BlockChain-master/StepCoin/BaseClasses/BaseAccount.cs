using System.Runtime.Serialization;
using StepCoin.Hash;
using StepCoin.User;

namespace StepCoin.BaseClasses
{    /// <summary>
     /// Класс BaseAccount, предназначен для создания счета пользователем
     /// Хранит адрес (строка, по которой данный Account ищется в списке всех счетов, 
     /// которая служит адресом отправителя или получателя в транзакции.
     /// SecretKey - строка, которая служит паролем при совершении транзакции.
     /// Класс имеет список всех исходящих и входящих транзакций и метод, который вычисляет баланс
     /// по истории всех транзакций
     /// </summary>
    [DataContract]
    [KnownType(typeof(Account))]
    public abstract class BaseAccount
    {
        [DataMember] public HashCode PublicCode { get; protected set; }
        [DataMember] public HashCode SecretCode { get; protected set; }
    }
}
