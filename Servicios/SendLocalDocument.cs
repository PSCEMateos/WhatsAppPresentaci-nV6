using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using WhatsAppPresentacionV6.Servicios;
using WhatsAppPresentacionV6.Modelos;
using WhatsAppPresentacionV6.Modelos.RecivedMedia;

namespace WhatsAppPresentacionV6.Servicios
{
    /// <summary>
    /// Servicio que recive un token de verificación, ID de teléfono, teléfono objetivo.
    /// Lee un archivo local lo convierte en binairo y lo envía a whatsapp
    /// </summary>
    /// 
    public class SendLocalDocument
    {
        private readonly string _tokenAcceso;
        private readonly string _idTelefono;
        private readonly string _facebookGraphVersion = "v21.0";
        private readonly HanldeDocument _hanldeDocument;
        private readonly WhatsAppMessageService _messageSendService;
        public SendLocalDocument(string token, string idTelefono/*, HanldeDocument hanldeDocument*/)
        {
            _tokenAcceso = token;
            _idTelefono = idTelefono;
            _hanldeDocument = new HanldeDocument(_tokenAcceso);/*Posiblemente cambiar a:
             * _hanldeDocument = hanldeDocument ?? throw new ArgumentNullException(nameof(hanldeDocument));
             */
            _messageSendService =new WhatsAppMessageService(_idTelefono, _tokenAcceso);
        }
        public async Task<string> SubirDocumentoWhatsApp(string rutaCompletaDocumento, string telefonoObjetivo)
        {
            if (string.IsNullOrEmpty(rutaCompletaDocumento))
            {
                throw new ArgumentException("Ruta al documento requerida.");
            }
            if (!File.Exists(rutaCompletaDocumento))
            {
                throw new ArgumentException("Documento no existe.");
            }
            //Colocar el documento en un binary array
            byte[] documentoMandarBytes = await File.ReadAllBytesAsync(rutaCompletaDocumento);

            //Recabar información del documento
            string nombreDocumentoMandar = Path.GetFileName(rutaCompletaDocumento);
            var mimeType = GetMimeType(rutaCompletaDocumento);
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"https://graph.facebook.com/{_facebookGraphVersion}/{_idTelefono}/media");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAcceso);
                var content = new MultipartFormDataContent
                {
                    { new StringContent("whatsapp"), "messaging_product" },
                    { new ByteArrayContent(documentoMandarBytes) { Headers = { ContentType = new MediaTypeHeaderValue(mimeType) } }, "file", nombreDocumentoMandar }
                };
                request.Content = content;

                HttpResponseMessage uploadResponse = await client.SendAsync(request);
                if (!uploadResponse.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Error al subir el archivo: " + await uploadResponse.Content.ReadAsStringAsync());
                }

                //Obtener el id del documento en whatsapp
                var responseContent = await uploadResponse.Content.ReadAsStringAsync();
                var mediaResponse = JsonSerializer.Deserialize<MediaResponse>(responseContent);

                if (mediaResponse == null || string.IsNullOrEmpty(mediaResponse.id))
                {
                    throw new InvalidOperationException("Serialización de media incorrecta");
                }

                string idDocumentoMandar = mediaResponse.id;

                //string urlDocumentoMandar = await _hanldeDocument.ObtenerURLDocumentoDeID(idDocumentoMandar);

                return await _messageSendService.EnviarDocumentoPorId(telefonoObjetivo, idDocumentoMandar, $"{nombreDocumentoMandar}.{mimeType}");
            }
        }
        private string GetMimeType(string rutaDocumentoMandar)
        {
            var extension = Path.GetExtension(rutaDocumentoMandar).ToLowerInvariant();
            return extension switch
            {
                ".txt" => "text/plain",//empezando con documentos
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".pdf" => "application/pdf",
                ".png" => "image/png",// imágenes
                                      //".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".3gp" => "video/3gp",// videos
                ".mp4" => "video/mp4",
                ".aac" => "audio/aac",//audio
                ".amr" => "audio/amr",
                ".mp3" => "audio/mpeg",
                ".m4a" => "audio/mp4",
                ".ogg" => "audio/ogg", //OPUS codecs only; base audio/ogg not supported
                _ => "application/octet-stream",
            };
        }
    }
}