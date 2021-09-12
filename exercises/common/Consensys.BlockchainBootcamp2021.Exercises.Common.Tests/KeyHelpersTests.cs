using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Consensys.BlockchainBootcamp2021.Exercises.Common.Tests
{
    public class KeyHelpersTests
    {
        [Test]
        public void ShouldGenerateRsaKeyRingPair()
        {
            var keyName = Guid.NewGuid().ToString();
            
            var keyRingPair = KeyHelpers.GenerateRsaKeyRingPair(
                RsaKeySize.Size2048,
                $"key-{keyName}",
                "john.doe@acme.com", 
                "pass");
            
            Assert.IsNotNull(keyRingPair);
            Assert.AreEqual(keyRingPair.Name, $"key-{keyName}");
            Assert.AreEqual(keyRingPair.Identity, "john.doe@acme.com");
            Assert.AreEqual(keyRingPair.Size, RsaKeySize.Size2048);
            Assert.IsNotNull(keyRingPair.PrivateKey);
            Assert.IsNotNull(keyRingPair.PublicKey);
            
            Assert.IsTrue(keyRingPair.PrivateKey.StartsWith("-----BEGIN PGP PRIVATE KEY BLOCK-----"));
            Assert.IsTrue(keyRingPair.PrivateKey.EndsWith("-----END PGP PRIVATE KEY BLOCK-----"));
            Assert.IsTrue(keyRingPair.PublicKey.StartsWith("-----BEGIN PGP PUBLIC KEY BLOCK-----"));
            Assert.IsTrue(keyRingPair.PublicKey.EndsWith("-----END PGP PUBLIC KEY BLOCK-----"));
        }
        
        [Test]
        public async Task ShouldGenerateRsaKeyRingPairToFile()
        {
            var keyName = Guid.NewGuid().ToString();
            
            var keyPair = KeyHelpers.GenerateRsaKeyRingPair(
                RsaKeySize.Size2048,
                $"key-{keyName}",
                "john.doe@acme.com", 
                "pass");

            await keyPair.ToFile(Environment.CurrentDirectory); 
            
            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"key-{keyName}.skr")));
            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"key-{keyName}.pkr")));
            
            File.Delete(Path.Combine(Environment.CurrentDirectory, $"key-{keyName}.skr"));
            File.Delete(Path.Combine(Environment.CurrentDirectory, $"key-{keyName}.pkr"));
        }
        
        [Test]
        public void ShouldCreateRsaKeyPair()
        {
            var keyName = Guid.NewGuid().ToString();
            
            var keyPair = KeyHelpers.GenerateRsaKeyPair(
                RsaKeySize.Size2048, 
                $"key-{keyName}",
                "john.doe@acme.com",
                "pass");
            
            Assert.IsNotNull(keyPair);
            Assert.AreEqual(keyPair.Name, $"key-{keyName}");
            Assert.AreEqual(keyPair.Identity, "john.doe@acme.com");
            Assert.AreEqual(keyPair.Size, RsaKeySize.Size2048);
            Assert.IsNotNull(keyPair.PrivateKey);
            Assert.IsNotNull(keyPair.PublicKey);
            
            Assert.IsTrue(keyPair.PrivateKey.StartsWith("-----BEGIN PGP PRIVATE KEY BLOCK-----"));
            Assert.IsTrue(keyPair.PrivateKey.EndsWith("-----END PGP PRIVATE KEY BLOCK-----"));
            Assert.IsTrue(keyPair.PublicKey.StartsWith("-----BEGIN PGP PUBLIC KEY BLOCK-----"));
            Assert.IsTrue(keyPair.PublicKey.EndsWith("-----END PGP PUBLIC KEY BLOCK-----"));
        }
        
        [Test]
        public async Task ShouldCreateRsaKeyPairToFile()
        {
            var keyName = Guid.NewGuid().ToString();
            
            var keyPair = KeyHelpers.GenerateRsaKeyPair(
                RsaKeySize.Size2048,
                $"key-{keyName}",
                "john.doe@acme.com");

            await keyPair.ToFile(Environment.CurrentDirectory); 
            
            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"key-{keyName}")));
            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, $"key-{keyName}.pub")));
            
            File.Delete(Path.Combine(Environment.CurrentDirectory, $"key-{keyName}"));
            File.Delete(Path.Combine(Environment.CurrentDirectory, $"key-{keyName}.pub"));
        }
        
        [Test]
        public async Task ShouldSignAndVerify()
        {
            var data = string.Concat(Enumerable.Range(1, 50).Select(p => Guid.NewGuid().ToString().Replace("-", "")));
            var keyName = Guid.NewGuid().ToString();
            
            var keyPair = KeyHelpers.GenerateRsaKeyRingPair(
                RsaKeySize.Size2048,
                $"key-{keyName}",
                "john.doe@acme.com", 
                "pass");

            await keyPair.ToFile(Environment.CurrentDirectory); 
            
            var signature = KeyHelpers.SignData(
                Path.Combine(Environment.CurrentDirectory, $"key-{keyName}.skr"), 
                "pass", 
                data);

            var isValid = KeyHelpers.VerifyData(
                Path.Combine(Environment.CurrentDirectory, $"key-{keyName}.pkr"), 
                data,
                signature);
            
            Assert.IsTrue(isValid);
            
            File.Delete(Path.Combine(Environment.CurrentDirectory, $"key-{keyName}.skr"));
            File.Delete(Path.Combine(Environment.CurrentDirectory, $"key-{keyName}.pkr"));
        }
        
       
    }
}