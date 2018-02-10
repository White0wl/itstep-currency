﻿using StepCoin.BaseClasses;
using StepCoin.Hash;
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
    public class Account : IAccount
    {
        public HashCode PublicAddress { get; }//Address
        public HashCode Password { get; }//SecretKey

        public Account(string name, string password)
        {
            PublicAddress = new HashCode(HashGenerator.GenerateString(BlockChainConfigurations.AlgorithmPublicHashAccout, Encoding.Unicode.GetBytes(name)));
            Password = GetPassword(PublicAddress, password);
        }

        public static HashCode GetPassword(HashCode publicAddress, string password) =>
            new HashCode(HashGenerator.GenerateString(BlockChainConfigurations.AlgorithmSecretHashAccout, Encoding.Unicode.GetBytes(publicAddress.Code + password)));

    }
}