using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepCoin.Validators
{
    public static class Validator
    {

        /// <summary>
        /// Метод проверки корректности нового блока по двум параметрам:
        /// - Равен ли хэш предыдущего блока предыдущему хэшу нового блока;
        /// - Соотвтествует ли хэш нового блока установленной сложности.
        /// ??? Неплохо бы еше реализовать проверку, включает ли новый блок нужный список транзакций, но 
        /// ??? это будет сделано после того, как решим, как производить отсечку pending transactions
        /// </summary>
        /// <returns></returns>
        public static bool IsCanBeAddedToChain(Block newBlock, Block lastBlock)
        {
            if (HashCode.IsNullOrWhiteSpace(newBlock.Hash)) return false;

            return lastBlock.Hash == newBlock.PrevHash && 
                newBlock.Hash.ToString().Substring(0, Configurations.ActualDifficulty) == new string('0', Configurations.ActualDifficulty);
        }

        /// <summary>
        /// Проверка корректности блок-чейна:
        /// Пересчитываем для каждого блока хэш и сверяем, равен ли он тому, 
        /// который указан в его свойстве.
        /// Сверяем, равен ли PreviousHash каждого блока хэшу предыдущего.
        /// </summary>
        /// <returns></returns>
        public static bool IsBlockChainValid(BlockChain blockChain)
        {
            bool result = false;
            List<Block> blocks = blockChain.Chain.ToList();
            for (int i = 1; i < blocks.Count; i++)
            {
                Block prevBlock = blocks[i - 1];
                Block block = blocks[i];
                result = block.PrevHash == prevBlock.Hash && block.Hash == block.CalculateHash();
                if (!result) break;
            }
            return result;
        }

        public static IEnumerable<Block> ConfirmedBlocks(IEnumerable<PendingConfirmChainElement> pendingConfirmElements) =>
    pendingConfirmElements
    .Where(pe => pe.Element is Block)//Нахождение всех ожидающих блоков, исключая транзакции
    .Where(pe => pe.Confirmations.Where(c => c.Value).Count() >= Configurations.BlockCountConfirmations)//Проверка кол.подтверждений
    .Where(pe => (DateTime.Now - pe.PendingStartTime) >= Configurations.BlockConfirmationTime)//Проверка времени распространения
    .Select(pe => pe.Element as Block);

        /// <summary>
        /// Добавление нового блока с валидацией его.
        /// Нужно еще добавить здесь формирование транзакции с премией автору блока
        /// и добавлением этой транзакции в список pending transactions. 
        /// Будет реализовано, когда определимся как и когда опустошается список pending transactions
        /// </summary>

        //Добавление и предварительная проверка в BlockChain

        //public static void AddNewBlock(Block newBlock, ChainBlocks.BlockChain blockChain)
        //{
        //    if (IsCanBeAddedToChain(newBlock, blockChain) && IsBlockChainValid(blockChain))
        //    {
        //        blockChain.AddBlock(newBlock);
        //    }
        //}
    }
}