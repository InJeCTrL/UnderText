using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace UnderText
{
    public class AESKey : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// validate if the key is right formatted
        /// </summary>
        /// <param name="key">AES key</param>
        /// <returns></returns>
        public static bool Validate(string key)
        {
            if (key.Length != 32)
            {
                return false;
            }
            foreach (var ch in key)
            {
                if (ch > 136 || ch < 33)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// generate an random AES256(32 bytes) key
        /// </summary>
        /// <returns>AES256 key</returns>
        public static string Generate()
        {
            string tKey = "";
            Random random = new Random();
            for (int i = 0; i < 32; ++i)
            {
                tKey += Convert.ToChar(random.Next(33, 126));
            }
            return tKey;
        }
        public AESKey()
        {

        }
        /// <summary>
        /// initialize AESKey object by the given key
        /// </summary>
        /// <param name="key"></param>
        public AESKey(string key) : this()
        {
            Key = key;
            CreateEncDec();
        }
        /// <summary>
        /// decrypt base64 text encrypted by AES256 to normal string
        /// </summary>
        /// <param name="encrypted">encrypted string</param>
        /// <returns>decrypted string or string.Empty</returns>
        public string Decrypt(string encrypted)
        {
            try
            {
                if (decryptor == null)
                {
                    CreateEncDec();
                }
                byte[] data = Convert.FromBase64String(encrypted);
                string dec = Encoding.UTF8.GetString(decryptor.TransformFinalBlock(data, 0, data.Length));
                return dec;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// encrypt raw text by AES256 and convert to base64 text
        /// </summary>
        /// <param name="text">raw text</param>
        /// <returns>encrypted string(base64)</returns>
        public string Encrypt(string text)
        {
            try
            {
                if (encryptor == null)
                {
                    CreateEncDec();
                }
                byte[] data = Encoding.UTF8.GetBytes(text);
                string dec = Convert.ToBase64String(encryptor.TransformFinalBlock(data, 0, data.Length));
                return dec;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// initialize AES256 encryptor and decryptor
        /// </summary>
        private void CreateEncDec()
        {
            AesCryptoServiceProvider provider = new AesCryptoServiceProvider();
            provider.Key = Encoding.UTF8.GetBytes(Key);
            provider.Mode = CipherMode.ECB;
            provider.Padding = PaddingMode.PKCS7;
            encryptor = provider.CreateEncryptor();
            decryptor = provider.CreateDecryptor();
        }
        /// <summary>
        /// Key string(32 bytes)
        /// </summary>
        public string Key { get; set; }
        private string _Remark;
        /// <summary>
        /// note of the key
        /// </summary>
        public string Remark
        {
            get
            {
                return _Remark;
            }
            set
            {
                _Remark = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Remark"));
                    MainPage.SaveList();
                }
            }
        }
        private ICryptoTransform encryptor;
        private ICryptoTransform decryptor;
    }
}
