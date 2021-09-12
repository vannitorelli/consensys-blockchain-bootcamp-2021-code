using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Consensys.BlockchainBootcamp2021.Exercises.Common;
using Nethereum.Util;

namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Transactions
{
    public static class Extensions
    {
        private static readonly string EmptyHash = new('0', 64);
        private static readonly Sha3Keccack HashProvider = new();
        
        public static string CalculateHash(
            long index, 
            DateTimeOffset timestamp, 
            string previousHash, 
            IEnumerable<Transaction> transactions)
        {
            var transactionHash = string.Concat(transactions.Select(CalculateHash));
            var hashString = $"{index:000000000000000}{timestamp.ToUnixTimeMilliseconds():000000000000000}{previousHash}{transactionHash}";
            return HashProvider.CalculateHash(hashString);
        }

        public static string CalculateHash(
            PendingTransaction pendingTransaction, 
            int transactionIndex,
            string signature)
        {
            var hashString = 
                $"{transactionIndex:000000000000000}{pendingTransaction.FromAddress}{pendingTransaction.ToAddress}" +
                $"{signature}{pendingTransaction.Data}";
            return HashProvider.CalculateHash(hashString);
        }

        public static string CalculateHash(Transaction transaction)
        {
            var hashString = 
                $"{transaction.Index:000000000000000}{transaction.FromAddress}{transaction.ToAddress}" +
                $"{transaction.Signature}{transaction.Hash}{transaction.Data}";
            return HashProvider.CalculateHash(hashString);
        }
        
        public static Transaction AuthorisePendingTransaction(
            this PendingTransaction pendingTransaction, 
            int transactionIndex, 
            Context context)
        {
            // Sign with fromAddress private key
            var keyPair = context.GetKeyPairForAddress(pendingTransaction.FromAddress);
            if (keyPair == null)
            {
                throw new ApplicationException($"Address {pendingTransaction.FromAddress} could not be resolved.");
            }
            
            var signature = KeyHelpers.SignDataFromKeyText(
                keyPair.PrivateKey, 
                keyPair.PassPhrase, 
                pendingTransaction.Data);
            
            var hash = CalculateHash(
                pendingTransaction, 
                transactionIndex, 
                signature);

            return new Transaction(
                transactionIndex,
                pendingTransaction.FromAddress,
                pendingTransaction.ToAddress,
                signature,
                hash,
                pendingTransaction.Data);
        }
 
        public static bool Verify(this Blockchain blockchain, Context context)
        {
            if (blockchain.Blocks.Count == 0)
            {
                return false;
            }

            if (!blockchain.Blocks[0].VerifyGenesisBlock())
            {
                return false;
            }

            int lineCount = 0;  
            for (int i = 1; i < blockchain.Blocks.Count; i++)
            {
                if (!blockchain.Blocks[i].VerifyBlock(context, ref lineCount))
                {
                    return false;
                }

                if (blockchain.Blocks[i - 1].Hash != blockchain.Blocks[i].PreviousHash)
                {
                    return false;
                }
            }

            return true;
        }
        
        private static bool VerifyGenesisBlock(this Block block)
        {
            if (block.Index != 0)
            {
                return false;
            }
            if (block.PreviousHash != EmptyHash || block.Hash != EmptyHash)
            {
                return false;
            }
            if (block.Transactions.Count > 0)
            {
                return false;
            }
            return true;
        }

        private static bool VerifyBlock(this Block block, Context context, ref int lineCount)
        {
            if (block.Transactions.Count == 0)
            {
                return false;
            }

            foreach (var transaction in block.Transactions)
            {
                if (!transaction.VerifyTransaction(context, ref lineCount))
                {
                    return false;
                }
            }
         
            if (block.Hash != CalculateHash(block.Index, block.Timestamp, block.PreviousHash, block.Transactions))
            {
                return false;
            }
            return true;
        }
        
        private static bool VerifyTransaction(this Transaction transaction, Context context, ref int lineCount)
        {
            if (string.IsNullOrEmpty(transaction.FromAddress) || 
                string.IsNullOrEmpty(transaction.ToAddress) || 
                string.IsNullOrEmpty(transaction.Hash) || 
                string.IsNullOrEmpty(transaction.Signature) || 
                string.IsNullOrEmpty(transaction.Data))
            {
                return false;
            }
            
            // Validate data
            if (transaction.Data != context.Poem[lineCount])
            {
                return false;
            }

            lineCount++;
            
            // Validate signature
            var keyPair = context.GetKeyPairForAddress(transaction.FromAddress);
            if (keyPair == null)
            {
                throw new ApplicationException($"Address {transaction.FromAddress} could not be resolved.");
            }

            if (!KeyHelpers.VerifyDataFromKeyText(keyPair.PublicKey, transaction.Data, transaction.Signature))
            {
                return false;
            }

            // Validate hash
            if (transaction.Hash != CalculateHash(transaction, transaction.Index, transaction.Signature))
            {
                return false;
            }
            
            return true;
        }
        
        
    }
}