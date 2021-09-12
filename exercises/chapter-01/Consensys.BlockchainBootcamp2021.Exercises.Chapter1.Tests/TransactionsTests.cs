using System;
using System.Linq;
using Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Transactions;
using Consensys.BlockchainBootcamp2021.Exercises.Common;
using NUnit.Framework;

namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Tests
{
    public class TransactionsTests
    {
        private string[] _poem;

        [SetUp]
        public void Setup()
        {
            _poem = new []
                {
                    "The power of a gun can kill",
                    "and the power of fire can burn",
                    "the power of wind can chill",
                    "and the power of a mind can learn",
                    "the power of anger can rage",
                    "inside until it tears u apart",
                    "but the power of a smile",
                    "especially yours can heal a frozen heart",
                };
        }

        [Test]
        public void ShouldGenerateAndVerify()
        {
            var addresses = 7;
            var addressMap = ParallelEnumerable
                .Range(0, addresses)
                .Select(_ =>
                {
                    var address = Guid.NewGuid().ToString();
                    return KeyHelpers.GenerateRsaKeyRingPair(RsaKeySize.Size2048, address, address, "pass");
                })
                .ToDictionary(k => k.Identity, v => v);

            var context = new Context(_poem, addressMap);
            var blockchain = new Blockchain(context);

            AddToBlock(blockchain, context, 0, _poem.Length / 2);
            AddToBlock(blockchain, context, _poem.Length / 2, _poem.Length / 2);

            Assert.IsTrue(blockchain.Verify(context));
            Console.WriteLine("This blockchain is valid.");
        }

        private void AddToBlock(
            Blockchain blockchain, 
            Context context, 
            int start, 
            int count)
        {
            var pendingBlock = blockchain.NewBlock();
            for (var i = 0; i < count; i++)
            {
                var addressPair = context.GetRandomFromAndToAddresses();
                pendingBlock.AddTransaction(addressPair.FromAddress, addressPair.ToAddress, _poem[start + i]);
            }
            
            pendingBlock.Commit(context);
        }
    }
}