using System;
using Nethereum.Util;

namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Hashing
{
    public static class BlockchainExtensions
    {
        private static readonly string EmptyHash = new('0', 64);
        private static readonly Sha3Keccack HashProvider = new();

        public static string CalculateHash(string data)
        {
            return HashProvider.CalculateHash(data);
        }
        
        public static string CalculateHash(
            long index, 
            DateTimeOffset timestamp, 
            string previousHash, 
            string data)
        {
            var hashString = $"{index:000000000000000}{timestamp.ToUnixTimeMilliseconds():000000000000000}{previousHash}{data}";
            return HashProvider.CalculateHash(hashString);
        }
        
        public static bool Verify(this Blockchain blockchain)
        {
            if (blockchain.Blocks.Count == 0)
            {
                return false;
            }

            if (!blockchain.Blocks[0].VerifyGenesisBlock())
            {
                return false;
            }

            for (int i = 1; i < blockchain.Blocks.Count; i++)
            {
                if (!blockchain.Blocks[i].VerifyBlock())
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
            if (block.Data != string.Empty)
            {
                return false;
            }
            return true;
        }

        private static bool VerifyBlock(this Block block)
        {
            var hash = CalculateHash(block.Index, block.Timestamp, block.PreviousHash, block.Data);
            return block.Hash == hash;
        }
    }
}