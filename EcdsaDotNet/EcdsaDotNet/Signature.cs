using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CPSS.EllipticCurve.Utils;

namespace CPSS.EllipticCurve
{
    public class Signature
    {
        public const string XmlDsigNamespaceUrl = "http://www.w3.org/2000/09/xmldsig#";
        public Signature(BigInteger r, BigInteger s)
        {
            R = r;
            S = s;
        }

        public BigInteger R { get; }
        public BigInteger S { get; }

        public byte[] ToDer()
        {
            var sequence = new List<byte[]> { Der.EncodeInteger(R), Der.EncodeInteger(S) };
            return Der.EncodeSequence(sequence);
        }

        public string ToBase64()
        {
            return Base64.Encode(ToDer());
        }

        public XElement ToXml()
        {
            // 006   <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
            //     007     <SignedInfo>
            //     008       <CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315" />
            //     009       <SignatureMethod Algorithm="http://www.w3.org/2000/09/xmldsig#rsa-sha1" />
            //     010       <Reference URI="">
            //     011         <Transforms>
            //     012           <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature" />
            //     013         </Transforms>
            //     014         <DigestMethod Algorithm="http://www.w3.org/2000/09/xmldsig#sha1" />
            //     015         <DigestValue>UWuYTYug10J1k5hKfonxthgrAR8=</DigestValue>
            //     016       </Reference>
            //     017     </SignedInfo>
            //     018     <SignatureValue>
            //     019       TSQUoVrQ0kg1eiltNwIhKPrIdsi1VhWjYNJlXvfQqW2EKk3X37X862SCfrz7v8IYJ7OorWwlFpGDStJDSR6saO
            // 020       ScqSvmesCrGEEq+U6zegR9nH0lvcGZ8Rvc/y7U9kZrE4fHqEiLyfpmzJyPmWUT9Uta14nPJYsl3cmdThHB8Bs=
            //     021     </SignatureValue>
            //     022     <KeyInfo>
            //     023       <KeyValue>
            //     024          <RSAKeyValue>
            //     025            <Modulus>
            //     026              4IlzOY3Y9fXoh3Y5f06wBbtTg94Pt6vcfcd1KQ0FLm0S36aGJtTSb6pYKfyX7PqCUQ8wgL6xUJ5GRPEsu9gyz8
            // 027              ZobwfZsGCsvu40CWoT9fcFBZPfXro1Vtlh/xl/yYHm+Gzqh0Bw76xtLHSfLfpVOrmZdwKmSFKMTvNXOFd0V18=
            //     028            </Modulus>
            //     029            <Exponent>AQAB</Exponent>
            //     030          </RSAKeyValue>
            //     031       </KeyValue>
            //     032     </KeyInfo>
            //     033   </Signature>

            var signatureElement = 
                new XElement("Signature",
                    new XAttribute("xmlns", XmlDsigNamespaceUrl),
                    new XElement("SignedInfo", 
                        new XElement("CanonicalizationMethod", new XAttribute("Algorithm", "")),
                        new XElement("SignatureMethod", new XAttribute("Algorithm", $"http://www.w3.org/2000/09/xmldsig#rsa-"_))
                );  
            
            signatureElement.AppendChild(_signedInfo.GetXml(document));
            var signatureValueElement = new XElement("SignatureValue", ToBase64());
            signatureElement.Add(signatureValueElement);

            // Add the KeyInfo
            if (KeyInfo.Count > 0)
                signatureElement.AppendChild(KeyInfo.GetXml(document));

            // Add the Objects
            foreach (object obj in _embeddedObjects)
            {
                DataObject dataObj = obj as DataObject;
                if (dataObj != null)
                {
                    signatureElement.AppendChild(dataObj.GetXml(document));
                }
            }

            return signatureElement;        }
        
        public static Signature FromDer(byte[] bytes)
        {
            var (rs, removeSequenceTrail) = Der.RemoveSequence(bytes);

            if (removeSequenceTrail.Length > 0)
                throw new ArgumentException("trailing junk after DER signature: " + BinaryAscii.HexFromBinary(removeSequenceTrail));

            var removeInteger = Der.RemoveInteger(rs);
            var r = removeInteger.Item1;
            var rest = removeInteger.Item2;

            removeInteger = Der.RemoveInteger(rest);
            var s = removeInteger.Item1;
            var removeIntegerTrail = removeInteger.Item2;

            if (removeIntegerTrail.Length > 0)
                throw new ArgumentException("trailing junk after DER numbers: " + BinaryAscii.HexFromBinary(removeIntegerTrail));

            return new Signature(r, s);
        }

        public static Signature FromBase64(string str)
        {
            return FromDer(Base64.Decode(str));
        }
    }
}