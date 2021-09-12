namespace Consensys.BlockchainBootcamp2021.Exercises.Chapter1.Transactions
{
    public interface IPendingBlock
    {
        void AddTransaction(string fromAddress, string toAddress, string data);
        void Commit(Context context);
    }
}