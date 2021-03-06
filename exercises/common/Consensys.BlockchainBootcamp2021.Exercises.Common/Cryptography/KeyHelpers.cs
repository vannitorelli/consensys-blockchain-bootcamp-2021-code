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

namespace Consensys.BlockchainBootcamp2021.Exercises.Common.Cryptography
{
	public static class KeyHelpers
	{
		private const int BufferSize = 16 * 1024;
		private static readonly SecureRandom SecureRandom = SecureRandom.GetInstance("SHA1PRNG", false);
		
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
		    string passPhrase = null)
	    {
		   var rsaKeyGenerationParameters = new RsaKeyGenerationParameters(
		        BigInteger.ValueOf(0x10001), 
		        SecureRandom,
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
	            SecureRandom);

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
			    SecureRandom,
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
			    SecureRandom);
		    
		    return CreateKeyPair(
			    KeyType.Standalone,
			    keySize, 
			    name, 
			    identity, 
			    passPhrase, 
			    p => secretKey.Encode(p),
			    p => secretKey.PublicKey.Encode(p));
	    }
	    
	    public static string SignDataFromKeyFile(
		    string privateKeyFilePath,
		    string passPhrase,
		    string data)
	    {
		    if (!File.Exists(privateKeyFilePath))
		    {
			    throw new ApplicationException($"File {privateKeyFilePath} does not exist.");
		    }
		    
		    var stream = File.OpenRead(privateKeyFilePath);
		    
		    var binarySignature = DoSignData(
			    stream, 
			    passPhrase, 
			    Encoding.UTF8.GetBytes(data));
		    
		    stream.Close();
		    
		    return Convert.ToBase64String(binarySignature);
	    }

	    public static string SignDataFromKeyText(
		    string privateKeyText,
		    string passPhrase,
		    string data)
	    {
		    var stream = new MemoryStream(Encoding.Default.GetBytes(privateKeyText));
		    
		    var binarySignature = DoSignData(
			    stream, 
			    passPhrase, 
			    Encoding.UTF8.GetBytes(data));
		    
		    stream.Close();

		    return Convert.ToBase64String(binarySignature);;
	    }

	    public static bool VerifyDataFromKeyFile(
		    string publicKeyFilePath,
		    string data,
		    string signature)
	    {
		    if (!File.Exists(publicKeyFilePath))
		    {
			    throw new ApplicationException($"File {publicKeyFilePath} does not exist.");
		    }
		    
		    var stream = File.OpenRead(publicKeyFilePath);
		    var outcome = DoVerifyData(
			    stream, 
			    Encoding.UTF8.GetBytes(data),
			    Convert.FromBase64String(signature));
		    
		    stream.Close();
		    return outcome;
	    }
	    
	    public static bool VerifyDataFromKeyText(
		    string publicKeyText,
		    string data,
		    string signature)
	    {
		    var stream = new MemoryStream(Encoding.Default.GetBytes(publicKeyText));
		    var outcome = DoVerifyData(
			    stream, 
			    Encoding.UTF8.GetBytes(data),
			    Convert.FromBase64String(signature));
		    
		    stream.Close();
		    return outcome;
	    }

	   

	    public static IReadOnlyList<PgpPublicKey> GetPublicKeysFromKeyFile(string publicKeyFilePath)
	    {
		    if (!File.Exists(publicKeyFilePath))
		    {
			    throw new ApplicationException($"File {publicKeyFilePath} does not exist.");
		    }

		    var stream = File.OpenRead(publicKeyFilePath);
		    var publicKeys = DoGetPublicKeys(stream);
		    stream.Close();
		    return publicKeys;
	    }
	    
	    public static IReadOnlyList<PgpPublicKey> GetPublicKeysFromKeyText(string publicKeyText)
	    {
		    var stream = new MemoryStream(Encoding.Default.GetBytes(publicKeyText));
		    var publicKeys = DoGetPublicKeys(stream);
		    stream.Close();
		    return publicKeys;
	    }
	    
	    public static IReadOnlyList<PgpPrivateKey> GetPrivateKeysFromKeyFile(
		    string privateKeyFilePath,
		    string passPhrase)
	    {
		    if (!File.Exists(privateKeyFilePath))
		    {
			    throw new ApplicationException($"File {privateKeyFilePath} does not exist.");
		    }

		    var stream = File.OpenRead(privateKeyFilePath);
		    var privateKeys = DoGetPrivateKeys(stream, passPhrase);
		    stream.Close();
		    return privateKeys;
	    }
	    
	    public static IReadOnlyList<PgpPrivateKey> GetPrivateKeysFromKeyText(
		    string privateKeyText,
		    string passPhrase)
	    {
		    var stream = new MemoryStream(Encoding.Default.GetBytes(privateKeyText));
		    var privateKeys = DoGetPrivateKeys(stream, passPhrase);
		    stream.Close();
		    return privateKeys;
	    }
	    
	    private static byte[] DoSignData(
		    Stream stream,
		    string passPhrase,
		    byte[] data)
	    {
		    var privateKey = DoGetPrivateKeys(stream, passPhrase).FirstOrDefault();
		    if (privateKey == null)
		    {
			    throw new ApplicationException("Could not find a suitable private key in the key ring.");
		    }
		    
		    var signer = SignerUtilities.GetSigner("SHA1withRSA");
		    signer.Init(true, privateKey.Key);
		    signer.BlockUpdate(data, 0, data.Length);
		    
		    return signer.GenerateSignature();
	    }
	    
	    private static bool DoVerifyData(
		    Stream stream,
		    byte[] data, 
		    byte[] signature)
	    {
		    var publicKey = DoGetPublicKeys(stream).FirstOrDefault(p => p.IsMasterKey);
		    if (publicKey == null)
		    {
			    throw new ApplicationException("Could not find a suitable public key in the key ring.");
		    }
		    
		    var signer = SignerUtilities.GetSigner("SHA1withRSA");
		    signer.Init(false, publicKey.GetKey());
		    signer.BlockUpdate(data, 0, data.Length);
		    
		    return signer.VerifySignature(signature);
	    }
	    
	    private static IReadOnlyList<PgpPublicKey> DoGetPublicKeys(Stream stream)
	    {
		    var publicKeyRingBundle = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(stream));
		    var publicKeys = publicKeyRingBundle
			    .GetKeyRings()
			    .Cast<PgpPublicKeyRing>()
			    .SelectMany(p => p.GetPublicKeys().Cast<PgpPublicKey>())
			    .ToList();
		    
		    return publicKeys;
	    }
	    
	    private static IReadOnlyList<PgpPrivateKey> DoGetPrivateKeys(
		    Stream stream, 
		    string passPhrase)
	    {
		    var secretKeyRingBundle = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(stream));
		    var privateKeys = secretKeyRingBundle
			    .GetKeyRings()
			    .Cast<PgpSecretKeyRing>()
			    .SelectMany(p => p.GetSecretKeys().Cast<PgpSecretKey>())
			    .Where(p => p.IsSigningKey)
			    .Select(p =>
				    {
					    try
					    {
						    return p.ExtractPrivateKey(passPhrase.ToCharArray());
					    }
					    catch (Exception)
					    {
						    return null;
					    }
				    })
			    .ToList();

		    if (privateKeys.Any(p => p == null))
		    {
			    throw new ApplicationException("Passphrase wrong for this keyring");
		    }
		    
		    return privateKeys;
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