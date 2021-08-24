using EllipticCurve;
using Xunit;

namespace CPCC.EcdsaTests
{
    public class TestPublicKey
    {
        [Fact]
        public void TestPemConversion()
        {
            PrivateKey privateKey = new();
            var publicKey1 = privateKey.PublicKey();
            var pem = publicKey1.ToPem();
            var publicKey2 = PublicKey.FromPem(pem);

            Assert.Equal(publicKey1.Point.X, publicKey2.Point.X);
            Assert.Equal(publicKey1.Point.Y, publicKey2.Point.Y);
            Assert.Equal(publicKey1.Curve, publicKey2.Curve);
        }

        [Fact]
        public void TestDerConversion()
        {
            PrivateKey privateKey = new();
            var publicKey1 = privateKey.PublicKey();
            var der = publicKey1.ToDer();
            var publicKey2 = PublicKey.FromDer(der);

            Assert.Equal(publicKey1.Point.X, publicKey2.Point.X);
            Assert.Equal(publicKey1.Point.Y, publicKey2.Point.Y);
            Assert.Equal(publicKey1.Curve, publicKey2.Curve);
        }

        [Fact]
        public void TestStringConversion()
        {
            PrivateKey privateKey = new();
            var publicKey1 = privateKey.PublicKey();
            var str = publicKey1.ToString();
            var publicKey2 = PublicKey.FromString(str);

            Assert.Equal(publicKey1.Point.X, publicKey2.Point.X);
            Assert.Equal(publicKey1.Point.Y, publicKey2.Point.Y);
            Assert.Equal(publicKey1.Curve, publicKey2.Curve);
        }
    }
}