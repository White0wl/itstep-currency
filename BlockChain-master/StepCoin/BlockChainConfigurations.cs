using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Reflection;

namespace StepCoin
{
    public static class BlockChainConfigurations
    {

        static BlockChainConfigurations()
        {
            DefaultValueForIntProperty("TransactionsCountInBlock", 1);
            DefaultValueForIntProperty("ActualDifficulty", 5);
            DefaultValueForIntProperty("ConfirmationsCountTransaction", 2);
            DefaultValueForIntProperty("ConfirmationsCount", 2);
            DefaultValueForIntProperty("StartBalance", 50);

            DefaultValueForTimeSpanProperty("ConfirmationTimeTransaction", TimeSpan.FromMinutes(0));
            DefaultValueForTimeSpanProperty("ConfirmationTimeBlock", TimeSpan.FromMinutes(0));
        }
        /// <summary>
        /// Множитель количества подтверждений
        /// Нап.
        /// Активных учасников - 3
        /// Множитель - 0.5
        /// Готовым к использованию блок или транзакция станут в том случае если 2 учасника их подтвердят (Кол. Активных учасники * Множитель)(в большую сторону) 
        /// </summary>
        public const double MultiplierUserConfirmations = 0.5;
        private const int MinCountConfirmations = 2;

        private static void DefaultValueForIntProperty(string field, int defaultValue)
        {
            var prop = typeof(BlockChainConfigurations).GetProperty(field);
            try
            { prop.SetValue(null, int.Parse(ConfigurationManager.AppSettings[field])); }
            catch
            { prop.SetValue(null, defaultValue); }
        }

        private static void DefaultValueForTimeSpanProperty(string field, TimeSpan defaultValue)
        {
            var prop = typeof(BlockChainConfigurations).GetProperty(field);
            try
            { prop.SetValue(null, TimeSpan.FromMinutes(double.Parse(ConfigurationManager.AppSettings[field]))); }
            catch
            { prop.SetValue(null, defaultValue); }
        }

        public static int TransactionsCountInBlock { get; private set; }
        public static int ActualDifficulty { get; private set; }//Актуальная сложность для вычисления хэша

        //Требуемое количество подтверждений транзакции для добавления в новый блок 
        //*На данный момент значение будет в зависимости от количества активных(в сети) пользователей
        //public static int ConfirmationsCountTransaction { get; private set; }

        //Требуемое количество подтверждений нового блока/транзакции
        //*На данный момент значение будет в зависимости от количества активных(в сети) пользователей
        public static int ConfirmationsCount { get; private set; } 

        public static int StartBalance { get; private set; }
        public static TimeSpan ConfirmationTimeTransaction { get; private set; } //Время для распространения и подтверждения/опровержения новой транзакции
        public static TimeSpan ConfirmationTimeBlock { get; private set; }//Время для распространения и подтверждения/опровержения нового блока

        public static HashAlgorithm HashAlgorithmBlock => SHA256.Create();
        public static HashAlgorithm HashAlgorithmTransaction => MD5.Create();
        public static HashAlgorithm HashAlgorithmSecretCode => MD5.Create();
        public static HashAlgorithm HashAlgorithmPublicCode => MD5.Create();

        public static int ActiveUsers
        {
            set
            {
                var countConfirmations = Convert.ToInt32(Math.Ceiling(value * MultiplierUserConfirmations));
                if (countConfirmations < MinCountConfirmations) return;
                //ConfirmationsCountTransaction = countConfirmations;
                ConfirmationsCount = countConfirmations;
            }
        }
    }
}
