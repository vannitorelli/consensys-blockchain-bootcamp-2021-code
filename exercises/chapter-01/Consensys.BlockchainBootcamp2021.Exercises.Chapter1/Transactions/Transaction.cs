namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Transactions
{
    public class Transaction : PendingTransaction
    {
        public Transaction(
            int index, 
            string fromAddress, 
            string toAddress, 
            string signature,
            string hash,
            string data) 
            : base(index, fromAddress, toAddress, data)
        {
            Signature = signature;
            Hash = hash;
        }
        
        public override string ToString() => $"{FromAddress} -> {ToAddress}: {Data}";
        
        public string Signature { get; }
        
        public string Hash { get; }
    }
}