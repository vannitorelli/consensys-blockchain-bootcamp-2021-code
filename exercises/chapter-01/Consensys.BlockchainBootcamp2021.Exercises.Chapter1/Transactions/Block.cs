using System;
using System.Collections.Generic;

namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Transactions
{
    public class Block
    {
        private readonly List<Transaction> _transactions;
        
        public Block(
            long index,
            string hash, 
            string previousHash, 
            DateTimeOffset timestamp,
            IEnumerable<Transaction> transactions)
        {
            Index = index;
            Hash = hash;
            PreviousHash = previousHash;
            Timestamp = timestamp;

            _transactions = new List<Transaction>(transactions);
        }
        
        public long Index { get; }
        
        public string Hash { get; }
        
        public string PreviousHash { get; }
        
        public DateTimeOffset Timestamp { get; }

        public IReadOnlyList<Transaction> Transactions => _transactions;
    }
}