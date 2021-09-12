using System;

namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Hashing
{
    public class Block
    {
        public Block(
            long index, 
            string data, 
            string hash, 
            string previousHash, 
            DateTimeOffset timestamp)
        {
            Index = index;
            Data = data;
            Hash = hash;
            PreviousHash = previousHash;
            Timestamp = timestamp;
        }
        
        public long Index { get; }
        public string Data { get; }
        public string Hash { get; }
        public string PreviousHash { get; }
        public DateTimeOffset Timestamp { get; }
    }
}