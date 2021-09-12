using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Consensys.BlockchainBootcamp2021.Exercises.Common
{
	public static class KeyHelpers
	{
		private const int BufferSize = 8 * 1024;
		
		private static readonly ThreadLocal<List<byte[]>> Buffers = new(() => new List<byte[]>
			{
				new byte[BufferSize],
				new byte[BufferSize]
			});

	    private static readonly SymmetricKeyAlgorithmTag[] SymmetricAlgorithms = {
		    SymmetricKeyAlgorithmTag.Aes256,
		    SymmetricKeyAlgorithmTag.Aes192,
		    SymmetricKeyAlgorithmTag.Aes128
	    };
	    
		private static readonly HashAlgorithmTag[] HashAlgorithms = {
			HashAlgorithmTag.Sha256,
			HashAlgorithmTag.Sha1,
			HashAlgorithmTag.Sha384,
			HashAlgorithmTag.Sha512,
			HashAlgorithmTag.Sha224,
		};
		
	    public static KeyPair GenerateRsaKeyRingPair(
			int keySize = RsaKeySize.Size2048,
			string name = "id_rsa", 
		    string identity = null, 
		    string passPhrase = null) {

	        var rsaKeyGenerationParameters = new RsaKeyGenerationParameters(
		        BigInteger.ValueOf(0x10001), 
		        new SecureRandom(), 
		        keySize, 
		        25);
	        
	        var keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator("RSA");
	        keyPairGenerator.Init(rsaKeyGenerationParameters);

	        var timestamp = DateTime.UtcNow;

	        // Create the master (signing-only) key
	        var masterKeyPair = new PgpKeyPair(
	            PublicKeyAlgorithmTag.RsaSign,
	            keyPairGenerator.GenerateKeyPair(),
	            timestamp);
	        
	        var masterSubpacketGenerator = new PgpSignatureSubpacketGenerator();
	        SetSubpacketGenerator(
		        masterSubpacketGenerator, 
		        PgpKeyFlags.CanSign | PgpKeyFlags.CanCertify);
	        
	        // Create a signing and encryption key for general use
	        var encryptionKeyPair = new PgpKeyPair(
	            PublicKeyAlgorithmTag.RsaGeneral,
	            keyPairGenerator.GenerateKeyPair(),
	            timestamp);
	        
	        var encryptionSubpacketGenerator = new PgpSignatureSubpacketGenerator();
	        SetSubpacketGenerator(
		        masterSubpacketGenerator, 
		        PgpKeyFlags.CanEncryptCommunications | PgpKeyFlags.CanEncryptStorage);
	        
	        // Create the key ring 
	        var keyRingGenerator = new PgpKeyRingGenerator(
	            PgpSignature.DefaultCertification,
	            masterKeyPair,
	            identity ?? string.Empty,
	            SymmetricKeyAlgorithmTag.Aes128,
	            (passPhrase ?? string.Empty).ToCharArray(),
	            true,
	            masterSubpacketGenerator.Generate(),
	            null,
	            new SecureRandom());

			// Add encryption subkey
	        keyRingGenerator.AddSubKey(
		        encryptionKeyPair, 
		        encryptionSubpacketGenerator.Generate(), 
		        null);

	        var secretKeyRing = keyRingGenerator.GenerateSecretKeyRing();
	        var publicKeyRing = keyRingGenerator.GeneratePublicKeyRing();

	        return CreateKeyPair(
		        KeyType.KeyRing,
		        keySize, 
		        name, 
		        identity, 
		        passPhrase, 
		        p => secretKeyRing.Encode(p),
		        p => publicKeyRing.Encode(p));
	    }
  
	    public static KeyPair GenerateRsaKeyPair(
		    int keySize = RsaKeySize.Size2048,
		    string name = "id_rsa", 
		    string identity = null, 
		    string passPhrase = null)
	    {
		    var keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator("RSA");
   
		    keyPairGenerator.Init(new RsaKeyGenerationParameters(
			    BigInteger.ValueOf(0x10001),
			    new SecureRandom(),
			    keySize,
			    25));
   
		    var keyPair = keyPairGenerator.GenerateKeyPair();
		   
		    var secretKey = new PgpSecretKey(
			    PgpSignature.DefaultCertification,
			    PublicKeyAlgorithmTag.RsaGeneral,
			    keyPair.Public,
			    keyPair.Private,
			    DateTime.UtcNow,
			    identity ?? string.Empty,
			    SymmetricKeyAlgorithmTag.Cast5,
			    (passPhrase ?? string.Empty).ToCharArray(),
			    null,
			    null,
			    new SecureRandom());
		    
		    return CreateKeyPair(
			    KeyType.Standalone,
			    keySize, 
			    name, 
			    identity, 
			    passPhrase, 
			    p => secretKey.Encode(p),
			    p => secretKey.PublicKey.Encode(p));
	    }
	    
	    public static string SignData(
		    string privateKeyFilePath,
		    string passPhrase,
		    string data)
	    {
		    var binarySignature = SignData(
			    privateKeyFilePath, 
			    passPhrase, 
			    Encoding.UTF8.GetBytes(data));
		    
		    return Convert.ToBase64String(binarySignature);
	    }

	    public static byte[] SignData(
		    string privateKeyFilePath,
		    string passPhrase,
		    byte[] data)
	    {
		    if (!File.Exists(privateKeyFilePath))
		    {
			    throw new ApplicationException($"File {privateKeyFilePath} does not exist.");
		    }
		    
		    var privateKeyFile = File.OpenRead(privateKeyFilePath);
		    var secretKey = ReadSecretKey(privateKeyFile, p => p.IsSigningKey);

		    PgpPrivateKey privateKey;
		    try
		    {
			    privateKey = secretKey.ExtractPrivateKey(passPhrase.ToCharArray());
		    }
		    catch (Exception)
		    {
			    throw new ApplicationException("Passphrase is wrong for this keyring.");
		    }

		    var signer = SignerUtilities.GetSigner("SHA1withRSA");
		    signer.Init(true, privateKey.Key);
		    signer.BlockUpdate(data, 0, data.Length);
		    
		    privateKeyFile.Close();
		    return signer.GenerateSignature();
	    }

	    public static bool VerifyData(
		    string publicKeyFilePath,
		    string data,
		    string signature)
	    {
		    return VerifyData(
			    publicKeyFilePath, 
			    Encoding.UTF8.GetBytes(data),
			    Convert.FromBase64String(signature));
	    }

	    public static bool VerifyData(
		    string publicKeyFilePath,
		    byte[] data, 
		    byte[] signature)
	    {
		    if (!File.Exists(publicKeyFilePath))
		    {
			    throw new ApplicationException($"File {publicKeyFilePath} does not exist.");
		    }
		    
		    var publicKeyFile = File.OpenRead(publicKeyFilePath);
		    var publicKey = ReadPublicKey(publicKeyFile, p => p.IsMasterKey);
		    
		    var signer = SignerUtilities.GetSigner("SHA1withRSA");
		    signer.Init(false, publicKey.GetKey());
		    signer.BlockUpdate(data, 0, data.Length);
		    
		    publicKeyFile.Close();

		    return signer.VerifySignature(signature);
	    }
	    
		private static PgpPublicKey ReadPublicKey(Stream stream, Func<PgpPublicKey, bool> filter)
		{
			var publicKeyRingBundle = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(stream));
			var key = publicKeyRingBundle
				.GetKeyRings()
				.Cast<PgpPublicKeyRing>()
				.SelectMany(p => p.GetPublicKeys().Cast<PgpPublicKey>())
				.FirstOrDefault(filter);
			
			if (key == null)
			{
				throw new ArgumentException("No public key suitable for encryption could be found in the key ring.");
			}
			return key;
		}

		private static PgpSecretKey ReadSecretKey(Stream stream, Func<PgpSecretKey, bool> filter)
		{
			var secretKeyRingBundle = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(stream));
			var key = secretKeyRingBundle
				.GetKeyRings()
				.Cast<PgpSecretKeyRing>()
				.SelectMany(p => p.GetSecretKeys().Cast<PgpSecretKey>())
				.FirstOrDefault(filter);

			if (key == null)
			{
				throw new ArgumentException("No private key suitable for signing could be found in the key ring.");
			}
			return key;
		}
	
		private static KeyPair CreateKeyPair(
		    KeyType type,
		    int keySize, 
		    string name, 
		    string identity, 
		    string passPhrase,
		    Action<ArmoredOutputStream> privateKeyAction, 
		    Action<ArmoredOutputStream> publicKeyAction)
	    {
		    var buffers = Buffers.Value;
		    var privateKeyMemoryStream = new MemoryStream(buffers[0]);
		    var publicKeyMemoryStream = new MemoryStream(buffers[1]);
		    var privateKeyEncodeStream = new ArmoredOutputStream(privateKeyMemoryStream);
		    var publicKeyEncodeStream = new ArmoredOutputStream(publicKeyMemoryStream);

		    privateKeyAction(privateKeyEncodeStream);
		    publicKeyAction(publicKeyEncodeStream);
		    
		    privateKeyEncodeStream.Close();
		    publicKeyEncodeStream.Close();
	        
		    var privateKeyText = Encoding.Default.GetString(
			    buffers[0], 
			    0, 
			    (int)privateKeyMemoryStream.Position).Trim();
		    
		    var publicKeyText = Encoding.Default.GetString(
			    buffers[1], 
			    0, 
			    (int)publicKeyMemoryStream.Position).Trim();

		    return new KeyPair(
			    type,
			    keySize, 
			    name, 
			    identity, 
			    passPhrase,
			    privateKeyText, 
			    publicKeyText);
	    }
	    
	    public static async Task ToFile(
		    this KeyPair keyPair, 
		    string path = null)
	    {
		    path = string.IsNullOrEmpty(path) ? Environment.CurrentDirectory : path;

		    switch (keyPair.Type)
		    {
			    case KeyType.Standalone:
				    await File.WriteAllTextAsync(Path.Combine(path, $"{keyPair.Name}"), keyPair.PrivateKey);
				    await File.WriteAllTextAsync(Path.Combine(path, $"{keyPair.Name}.pub"), keyPair.PublicKey);
				    break;
			    case KeyType.KeyRing:
				    await File.WriteAllTextAsync(Path.Combine(path, $"{keyPair.Name}.skr"), keyPair.PrivateKey);
				    await File.WriteAllTextAsync(Path.Combine(path, $"{keyPair.Name}.pkr"), keyPair.PublicKey);
				    break;
		    }
	    }
	    
	    private static void SetSubpacketGenerator(
		    PgpSignatureSubpacketGenerator subpacketGenerator, 
		    int flags)
	    {
		    subpacketGenerator.SetKeyFlags(false, flags);
		    subpacketGenerator.SetPreferredSymmetricAlgorithms(false, SymmetricAlgorithms.Select(p => (int) p).ToArray());
		    subpacketGenerator.SetPreferredHashAlgorithms(false, HashAlgorithms.Select(p => (int) p).ToArray());
	    }
    }
}