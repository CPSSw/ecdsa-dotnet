using EllipticCurve;
using Xunit;

namespace CPCC.EcdsaTests
{
    public class TestEcdsa
    {
        [Fact]
        public void TestVerifyRightMessage()
        {
            PrivateKey privateKey = new();
            var publicKey = privateKey.PublicKey();
            var message = "This is the right message";
            var signature = Ecdsa.Sign(message, privateKey);

            Assert.True(Ecdsa.Verify(message, signature, publicKey));
        }

        [Fact]
        public void TestVerifyWrongMessage()
        {
            PrivateKey privateKey = new();
            var publicKey = privateKey.PublicKey();
            var message1 = "This is the right message";
            var message2 = "This is the wrong message";
            var signature = Ecdsa.Sign(message1, privateKey);

            Assert.False(Ecdsa.Verify(message2, signature, publicKey));
        }
    }
}