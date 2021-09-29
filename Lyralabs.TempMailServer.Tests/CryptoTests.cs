using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lyralabs.TempMailServer.Tests
{
    [TestClass]
    public class CryptoTests
    {
        [TestMethod]
        public void CanDecryptPublicKeyFromPrivateKey()
        {
            // Arrange
            var cryptoServiceSource = new AsymmetricCryptoService();
            var cryptoServiceTarget = new AsymmetricCryptoService();

            var secret = cryptoServiceSource.GenerateUserSecret();

            // Act
            var publicKey = cryptoServiceTarget.GetPublicKey(secret.PrivateKey);

            // Assert
            Assert.AreEqual(secret.PublicKey, publicKey);
        }
    }
}
