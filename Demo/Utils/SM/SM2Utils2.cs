using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverSoft.Utils.SM
{
    public class SM2Utils2
    {
        private static X9ECParameters sm2Parameters = GMNamedCurves.GetByName("sm2p256v1");
        private static ECCurve sm2Curve = sm2Parameters.Curve;
        private static ECDomainParameters sm2DomainParameters = new ECDomainParameters(sm2Curve, sm2Parameters.G, sm2Parameters.N, sm2Parameters.H);

        public static void GenerateKeyPair(out string publicKey, out string privateKey)
        {
            var keyPairGenerator = new ECKeyPairGenerator();
            keyPairGenerator.Init(new ECKeyGenerationParameters(sm2DomainParameters, new SecureRandom()));

            AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateKeyPair();

            ECPublicKeyParameters publicKeyP = (ECPublicKeyParameters)keyPair.Public;
            ECPrivateKeyParameters privateKeyP = (ECPrivateKeyParameters)keyPair.Private;
            publicKey = Hex.ToHexString(publicKeyP.Q.GetEncoded(false));
            privateKey = Hex.ToHexString(privateKeyP.D.ToByteArray());
        }

        private static byte[] Process(byte[] data, ICipherParameters parameters, bool froEncryption)
        {
            SM2Engine engine = new SM2Engine();
            engine.Init(froEncryption, parameters);
            return engine.ProcessBlock(data, 0, data.Length);
        }

        public static byte[] Encrypt(byte[] plaintext, byte[] publicKey, Mode mode = Mode.C1C3C2)
        {
            var ciphertext = Process(plaintext, new ParametersWithRandom(new ECPublicKeyParameters(sm2Curve.DecodePoint(publicKey), sm2DomainParameters)), true);

            if (mode == Mode.C1C3C2)
            {
                ciphertext = SM2Utils.C123ToC132(ciphertext);
            }

            return ciphertext;
        }

        public static byte[] Decrypt(byte[] ciphertext, byte[] privateKey, Mode mode = Mode.C1C3C2)
        {
            if (mode == Mode.C1C3C2)
            {
                ciphertext = SM2Utils.C132ToC123(ciphertext);
            }
            return Process(ciphertext, new ECPrivateKeyParameters(new BigInteger(1, privateKey), sm2DomainParameters), false);
        }

        public static string Decrypt(string cipherText)
        {
            var privateKey = GlobalVariables.Configuration.GetValue<string>("SM:PrivateKey");
            var content = Encoding.UTF8.GetString(SM2Utils2.Decrypt(Hex.Decode("04"+cipherText),Hex.Decode(privateKey)));
            return content;
        }
    }
}