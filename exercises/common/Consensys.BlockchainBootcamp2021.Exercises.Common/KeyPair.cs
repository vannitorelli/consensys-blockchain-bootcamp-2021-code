namespace Consensys.BlockchainBootcamp2021.Exercises.Common
{
    public class KeyPair
    {
        public KeyPair(
            KeyType type,
            int size, 
            string name, 
            string identity, 
            string passPhrase,
            string privateKey, 
            string publicKey)
        {
            Type = type;
            Size = size;
            Name = name;
            Identity = identity;
            PassPhrase = passPhrase;
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }

        public override string ToString()
        {
            var output = $"RSA-{Size} {Name}";
            return !string.IsNullOrEmpty(Identity) ? $"{output} ({Identity})" : output;
        }

        public KeyType Type { get; }
        public int Size { get; }
        public string Name { get; }
        public string Identity { get; }
        public string PassPhrase { get; }
        public string PrivateKey { get; }
        public string PublicKey { get; }
    }
}