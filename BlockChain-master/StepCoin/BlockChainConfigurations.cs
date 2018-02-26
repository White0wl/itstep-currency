using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

namespace StepCoin
{
    public static class BlockChainConfigurations
    {

        #region Initializing
        static BlockChainConfigurations()
        {
            InitializeIntProperties();
            InitializeTimeSpanProperties();
        }
        private static void InitializeTimeSpanProperties()
        {
            var tSDefault = TimeSpan.FromMinutes(0);
            try { DefaultValueForTimeSpanProperty("ConfirmationTimeTransaction", TimeSpan.FromMinutes(0)); }
            catch { ConfirmationTimeTransaction = tSDefault; }

            try { DefaultValueForTimeSpanProperty("ConfirmationTimeBlock", TimeSpan.FromMinutes(0)); }
            catch { ConfirmationTimeBlock = tSDefault; }
        }
        private static void InitializeIntProperties()
        {
            var iDefault = 1;
            try { DefaultValueForIntProperty("TransactionsCountInBlock", iDefault); }
            catch { TransactionsCountInBlock = iDefault; }

            iDefault = 5;
            try { DefaultValueForIntProperty("ActualDifficulty", iDefault); }
            catch { ActualDifficulty = iDefault; }

            iDefault = 2;
            try { DefaultValueForIntProperty("ConfirmationsCount", iDefault); }
            catch { ConfirmationsCount = iDefault; }

            iDefault = 50;
            try { DefaultValueForIntProperty("StartBalance", iDefault); }
            catch { StartBalance = iDefault; }
        }

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
        #endregion
        /// <summary>
        /// Множитель количества подтверждений
        /// Нап.
        /// Активных учасников - 3
        /// Множитель - 0.5
        /// Готовым к использованию блок или транзакция станут в том случае если 2 учасника их подтвердят (Кол. Активных учасники * Множитель)(в большую сторону) 
        /// </summary>
        public const double MultiplierUserConfirmations = 0.5;
        private const int MinCountConfirmations = 2;
        private static List<string> _activeUserKeys;

        public static IEnumerable<string> ActiveUserKeys
        {
            get => _activeUserKeys;
            set
            {
                _activeUserKeys = new List<string>(value.Where(a => !string.IsNullOrWhiteSpace(a)));
                var countConfirmations = Convert.ToInt32(Math.Ceiling(_activeUserKeys.Count() * MultiplierUserConfirmations));
                if (countConfirmations < MinCountConfirmations) return;
                //ConfirmationsCountTransaction = countConfirmations;
                ConfirmationsCount = countConfirmations;
            }
        }

        /// <summary>
        /// Количество допустимых транзакций в одном блоке
        /// </summary>
        public static int TransactionsCountInBlock { get; private set; }
        public static int ActualDifficulty { get; private set; }//Актуальная сложность для вычисления хэша

        //Требуемое количество подтверждений транзакции для добавления в новый блок 
        //*На данный момент значение будет в зависимости от количества активных(в сети) пользователей
        //public static int ConfirmationsCountTransaction { get; private set; }
        
        /// <summary>
        /// Количество подтверждений для валидации блока/транзакции
        /// </summary>
        //*На данный момент значение будет в зависимости от количества активных(в сети) пользователей
        public static int ConfirmationsCount { get; private set; }
        public static int StartBalance { get; private set; }
        /// <summary>
        /// Промежуток времени на распотранение транзакции
        /// </summary>
        public static TimeSpan ConfirmationTimeTransaction { get; private set; } //Время для распространения и подтверждения/опровержения новой транзакции
        /// <summary>
        /// Промежуток времени на распотранение нового блока
        /// </summary>
        public static TimeSpan ConfirmationTimeBlock { get; private set; }//Время для распространения и подтверждения/опровержения нового блока

        public static HashAlgorithm HashAlgorithmBlock => SHA256.Create();
        public static HashAlgorithm HashAlgorithmTransaction => MD5.Create();
        public static HashAlgorithm HashAlgorithmSecretCode => MD5.Create();
        public static HashAlgorithm HashAlgorithmPublicCode => MD5.Create();

    }
}
