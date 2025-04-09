using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WhatsAppPresentacionV6.Servicios
{
    public class WhatsAppBusinessEncryptionService
    {

        private readonly string _tokenAcceso;
        private readonly string _facebookGraphVersion = "v21.0";
        //private readonly string _WHATSAPP_BUSINESS_ACCOUNT_ID;
        private readonly string _idTelefono;
        public WhatsAppBusinessEncryptionService(string idTelefono, string tokenAcceso)
        {
            _tokenAcceso = tokenAcceso;
            _idTelefono = idTelefono;
        }
        private string BuildApiUrl() =>
            $"https://graph.facebook.com/{_facebookGraphVersion}/{_idTelefono}/whatsapp_business_encryption";
        public async Task<string> UploadPublicKeyAsync(string publicKey)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAcceso);

            var payload = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("business_public_key", publicKey) //encryption_key = publicKey.Replace("\n", "").Replace("\r", "")
            });

            HttpResponseMessage response = await client.PostAsync(BuildApiUrl(), payload);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error al subir la clave pública: {responseBody}");
            }

            return $"Clave pública subida correctamente: {responseBody}";
        }
    }
}