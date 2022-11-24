using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverSoft.Utils.SM
{
    public class SM4Utils
    {
        public String secretKey = "";
        public String iv = "";
        public bool hexString = false;

        public String Encrypt_ECB(String plainText,Encoding encoding=null)
        {
            SM4_Context ctx = new SM4_Context();
            ctx.isPadding = true;
            ctx.mode = SM4.SM4_ENCRYPT;

            encoding = encoding ?? Encoding.UTF8;

            byte[] keyBytes;
            if (hexString)
            {
                keyBytes = Hex.Decode(secretKey);
            }
            else
            {
                keyBytes = encoding.GetBytes(secretKey);
            }

            SM4 sm4 = new SM4();
            sm4.sm4_setkey_enc(ctx, keyBytes);
            byte[] encrypted = sm4.sm4_crypt_ecb(ctx, encoding.GetBytes(plainText));

            String cipherText = encoding.GetString(Hex.Encode(encrypted));
            return cipherText;
        }

        public String Decrypt_ECB(String cipherText, Encoding encoding = null)
        {
            SM4_Context ctx = new SM4_Context();
            ctx.isPadding = true;
            ctx.mode = SM4.SM4_DECRYPT;

            encoding = encoding ?? Encoding.UTF8;

            byte[] keyBytes;
            if (hexString)
            {
                keyBytes = Hex.Decode(secretKey);
            }
            else
            {
                keyBytes = encoding.GetBytes(secretKey);
            }

            SM4 sm4 = new SM4();
            sm4.sm4_setkey_dec(ctx, keyBytes);
            byte[] decrypted = sm4.sm4_crypt_ecb(ctx, Hex.Decode(cipherText));
            return encoding.GetString(decrypted);
        }
        public String Encrypt_CBC(String plainText, Encoding encoding = null)
        {
            SM4_Context ctx = new SM4_Context();
            ctx.isPadding = true;
            ctx.mode = SM4.SM4_ENCRYPT;

            encoding = encoding ?? Encoding.UTF8;

            byte[] keyBytes;
            byte[] ivBytes;
            if (hexString)
            {
                keyBytes = Hex.Decode(secretKey);
                ivBytes = Hex.Decode(iv);
            }
            else
            {
                keyBytes = encoding.GetBytes(secretKey);
                ivBytes = encoding.GetBytes(iv);
            }

            SM4 sm4 = new SM4();
            sm4.sm4_setkey_enc(ctx, keyBytes);
            byte[] encrypted = sm4.sm4_crypt_cbc(ctx, ivBytes, encoding.GetBytes(plainText));

            String cipherText = encoding.GetString(Hex.Encode(encrypted));
            return cipherText;
        }

        public String Decrypt_CBC(String cipherText, Encoding encoding = null)
        {
            SM4_Context ctx = new SM4_Context();
            ctx.isPadding = true;
            ctx.mode = SM4.SM4_DECRYPT;

            encoding = encoding ?? Encoding.UTF8;

            byte[] keyBytes;
            byte[] ivBytes;
            if (hexString)
            {
                keyBytes = Hex.Decode(secretKey);
                ivBytes = Hex.Decode(iv);
            }
            else
            {
                keyBytes = encoding.GetBytes(secretKey);
                ivBytes = encoding.GetBytes(iv);
            }

            SM4 sm4 = new SM4();
            sm4.sm4_setkey_dec(ctx, keyBytes);
            byte[] decrypted = sm4.sm4_crypt_cbc(ctx, ivBytes, Hex.Decode(cipherText));
            return encoding.GetString(decrypted);
        }

        //[STAThread]
        //public static void Main()
        //{
        //    String plainText = "ererfeiisgod";  

        //    SM4Utils sm4 = new SM4Utils();  
        //    sm4.secretKey = "JeF8U9wHFOMfs2Y8";  
        //    sm4.hexString = false;  

        //    System.Console.Out.WriteLine("ECB模式");  
        //    String cipherText = sm4.Encrypt_ECB(plainText);  
        //    System.Console.Out.WriteLine("密文: " + cipherText);  
        //    System.Console.Out.WriteLine("");  

        //    plainText = sm4.Decrypt_ECB(cipherText);  
        //    System.Console.Out.WriteLine("明文: " + plainText);  
        //    System.Console.Out.WriteLine("");  

        //    System.Console.Out.WriteLine("CBC模式");  
        //    sm4.iv = "UISwD9fW6cFh9SNS";  
        //    cipherText = sm4.Encrypt_CBC(plainText);  
        //    System.Console.Out.WriteLine("密文: " + cipherText);  
        //    System.Console.Out.WriteLine("");  

        //    plainText = sm4.Decrypt_CBC(cipherText);  
        //    System.Console.Out.WriteLine("明文: " + plainText);

        //    Console.ReadLine();
        //}
    }
}