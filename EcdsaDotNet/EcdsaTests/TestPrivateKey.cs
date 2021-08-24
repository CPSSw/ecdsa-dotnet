using CPSS.EllipticCurve;
using Xunit;

namespace CPCC.EcdsaTests
{
    public class TestPrivateKey
    {
        [Fact]
        public void TestPemConversion()
        {
            PrivateKey privateKey1 = new();
            var pem = privateKey1.ToPem();
            var privateKey2 = PrivateKey.FromPem(pem);

            Assert.Equal(privateKey1.Secret, privateKey2.Secret);
            Assert.Equal(privateKey1.Curve, privateKey2.Curve);
        }

        [Fact]
        public void TestDerConversion()
        {
            PrivateKey privateKey1 = new();
            var der = privateKey1.ToDer();
            var privateKey2 = PrivateKey.FromDer(der);

            Assert.Equal(privateKey1.Secret, privateKey2.Secret);
            Assert.Equal(privateKey1.Curve, privateKey2.Curve);
        }

        [Fact]
        public void TestStringConversion()
        {
            PrivateKey privateKey1 = new();
            var str = privateKey1.ToStringFromNumber();
            var privateKey2 = PrivateKey.FromString(str);

            Assert.Equal(privateKey1.Secret, privateKey2.Secret);
            Assert.Equal(privateKey1.Curve, privateKey2.Curve);
        }
    }
}