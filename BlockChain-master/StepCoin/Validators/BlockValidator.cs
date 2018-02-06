using StepCoin.BaseClasses;
using StepCoin.BlockChainClasses;
using StepCoin.Hash;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepCoin.Validators
{
    public static class BlockValidator
    {

        /// <summary>
        /// Метод проверки корректности нового блока по двум параметрам:
        /// - Равен ли хэш предыдущего блока предыдущему хэшу нового блока;
        /// - Соотвтествует ли хэш нового блока установленной сложности.
        /// ??? Неплохо бы еше реализовать проверку, включает ли новый блок нужный список транзакций, но 
        /// ??? это будет сделано после того, как решим, как производить отсечку pending transactions
        /// </summary>
        /// <returns></returns>
        public static bool IsCanBeAddedToChain(BaseBlock newBlock, BaseBlock lastBlock)
        {
            if (newBlock is null) throw new ArgumentNullException(nameof(newBlock));
            if (lastBlock is null) throw new ArgumentNullException(nameof(lastBlock));

            if (HashCode.IsNullOrWhiteSpace(newBlock.Hash)) return false;
            return lastBlock.Hash == newBlock.PrevHash &&
                newBlock.Hash.ToString().Substring(0, BlockChainConfigurations.ActualDifficulty) == new string('0', BlockChainConfigurations.ActualDifficulty);
        }

        /// <summary>
        /// Проверка корректности блок-чейна:
        /// Пересчитываем для каждого блока хэш и сверяем, равен ли он тому, 
        /// который указан в его свойстве.
        /// Сверяем, равен ли PreviousHash каждого блока хэшу предыдущего.
        /// </summary>
        /// <returns></returns>
        public static bool IsBlockChainValid(params BaseBlock[] blocks)
        {
            var result = false;
            if (blocks.Length < 2) throw new ArgumentException($"Less than 2 items transferred in {nameof(blocks)}");
            for (var i = 1; i < blocks.Length; i++)
            {
                var prevBlock = blocks[i - 1];
                var block = blocks[i];
                result = block.PrevHash == prevBlock.Hash && block.Hash == block.CalculateHash();
                if (!result) break;
            }
            return result;
        }

        public static IEnumerable<BaseBlock> ConfirmedBlocks(IEnumerable<PendingConfirmChainElement> pendingConfirmElements) => pendingConfirmElements
            .Where(pe => pe.Element is BaseBlock)//Нахождение всех ожидающих блоков, исключая транзакции
            .Where(pe => pe.Confirmations.Count(c => c.Value) >= BlockChainConfigurations.BlockCountConfirmations)//Проверка кол.подтверждений
            .Where(pe => (DateTime.Now - pe.PendingStartTime) >= BlockChainConfigurations.BlockConfirmationTime)//Проверка времени распространения
            .Select(cb => cb.Element as BaseBlock);
    }
}