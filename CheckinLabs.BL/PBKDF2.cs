using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CheckinLabs.BL
{
    public static class PBKDF2
    {
        public static byte[] Hash(string data, string key, byte[] salt)
        {
            using (var k = new Rfc2898DeriveBytes(key, salt, 1000))
            using (var encAlg = TripleDES.Create())
            {
                encAlg.Key = k.GetBytes(16);
                encAlg.IV = salt.Take(8).ToArray();
                using (var encryptionStream = new MemoryStream())
                using (var encrypt = new CryptoStream(encryptionStream, encAlg.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] utfD1 = new UTF8Encoding(false).GetBytes(data);
                    encrypt.Write(utfD1, 0, utfD1.Length);
                    encrypt.FlushFinalBlock();
                    encrypt.Close();
                    byte[] edata = encryptionStream.ToArray();
                    k.Reset();
                    return edata;
                }
            }
        }

        public static byte[] GenerateSalt()
        {
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                byte[] s = new byte[16];
                rngCsp.GetBytes(s);
                return s;
            }
        }
    }

}
