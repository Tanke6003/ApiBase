using Microsoft.AspNetCore.Mvc;
using Models;
using Utilities;
using Newtonsoft.Json;

namespace Controllers
{
    /// <summary>
    /// Controller for the utilities methods
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class UtilitesController : ControllerBase
    {
        /// <summary>
        /// Encrypts a string
        /// </summary>
        /// <param name="encryptStr"> Structure containing the string to be encrypted and an optional key </param>
        /// <returns> The same structure that is sent but with a property call result that contains the encryption result</returns>
        [HttpPost("Encrypt")]
        public string Encrypt([FromBody] EncryptStr encryptStr)
        {
            return  JsonConvert.SerializeObject(MultiUtilities.Encrypt(encryptStr));
        }
        /// <summary>
        /// Decrypts a string
        /// </summary>
        /// <param name="decryptStr"> Structure containing the string to be decrypted and an optional key </param>
        /// <returns> The same structure that is sent but with a property call result that contains the decryption result</returns>
        [HttpPost("Decrypt")]
        public string Decrypt([FromBody] EncryptStr decryptStr)
        {
            return JsonConvert.SerializeObject(MultiUtilities.Decrypt(decryptStr));
        }
        /// <summary>
        /// Test method
        /// </summary>
        /// <returns> Test </returns>
        [HttpGet("Test")]
        public string Test()
        {
            return "Test";
        }



    }
}
