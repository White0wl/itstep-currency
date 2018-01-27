using System;
using System.Security.Cryptography;

namespace StepCoin
{
    public static class BlockChainConfigurations
    {
        public static int CountTransactionsToBlock { get; set; } = 1;
        public static int ActualDifficulty { get; set; } = 3;//Актуальная сложность для вычисления хэша
        public static int TransactionCountConfirmations { get; set; }//Требуемое количество подтверждений транзакции для добавления в новый блок 
                                                                     //*На данный момент значение будет в зависимости от количества зарегестрированых аккаунтов (AccountList.cs)

        public static int BlockCountConfirmations { get; set; }//Требуемое количество подтверждений нового блока для добавления в цепь(BlockChain) 
                                                               //*На данный момент значение будет в зависимости от количества зарегестрированых аккаунтов (AccountList.cs)

        public static int StartBalance { get; set; } = 50;
        public static TimeSpan TransactionConfirmationTime { get; set; } = new TimeSpan(0, 0, 0);//Время для распространения и подтверждения/опровержения новой транзакции
        public static TimeSpan BlockConfirmationTime { get; set; } = new TimeSpan(0, 0, 0);//Время для распространения и подтверждения/опровержения нового блока

        public static HashAlgorithm AlgorithmBlockHash => SHA256.Create();
        public static HashAlgorithm AlgorithmTransactionHash => MD5.Create();
        public static HashAlgorithm AlgorithmSecretHashAccout => MD5.Create();
        public static HashAlgorithm AlgorithmPublicHashAccout => MD5.Create();
    }
}
