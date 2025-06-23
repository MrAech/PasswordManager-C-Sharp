using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Services
{
    public class EncryptionService
    {
        private readonly byte[] Key;
        private readonly byte[] IV;

        public EncryptionService(string key, string iv)
        {
            if (key == null || iv == null || key.Length != 16 || iv.Length != 16)
                throw new ArgumentException("bad key/iv (should be 16 chars lol)");
            Key = Encoding.UTF8.GetBytes(key);
            IV = Encoding.UTF8.GetBytes(iv);
        }

        public string Encrypt(string data)
        {
            // just encrypt it, hope it works
            if (string.IsNullOrEmpty(data)) return data;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                ICryptoTransform enc = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(data);
                    sw.Flush();
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Decrypt(string data)
        {
            // hope this decrypts right -- it did past me 
            if (string.IsNullOrEmpty(data)) return data;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                ICryptoTransform dec = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(data)))
                using (CryptoStream cs = new CryptoStream(ms, dec, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}