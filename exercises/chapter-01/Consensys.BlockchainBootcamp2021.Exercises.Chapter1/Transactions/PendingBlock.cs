using System.Collections.Generic;
using System.Linq;

namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Transactions
{
    class PendingBlock : IPendingBlock
    {
        private readonly Blockchain _blockchain;
        private readonly List<PendingTransaction> _pendingTransactions;

        public PendingBlock(Blockchain blockchain)
        {
            _blockchain = blockchain;
            _pendingTransactions = new List<PendingTransaction>();
        } 
        
        public void Commit(Context context)
        {
            // Authorize all transactions
            var authorisedTransactions = ParallelEnumerable
                .Range(0, _pendingTransactions.Count)
                .Select(i => _pendingTransactions[i].AuthorisePendingTransaction(i, context))
                .ToList();
            
            _blockchain.AddBlock(authorisedTransactions);
        }

        public void AddTransaction(
            string fromAddress, 
            string toAddress, 
            string data)
        {
            var index = _pendingTransactions.Count;
            _pendingTransactions.Add(new PendingTransaction(index, fromAddress, toAddress, data));
        }
    }
}