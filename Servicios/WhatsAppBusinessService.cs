using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using WhatsAppPresentacionV6.Modelos;
using WhatsAppPresentacionV6.Servicios;
using System.Net.Http;

namespace WhatsAppPresentacionV6.Servicios
{
    public class WhatsAppBusinessService
    {
        private readonly string _tokenAcceso;
        private readonly string _facebookGraphVersion = "v21.0";
        private readonly string _WHATSAPP_BUSINESS_ACCOUNT_ID;


        //identificadores de acceso de usuario del sistema: EAAGf6qvOtkYBO3GsQMED0iOQsKZA6yYwZBhsFN4qjO39cUSuBZCgJKGlA0Ma0KJoxhoH7uZAC8eK2OrSonWKsO9AeJce5iRqWZBQdb2sF3PsjD5llXPtYeVjtrr2fwleNTnqEPLZBhKIG7mZCUYmuXXGsRvHZCSU1WpPpVfRMOi5ZBHcZBuI0l9DcktSOZCTISUHTbpk8WOSNweAAjKcKABJgigpZBhfyqZC5eo3UMjub2SIE

        /// <summary>
        /// Constructor de la clase WhatsAppMessageService. Inicializa el servicio con el ID de teléfono y el token de acceso.
        /// </summary>
        /// <param name="_WHATSAPP_BUSINESS_ACCOUNT_ID"> ID de la cuenta de WhatsApp Business.</param>
        /// <param name="tokenAcceso">Token de acceso para autenticación en la API de Facebook Graph. Se tiene que generar cada cierto tiempo y modificar en el programa.</param>
        public WhatsAppBusinessService(string BUSINESS_ACCOUNT_ID, string tokenAcceso)
        {
            _WHATSAPP_BUSINESS_ACCOUNT_ID = BUSINESS_ACCOUNT_ID;
            _tokenAcceso = tokenAcceso;
        }
        private string BuildApiUrl() =>
            $"https://graph.facebook.com/{_facebookGraphVersion}/{_WHATSAPP_BUSINESS_ACCOUNT_ID}/message_templates";

        /// <summary>
        /// Crea una nueva plantilla de mensaje en la cuenta de WhatsApp Business.
        /// </summary>
        /// <param name="nombrePlantilla">Nombre único de la plantilla.</param>
        /// <param name="categoria">Categoría de la plantilla (UTILITY, MARKETING, AUTHENTICATION).</param>
        /// <param name="idioma">Código del idioma de la plantilla (ej: es_MX).</param>
        /// <param name="cuerpo">Contenido del mensaje, incluyendo variables (ej: "Hola, {1}").</param>
        /// <returns>Respuesta de la API en formato JSON.</returns>
        public async Task<string> CrearPlantillaTextAsync(string nombrePlantilla, string categoria, string idioma/*, string cuerpo*/)
        {
            if (string.IsNullOrEmpty(nombrePlantilla) || string.IsNullOrEmpty(categoria) || string.IsNullOrEmpty(idioma) /*|| string.IsNullOrEmpty(cuerpo)*/)
                return "Se ha recibido un elemento vacio";
            if (!(categoria.ToUpper() == "UTILITY" || categoria.ToUpper() == "MARKETING" || categoria.ToUpper() == "AUTHENTICATION"))
                return "categoria no reconocida";

            var plantilla = new
            {
                name = nombrePlantilla,
                category = categoria.ToUpper(),
                //allow_category_change = true,     //Es opcional. Configúralo como verdadero para permitir a Meta asignar automáticamente una categoría. Si se omite, se puede rechazar la plantilla debido a una categoría incorrecta.
                language = idioma,
                components = new[]
            {
                new
                {
                    type = "BODY",
                    text = "Thank you for your order, {{1}}! Your confirmation number is {{2}}. If you have any questions, please use the buttons below to contact support. Thank you for being a customer!",
                    example = new
                    {
                        body_text = new[]
                        {
                            new[]{"Pablo", "860198-230332"}
                        }
                    }
                }
            }
            };

            using HttpClient client = new HttpClient();
            string urlBUSINESS = BuildApiUrl();
            var request = new HttpRequestMessage(HttpMethod.Post, urlBUSINESS);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAcceso);
            request.Content = new StringContent(JsonSerializer.Serialize(plantilla), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return $"Error al crear la plantilla: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
            string responseBody = await response.Content.ReadAsStringAsync();
            return "Response: " + responseBody;
        }

        /// <summary>
        /// Crea una nueva plantilla de mensaje en la cuenta de WhatsApp Business con un header de texto, body de texto, footer y botones.
        /// </summary>
        /// <param name="nombrePlantilla">Nombre único de la plantilla.</param>
        /// <param name="categoria">Categoría de la plantilla (UTILITY, MARKETING, AUTHENTICATION).</param>
        /// <param name="idioma">Código del idioma de la plantilla (ej: es_MX).</param>
        /// <param name="headerText">Contenido del header, incluyendo variables (ej: "Hola, {1}").</param>
        /// <param name="headerExamples"> Lista de variables ejemplo en orden de número de identidad</param>
        /// <param name="bodyText">Contenido del cuerpo, incluyendo variables (ej: "Hola, {1}").</param>
        /// <param name="bodyExamples">Lista de variables ejemplo en orden de número de identidad separado de las de header</param>
        /// <param name="footerText">Contenido del pie de página, no puede llevar variables</param>
        /// <param name="buttonList">Lista de texto en botones. Sólo usa QUICK_REPLY. Se puede modificar para soportar otros tipos.</param>
        /// <returns>Respuesta de la API en formato JSON.</returns>
        public async Task<string> CrearPlantillaHeaderTextFooterButtonsAsync(
            string nombrePlantilla, 
            string categoria, 
            string idioma, 
            string headerText,
            List<string> headerExamples,
            string bodyText,
            List<string> bodyExamples,
            string footerText,
            List<string> buttonList)
        {
            if (string.IsNullOrEmpty(nombrePlantilla) || string.IsNullOrEmpty(categoria) || string.IsNullOrEmpty(idioma) || string.IsNullOrEmpty(headerText) || string.IsNullOrEmpty(bodyText) || string.IsNullOrEmpty(footerText))
                return "Se ha recibido un elemento vacio";
            if (headerExamples.Count == 0 || bodyExamples.Count == 0)
                return "Este servicio requiere uso de ejemplos en el header y en el cuerpo";
            if (buttonList.Count == 0)
                return "Este servicio requiere uso de botones";
            if (!(categoria.ToUpper() == "UTILITY" || categoria.ToUpper() == "MARKETING" || categoria.ToUpper() == "AUTHENTICATION"))
                return "categoria no reconocida";

            var plantilla = new
            {
                name = nombrePlantilla,
                category = categoria.ToUpper(),
                language = idioma,
                components = new object[]
                {
                    new
                    {
                        type = "HEADER",
                        format = "TEXT",
                        text = headerText,
                        example = new
                        {
                            header_text = headerExamples.ToArray()
                        }
                    },
                    new
                    {
                        type = "BODY",
                        text = bodyText,
                        example = new
                        {
                            body_text = new[]
                            {
                                bodyExamples.ToArray()
                            }
                        }
                    },
                    new
                    {
                        type = "FOOTER",
                        text = footerText
                    },
                    new
                    {
                        type = "BUTTONS",
                        buttons = buttonList.Select(b => new { type = "QUICK_REPLY", text = b }).ToArray() // Convierte la lista de botones en un array de objetos
                    }
                }
            };

            using HttpClient client = new HttpClient();
            string urlBUSINESS = BuildApiUrl();
            var request = new HttpRequestMessage(HttpMethod.Post, urlBUSINESS);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAcceso);
            request.Content = new StringContent(JsonSerializer.Serialize(plantilla), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return $"Error al crear la plantilla: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
            string responseBody = await response.Content.ReadAsStringAsync();
            return "Response: " + responseBody;
        }
    }
}
