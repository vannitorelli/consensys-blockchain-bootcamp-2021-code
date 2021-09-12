using System;
using System.Collections;
using System.Collections.Generic;

namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Transactions
{
    public class Blockchain : IEnumerable<Block>
    {
        private readonly List<Block> _blocks;
        
        public Blockchain()
        {
            _blocks = new List<Block>();
            CreateGenesisBlock();
        }

        public IReadOnlyList<Block> Blocks => _blocks;

        public void AddBlock(string data)
        {
            var latestBlock = _blocks[^1];
            var index = latestBlock.Index + 1;
            var timestamp = DateTimeOffset.UtcNow;
            var hash = BlockchainExtensions.CalculateHash(latestBlock.Index + 1, timestamp, latestBlock.Hash, data);
            
            var block = new Block(
                index, 
                data, 
                hash, 
                latestBlock.Hash, 
                timestamp);
            
            _blocks.Add(block);
        }
        
        public IEnumerator<Block> GetEnumerator() => _blocks.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        private void CreateGenesisBlock()
        {
            _blocks.Add(new Block(
                0,
                string.Empty, 
                new string('0', 64), 
                new string('0', 64), 
                DateTimeOffset.UtcNow));
        }
    }
}