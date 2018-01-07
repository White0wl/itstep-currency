using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepCoin
{
    public static class Configurations
    {
        public static int ActualDifficulty { get; set; } = 4;//Актуальная сложность для вычисления хэша

        public static int TransactionCountConfirmations { get; set; }//Требуемое количество подтверждений транзакции для добавления в новый блок 
                                                                     //*На данный момент значение будет в зависимости от количества зарегестрированых аккаунтов (AccountList.cs)

        public static int BlockCountConfirmations { get; set; }//Требуемое количество подтверждений нового блока для добавления в цепь(BlockChain) 
                                                               //*На данный момент значение будет в зависимости от количества зарегестрированых аккаунтов (AccountList.cs)

        public static TimeSpan TransactionConfirmationTime { get; set; } = new TimeSpan(0, 0, 0);//Время для распространения и подтверждения/опровержения новой транзакции
        public static TimeSpan BlockConfirmationTime { get; set; } = new TimeSpan(0, 0, 0);//Время для распространения и подтверждения/опровержения нового блока
    }
}
