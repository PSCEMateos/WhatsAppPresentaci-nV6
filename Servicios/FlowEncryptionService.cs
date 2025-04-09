using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace WhatsAppPresentacionV6.Servicios
{
    public class FlowEncryptionService
    {
        private const int TAG_LENGTH = 16; // Longitud estándar para AES-GCM
        private static readonly Encoding EncodingUTF8 = Encoding.UTF8;
        public static string EncryptResponse(string jsonResponse, byte[] aesKeyBytes, byte[] initialVectorBytes)
        {
            byte[] plainTextBytes = EncodingUTF8.GetBytes(jsonResponse);
            byte[] cipherTextBytes = new byte[plainTextBytes.Length + TAG_LENGTH];

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(aesKeyBytes), TAG_LENGTH * 8, initialVectorBytes);
            cipher.Init(true, parameters);

            int offset = cipher.ProcessBytes(plainTextBytes, 0, plainTextBytes.Length, cipherTextBytes, 0);
            cipher.DoFinal(cipherTextBytes, offset);

            return Convert.ToBase64String(cipherTextBytes);
        }
        public static byte[] InvertBits(byte[] iv)
        {
            byte[] invertedIV = new byte[iv.Length];
            for (int i = 0; i < iv.Length; i++)
            {
                invertedIV[i] = (byte)~iv[i];  // Bitwise NOT (~) operation to invert bits
            }
            return invertedIV;
        }
    }
}
