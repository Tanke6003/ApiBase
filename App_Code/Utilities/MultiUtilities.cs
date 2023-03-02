using System;
using System.Text;
using Models;
namespace Utilities
{
    /// <summary>
    /// Library of multi utilities methods to be used in the application like encryption, decryption, etc.
    /// </summary>
    public class MultiUtilities
    {
        #region Public Methods
        #region Encrypt
        public static EncryptStr Encrypt(EncryptStr encryptStr)
        {
            string result = string.Empty;
            encryptStr = MatchString(encryptStr);
            encryptStr.result =  Convert.ToBase64String( JoinByteArrays(Encoding.Unicode.GetBytes(encryptStr.str), Encoding.Unicode.GetBytes(encryptStr.key)));
            encryptStr.key = encryptStr.key.Replace("\0", "");
            encryptStr.str = encryptStr.str.Replace("\0", "");
            return encryptStr;
        }
        #endregion
        #region Decrypt
        public static EncryptStr Decrypt(EncryptStr encryptStr)
        {
            string result = string.Empty;
            encryptStr.result = Encoding.Unicode.GetString( SplitByteArrays(Convert.FromBase64String(encryptStr.str))).Replace("\0", "");
            return encryptStr;
        }
        #endregion
        #endregion
        #region Private Methods
        #region  MatchString
        private static EncryptStr MatchString(EncryptStr encryptStr)
        {
            string result = string.Empty;
            if (encryptStr.key.Length < encryptStr.str.Length)
            {
                while (encryptStr.key.Length < encryptStr.str.Length)
                {
                    encryptStr.key = encryptStr.key + "\0";
                }
            }
            if (encryptStr.str.Length < encryptStr.key.Length)
            {
                while (encryptStr.str.Length < encryptStr.key.Length)
                {
                    encryptStr.str = encryptStr.str + "\0";
                }
            }
            return encryptStr;
        }
        #endregion
        #region JoinByteArrays
        private static Byte[] JoinByteArrays(Byte[] bytesA, Byte[] bytesB)
        {
            int len = bytesA.Length + bytesB.Length;
            Byte[] bytesC = new Byte[len];
            int i = 0, j = 0, k = 0;
            while (i < len)
            {
                if (j < bytesA.Length)
                {
                    bytesC[i] = bytesA[j];
                    i++;
                    j++;
                }
                if (k < bytesB.Length)
                {
                    bytesC[i] = bytesB[k];
                    i++;
                    k++;
                }
            }
            return bytesC;
        }
        #endregion
        #region SplitByteArrays
        private static Byte[] SplitByteArrays(Byte[] bytesC)
        {
            byte[] bytesA = new byte[bytesC.Length / 2];
            byte[] bytesB = new byte[bytesC.Length / 2];
            int i = 0, j = 0, k = 0;
            while (i < bytesC.Length)
            {
                if (j < bytesA.Length)
                {
                    bytesA[j] = bytesC[i];
                    i++;
                    j++;
                }
                if (k < bytesB.Length)
                {
                    bytesB[k] = bytesC[i];
                    i++;
                    k++;
                }
            }
            return bytesA;
        }
        #endregion
        #endregion
    }
}