namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Transactions
{
    public readonly struct AddressPair
    {
        public AddressPair(string fromAddress, string toAddress)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
        }

        public string FromAddress { get; }
        
        public string ToAddress { get; }
    }
}