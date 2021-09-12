namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Transactions
{
    public class PendingTransaction
    {
        public PendingTransaction(
            int index,
            string fromAddress, 
            string toAddress, 
            string data)
        {
            Index = index;
            FromAddress = fromAddress;
            ToAddress = toAddress;
            Data = data;
        }

        public override string ToString() => $"{FromAddress} -> {ToAddress}: {Data}";

        public int Index { get; }
        
        public string FromAddress { get; }
            
        public string ToAddress { get; }
            
        public string Data { get; }
    }
}