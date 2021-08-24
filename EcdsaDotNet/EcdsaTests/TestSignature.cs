using EllipticCurve;
using Xunit;

namespace CPCC.EcdsaTests
{
    public class TestSignature
    {
        [Fact]
        public void TestDerConversion()
        {
            PrivateKey privateKey = new();
            var message = "This is a text message";

            var signature1 = Ecdsa.Sign(message, privateKey);

            var der = signature1.ToDer();
            var signature2 = Signature.FromDer(der);

            Assert.Equal(signature1.R, signature2.R);
            Assert.Equal(signature1.S, signature2.S);
        }

        [Fact]
        public void TestBase64Conversion()
        {
            PrivateKey privateKey = new();
            var message = "This is a text message";

            var signature1 = Ecdsa.Sign(message, privateKey);

            var base64 = signature1.ToBase64();
            var signature2 = Signature.FromBase64(base64);

            Assert.Equal(signature1.R, signature2.R);
            Assert.Equal(signature1.S, signature2.S);
        }
    }
}