using System;
using System.Linq;
using Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Hashing;
using Consensys.BlockchainBootcamp2021.Exercises.Common;
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
        public void ShouldGenerateAddressKeys()
        {
            var addressMap = Enumerable
                .Range(0, 4)
                .Select(_ => Guid.NewGuid().ToString())
                .ToDictionary(k => k, v => KeyHelpers.GenerateRsaKeyRingPair(RsaKeySize.Size2048, v, v, "pass"));

           

        }
        
       
    }
}