using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    /// <summary>
    /// structure for encrypting a string
    /// </summary>
    public class EncryptStr
    {
        /// <summary>
        /// The string to encrypt or decrypt
        /// </summary>
        public string str { get; set; }
        /// <summary>
        /// The key to use for encryption or decryption
        /// </summary>
        public string? key { get; set; }
        /// <summary>
        /// The result of the encryption or decryption
        /// </summary>
        public string? result { get; set; }
    }
}