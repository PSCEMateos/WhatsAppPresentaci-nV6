using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace WhatsAppPresentacionV6.Servicios
{
    public class FlowDecryptionEncryptionService
    {
        private readonly RSA _rsa;
        private const int TAG_LENGTH = 16;
        private static readonly Encoding EncodingUTF8 = Encoding.UTF8;
        public FlowDecryptionEncryptionService(string privateKeyPath)
        {
            _rsa = RSA.Create();
            _rsa.ImportFromPem(System.IO.File.ReadAllText(privateKeyPath));
        }
        //Decryption---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public string DecryptAESKey(string encryptedAesKey)
        {
            byte[] encryptedAesKeyBytes = Convert.FromBase64String(encryptedAesKey);
            byte[] aesKeyBytes = _rsa.Decrypt(encryptedAesKeyBytes, RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(aesKeyBytes);
        }
        public string DecryptFlowData(string encryptedFlowData, string encryptedAesKey, string initialVector)
        {
            byte[] aesKeyBytes = Convert.FromBase64String(DecryptAESKey(encryptedAesKey));
            byte[] initialVectorBytes = Convert.FromBase64String(initialVector);
            byte[] flowDataBytes = Convert.FromBase64String(encryptedFlowData);
            byte[] plainTextBytes = new byte[flowDataBytes.Length - TAG_LENGTH];

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(aesKeyBytes), TAG_LENGTH * 8, initialVectorBytes);
            cipher.Init(false, parameters);
            int offset = cipher.ProcessBytes(flowDataBytes, 0, flowDataBytes.Length, plainTextBytes, 0);
            cipher.DoFinal(plainTextBytes, offset);

            return Encoding.UTF8.GetString(plainTextBytes);
        }
        //Encryption---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public string FullFlowEncryptResponse(string jsonResponse, string encryptedAesKey, string initialVector)
        {
            byte[] aesKeyBytes = Convert.FromBase64String(DecryptAESKey(encryptedAesKey));
            byte[] invertedInitialVectorBytes = InvertBits(Convert.FromBase64String(initialVector));

            byte[] plainTextBytes = EncodingUTF8.GetBytes(jsonResponse);
            byte[] cipherTextBytes = new byte[plainTextBytes.Length + TAG_LENGTH];

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(aesKeyBytes), TAG_LENGTH * 8, invertedInitialVectorBytes);
            cipher.Init(true, parameters);

            int offset = cipher.ProcessBytes(plainTextBytes, 0, plainTextBytes.Length, cipherTextBytes, 0);
            cipher.DoFinal(cipherTextBytes, offset);

            return Convert.ToBase64String(cipherTextBytes);
        }
        public string EncryptResponse(string jsonResponse, byte[] aesKeyBytes, byte[] initialVectorBytes)
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
        public byte[] InvertBits(byte[] iv)
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
