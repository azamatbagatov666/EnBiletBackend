using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace Crypt.Class
{
    internal class Enigma
    {
        private readonly byte[] key;
        private readonly byte[] iv;

        public Enigma(string keyString, string ivString)
        {
            using (SHA256 sha = SHA256.Create())
            {
                key = sha.ComputeHash(Encoding.UTF8.GetBytes(keyString)); // 32 bytes
                iv = sha.ComputeHash(Encoding.UTF8.GetBytes(ivString));   // 32 bytes → 16 use
                Array.Resize(ref iv, 16);
            }
        }

        public string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    byte[] data = Encoding.UTF8.GetBytes(plainText);
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream(buffer))
                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
