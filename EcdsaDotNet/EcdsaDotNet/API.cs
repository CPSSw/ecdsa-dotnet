using System;
using System.IO;

namespace CPSS.EllipticCurve
{
    public sealed class API
    {
        public void SignXmlFile(FileInfo fileInfo, FileInfo pemFile)
        {
            var pem = File.ReadAllText(pemFile.FullName);
            var privateKey = PrivateKey.FromPem(pem);
            var signature = Ecdsa.Sign(message, privateKey);
            signature.To
            var publicKey = privateKey.PublicKey();
        }

        public void SignBinaryFile(FileInfo fileInfo)
        {
            
        }

        public bool VerifyXmlFileSignature(FileInfo fileInfo)
        {
            string publicKeyPem = EllipticCurve.Utils.File.read("publicKey.pem");
            byte[] signatureDer = EllipticCurve.Utils.File.readBytes("signatureDer.txt");
            string message = EllipticCurve.Utils.File.read("message.txt");

            PublicKey publicKey = PublicKey.fromPem(publicKeyPem);
            Signature signature = Signature.fromDer(signatureDer);

            Console.WriteLine(Ecdsa.verify(message, signature, publicKey));
        }
        public bool VerifyBinaryFileSignature(FileInfo fileInfo)
        {
            try
            {

            }
            catch (Exception exception)
            {
                
            }
        }
    }
}