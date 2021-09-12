using System;
using System.Linq;
using Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Hashing;
using NUnit.Framework;

namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Tests
{
    public class TransactionsTests
    {
        private string[] _poem;
        private Blockchain _blockchain;
        
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

            _blockchain = new Blockchain();
        }

        [Test]
        public void ShouldCreteBlockchainFromStrings()
        {
            foreach (var line in _poem)
            {
               _blockchain.AddBlock(line);
            }
            
            Assert.AreEqual(_blockchain.Blocks.Count, _poem.Length + 1);
            CollectionAssert.AreEqual(_blockchain.Blocks.Skip(1).Select(p => p.Data), _poem);

            for (var i = 0; i < _poem.Length; i++)
            {
                Assert.AreEqual(_blockchain.Blocks[i].Hash, _blockchain.Blocks[i+1].PreviousHash);
            }
        }
        
        [Test]
        public void ShouldVerifyBlockchain()
        {
            foreach (var line in _poem)
            {
                _blockchain.AddBlock(line);
            }
            
            Assert.IsTrue( _blockchain.Verify());
            Console.WriteLine("This blockchain is valid.");
        }
    }
}