using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Consensys.BlockchainBootcamp2021.Exercises.Common;
using Consensys.BlockchainBootcamp2021.Exercises.Common.Cryptography;

namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Transactions
{
    public class Context
    {
        private ThreadLocal<Random> Random = new(() => new Random(
            Thread.CurrentThread.ManagedThreadId + (int) DateTime.UtcNow.Ticks));
        
        private readonly Dictionary<string, KeyPair> _addressMap;
        private readonly List<string> _addresses;

        public Context(string[] poem, Dictionary<string, KeyPair> addressMap)
        {
            Poem = poem;
            _addressMap = addressMap;
            _addresses = new List<string>(_addressMap.Select(p => p.Key).OrderBy(p => p));
        }
        
        public string[] Poem { get; }

        public string GetAddressByIndex(int index) => _addresses[index];

        public AddressPair GetRandomFromAndToAddresses()
        {
            var fromAddress = _addresses[Random.Value.Next(0, _addresses.Count)];
            var toAddress = fromAddress;
            while (toAddress == fromAddress)
            {
                toAddress = _addresses[Random.Value.Next(0, _addresses.Count)];
            }
            return new AddressPair(fromAddress, toAddress);
        }
        
        public KeyPair GetKeyPairForAddress(string address) => 
            _addressMap.TryGetValue(address, out var keyPair) ? keyPair : null;
    }
}