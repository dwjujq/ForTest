using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverSoft.Utils.SM
{
    public class SM2Utils
    {
        public static void GenerateKeyPair()
        {
            SM2 sm2 = SM2.Instance;
            AsymmetricCipherKeyPair key = sm2.ecc_key_pair_generator.GenerateKeyPair();
            ECPrivateKeyParameters ecpriv = (ECPrivateKeyParameters)key.Private;
            ECPublicKeyParameters ecpub = (ECPublicKeyParameters)key.Public;
            BigInteger privateKey = ecpriv.D;
            ECPoint publicKey = ecpub.Q;

            System.Console.Out.WriteLine("公钥: " + Encoding.UTF8.GetString(Hex.Encode(publicKey.GetEncoded())).ToUpper());
            System.Console.Out.WriteLine("私钥: " + Encoding.UTF8.GetString(Hex.Encode(privateKey.ToByteArray())).ToUpper());
        }

        public static String Encrypt(byte[] publicKey, byte[] data,Mode mode=Mode.C1C3C2, Encoding encoding = null)
        {
            if (null == publicKey || publicKey.Length == 0)
            {
                return null;
            }
            if (data == null || data.Length == 0)
            {
                return null;
            }

            encoding = encoding ?? Encoding.UTF8;

            byte[] source = new byte[data.Length];
            Array.Copy(data, 0, source, 0, data.Length);

            Cipher cipher = new Cipher();
            SM2 sm2 = SM2.Instance;

            ECPoint userKey = sm2.ecc_curve.DecodePoint(publicKey);

            ECPoint c1 = cipher.Init_enc(sm2, userKey);
            cipher.Encrypt(source);

            byte[] c3 = new byte[32];
            cipher.Dofinal(c3);

            String sc1 = encoding.GetString(Hex.Encode(c1.GetEncoded()));
            String sc2 = encoding.GetString(Hex.Encode(source));
            String sc3 = encoding.GetString(Hex.Encode(c3));

            if(mode==Mode.C1C3C2)
            {
                return (sc1 + sc3 + sc2).ToUpper();
            }
            return (sc1 + sc2 + sc3).ToUpper();
        }

        public static byte[] Decrypt(byte[] privateKey, byte[] encryptedData, Mode mode = Mode.C1C3C2, Encoding encoding = null)
        {
            if (null == privateKey || privateKey.Length == 0)
            {
                return null;
            }
            if (encryptedData == null || encryptedData.Length == 0)
            {
                return null;
            }

            encoding = encoding ?? Encoding.UTF8;

            if (mode == Mode.C1C3C2) encryptedData = C132ToC123(encryptedData);

            String data = encoding.GetString(Hex.Encode(encryptedData));

            byte[] c1Bytes = Hex.Decode(encoding.GetBytes(data.Substring(0, 130)));
            int c2Len = encryptedData.Length - 97;
            byte[] c2 = Hex.Decode(encoding.GetBytes(data.Substring(130, 2 * c2Len)));
            byte[] c3 = Hex.Decode(encoding.GetBytes(data.Substring(130 + 2 * c2Len, 64)));

            SM2 sm2 = SM2.Instance;
            BigInteger userD = new BigInteger(1, privateKey);

            ECPoint c1 = sm2.ecc_curve.DecodePoint(c1Bytes);
            Cipher cipher = new Cipher();
            cipher.Init_dec(userD, c1);
            cipher.Decrypt(c2);
            cipher.Dofinal(c3);

            return c2;
        }


        public static string Decrypt(string cipherText)
        {
            var privateKey =GlobalVariables.Configuration.GetValue<string>("SM:PrivateKey");
            var content = Encoding.UTF8.GetString(SM2Utils.Decrypt(Hex.Decode(privateKey), Hex.Decode(cipherText)));
            return content;
        }

        public static byte[] C123ToC132(byte[] c1c2c3)
        {
            var gn = GMNamedCurves.GetByName("SM2P256V1");
            int c1Len = (gn.Curve.FieldSize + 7) / 8 * 2 + 1; //sm2p256v1的这个固定65。可看GMNamedCurves、ECCurve代码。
            int c3Len = 32; //new SM3Digest().getDigestSize();
            byte[] result = new byte[c1c2c3.Length];
            Array.Copy(c1c2c3, 0, result, 0, c1Len); //c1
            Array.Copy(c1c2c3, c1c2c3.Length - c3Len, result, c1Len, c3Len); //c3
            Array.Copy(c1c2c3, c1Len, result, c1Len + c3Len, c1c2c3.Length - c1Len - c3Len); //c2
            return result;
        }

        public static byte[] C132ToC123(byte[] c1c3c2)
        {
            var gn = GMNamedCurves.GetByName("SM2P256V1");
            int c1Len = (gn.Curve.FieldSize + 7) / 8 * 2 + 1;
            int c3Len = 32; //new SM3Digest().getDigestSize();
            byte[] result = new byte[c1c3c2.Length];
            Array.Copy(c1c3c2, 0, result, 0, c1Len); //c1: 0->65
            Array.Copy(c1c3c2, c1Len + c3Len, result, c1Len, c1c3c2.Length - c1Len - c3Len); //c2
            Array.Copy(c1c3c2, c1Len, result, c1c3c2.Length - c3Len, c3Len); //c3
            return result;
        }

        //[STAThread]
        //public static void Main()
        //{
        //    GenerateKeyPair();

            //    String plainText = "ererfeiisgod";
            //    byte[] sourceData = Encoding.Default.GetBytes(plainText);

            //    //下面的秘钥可以使用generateKeyPair()生成的秘钥内容  
            //    // 国密规范正式私钥  
            //    String prik = "3690655E33D5EA3D9A4AE1A1ADD766FDEA045CDEAA43A9206FB8C430CEFE0D94";
            //    // 国密规范正式公钥  
            //    String pubk = "04F6E0C3345AE42B51E06BF50B98834988D54EBC7460FE135A48171BC0629EAE205EEDE253A530608178A98F1E19BB737302813BA39ED3FA3C51639D7A20C7391A";

            //    System.Console.Out.WriteLine("加密: ");
            //    String cipherText = SM2Utils.Encrypt(Hex.Decode(pubk), sourceData);
            //    System.Console.Out.WriteLine(cipherText);
            //    System.Console.Out.WriteLine("解密: ");
            //    plainText = Encoding.Default.GetString(SM2Utils.Decrypt(Hex.Decode(prik), Hex.Decode(cipherText)));
            //    System.Console.Out.WriteLine(plainText);

            //    Console.ReadLine();
            //}
        }
}