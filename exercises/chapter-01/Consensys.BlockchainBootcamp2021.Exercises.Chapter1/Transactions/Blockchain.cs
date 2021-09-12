using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Transactions
{
    public class Blockchain : IEnumerable<Block>
    {
        private readonly Context _context;
        private readonly List<Block> _blocks;

        public Blockchain(Context context)
        {
            _context = context;
            _blocks = new List<Block>();
            CreateGenesisBlock();
        }

        public IReadOnlyList<Block> Blocks => _blocks;

        public IPendingBlock NewBlock()
        {
            return new PendingBlock(this);
        }
        
        public void AddBlock(IReadOnlyList<Transaction> transactions)
        {
            var latestBlock = _blocks[^1];
            var index = latestBlock.Index + 1;
            var timestamp = DateTimeOffset.UtcNow;
            
            var blockHash = Extensions.CalculateHash(latestBlock.Index + 1, timestamp, latestBlock.Hash, transactions);
            
            var block = new Block(
                index,
                blockHash, 
                latestBlock.Hash, 
                timestamp, 
                transactions);
            
            _blocks.Add(block);
        }
        
        public IEnumerator<Block> GetEnumerator() => _blocks.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        private void CreateGenesisBlock()
        {
            _blocks.Add(new Block(
                0,
                new string('0', 64), 
                new string('0', 64), 
                DateTimeOffset.UtcNow, 
                new List<Transaction>()));
        }
    }
}