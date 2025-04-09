using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using WhatsAppPresentacionV6.Modelos;
using WhatsAppPresentacionV6.Modelos.RecivedMedia;

namespace WhatsAppPresentacionV6.Servicios
{
    public class HanldeDocument
    {
        private readonly string _tokenAcceso;
        private readonly string _facebookGraphVersion = "v21.0";
        public HanldeDocument(string tokenAcceso)
        {
            _tokenAcceso = tokenAcceso;
        }

        public async Task<HttpResponseMessage> ProcesaRecibirDocumento(WebHookResponseModel entry)
        {
            //Guardamos el mensaje 
            var mensaje_recibido = entry.entry[0].changes[0].value.messages[0];
            //Guardamos el ID del mensaje 
            //string id_wa = mensaje_recibido.id;
            if (mensaje_recibido.document != null)
            {
                //string urlDocumento = mensaje_recibido.document.link;
                string nombreDocumento = mensaje_recibido.document.filename;
                string mime_type = mensaje_recibido.document.mime_type;
                string idDocumento = mensaje_recibido.document.id;

                //Prepara la localización de descarga
                string rutaDescarga = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DocumentosDescargados");
                //Prepara la localización de descarga con el nombre del documento
                string rutaCompleta = Path.Combine(rutaDescarga, nombreDocumento +"."+ mime_type);

                //Comprueba la excistencia de la ruta de descarga
                if (!Directory.Exists(rutaDescarga))
                {
                    Directory.CreateDirectory(rutaDescarga);
                }

                string urlDocumento = await ObtenerURLDocumentoDeID(idDocumento);
                bool exito = await DescargarDocumento(urlDocumento, rutaCompleta);

                if (exito)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK) 
                    {
                        Content = new StringContent("Documento procesado y descargado correctamente.", System.Text.Encoding.UTF8, "text/plain")
                    };
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent("Error al descargar el documento.", System.Text.Encoding.UTF8, "text/plain")
                    };
                }
            }
            else
            {
                Console.WriteLine("No se recibió un documento.");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("No se procesó un documento.", System.Text.Encoding.UTF8, "text/plain")
                };
            }
        }
        public async Task<string> ObtenerURLDocumentoDeID(string idDocumento)
        {
            string apiURL = $"https://graph.facebook.com/{_facebookGraphVersion}/{idDocumento}";
            using (HttpClient cliente = new())
            {
                //Crea los headers y los añade al cliente
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAcceso);
                try
                {
                    HttpResponseMessage response = await cliente.GetAsync(apiURL);
                    if (response.IsSuccessStatusCode)
                    {
                        string contenidoRespuesta = await response.Content.ReadAsStringAsync();
                        var infoDocumento = JsonSerializer.Deserialize<RespuestaDocumento>(contenidoRespuesta);
                        if (infoDocumento?.url != null)
                        {
                            return infoDocumento.url;
                        }
                    }
                    return "URL no encontrado";
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex.ToString());
                    return "Error al buscar URL";
                }
            }
        }
        public async Task<bool> DescargarDocumento(string urlDocumento, string rutaCompleta)
        {
            if (urlDocumento == null || urlDocumento == "URL no encontrado" || urlDocumento == "Error al buscar URL") 
            {
                return false;
            }
            try
            {
                using (HttpClient cliente = new HttpClient())
                {
                    //Crea los headers y los añade al cliente
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAcceso);

                    HttpResponseMessage response = await cliente.GetAsync(urlDocumento);

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] documentData = await response.Content.ReadAsByteArrayAsync();
                        await File.WriteAllBytesAsync(rutaCompleta, documentData);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Error al descargar el documento: {response.StatusCode}");
                        return false;
                    }
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Excepción al descargar el documento: {ex.Message}");
                return false;
            }

        }
    }
}
