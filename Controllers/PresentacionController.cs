
using WhatsAppPresentacionV11.Modelos;
using WhatsAppPresentacionV11.Servicios;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text;
using System.Net.Http;




namespace WhatsAppPresentacionV11.Controllers
{
    public class PresentacionController : ControllerBase
    {
        private readonly WhatsAppMessageService _messageSendService;
        private readonly HanldeDocument _hanldeDocument;
        private readonly WhatsAppFlow _manejoDeComprador;
        private static string _lastMessageState = string.Empty;
        private readonly FlowDecryptionEncryptionService _decryptionEncryptionService;

        private static readonly List<Flow_response_json_Model_1142951587576244_Crear_Producto> _productos = new(){};
        private static List<string> _receivedMessages = new List<string>();
        private static List<(string ButtonId, string ButtonLabelText)> _botones = new();
        private static List<string> correosACopiar = new List<string>();

        private static Dictionary<string, (string productoCodigoModificar, string productoNombreOld)> _producto_a_modificar = new() { };
        private static Dictionary<string, (string lastMessageState, string oldUserMessage, 
            string montoFactura, string NITCliente, 
            string NITComprador, string NombreComprador,
            string Factura_Observaciones, List<string> ListaProductos, List<Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad> ListaPreciosYCantidades)> 
                _RegisteredPhoneDicitonary = new Dictionary
            <string, (string lastMessageState, string oldUserMessage,
            string montoFactura, string NITCliente,
            string NITComprador, string NombreComprador,
            string Factura_Observaciones, List<string> ListaProductos, List<Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad> ListaPreciosYCantidades)>();

        public PresentacionController()
        {
            string idTelefono = "516368431570925"; // Replace with actual Phone ID
            string tokenAcceso = "EAAJWVACSawYBOxXZAEWPZBgvVZBlhQOuzztW0J91E6CVLSpStVpOeWd4i7FEqYI4SEC230WSIH9knK93eZCJepzTftQhu5ZAoQWb5nHNwdPSPSAOkja8XdnBZAbg1w3JMSZAxmxs9nZCx0k8AngZCBlMAdgHm9jadJmGXBMVsNPnzOF8P5ZCAjywzeAbEYIKADnZAAIkQZDZD"; // Replace with actual Access Token
            
            _messageSendService = new WhatsAppMessageService(idTelefono, tokenAcceso);
            _hanldeDocument = new HanldeDocument(tokenAcceso);
            _manejoDeComprador = new WhatsAppFlow(idTelefono, tokenAcceso);
        }

        //RECIBIMOS LOS DATOS DE VALIDACION VIA GET 
        [HttpGet("api/webhook")]

        //RECIBIMOS LOS PARAMETROS QUE NOS ENVIA WHATSAPP PARA VALIDAR NUESTRA URL
        public string WebhookValidation(
            [FromQuery(Name ="hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string tokenVerificacion
            )
        {
            _receivedMessages.Add("Reciví algo");
            _receivedMessages.Add($"Token De Verificacion: {tokenVerificacion}");
            _receivedMessages.Add($"Mode: {mode}");
            _receivedMessages.Add($"Challenge: {challenge}");
            if (tokenVerificacion.Equals("LIEesmoainalecilo"))
            {
                _receivedMessages.Add($"Token Correcto");
                return challenge;
            }
            _receivedMessages.Add($"Token incorrecto");
            return "Token Incorrecto";
        }

        [HttpPost("api/webhook")]
        public async Task<IActionResult> Webhook()
        {
            WebHookResponseModel? webhookData = null;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string rawBody = await reader.ReadToEndAsync();
                try
                {
                    webhookData = JsonSerializer.Deserialize<WebHookResponseModel>(rawBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Permite nombres de propiedad en minúsculas/mayúsculas sin errores
                    });
                }
                catch (Exception ex)
                {
                    _receivedMessages.Add($"Error deserializando JSON: {ex.Message}");
                    return Ok("Formato JSON inválido."); //Return Ok() even on failure, unless you want WhatsApp to retry
                }
            }
            if (webhookData == null)
            {
                _receivedMessages.Add("Error: webhookData es NULL. Posible problema de deserialización.");
                return Ok();
            }

            if ((webhookData.entry == null || !webhookData.entry.Any())&&(string.IsNullOrEmpty(webhookData.encrypted_flow_data) ||
                string.IsNullOrEmpty(webhookData.encrypted_aes_key) ||
                string.IsNullOrEmpty(webhookData.initial_vector)))
            {
                _receivedMessages.Add("Estructura de mensaje recibido inválida");
                return BadRequest("Estructura inválida");
            }
            //_receivedMessages.Add("1");

            //Revisa si es un mensaje encriptado
            if (!(string.IsNullOrEmpty(webhookData.encrypted_flow_data) ||
                string.IsNullOrEmpty(webhookData.encrypted_aes_key) ||
                string.IsNullOrEmpty(webhookData.initial_vector)))
            {
                _receivedMessages.Add("Respuesta a mensaje interactivo");
                _receivedMessages.Add($"Encription: {webhookData.encrypted_flow_data}");
                _receivedMessages.Add($"Key: {webhookData.encrypted_aes_key}");
                _receivedMessages.Add($"Initial Vector: {webhookData.initial_vector}");

                string messageStatus = await HandleEncryptedInteractiveFlowMessage(webhookData);
                return Ok();
            }

            //_receivedMessages.Add("Mensaje válido"); //Marca que está funcionando
            string fromPhoneNumber = NormalizarNumeroMexico(webhookData.entry[0].changes[0].value.messages[0].from);
            try
            {
                _receivedMessages.Add("Número normalizado");

                if (!_RegisteredPhoneDicitonary.ContainsKey(fromPhoneNumber))
                {
                    _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                        "", // lastMessageState
                        "", // oldUserMessage
                        "", // montoFactura
                        "", // NITCliente
                        "", // NITComprador
                        "",  // _NombreComprador
                        //"", //Factura_Telefono
                        //"", //Factura_dirección
                        //"", //Factura_Monto
                        "", //Factura_Descripción era. Ahora es Factura_Observaciones
                        new List<string> { }, // ListaProductos
                        new List<Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad> { } //ListaPreciosYCantidades
                        );
                }

                string messageStatus = await HandleIncomingMessage(webhookData, fromPhoneNumber);

                _receivedMessages.Add("Mensaje manejado");
                
                _receivedMessages.Add(messageStatus);
                _receivedMessages.Add(_lastMessageState); //
                _receivedMessages.Add(GetLastMessageState(fromPhoneNumber));
                return Ok();
            }
            catch (Exception ex)
            {
                try
                {
                    string incomingJson = System.Text.Json.JsonSerializer.Serialize(webhookData, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true // Makes the JSON easier to read
                    });

                    _receivedMessages.Add("Error:" + ex.Message);
                    _receivedMessages.Add("WhatsApp JSON: " + incomingJson);
                }
                catch (Exception serializationEx)
                {
                    _receivedMessages.Add($"Error serializing incoming JSON: {serializationEx.Message}");
                }
                await _messageSendService.EnviarMensajeError(fromPhoneNumber, ex.Message);
                return Ok(ex.Message); //Return Ok() even on failure, unless you want WhatsApp to retry
            }

        }

        //Recabamos los Mensajes VIA GET 
        [HttpGet]
        //DENTRO DE LA RUTA webhook 
        [Route("messages")]
        public dynamic GetMessages()
        {
            _receivedMessages.Add("Recabando mensajes: ");
            // Return the list of received messages as the response 
            return _receivedMessages;
        }

        //Recabamos los Mensajes del servicio VIA GET 
        [HttpGet]
        [Route("Service/Logs")]
        public dynamic GetWhatsAppFlowServiceLog()
        {
            // Return the list of received messages as the response 
            return _manejoDeComprador.GetEventsList();
        }

        [HttpGet]
        [Route("Service/Compradores/List")]
        public dynamic GetCompradores()
        {
            // Return the list of compradores as the response 
            return _manejoDeComprador.ReturnCompradores();
        }

        [HttpGet("productos/todos")]
        public IActionResult GetTodosLosProductos()
        {
            if (_productos == null || !_productos.Any())
                return Ok("No hay productos registrados.");

            var resultado = new StringBuilder();
            int contador = 1;

            foreach (var producto in _productos)
            {
                resultado.AppendLine($"Producto #{contador++}:");
                resultado.AppendLine($"  Nombre: {producto.Agregar_Producto_Nombre}");
                resultado.AppendLine($"  Precio Unitario: {producto.Agregar_Producto_Precio_Unitario}");
                resultado.AppendLine($"  Código: {producto.Agregar_Producto_Codigo}");
                resultado.AppendLine($"  Unidad de Medida: {producto.Agregar_Producto_Unidad_Medida}");
                resultado.AppendLine($"  Activo: {producto.Agregar_Producto_Activo}");
                resultado.AppendLine($"  Traslados: {producto.Agregar_Producto_traslados}");
                resultado.AppendLine($"  Impuesto: {producto.Agregar_Producto_Impuesto}");
                resultado.AppendLine($"  Tasa o Cuota: {producto.Agregar_Producto_Tasa_cuota}");
                resultado.AppendLine($"  Impuestos Saludables: {producto.Agregar_Producto_Impuestos_Saludables}");
                resultado.AppendLine($"  Impuestos Saludables (Lista): {string.Join(", ", producto.Agregar_Producto_Impuestos_Saludables2 ?? new List<string>())}");
                resultado.AppendLine($"  Info Adicional: {producto.Agregar_Producto_Info_Adicional}");
                resultado.AppendLine($"  Flow Token: {producto.flow_token}");
                resultado.AppendLine(new string('-', 40));
            }
            return Ok(resultado.ToString());
        }

        [HttpPost("producto/Añadir")]
        public IActionResult CrearEjemplo()
        {
            var productoEjemeplos = new List<Flow_response_json_Model_1142951587576244_Crear_Producto>
            {
                new Flow_response_json_Model_1142951587576244_Crear_Producto
                {
                    flow_token = "token-ejemplo-123",
                    Agregar_Producto_Nombre = "Pan Integral",
                    Agregar_Producto_Precio_Unitario = "25",
                    Agregar_Producto_Info_Adicional = "500g, hecho con harina integral",
                    Agregar_Producto_Codigo = "PANINT500",
                    Agregar_Producto_Unidad_Medida = "pieza",
                    Agregar_Producto_Activo = "true",
                    Agregar_Producto_traslados = "incluye",
                    Agregar_Producto_Impuesto = "IVA",
                    Agregar_Producto_Tasa_cuota = "16",
                    Agregar_Producto_Impuestos_Saludables = "Ninguno",
                    Agregar_Producto_Impuestos_Saludables2 = new List<string> { "Ninguno" }
                },
                new Flow_response_json_Model_1142951587576244_Crear_Producto
                {
                    flow_token = "opcionCrearProductoFlow573214304814",
                    Agregar_Producto_Nombre = "Camiseta",
                    Agregar_Producto_Precio_Unitario = "50000",
                    Agregar_Producto_Codigo = "01",
                    Agregar_Producto_Unidad_Medida = "Unidad",
                    Agregar_Producto_Activo = "Activar",
                    Agregar_Producto_traslados = "IVA",
                    Agregar_Producto_Impuesto = "0",
                    Agregar_Producto_Tasa_cuota = "19",
                    Agregar_Producto_Impuestos_Saludables = "Desactivar",
                    Agregar_Producto_Impuestos_Saludables2 = new List<string>()
                },
                new Flow_response_json_Model_1142951587576244_Crear_Producto
                {
                    flow_token = "opcionCrearProductoFlow573214304814",
                    Agregar_Producto_Nombre = "Chaqueta",
                    Agregar_Producto_Precio_Unitario = "200000",
                    Agregar_Producto_Codigo = "02",
                    Agregar_Producto_Unidad_Medida = "Unidad",
                    Agregar_Producto_Activo = "Activar",
                    Agregar_Producto_traslados = "IVA",
                    Agregar_Producto_Impuesto = "0",
                    Agregar_Producto_Tasa_cuota = "19",
                    Agregar_Producto_Impuestos_Saludables = "Desactivar",
                    Agregar_Producto_Impuestos_Saludables2 = new List<string>()
                },
                new Flow_response_json_Model_1142951587576244_Crear_Producto
                {
                    flow_token = "opcionCrearProductoFlow573023871573",
                    Agregar_Producto_Nombre = "Chocoramo",
                    Agregar_Producto_Precio_Unitario = "1000",
                    Agregar_Producto_Codigo = "1234",
                    Agregar_Producto_Unidad_Medida = "Unidad",
                    Agregar_Producto_Activo = "Activar",
                    Agregar_Producto_traslados = "IVA",
                    Agregar_Producto_Impuesto = "0",
                    Agregar_Producto_Tasa_cuota = "19",
                    Agregar_Producto_Impuestos_Saludables = "Desactivar",
                    Agregar_Producto_Impuestos_Saludables2 = new List<string>(),
                    Agregar_Producto_Info_Adicional = "Chocolate"
                }
            };
            _receivedMessages.Add("Productos ejemplo");
            _productos.AddRange(productoEjemeplos);

            return Ok("Ejemplos creados exitosamente");
        }

        private async Task<string> HandleIncomingMessage(WebHookResponseModel webhookData, string fromPhoneNumber)
        {
            _receivedMessages.Add("Handeling incoming Message");
            var incomingMessage = webhookData.entry[0].changes[0].value.messages[0];
                    
            if (incomingMessage.interactive != null)
            {
                _receivedMessages.Add("Message is interactive");
                return await HandleInteractiveMessage(webhookData, fromPhoneNumber);
            }
            else if (incomingMessage.text != null)
            {
                _receivedMessages.Add("Message is text");
                return await HandleTextMessage(webhookData, fromPhoneNumber);
            }
            else if (incomingMessage.document != null)
            {
                _receivedMessages.Add("Message is document");
                return await HandleDocumentMessage(webhookData);
            }
            _receivedMessages.Add("Message not supported");
            return "Message type not supported";
        }
        private async Task<string> HandleInteractiveMessage(
            WebHookResponseModel webhookData, 
            string fromPhoneNumber)
        {
            var incomingMessage = webhookData.entry[0].changes[0].value.messages[0];
            if (incomingMessage.interactive.button_reply != null)
            {
                string selectedButtonId = incomingMessage.interactive.button_reply.id;
                _receivedMessages.Add($"Message is button reply with ID: {selectedButtonId}");
                return await HandleInteractiveButtonMessage(webhookData, selectedButtonId, fromPhoneNumber);
            }
            else if (incomingMessage.interactive.list_reply != null)
            {
                string selectedListButtonId = incomingMessage.interactive.list_reply.id;
                _receivedMessages.Add($"Message is List reply with ID: {selectedListButtonId}");
                return await HandleInteractiveListMessage(webhookData, selectedListButtonId, fromPhoneNumber);
            }
            else if (incomingMessage.interactive.nfm_reply != null)
            {
                string flowName = incomingMessage.interactive.nfm_reply.name;
                string flowBody = incomingMessage.interactive.nfm_reply.body;
                _receivedMessages.Add($"Flow received: {flowName} - {flowBody}");
                return await HandleInteractiveFlowMessage(webhookData, fromPhoneNumber);
            }
            _receivedMessages.Add($"unknown interactive message: {incomingMessage.interactive.type}");
                return "";
        }
        private async Task<string> HandleInteractiveButtonMessage(
            WebHookResponseModel webhookData, 
            string selectedButtonId, 
            string fromPhoneNumber)
        {
            _receivedMessages.Add($"Interactive Button Message");
            string messageStatus = "";

            _receivedMessages.Add($"Selecting path");
            switch (selectedButtonId)
            {
                case "opcion_Iniciar_Sesión":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Cuál es tu NIT o nombre como facturador?");
                    break; 
                case "opcion_Generar_Factura_Text":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Qué NIT o nombre usarás para facturar?");
                    //messageStatus = messageStatus + await _messageSendService.EnviarTexto(fromPhoneNumber, "Escribe el nombre o el NIT");
                    break;
                case "opcion_Recibir_Registro_Flow":
                    
                    (string, string)? phoneExistance = _manejoDeComprador.TeléfonoExiste(fromPhoneNumber);

                    string messageText = phoneExistance != null
                        ? $"Tu teléfono {fromPhoneNumber} está registrado con {phoneExistance.Value.Item1} - {phoneExistance.Value.Item2}"
                        : $"Para registrar una persona física deberás llenar el siguiente formulario";
                    if (phoneExistance != null)
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, $"Para registrar una persona física deberás llenar el siguiente formulario.\n Para modificar una persona física existente, registrar usando mismo nombre y NIT.");
                    }

                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "637724539030495", $"opcionRecibirRegistroFlow{fromPhoneNumber}", "published", messageText, "Registrar");
                    
                    break;
                case "opcion_Registrar_Empresa_Flow":

                    //(string, string)? phoneCompanyExistance = _manejoDeComprador.TeléfonoExiste(fromPhoneNumber);

                    //string messageCompanyText = phoneCompanyExistance != null
                        //? $"está registrado con {phoneCompanyExistance.Value.Item1} - {phoneCompanyExistance.Value.Item2}"
                        //: "aún no está registrado";

                    //string buttonCompanyText = phoneCompanyExistance != null
                        //? $"Modificar"
                        //: "Registrar";

                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1187351356327089", $"opcionRegistrarEmpresaFlow{fromPhoneNumber}", "published", $"Para registrar una empresa deberás llenar el siguiente formulario"/*$"Tu teléfono {fromPhoneNumber} {messageCompanyText}."*/, "Registrar"/*buttonCompanyText*/);// sustituir flow correcto
                    break;
                case "opcion_Generar_Factura_Registrar_Persona_Flow":

                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "637724539030495", $"opcionRecibirRegistroFlow{fromPhoneNumber}", "published", "Para registrar una persona física deberás llenar el siguiente formulario", "Registrar");

                    break;
                case "opcion_Generar_Factura_Registrar_Empresa_Flow":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1187351356327089", $"opcionRegistrarEmpresaFlow{fromPhoneNumber}", "published", $"Para registrar una empresa deberás llenar el siguiente formulario"/*$"Tu teléfono {fromPhoneNumber} {messageCompanyText}."*/, "Registrar"/*buttonCompanyText*/);// sustituir flow correcto
                    break;
                case "opcion_Registrar_Cliente":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1584870855544061",/*"1297640437985053",*/ $"opcionRegistrarCliente{fromPhoneNumber}", "published", $"Registrando Cliente", "Registrar");
                    break;
                case "opcion_Continuar_Con_La_Factura":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1414924193269074",/*"682423707677994",*/ $"opcionFacturar{fromPhoneNumber}", "published", $"Deseas facturar?", "Facturar");
                    break;
                case "opcion_Cancelar":
                    UpdateRestart(fromPhoneNumber);
                    messageStatus = await MainMenuButton(fromPhoneNumber, null);
                    break;
                case "opcion_configurar":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Configurar_Registros_DIAN", "Registros DIAN"), ("opcion_Configurar_Productos ", "Productos"), ("opcion_configurar_Clientes", "Clientes") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que quieres configurar?", "Selecciona opción", "Hay 3 opciones", _botones);
                    break;
                case "opcion_configurar_Clientes":
                    int CliPorCel = _manejoDeComprador.ContarClientesPorTelefono(fromPhoneNumber);
                    if (CliPorCel <= 0)
                    {
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Agregar_Clientes", "Agregar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que quieres hacer con un cliente?", "Selecciona opción", "Hay 3 opciones", _botones);
                        _receivedMessages.Add(messageStatus);
                        break;
                    }
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Agregar_Clientes", "Agregar"), ("opcion_Modificar_Clientes ", "Modificar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que quieres hacer con un cliente?", "Selecciona opción", "Hay 2 opciones", _botones);
                    _receivedMessages.Add(messageStatus);
                    break;
                case "opcion_Agregar_Clientes":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Cuál es tu NIT o nombre como facturador?");
                    break;
                case "opcion_Modificar_Clientes":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Cuál es tu NIT o nombre como facturador?");
                    break;
                case "opcion_Modificar_Clientes_Persona_Natural":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿A quien modificas? Puedes escribir su NIT, Nombre o una parte de estos");
                    break;
                case "opcion_Modificar_Clientes_Persona_Juridica":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Que empresa modificas? Puedes escribir su NIT, Razon social o una parte de estos");
                    break;
                case "opcion_Configurar_Productos":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Configurar_Productos_agregar", "Agregar"), ("opcion_Configurar_Productos_Modificar", "Modificar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que quieres hacer con productos?", "Pudes agregar o modificar uno", "Hay 3 opciones", _botones);
                    break;
                case "opcion_Configurar_Productos_agregar":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1142951587576244", $"opcionCrearProductoFlow{fromPhoneNumber}", "published", $"Creando Producto", "Crear");
                    break;
                case "opcion_Anadir_Producto_agregar":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1142951587576244", $"opcionCrearProductoFlow{fromPhoneNumber}", "published", $"Creando Producto", "Crear");
                    break;
                case "opcion_Configurar_Productos_Modificar":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Que producto modificas? Puedes escribir su número, nombre o una parte de estos");
                    break;
                case "opcion_Configurar_Registros_DIAN":
                    UpdateRestart(fromPhoneNumber);
                    //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¡Registros DIAN configurados!"); 
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1232523891647775", $"opcionFacturar{fromPhoneNumber}", "published", $"Registros DIAN?", "Registrar");
                    return messageStatus;
                    break;
                case "opcion_configurar_usuario":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Elegir_tipo_Registro", "Regístrame"), ("opcion_Iniciar_Sesión", "Iniciar Sesión"), ("opcion_configurar_usuario_modificar", "Modificar") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que deseas hacer con un usuario?", "¿Registrar uno nuevo, relacionar uno existente a tu teléfono o modificar uno?", "Regístrate", _botones);
                    break;
                case "opcion_configurar_usuario_modificar":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_configurar_usuario_modificar_modificar", "Modificar existente"), ("opcion_configurar_usuario_modificar_borrar", "Borrar existente"), ("opcion_Cancelar", "Cancelar") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que deseas hacerle a un usuario?", "¿Modificar o borrar?", "Regístrate", _botones);
                    break;
                case "opcion_configurar_usuario_modificar_modificar":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "No implementado");
                    break;
                case "opcion_configurar_usuario_modificar_borrar":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "No implementado");
                    break;
                case "opcion_Recibir_Elegir_tipo_Registro":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Registro_Flow", "Persona Natural"), ("opcion_Registrar_Empresa_Flow", "Empresa"), ("opcion_Cancelar", "Reiniciar Proceso") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Registrando, ¿A quien quieres registrar?", "Selecciona opción", "Hay 3 opciones", _botones);
                    break;
                case "opcion_Anadir_Producto":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Favor de indicar nombre del producto");
                    break;
                case "opcion_Generar_Factura_Cliente":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, $"¿A quién facturarás?\nPuedes escribir su NIT, Nombre o una parte de estos");
                    break;
                case "opcion_Observaciones_Y_Mail":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1414924193269074", $"opcion_Observaciones_Y_Mail{fromPhoneNumber}", "published", "Llenar para añadir observaciones o emails a copiar", "Abrir");
                    break;
                case "opcion_Finalizar_Factura":
                    //messageStatus = await _messageSendService.EnviarDocumentoPorUrl(fromPhoneNumber, "https://test-timbrame.azurewebsites.net/Ejemplo/24a8a905-f155-4726-a27f-1451a8bf5388.pdf", "Preview.pdf");
                    _receivedMessages.Add("Botón: Finalizar Factura");
                    var nombreCompradorpre = GetNombreComprador(fromPhoneNumber);
                    var nitCompradorpre = GetNITComprador(fromPhoneNumber);
                    var nitClientepre = GetNITCliente(fromPhoneNumber);

                    string observacionespre = GetObservaciones(fromPhoneNumber) ?? "-";
                    var productospre = _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var datapre) ? datapre.ListaProductos : new List<string>();
                    var preciosYCantidadespre = GetPrecioUnitarioYCantidad(fromPhoneNumber);


                    var productosCompletospre = new List<Flow_response_json_Model_1142951587576244_Crear_Producto>();
                    _receivedMessages.Add($"Cantidad de productos registrados: {productospre?.Count ?? 0}");
                    for (int i = 0; i < productospre.Count; i++)
                    {
                        var codigo = productospre[i];
                        var producto = _productos.FirstOrDefault(p => p.Agregar_Producto_Codigo == codigo);
                        if (producto != null)
                        {
                            productosCompletospre.Add(producto);
                            _receivedMessages.Add($"Producto {producto.Agregar_Producto_Codigo} añadido a lista");
                        }
                    }

                    var JSONtoSendPre = _manejoDeComprador.CrearFacturaJson(nombreCompradorpre, nitCompradorpre, nitClientepre, observacionespre, productosCompletospre, preciosYCantidadespre, correosACopiar);
                    _receivedMessages.Add("JSONtoSend: ");
                    _receivedMessages.Add(JSONtoSendPre);
                    _receivedMessages.Add("--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, JSONtoSendPre);
                    //_receivedMessages.Add("JSONtoSend mandado");
                    //_receivedMessages.Add(messageStatus);

                    string preURLFactura = await _manejoDeComprador.PedirPreviewJson(JSONtoSendPre);

                    _receivedMessages.Add("http recibido");
                    _receivedMessages.Add(preURLFactura);

                    messageStatus = await _messageSendService.EnviarDocumentoPorUrl(fromPhoneNumber, preURLFactura, "Preview.pdf");

                    //Final nuevo código

                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Cancelar", "Reiniciar Proceso"), ("opcion_Finalizar_Finalizar_Factura", "Enviar factura"), ("opcion_Finalizar_Modificar_Factura", "Modificar factura") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Deseas Finalizar la factura?", "El documento es un preview", "Hay 3 opciones", _botones);
                    break;
                case "opcion_Finalizar_Finalizar_Factura":
                    var nombreComprador = GetNombreComprador(fromPhoneNumber);
                    var nitComprador = GetNITComprador(fromPhoneNumber);
                    var nitCliente = GetNITCliente(fromPhoneNumber);

                    string observaciones = GetObservaciones(fromPhoneNumber);
                    var productos = _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.ListaProductos : new List<string>();
                    var preciosYCantidades = GetPrecioUnitarioYCantidad(fromPhoneNumber);

                    string resumen = $"*Resumen del Formulario Recibido:*\n\n";

                    resumen += $"*Comprador:* {nombreComprador}, NIT: {nitComprador}\n";


                    resumen += $"\n*Información de Factura:*\n";
                    /*resumen += $"📍 *Dirección:* {direccion}\n";
                    resumen += $"📞 *Teléfono:* {telefono}\n";
                    resumen += $"📝 *Descripción:* {descripcion}\n";
                    resumen += $"💰 *Monto Total:* {monto}\n";*/


                    resumen += $"*Cliente:* {nitCliente}\n";

                    if (productos.Any())
                    {
                        resumen += $"\n*Productos:*\n";
                        for (int i = 0; i < productos.Count; i++)
                        {
                            var productoCodigo = productos[i]; // invertir codigo y nombre si se usan al revés
                            var producto = _productos.FirstOrDefault(p => p.Agregar_Producto_Codigo == productoCodigo);
                            var precioCantidad = i < preciosYCantidades.Count ? preciosYCantidades[i] : null;
                            var productoNombre = producto?.Agregar_Producto_Nombre ?? "Nombre no encontrado";

                            resumen += $"- {productoNombre}, {productoCodigo}";
                            if (precioCantidad != null)
                            {
                                resumen += $" | Cantidad: {precioCantidad.PUyC_Cantidad}, Precio Unitario: {precioCantidad.PUyC_Precio_Unitario}";
                            }
                            resumen += "\n";
                        }
                    }
                    else
                    {
                        resumen += "\n*Productos:* No se registraron productos.\n";
                    }

                    //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, resumen);

                    var productosCompletos = new List<Flow_response_json_Model_1142951587576244_Crear_Producto>();

                    for (int i = 0; i < productos.Count; i++)
                    {
                        var codigo = productos[i];
                        var producto = _productos.FirstOrDefault(p => p.Agregar_Producto_Codigo == codigo);
                        if (producto != null)
                        {
                            productosCompletos.Add(producto);
                            _receivedMessages.Add("Producto añadido a lista");
                        }
                    }

                    var JSONtoSend = _manejoDeComprador.CrearFacturaJson(nombreComprador, nitComprador, nitCliente, observaciones, productosCompletos, preciosYCantidades, correosACopiar);
                    _receivedMessages.Add("JSONtoSend: ");
                    _receivedMessages.Add(JSONtoSend);
                    _receivedMessages.Add("--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, JSONtoSend);
                    //_receivedMessages.Add("JSONtoSend mandado");
                    //_receivedMessages.Add(messageStatus);

                    messageStatus = messageStatus + await _messageSendService.EnviarTexto(fromPhoneNumber, "Enviando Factura");

                    string respuestaFactura = await _manejoDeComprador.MandarFacturaJson(JSONtoSend);
                    _receivedMessages.Add("Respuesta Factura");
                    _receivedMessages.Add(respuestaFactura);

                    if (respuestaFactura.StartsWith("https"))
                    {
                        _receivedMessages.Add("http recibido");
                        _receivedMessages.Add(messageStatus);

                        messageStatus = await _messageSendService.EnviarDocumentoPorUrl(fromPhoneNumber, respuestaFactura, "Factura.pdf");
                    }
                    else if (respuestaFactura == "true" || respuestaFactura.StartsWith("Factura creada correctamente"))
                    {
                        _receivedMessages.Add("JSON True");
                        _receivedMessages.Add(respuestaFactura);
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Ciclo completo");

                        UpdateRestart(fromPhoneNumber);

                        _receivedMessages.Add("Final Final");

                        messageStatus = await MainMenuButton(fromPhoneNumber, null);
                    }
                    else if( respuestaFactura == "false")
                    {
                        _receivedMessages.Add("JSON False");
                        _receivedMessages.Add(respuestaFactura);

                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Cancelar", "Reiniciar Proceso"), ("opcion_Finalizar_Finalizar_Factura", "Reintentar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Factura no se pudo regitrar en la DIAN", "¿Deseas reintentar?", "Hay 2 opciones", _botones);

                        //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Factura no se pudo regitrar en la DIAN, Reiniciando");
                    }
                    else
                    {
                        _receivedMessages.Add("JSON mandado");
                        _receivedMessages.Add(respuestaFactura);

                        //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, respuestaFactura);

                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Cancelar", "Reiniciar Proceso"), ("opcion_Finalizar_Finalizar_Factura", "Reintentar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, respuestaFactura, "¿Deseas reintentar?", "Hay 2 opciones", _botones);
                    }

                    _receivedMessages.Add("Status message");
                    _receivedMessages.Add(messageStatus);

                    break;
                case "opcion_Finalizar_Modificar_Factura":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Cliente", "Cliente a Facturar"), ("opcion_Finalizar_Modificar_Productos", "Productos"), ("opcion_Finalizar_Modificar_Opciones", "Más opciones") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que información de la factura deseas modificar?", "Información Guardada", "Hay 3 opciones", _botones);
                    break;
                case "opcion_Finalizar_Modificar_Opciones":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Info_Adicional", "Info adicional"), ("opcion_Finalizar_Factura", "Finalizar factura"), ("opcion_Cancelar", "Reiniciar Proceso") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que información de la factura deseas modificar?", "Información Guardada", "Hay 3 opciones", _botones);
                    break;
                case "opcion_Finalizar_Modificar_Cliente":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Cliente_Cambiar", "Cambiar"), ("opcion_Finalizar_Modificar_Cliente_Modificar", "Modificar"), ("opcion_Finalizar_Modificar_Factura", "Cancelar") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que quieres cambiar del cliente?", "¿Cambiar de cliente a facturar o modificar el cliente?", "Hay 3 opciones", _botones);
                    break;
                case "opcion_Finalizar_Modificar_Cliente_Cambiar":
                    // Función apuntando a lógica para cambiar de cliente
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Que cliente deseas usar?");
                    break;
                case "opcion_Finalizar_Modificar_Cliente_Modificar":
                    // implementar lógica para modificar cliente existente
                    _receivedMessages.Add("opcion_Finalizar_Modificar_Cliente_Modificar");
                    string clienteTipo = _manejoDeComprador.ReturnClientType(GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber), GetNITCliente(fromPhoneNumber));

                    if (string.IsNullOrEmpty(clienteTipo) || clienteTipo.StartsWith("[Error]"))
                    {
                        _receivedMessages.Add("Error al buscar cliente");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Cliente_Modificar", "Volver a intentar"), ("opcion_Finalizar_Factura", "Regresar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Error al buscar cliente", "Información sigue Guardada", "Hay 3 opciones", _botones);
                    }
                    else if(clienteTipo == "Tipo_Persona_Juridica")
                    {
                        messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1378725303264167", $"opcionModCliJuridicaFlow{fromPhoneNumber}", "published", $"Modificando Cliente", "Modificar");
                    }
                    else if (clienteTipo == "Tipo_Persona_Natural")
                    {
                        messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "931945452349522", $"opcionModCliNaturalFlow{fromPhoneNumber}", "published", $"Modificando Cliente", "Modificar");
                    }
                        break;
                case "opcion_Finalizar_Modificar_Productos":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Productos_Anadir", "Añadir"), ("opcion_Finalizar_Modificar_Productos_Quitar", "Quitar"), ("opcion_Finalizar_Modificar_Factura", "Cancelar") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que quieres cambiar de los productos?", "¿Añadir otros o quitar alguno?", "Hay 3 opciones", _botones);
                    break;
                case "opcion_Finalizar_Modificar_Productos_Anadir":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Favor de indicar nombre del producto");
                    break;
                case "opcion_Finalizar_Modificar_Productos_Quitar":
                    _receivedMessages.Add($"opcion_Finalizar_Modificar_Productos_Quitar button");

                    List<string> productosQuitarList = GetProductosEnFacturaList(fromPhoneNumber);

                    if (!productosQuitarList.Any())
                    {
                        _receivedMessages.Add("No hay productos para quitar");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Productos_Quitar", "Volver a intentar"), ("opcion_Finalizar_Factura", "Regresar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "No hay productos en la factura para eliminar", "Información sigue Guardada", "Hay 3 opciones", _botones);
                        break;
                    }

                    var productosEnFactura = _productos
                        .Where(p => productosQuitarList.Contains(p.Agregar_Producto_Codigo ?? ""))
                        .ToList();

                    if (!productosEnFactura.Any())
                    {
                        _receivedMessages.Add("Productos en factura no encontrados en _productos");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Productos_Quitar", "Volver a intentar"), ("opcion_Finalizar_Factura", "Regresar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Productos no encontrados en el catálogo", "Información sigue Guardada", "Hay 3 opciones", _botones);
                        break;
                    }

                    if (productosEnFactura.Count != productosQuitarList.Count)
                        _receivedMessages.Add($"Productos en factura y productos en la lista no coinciden: factura {productosEnFactura.Count}- lista {productosQuitarList.Count}");

                    int totalSets = (int)Math.Ceiling(productosEnFactura.Count / 10.0);
                    var listaSeccionesProductos = new List<(string, List<(string, string, string)>)>();

                    for (int i = 0; i < totalSets; i++)
                    {
                        string setTitle = $"Set {i + 1}";

                        var opciones = productosEnFactura
                            .Skip(i * 10)
                            .Take(10)
                            .Select((producto, index) => (
                                producto.Agregar_Producto_Codigo ?? $"{index}", // OptionId (usado para selectedListId)
                                (i * 10 + index + 1).ToString(),               // OptionTitle (número de opción)
                                producto.Agregar_Producto_Nombre ?? "Producto sin nombre" // OptionDescription
                            ))
                            .ToList();

                        listaSeccionesProductos.Add((setTitle, opciones));
                    }
                    listaSeccionesProductos.Add(("Otras opciones", new List<(string, string, string)>{
                        ("opcion_Cancelar", "Reiniciar", "Volver al menú principal"),
                        ("opcion_Finalizar_Factura", "Regresar", "Volver a ver PDF")
                    }));
                    messageStatus = await _messageSendService.EnviarListaDeOpciones( fromPhoneNumber, "Productos en la factura",
                        "Selecciona el producto a quitar", $"Total: {productosEnFactura.Count}", "Ver productos", listaSeccionesProductos );

                    _receivedMessages.Add($"Lista de opciones enviada");

                    break;
                case "opcion_Finalizar_Modificar_Info_Adicional":
                    // implementar lógica para modificar la información adicional
                    break;
                default:
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Opción no reconocida.");
                    messageStatus = await MainMenuButton(fromPhoneNumber, null);
                    break;
            }

            _receivedMessages.Add($"Updating _lastMessageState");
            UpdateLastMessageState(fromPhoneNumber, selectedButtonId);

            return messageStatus;
        }
        private async Task<string> HandleInteractiveListMessage(//Método puramente representativo
            WebHookResponseModel webhookData,
            string selectedListId,
            string fromPhoneNumber)
        {
            //string userMessage = webhookData.entry[0].changes[0].value.messages[0].text.body;
            string messageStatus = "";
            _receivedMessages.Add($"Selecting List path");
            switch (selectedListId)
            {
                case "opcion_Registrar_Cliente":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1584870855544061", $"opcionRegistrarCliente{fromPhoneNumber}", "published", $"Registrando Cliente", "Registrar");
                    UpdateLastMessageState(fromPhoneNumber, selectedListId);
                    _receivedMessages.Add($"List opcion_Registrar_Cliente");
                    return messageStatus;
                case "opcion_Configurar_Productos_agregar":
                    _receivedMessages.Add($"opcion_Configurar_Productos_agregar");
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1142951587576244", $"opcionCrearProductoFlow{fromPhoneNumber}", "published", $"Creando Producto", "Crear");
                    return messageStatus;
            }
            switch (GetLastMessageState(fromPhoneNumber))
            {
                case "opcion_Generar_Factura_Cliente":

                    //messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1414924193269074",/*"682423707677994", */$"opcionFacturar{fromPhoneNumber}", "published", "Cliente Identificado, ¿Deseas facturar?", "Facturar");

                    //_botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Observaciones_Y_Mail ", "Continuar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                    //messageStatus = messageStatus + await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Que deseas hacer", "Selecciona opción", "¿Regrezar al inicio o continuar?", _botones);
                    
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Favor de indicar nombre del producto a añadir a la factura");
                    UpdateNITCliente(fromPhoneNumber, selectedListId);
                    UpdateLastMessageState(fromPhoneNumber, "opcion_Anadir_Producto");
                    UpdateOldUserMessage(fromPhoneNumber, "0");
                    _receivedMessages.Add($"List Registrado, A facturar");
                    break;
                case "opcion_Finalizar_Modificar_Cliente_Cambiar":

                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Factura", "Ver Factura"), ("opcion_Cancelar", "Reiniciar Proceso") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Cliente a cambiar modificcado", "Selecciona opción", "No puedes registrar aquí", _botones);
                    _receivedMessages.Add(messageStatus);

                    UpdateNITCliente(fromPhoneNumber, selectedListId);
                    _receivedMessages.Add($"List Registrado, A facturar");
                    break;
                case "opcion_Anadir_Producto":
                    _receivedMessages.Add($"opcion_Anadir_Producto List");
                    _receivedMessages.Add($"Producto {selectedListId} seleccionado");
                    AddProductosEnFacturaList(fromPhoneNumber, selectedListId);
                    
                    var productoAUsar = _productos.FirstOrDefault(p =>
                        !string.IsNullOrWhiteSpace(p.Agregar_Producto_Codigo) &&
                        p.Agregar_Producto_Codigo.Equals(selectedListId, StringComparison.OrdinalIgnoreCase));

                    if (productoAUsar == null || string.IsNullOrWhiteSpace(productoAUsar.Agregar_Producto_Precio_Unitario))
                    {
                        _receivedMessages.Add($"precioUnitario null or empty");
                        UpdateRestart(fromPhoneNumber);
                        messageStatus = await MainMenuButton(fromPhoneNumber, "Error al añadir Precio Unitario");
                        _receivedMessages.Add(messageStatus);
                        break;
                    }

                    string precioString = productoAUsar.Agregar_Producto_Precio_Unitario;

                    if (float.TryParse(precioString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsedFloat))
                    {
                        var parameters = new { Precio_Unitario = parsedFloat, Precio_String = precioString };
                        messageStatus = await _messageSendService.EnviarFlowConDatos(fromPhoneNumber, "651685330992197", $"opcionFacturar{fromPhoneNumber}", "published", "Producto Identificado, proporciona Monto unitario y cantidad de unidades del producto", "Llenar", parameters, "DETAILS");
                        _receivedMessages.Add($"List Registrado, A facturar");
                        break;
                    }
                    _receivedMessages.Add($"[Error] Couldn't parse");

                    _receivedMessages.Add($"No Old Precio Unitario");
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "624251470432760", $"opcionFacturar{fromPhoneNumber}", "published", "Producto Identificado, proporciona Monto unitario y cantidad de unidades del producto", "Llenar");

                    _receivedMessages.Add($"List Registrado, A facturar");
                    break;
                case "opcion_Finalizar_Modificar_Productos_Anadir":
                    _receivedMessages.Add($"opcion_Finalizar_Modificar_Productos_Anadir List");
                    _receivedMessages.Add($"Producto {selectedListId} seleccionado");
                    AddProductosEnFacturaList(fromPhoneNumber, selectedListId);

                    var productoAUsarMod = _productos.FirstOrDefault(p =>
                        !string.IsNullOrWhiteSpace(p.Agregar_Producto_Codigo) &&
                        p.Agregar_Producto_Codigo.Equals(selectedListId, StringComparison.OrdinalIgnoreCase));

                    if (productoAUsarMod == null || string.IsNullOrWhiteSpace(productoAUsarMod.Agregar_Producto_Precio_Unitario))
                    {
                        _receivedMessages.Add($"precioUnitario null or empty");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Productos_Anadir", "Volver a intentar"), ("opcion_Cancelar", "Reiniciar Proceso"), ("opcion_Finalizar_Modificar_Factura", "Cancelar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Producto no encontrado", "Selecciona opción", "No puedes crear aquí", _botones);
                        _receivedMessages.Add(messageStatus);
                        break;
                    }

                    string precioStringMod = productoAUsarMod.Agregar_Producto_Precio_Unitario;

                    if (float.TryParse(precioStringMod, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsedFloatMod))
                    {
                        var parameters = new { Precio_Unitario = parsedFloatMod, Precio_String = precioStringMod };
                        messageStatus = await _messageSendService.EnviarFlowConDatos(fromPhoneNumber, "651685330992197", $"opcionFacturar{fromPhoneNumber}", "published", "Producto Identificado, proporciona Monto unitario y cantidad de unidades del producto", "Llenar", parameters, "DETAILS");
                        _receivedMessages.Add($"List Registrado, A facturar");
                        break;
                    }
                    _receivedMessages.Add($"[Error] Couldn't parse");

                    _receivedMessages.Add($"No Old Precio Unitario");
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "624251470432760", $"opcionFacturar{fromPhoneNumber}", "published", "Producto sin precio Identificado, proporciona Monto unitario y cantidad de unidades del producto", "Llenar");

                    _receivedMessages.Add($"List Registrado, A facturar");
                    break;
                case "opcion_Configurar_Productos_Modificar":
                    _receivedMessages.Add($"opcion_Configurar_Productos_Modificar List");
                    _receivedMessages.Add($"Producto {selectedListId} seleccionado");

                    var productoAModificar = _productos.FirstOrDefault(p =>
                        !string.IsNullOrWhiteSpace(p.Agregar_Producto_Codigo) &&
                        p.Agregar_Producto_Codigo.Equals(selectedListId, StringComparison.OrdinalIgnoreCase));

                    if (productoAModificar == null)
                    {
                        _receivedMessages.Add($"No existe el producto");
                        UpdateRestart(fromPhoneNumber);
                        messageStatus = await MainMenuButton(fromPhoneNumber, "Índice de producto no válido");
                        _receivedMessages.Add(messageStatus);
                        break;
                    }
                    _receivedMessages.Add($"Producto {productoAModificar.Agregar_Producto_Codigo} seleccionado");

                    string nombreProducto = productoAModificar.Agregar_Producto_Nombre ?? "Nombre del producto";
                    _receivedMessages.Add($"Nombre del producto: {nombreProducto}");

                    _producto_a_modificar[fromPhoneNumber] = (selectedListId, nombreProducto);

                    string unidadMedidaOriginal = productoAModificar.Agregar_Producto_Unidad_Medida ?? "Unidad de medida";
                    var mapeoUnidades = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        { { "EA", "CADA" }, { "94", "UNIDAD" }, { "Unidad de medida", "Unidad de medida" } };
                    string UnidadMedida = mapeoUnidades.TryGetValue(unidadMedidaOriginal, out string? valorTraducido)
                        ? valorTraducido
                        : "Unidad de medida";

                    string productoActivoOriginal = productoAModificar.Agregar_Producto_Activo ?? "Activo";
                    var mapeoActivo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        { { "Activar", "Activo activado" }, { "Desactivar", "Activo desactivado" }, { "Activo", "Activo" } };
                    string productoActivo = mapeoActivo.TryGetValue(productoActivoOriginal, out string? valorTraducido2)
                        ? valorTraducido2
                        : "Activo";

                    string productoImpuestoOriginal = productoAModificar.Agregar_Producto_traslados ?? "IVA";
                    var mapeoImpuesto = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        { { "0", "Impuesto sobre ventas - IVA" }, { "1", "No responsable de IVA" }, { "IVA", "IVA" } };
                    string productoImpuesto = mapeoImpuesto.TryGetValue(productoImpuestoOriginal, out string? valorTraducido3)
                        ? valorTraducido3
                        : "IVA";

                    string productoImpuestoSaludablesOriginal = productoAModificar.Agregar_Producto_Activo ?? "Impuestos Saludables";
                    var mapeoImpuestoSaludables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        { { "Activar", "Impuesto Saludables activado" }, { "Desactivar", "Impuesto Saludables desactivado" }, { "Impuestos Saludables", "Impuestos Saludables" } };
                    string productoImpuestoSaludables = mapeoImpuestoSaludables.TryGetValue(productoActivoOriginal, out string? valorTraducido4)
                        ? valorTraducido4
                        : "Impuestos Saludables";

                    var parameters2 = new
                    {
                        Modificar_Producto_Codigo = selectedListId,
                        Modificar_Producto_Nombre = nombreProducto ?? "Nombre del producto",
                        Modificar_Producto_Precio_Unitario = productoAModificar.Agregar_Producto_Precio_Unitario ?? "Precio Unitario",
                        Modificar_Producto_Info_Adicional = productoAModificar.Agregar_Producto_Info_Adicional ?? "Info",
                        Modificar_Producto_Unidad_Medida = UnidadMedida,
                        Modificar_Producto_Activo = productoActivo,
                        Modificar_Producto_traslados = productoAModificar.Agregar_Producto_traslados ?? "Impuestos traslados",
                        Modificar_Producto_Impuesto = productoImpuesto,
                        Modificar_Producto_Tasa_cuota = productoAModificar.Agregar_Producto_Tasa_cuota ?? "Tasa o cuota",
                        Modificar_Producto_Impuestos_Saludables = productoImpuestoSaludables
                    };
                    
                    messageStatus = await _messageSendService.EnviarFlowConDatos(fromPhoneNumber, "572042871927108", $"opcionFacturarConfigurarProductosModificar{fromPhoneNumber}", "published", "Producto Identificado", "Modificar", parameters2, "Modificar_Producto_Info_Base");

                    
                    //EnviarFlowConDatos

                    //messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "572042871927108", $"opcionFacturar{fromPhoneNumber}", "published", "Producto Identificado", "Modificar");

                    _receivedMessages.Add(messageStatus);
                    _receivedMessages.Add("Mandando a 572042871927108");
                    break;
                case "opcion_Finalizar_Modificar_Productos_Quitar":
                    _receivedMessages.Add($"opcion_Finalizar_Modificar_Productos_Quitar List");
                    _receivedMessages.Add($"Producto {selectedListId} seleccionado");

                    var productosEnFactura = GetProductosEnFacturaList(fromPhoneNumber);

                    if (!productosEnFactura.Contains(selectedListId))
                    {
                        _receivedMessages.Add($"Producto no encontrado en factura: {selectedListId}");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Productos_Quitar", "Volver a intentar"), ("opcion_Finalizar_Factura", "Regresar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, $"Producto {selectedListId} no encontrado en factura.", "Información sigue Guardada", "Hay 3 opciones", _botones);
                        break;
                    }

                    var producto = _productos.FirstOrDefault(p =>
                        p.Agregar_Producto_Codigo?.Equals(selectedListId, StringComparison.OrdinalIgnoreCase) ?? false);
                    string nombreProductoQuitar = producto?.Agregar_Producto_Nombre ?? "Producto sin nombre";

                    productosEnFactura.Remove(selectedListId);
                    _receivedMessages.Add($"Producto quitado: {selectedListId}");

                    //messageStatus = await _messageSendService.EnviarTexto( fromPhoneNumber, $"Producto eliminado: *{nombreProductoQuitar}*" );

                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Productos_Quitar", "Quitar otro"), ("opcion_Finalizar_Factura", "Continuar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, $"Producto eliminado: *{nombreProductoQuitar}*", "Información sigue Guardada", "Hay 3 opciones", _botones);

                    _receivedMessages.Add(messageStatus);

                    break;
            }
            return messageStatus;
        }

        private async Task<string> HandleTextMessage(
            WebHookResponseModel webhookData, 
            string fromPhoneNumber)
        {
            string messageStatus = "";
            string userMessage = webhookData.entry[0].changes[0].value.messages[0].text.body;
            if (userMessage == "Reinicio")
            {
                _receivedMessages.Add($"Reiniciando para: {fromPhoneNumber}");
                UpdateRestart(fromPhoneNumber);
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¡Reiniciando todo!");
                _receivedMessages.Add(messageStatus);
                messageStatus = await MainMenuButton(fromPhoneNumber, "Reinicio");
                _receivedMessages.Add(messageStatus);
                return messageStatus;
            }
            if (userMessage == "Forzar Finalizar Finalizar")
            {
                _receivedMessages.Add("Forzar Finalizar Finalizar");
                UpdateRestart(fromPhoneNumber);

                string nombreComprador = "ejemplo prueba";
                string nitComprador = "15083930";
                string nitCliente = "Amigo";
                string monto= "500";
                string telefono = "5526";
                string direccion = "No";
                string descripcion = "Puede ser";
                string observaciones = "Observacion"; 


                var productoEjemeplo = new Flow_response_json_Model_1142951587576244_Crear_Producto
                {
                    flow_token = "token-ejemplo-123",
                    Agregar_Producto_Nombre = "Pan Integral",
                    Agregar_Producto_Precio_Unitario = "25",
                    Agregar_Producto_Info_Adicional = "500g, hecho con harina integral",
                    Agregar_Producto_Codigo = "PANINT500",
                    Agregar_Producto_Unidad_Medida = "pieza",
                    Agregar_Producto_Activo = "true",
                    Agregar_Producto_traslados = "incluye",
                    Agregar_Producto_Impuesto = "IVA",
                    Agregar_Producto_Tasa_cuota = "16",
                    Agregar_Producto_Impuestos_Saludables = "Ninguno",
                    Agregar_Producto_Impuestos_Saludables2 = new List<string> { "Ninguno" }
                };
                _productos.Add(productoEjemeplo);

                AddProductosEnFacturaList(fromPhoneNumber, "PANINT500");
                var AddProductosPrecioYCantidadYPrecio = new Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad
                {
                    flow_token = "SuperEjemplo",
                    PUyC_Cantidad = "5",
                    PUyC_Precio_Unitario = "4"
                };
                AddProductosPrecioYCantidad(fromPhoneNumber, AddProductosPrecioYCantidadYPrecio);

                var productos = _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.ListaProductos : new List<string>();
                var preciosYCantidades = GetPrecioUnitarioYCantidad(fromPhoneNumber);

                string resumen = $"*Resumen del Formulario Recibido:*\n\n";

                resumen += $"*Comprador:* {nombreComprador}, NIT: {nitComprador}\n";


                resumen += $"\n*Información de Factura:*\n";
                resumen += $"📍 *Dirección:* {direccion}\n";
                resumen += $"📞 *Teléfono:* {telefono}\n";
                resumen += $"📝 *Descripción:* {descripcion}\n";
                resumen += $"💰 *Monto Total:* {monto}\n";


                resumen += $"*Cliente:* {nitCliente}\n";

                if (productos.Any())
                {
                    resumen += $"\n*Productos:*\n";
                    for (int i = 0; i < productos.Count; i++)
                    {
                        var productoCodigo = productos[i]; // invertir codigo y nombre si se usan al revés
                        var producto = _productos.FirstOrDefault(p => p.Agregar_Producto_Codigo == productoCodigo);
                        var precioCantidad = i < preciosYCantidades.Count ? preciosYCantidades[i] : null;
                        var productoNombre = producto?.Agregar_Producto_Nombre ?? "Nombre no encontrado";

                        resumen += $"- {productoNombre}, {productoCodigo}";
                        if (precioCantidad != null)
                        {
                            resumen += $" | Cantidad: {precioCantidad.PUyC_Cantidad}, Precio Unitario: {precioCantidad.PUyC_Precio_Unitario}";
                        }
                        resumen += "\n";
                    }
                }
                else
                {
                    resumen += "\n*Productos:* No se registraron productos.\n";
                }

                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, resumen);
                _receivedMessages.Add("Resumen mandado");
                _receivedMessages.Add(messageStatus);

                var productosCompletos = new List<Flow_response_json_Model_1142951587576244_Crear_Producto>();

                for (int i = 0; i < productos.Count; i++)
                {
                    var codigo = productos[i];
                    var producto = _productos.FirstOrDefault(p => p.Agregar_Producto_Codigo == codigo);
                    if (producto != null)
                    {
                        productosCompletos.Add(producto);
                        _receivedMessages.Add("Producto añadido a lista");
                    }
                }

                var JSONtoSend = _manejoDeComprador.CrearFacturaJson(nombreComprador, nitComprador, nitCliente, observaciones, productosCompletos, preciosYCantidades, correosACopiar);
                _receivedMessages.Add("JSONtoSend: ");
                _receivedMessages.Add(JSONtoSend);
                _receivedMessages.Add("--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, JSONtoSend);
                _receivedMessages.Add("JSONtoSend mandado");
                _receivedMessages.Add(messageStatus);

                messageStatus = await _manejoDeComprador.MandarFacturaJson(JSONtoSend);

                _receivedMessages.Add("JSON mandado");
                _receivedMessages.Add(messageStatus);

                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, messageStatus);

                _receivedMessages.Add("Status message");
                _receivedMessages.Add(messageStatus);

                UpdateRestart(fromPhoneNumber);

                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Final Forzado Final");
                _receivedMessages.Add("Final Forzado Final");

                //_botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Finalizar_Factura", "Ejemplo"), ("opcion_Cancelar", "Cancelar") };
                //messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Preparando ejemplo", "¿Usar ejemplo?", "¿O regrezar?", _botones);

                return messageStatus;
            }
            switch (GetLastMessageState(fromPhoneNumber))
            {
                case "":
                    _receivedMessages.Add("Inicio");

                    if (userMessage == GetOldUserMessage(fromPhoneNumber))
                        break;

                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¡Hola!");

                    _receivedMessages.Add("Mensaje inicial: " + messageStatus);
                    messageStatus = messageStatus + await MainMenuButton(fromPhoneNumber, null);

                    _receivedMessages.Add("Mensaje: " + userMessage + " De: " + fromPhoneNumber);
                    _receivedMessages.Add(messageStatus);

                    break;
                case "opcion_Iniciar_Sesión":

                    _receivedMessages.Add($"Text opcion_Iniciar_Sesión");

                    if (string.IsNullOrWhiteSpace(userMessage) || GetOldUserMessage(fromPhoneNumber) == userMessage)
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Por favor, envía un VALOR válido.");
                        break;
                    }

                    (string, string)? compradorExistance = _manejoDeComprador.CompradorExisteKeys(userMessage);
                    if (compradorExistance != null)
                    {
                        _receivedMessages.Add($"Comprador {compradorExistance.Value.Item1} existe");

                        string añadirTelEstado = _manejoDeComprador.AñadirTeléfonoAComprador(fromPhoneNumber, compradorExistance.Value);
                        _receivedMessages.Add(añadirTelEstado);

                        if (añadirTelEstado.StartsWith("Teléfono"))
                        {
                            messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, $"{fromPhoneNumber} registrado en usuario {compradorExistance.Value.Item1} ");
                            _receivedMessages.Add(messageStatus);
                        }
                        UpdateNITComprador(fromPhoneNumber, compradorExistance.Value.Item2);
                        UpdateNombreComprador(fromPhoneNumber, compradorExistance.Value.Item1);
                        messageStatus = await MainMenuButton(fromPhoneNumber, "Usuario reconocido");
                        _receivedMessages.Add("Usuario reconocido");
                        _receivedMessages.Add(messageStatus);
                    }
                    else 
                    {
                        messageStatus = messageStatus + await MainMenuButton(fromPhoneNumber, "Usuario no reconocido");
                        _receivedMessages.Add("Mensaje iniciar sesión no reconocido: " + messageStatus);
                    }
                    break;
                case "opcion_Generar_Factura_Text":

                    _receivedMessages.Add($"Text opcion_Generar_Factura_Text");
                    if (string.IsNullOrWhiteSpace(userMessage) || GetOldUserMessage(fromPhoneNumber) == userMessage)
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Por favor, envía un VALOR válido.");
                        break;
                    }

                    (string, string)? phoneExistance = _manejoDeComprador.CompradorExisteKeys(userMessage);
                    if (phoneExistance != null)
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, $"{phoneExistance.Value.Item1} - {phoneExistance.Value.Item2}\n¿A quién facturarás?\nPuedes escribir su NIT, Nombre o una parte de estos");
                        UpdateLastMessageState(fromPhoneNumber, "opcion_Generar_Factura_Cliente");
                        UpdateNombreComprador(fromPhoneNumber, phoneExistance.Value.Item1);
                        UpdateNITComprador(fromPhoneNumber, phoneExistance.Value.Item2);
                        _receivedMessages.Add($"Text Nit Reconocido");
                    }
                    else
                    {
                        _receivedMessages.Add($"Text Nit No Reconocido");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Generar_Factura_Registrar_Persona_Flow", "Persona física"), ("opcion_Generar_Factura_Registrar_Empresa_Flow", "Empresa"), ("opcion_Cancelar", "Cancelar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "NIT no registrado, por favor completa tu registro", "Selecciona opción", "2 opciones para registrar", _botones);
                        _receivedMessages.Add($"Button message: {messageStatus}");
                    }
                    break;
                case "opcion_Generar_Factura_Cliente":
                    _receivedMessages.Add($"Text opcion_Generar_Factura_Cliente");
                    if (!string.IsNullOrWhiteSpace(userMessage) && GetOldUserMessage(fromPhoneNumber) != userMessage)
                    {
                        List<Flow_response_json_Model_1584870855544061_CrearCliente> clientExistance = _manejoDeComprador.BuscarListaClientes2(GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber), userMessage );
                        _receivedMessages.Add($"Hay lista de clientes? {clientExistance.ToString()}");
                        if (!clientExistance.Any()) // revisa si hay clientes en la lista
                        {
                            _receivedMessages.Add("No existe Cliente en comprador");
                            _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Registrar_Cliente", "Registrar Cliente"), ("opcion_Generar_Factura_Cliente", "Volver a intentar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                            messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "No encontré este NIT en tus clientes", "Selecciona opción", "Lo vaz a registrar?", _botones);
                            _receivedMessages.Add(messageStatus);
                            UpdateOldUserMessage(fromPhoneNumber, "No existe Cliente en comprador");
                            break;
                        }

                        _receivedMessages.Add($"Text opcion_Generar_Factura_Cliente Cliente existe");
                        // Filtro de clientes con NIT no vacío y sin duplicados
                        var clientesConNIT = clientExistance
                            .Where(c => !string.IsNullOrWhiteSpace(c.RegisterClient_Natural_NIT) || 
                                !string.IsNullOrWhiteSpace(c.RegisterClient_Juridical_NIT))
                            .GroupBy(c => 
                                !string.IsNullOrWhiteSpace(c.RegisterClient_Natural_NIT)
                                ? c.RegisterClient_Natural_NIT
                                : c.RegisterClient_Juridical_NIT)
                            .Select(g => g.First())
                            .ToList();

                        _receivedMessages.Add($"Hay lista de clientes con NIT? Count={clientesConNIT.Count}");

                        // Validación: si no hay clientes con NIT, ofrecer registrar
                        if (!clientesConNIT.Any())
                        {
                            _receivedMessages.Add("No existe Cliente con NIT en comprador");
                            _botones = new List<(string ButtonId, string ButtonLabelText)>
                            {
                                ("opcion_Registrar_Cliente", "Registrar Cliente"), ("opcion_Generar_Factura_Cliente", "Volver a intentar"), ("opcion_Cancelar", "Reiniciar Proceso")
                            };

                            messageStatus = await _messageSendService.EnviarBotonInteractivo(
                                fromPhoneNumber,
                                "No encontré este NIT en tus clientes",
                                "Selecciona opción",
                                "Lo vaz a registrar?",
                                _botones
                            );

                            _receivedMessages.Add(messageStatus);
                            UpdateOldUserMessage(fromPhoneNumber, "No existe Cliente en comprador");
                            break;
                        }

                        // Dividir clientes en sets de 10
                        var totalClients = clientesConNIT.Count;
                        int totalSetsF = (int)Math.Ceiling(totalClients / 10.0);
                        var listaSeccionesClientes = new List<(string, List<(string, string, string)>)>();

                        if (totalSetsF > 9)
                        {
                            _receivedMessages.Add("Demasiadas opciones");
                            messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Hay demasiadas opciones, favor de ser más específico");
                            break;
                        }

                        // Generar secciones
                        for (int i = 0; i < totalSetsF; i++)
                        {
                            string setTitle = $"Set {i + 1}";


                            var opciones = clientesConNIT
                                .Skip(i * 10)
                                .Take(10)
                                .Select((client, index) => {
                                    // Obtener NIT prioritario
                                    string nit = client.RegisterClient_Natural_NIT
                                              ?? client.RegisterClient_Juridical_NIT
                                              ?? $"NIT_{i}_{index}";

                                    // Si es cliente natural
                                    string nombreCompleto; if (!string.IsNullOrWhiteSpace(client.RegisterClient_Natural_Nombre))
                                    {
                                        nombreCompleto = string.Join(" ", new[]
                                        {
                                            client.RegisterClient_Natural_Nombre,
                                            client.RegisterClient_Natural_Apellido_Paterno,
                                            client.RegisterClient_Natural_Apellido_Materno
                                        }.Where(s => !string.IsNullOrWhiteSpace(s)));
                                    }
                                    else
                                    {
                                        // Cliente jurídico
                                        nombreCompleto = client.RegisterClient_Juridical_Razon_Social?.Trim() ?? "";
                                    }

                                    // Limitar a 72 caracteres
                                    if (nombreCompleto.Length > 72)
                                    {
                                        nombreCompleto = nombreCompleto.Substring(0, 69).TrimEnd() + "...";
                                    }

                                    return (
                                        nit,
                                        (i * 10 + index + 1).ToString(),
                                        nombreCompleto
                                    );


                                })
                                .ToList();

                            listaSeccionesClientes.Add((setTitle, opciones));
                        }

                        // Agregar opción de registrar cliente al final
                        listaSeccionesClientes.Add((
                            "Other",
                            new List<(string, string, string)>
                            {
                                ("opcion_Registrar_Cliente", "Registrar", "Elegir opción para registrar nuevo cliente")
                            }
                        ));

                        // Enviar la lista por WhatsApp
                        messageStatus = await _messageSendService.EnviarListaDeOpciones(fromPhoneNumber, "Lista de Clientes", "Seleccionar una coincidencia o mandar nombre diferente", $"Hay {totalClients} coincidencias", "Expandir opciones", listaSeccionesClientes);
                        _receivedMessages.Add(messageStatus);
                    }
                    else
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Por favor, envía un VALOR válido.");
                        _receivedMessages.Add("Valor no válido");
                    }
                    break;

                case "opcion_Finalizar_Modificar_Cliente_Cambiar":
                    _receivedMessages.Add($"Text opcion_Finalizar_Modificar_Cliente_Cambiar");
                    if (!string.IsNullOrWhiteSpace(userMessage) && GetOldUserMessage(fromPhoneNumber) != userMessage)
                    {
                        List<Flow_response_json_Model_1584870855544061_CrearCliente> clientExistance = _manejoDeComprador.BuscarListaClientes2(GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber), userMessage);
                        _receivedMessages.Add($"Hay lista de clientes? {clientExistance.ToString()}");
                        if (!clientExistance.Any()) // revisa si hay clientes en la lista
                        {
                            _receivedMessages.Add("No existe Cliente en comprador");
                            _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Factura", "Regresar"), ("opcion_Finalizar_Modificar_Cliente_Cambiar", "Volver a intentar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                            messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "No encontré este NIT en tus clientes", "Selecciona opción", "No puedes registrar aquí", _botones);
                            _receivedMessages.Add(messageStatus);
                            UpdateOldUserMessage(fromPhoneNumber, "No existe Cliente en comprador");
                            break;
                        }

                        _receivedMessages.Add($"Text opcion_Finalizar_Modificar_Cliente_Cambiar Cliente existe");
                        // Filtro de clientes con NIT no vacío y sin duplicados
                        var clientesConNIT = clientExistance
                            .Where(c => !string.IsNullOrWhiteSpace(c.RegisterClient_Natural_NIT) ||
                                !string.IsNullOrWhiteSpace(c.RegisterClient_Juridical_NIT))
                            .GroupBy(c =>
                                !string.IsNullOrWhiteSpace(c.RegisterClient_Natural_NIT)
                                ? c.RegisterClient_Natural_NIT
                                : c.RegisterClient_Juridical_NIT)
                            .Select(g => g.First())
                            .ToList();

                        _receivedMessages.Add($"Hay lista de clientes con NIT? Count={clientesConNIT.Count}");


                        if (!clientesConNIT.Any())
                        {
                            _receivedMessages.Add("No existe Cliente con NIT en comprador");
                            _botones = new List<(string ButtonId, string ButtonLabelText)>
                            {
                                ("opcion_Finalizar_Modificar_Factura", "Regresar"), ("opcion_Finalizar_Modificar_Cliente_Cambiar", "Volver a intentar"), ("opcion_Cancelar", "Reiniciar Proceso")
                            };

                            messageStatus = await _messageSendService.EnviarBotonInteractivo(
                                fromPhoneNumber, "No encontré este NIT en tus clientes",
                                "Selecciona opción", "No puedes registrar aquí", _botones
                            );

                            _receivedMessages.Add(messageStatus);
                            UpdateOldUserMessage(fromPhoneNumber, "No existe Cliente en comprador");
                            break;
                        }

                        // Dividir clientes en sets de 10
                        var totalClients = clientesConNIT.Count;
                        int totalSetsF = (int)Math.Ceiling(totalClients / 10.0);
                        var listaSeccionesClientes = new List<(string, List<(string, string, string)>)>();

                        if (totalSetsF > 9)
                        {
                            _receivedMessages.Add("Demasiadas opciones");
                            messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Hay demasiadas opciones, favor de ser más específico");
                            break;
                        }

                        // Generar secciones
                        for (int i = 0; i < totalSetsF; i++)
                        {
                            string setTitle = $"Set {i + 1}";


                            var opciones = clientesConNIT
                                .Skip(i * 10)
                                .Take(10)
                                .Select((client, index) => {
                                    // Obtener NIT prioritario
                                    string nit = client.RegisterClient_Natural_NIT
                                              ?? client.RegisterClient_Juridical_NIT
                                              ?? $"NIT_{i}_{index}";

                                    // Si es cliente natural
                                    string nombreCompleto; if (!string.IsNullOrWhiteSpace(client.RegisterClient_Natural_Nombre))
                                    {
                                        nombreCompleto = string.Join(" ", new[]
                                        {
                                            client.RegisterClient_Natural_Nombre,
                                            client.RegisterClient_Natural_Apellido_Paterno,
                                            client.RegisterClient_Natural_Apellido_Materno
                                        }.Where(s => !string.IsNullOrWhiteSpace(s)));
                                    }
                                    else
                                    {
                                        // Cliente jurídico
                                        nombreCompleto = client.RegisterClient_Juridical_Razon_Social?.Trim() ?? "";
                                    }

                                    // Limitar a 72 caracteres
                                    if (nombreCompleto.Length > 72)
                                    {
                                        nombreCompleto = nombreCompleto.Substring(0, 69).TrimEnd() + "...";
                                    }

                                    return (
                                        nit,
                                        (i * 10 + index + 1).ToString(),
                                        nombreCompleto
                                    );


                                })
                                .ToList();

                            listaSeccionesClientes.Add((setTitle, opciones));
                        }

                        // Agregar opción de registrar cliente al final
                        /*listaSeccionesClientes.Add((
                            "Other",
                            new List<(string, string, string)>
                            {
                                ("opcion_Registrar_Cliente", "Registrar", "Elegir opción para registrar nuevo cliente")
                            }
                        ));*/

                        // Enviar la lista por WhatsApp
                        messageStatus = await _messageSendService.EnviarListaDeOpciones(fromPhoneNumber, "Lista de Clientes", "Seleccionar una coincidencia o mandar nombre diferente", $"Hay {totalClients} coincidencias", "Expandir opciones", listaSeccionesClientes);
                        _receivedMessages.Add(messageStatus);
                    }
                    else
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Por favor, envía un VALOR válido.");
                        _receivedMessages.Add("Valor no válido");
                    }
                    break;
                case "opcion_Agregar_Clientes":// Corregir e implementar ListaDeCoincidenciasCompradores

                    (string, string)? compradorExistanceCli = _manejoDeComprador.CompradorExisteKeys(userMessage);

                    if (compradorExistanceCli != null)
                    {
                        UpdateNITComprador(fromPhoneNumber, compradorExistanceCli.Value.Item2);
                        UpdateNombreComprador(fromPhoneNumber, compradorExistanceCli.Value.Item1);
                        UpdateLastMessageState(fromPhoneNumber, "opcion_Agregar_Clientes");
                        messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1584870855544061", $"opcionAgregarClienteFlow{fromPhoneNumber}", "published", $"Registrando Cliente", "Registrar");
                    }
                    else
                    {
                        //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Nombre o NIT no reconocido, favor de volver a intentar");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> {("opcion_Agregar_Clientes", "Reintentar"), ("opcion_Cancelar", "Cancelar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Nombre o NIT de comprador no reconocido", "Selecciona", "Hay 2 opciones", _botones);
                    }
                        break;
                case "opcion_Modificar_Clientes":
                    (string, string)? compradorExistanceModCli = _manejoDeComprador.CompradorExisteKeys(userMessage);

                    if (compradorExistanceModCli != null)
                    {
                        UpdateNITComprador(fromPhoneNumber, compradorExistanceModCli.Value.Item2);
                        UpdateNombreComprador(fromPhoneNumber, compradorExistanceModCli.Value.Item1);
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Modificar_Clientes_Persona_Natural", "Natural"), ("opcion_Modificar_Clientes_Persona_Juridica", "Jurídica"), ("opcion_Cancelar", "Cancelar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que tipo de persona es tu cliente?", "Selecciona opción", "Hay 2 opciones", _botones);
                    }
                    else
                    {
                        //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Nombre o NIT no reconocido, favor de volver a int
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Agregar_Clientes", "Crear"), ("opcion_Modificar_Clientes", "Reintentar"), ("opcion_Cancelar", "Cancelar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Nombre o NIT de cliente no reconocido", "O no existe en comprador", "Hay 3 opciones", _botones);
                    }
                    break;
                case "opcion_Modificar_Clientes_Persona_Natural":
                    if (_manejoDeComprador.ClienteExisteEnComprador(userMessage, GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber)))
                    {
                        messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "931945452349522", $"opcionModCliNaturalFlow{fromPhoneNumber}", "published", $"Modificando Cliente", "Modificar");
                    }
                    else
                    {
                        //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Nombre o NIT no reconocido, favor de volver a intentar");

                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Modificar_Clientes_Persona_Natural", "Reintentar"), ("opcion_Cancelar", "Cancelar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Nombre o NIT de cliente no reconocido", "O no existe en comprador", "Hay 2 opciones", _botones);
                    }
                    UpdateLastMessageState(fromPhoneNumber, "opcion_Modificar_Clientes_Persona_Natural");
                    break;
                case "opcion_Modificar_Clientes_Persona_Juridica":
                    if (_manejoDeComprador.ClienteExisteEnComprador(userMessage,GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber)))
                    {
                        messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1378725303264167", $"opcionModCliJuridicaFlow{fromPhoneNumber}", "published", $"Modificando Cliente", "Modificar");
                    }
                    else
                    {
                        //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Nombre o NIT no reconocido, favor de volver a intentar");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Modificar_Clientes_Persona_Juridica", "Reintentar"), ("opcion_Cancelar", "Cancelar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Nombre o NIT de cliente no reconocido", "O no existe en comprador", "Hay 2 opciones", _botones);
                    }
                    UpdateLastMessageState(fromPhoneNumber, "opcion_Modificar_Clientes_Persona_Juridica");
                    break;
                case "opcion_Configurar_Productos_Modificar":
                    _receivedMessages.Add($"Text opcion_Configurar_Productos_Modificar");
                    if (string.IsNullOrWhiteSpace(userMessage) || !_productos.Any(c =>
                        (!string.IsNullOrWhiteSpace(c.Agregar_Producto_Nombre) && c.Agregar_Producto_Nombre.Contains(userMessage, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(c.Agregar_Producto_Codigo) && c.Agregar_Producto_Codigo.Contains(userMessage, StringComparison.OrdinalIgnoreCase))))
                    {
                        _receivedMessages.Add("opcion_Configurar_Productos_Modificar, producto no encontrado");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Configurar_Productos_agregar", "Crear"), ("opcion_Configurar_Productos_Modificar", "Reintentar"), ("opcion_Cancelar", "Cancelar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Producto no encontrado, ¿Deseas reintentar con otro nombre?", "¿o crear un producto?", "Reintentar o Crear", _botones);
                        break;
                    }

                    _receivedMessages.Add($"Producto a mod:{userMessage}");

                    string productoPiesaMod = userMessage.ToLower();

                    var productosFiltradosMod = _productos.Where(p =>
                        (!string.IsNullOrWhiteSpace(p.Agregar_Producto_Nombre) && p.Agregar_Producto_Nombre.ToLower().Contains(productoPiesaMod)) ||
                        (!string.IsNullOrWhiteSpace(p.Agregar_Producto_Codigo) && p.Agregar_Producto_Codigo.ToLower().Contains(productoPiesaMod)))
                        .ToList();

                    if (!productosFiltradosMod.Any())
                    {
                        _receivedMessages.Add("opcion_Configurar_Productos_Modificar, producto no encontrado");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Configurar_Productos_agregar", "Crear"), ("opcion_Configurar_Productos_Modificar", "Reintentar"), ("opcion_Cancelar", "Cancelar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Producto no encontrado", "¿Deseas reintentar con otro nombre o crear un producto?", "Reintentar o Crear", _botones);
                        break;
                    }

                    int totalSetsMod = (int)Math.Ceiling(productosFiltradosMod.Count / 10.0);
                    var listaSeccionesProductosMod = new List<(string, List<(string, string, string)>)>();

                    if (totalSetsMod > 9)
                    {
                        _receivedMessages.Add("Demasiadas opciones");
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Hay demasiadas opciones, favor de ser más específico");
                        break;
                    }

                    for (int i = 0; i < totalSetsMod; i++)
                    {
                        string setTitle = $"Set {i + 1}";

                        var opciones = productosFiltradosMod
                                .Skip(i * 10)
                                .Take(10)
                                .Select((producto, index) => (
                                    producto.Agregar_Producto_Codigo ?? $"{index}", // OptionId
                                    (i * 10 + index + 1).ToString(), // OptionTitle
                                    producto.Agregar_Producto_Nombre ?? "Producto sin nombre" // OptionDescription
                                ))
                                .ToList();
                        listaSeccionesProductosMod.Add((setTitle, opciones));
                    }

                    listaSeccionesProductosMod.Add(("Otra opción", new List<(string, string, string)>{
                        ("opcion_Configurar_Productos_agregar", "Agregar", "Crear Producto")}));
                    if (productosFiltradosMod.Any())
                    {
                        messageStatus = await _messageSendService.EnviarListaDeOpciones(fromPhoneNumber, "Productos encontrados", "Selecciona un producto",
                        $"Coincidencias: {productosFiltradosMod.Count}", "Ver productos", listaSeccionesProductosMod);
                    }

                    UpdateLastMessageState(fromPhoneNumber, "opcion_Configurar_Productos_Modificar");

                    //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Producto encontrado");
                        //UpdateRestart(fromPhoneNumber);
                    break;
                case "opcion_Anadir_Producto":
                    _receivedMessages.Add($"Text opcion_Anadir_Producto");
                    if (string.IsNullOrWhiteSpace(userMessage) || !_productos.Any(c =>
                        (!string.IsNullOrWhiteSpace(c.Agregar_Producto_Nombre) && c.Agregar_Producto_Nombre.Contains(userMessage, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(c.Agregar_Producto_Codigo) && c.Agregar_Producto_Codigo.Contains(userMessage, StringComparison.OrdinalIgnoreCase))))
                    {
                        _receivedMessages.Add($"Producto no encontrado");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Anadir_Producto_agregar", "Crear"), ("opcion_Anadir_Producto", "Reintentar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Producto no encontrado, ¿Deseas reintentar con otro nombre?", "¿o crear un producto?", "Reintentar o Crear", _botones);
                        break;
                    }

                    _receivedMessages.Add($"Producto:{userMessage}");

                    string productoPiesa = userMessage.ToLower();

                    var productosFiltrados = _productos.Where(p =>
                        (!string.IsNullOrWhiteSpace(p.Agregar_Producto_Nombre) && p.Agregar_Producto_Nombre.ToLower().Contains(productoPiesa)) ||
                        (!string.IsNullOrWhiteSpace(p.Agregar_Producto_Codigo) && p.Agregar_Producto_Codigo.ToLower().Contains(productoPiesa)))
                        .ToList();
                    if (!productosFiltrados.Any())
                    {
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Anadir_Producto_agregar", "Crear"), ("opcion_Anadir_Producto", "Reintentar"), ("opcion_Cancelar", "Reiniciar Proceso") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Producto no encontrado", "¿Deseas reintentar con otro nombre o crear un producto?", "Reintentar o Crear", _botones);
                        break;
                    }

                    int totalSets = (int)Math.Ceiling(productosFiltrados.Count / 10.0);
                    var listaSeccionesProductos = new List<(string, List<(string, string, string)>)>();

                    if (totalSets > 9)
                    {
                        _receivedMessages.Add("Demasiadas opciones");
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Hay demasiadas opciones, favor de ser más específico");
                        break;
                    }

                    for (int i = 0; i < totalSets; i++)
                    {
                        string setTitle = $"Set {i + 1}";

                        var opciones = productosFiltrados
                                .Skip(i * 10)
                                .Take(10)
                                .Select((producto, index) => (
                                    producto.Agregar_Producto_Codigo ?? $"{index}", // OptionId
                                    (i * 10 + index + 1).ToString(), // OptionTitle
                                    producto.Agregar_Producto_Nombre ?? "Producto sin nombre" // OptionDescription
                                ))
                                .ToList();
                        listaSeccionesProductos.Add((setTitle, opciones));
                    }

                    listaSeccionesProductos.Add(("Otra opción", new List<(string, string, string)>{
                        ("opcion_Configurar_Productos_agregar", "Agregar", "Crear Producto")}));
                    if (productosFiltrados.Any())
                    {
                        messageStatus = await _messageSendService.EnviarListaDeOpciones(fromPhoneNumber, "Productos encontrados", "Selecciona un producto",
                        $"Coincidencias: {productosFiltrados.Count}", "Ver productos", listaSeccionesProductos);
                    }

                    UpdateLastMessageState(fromPhoneNumber, "opcion_Anadir_Producto");
                    break;
                case "opcion_Finalizar_Modificar_Productos_Anadir":
                    _receivedMessages.Add($"Text opcion_Finalizar_Modificar_Productos_Anadir");
                    if (string.IsNullOrWhiteSpace(userMessage) || !_productos.Any(c =>
                        (!string.IsNullOrWhiteSpace(c.Agregar_Producto_Nombre) && c.Agregar_Producto_Nombre.Contains(userMessage, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(c.Agregar_Producto_Codigo) && c.Agregar_Producto_Codigo.Contains(userMessage, StringComparison.OrdinalIgnoreCase))))
                    {
                        _receivedMessages.Add($"Producto no encontrado");
                        _botones = new List<(string ButtonId, string ButtonLabelText)> {("opcion_Finalizar_Modificar_Productos_Anadir", "Reintentar"), ("opcion_Cancelar", "Reiniciar Proceso"), ("opcion_Finalizar_Modificar_Factura", "Cancelar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Producto no encontrado, ¿Deseas reintentar con otro nombre?", "Hay 2 opciones", "Reintentar o Crear", _botones);
                        break;
                    }

                    _receivedMessages.Add($"Producto:{userMessage}");

                    string productoPiesaMod2 = userMessage.ToLower();

                    var productosFiltradosMod2 = _productos.Where(p =>
                        (!string.IsNullOrWhiteSpace(p.Agregar_Producto_Nombre) && p.Agregar_Producto_Nombre.ToLower().Contains(productoPiesaMod2)) ||
                        (!string.IsNullOrWhiteSpace(p.Agregar_Producto_Codigo) && p.Agregar_Producto_Codigo.ToLower().Contains(productoPiesaMod2)))
                        .ToList();
                    if (!productosFiltradosMod2.Any())
                    {
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Productos_Anadir", "Reintentar"), ("opcion_Cancelar", "Reiniciar Proceso"), ("opcion_Finalizar_Modificar_Factura", "Cancelar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Producto no encontrado", "¿Deseas reintentar con otro nombre?", "Hay 3 opciones", _botones);
                        break;
                    }

                    int totalSetsMod2 = (int)Math.Ceiling(productosFiltradosMod2.Count / 10.0);
                    var listaSeccionesProductosMod2 = new List<(string, List<(string, string, string)>)>();

                    if (totalSetsMod2 > 9)
                    {
                        _receivedMessages.Add("Demasiadas opciones");
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Hay demasiadas opciones, favor de ser más específico");
                        break;
                    }

                    for (int i = 0; i < totalSetsMod2; i++)
                    {
                        string setTitle = $"Set {i + 1}";

                        var opciones = productosFiltradosMod2
                                .Skip(i * 10)
                                .Take(10)
                                .Select((producto, index) => (
                                    producto.Agregar_Producto_Codigo ?? $"{index}", // OptionId
                                    (i * 10 + index + 1).ToString(), // OptionTitle
                                    producto.Agregar_Producto_Nombre ?? "Producto sin nombre" // OptionDescription
                                ))
                                .ToList();
                        listaSeccionesProductosMod2.Add((setTitle, opciones));
                    }

                    listaSeccionesProductosMod2.Add(("Otra opción", new List<(string, string, string)>{
                        ("opcion_Configurar_Productos_agregar", "Agregar", "Crear Producto")}));
                    if (productosFiltradosMod2.Any())
                    {
                        messageStatus = await _messageSendService.EnviarListaDeOpciones(fromPhoneNumber, "Productos encontrados", "Selecciona un producto",
                        $"Coincidencias: {productosFiltradosMod2.Count}", "Ver productos", listaSeccionesProductosMod2);
                    }

                    UpdateLastMessageState(fromPhoneNumber, "opcion_Finalizar_Modificar_Productos_Anadir");

                    break;
                default:
                    await _messageSendService.EnviarTexto(fromPhoneNumber, "Accion no reconocida");
                    messageStatus = await MainMenuButton(fromPhoneNumber, null);
                    break;
            }
            if (userMessage != GetOldUserMessage(fromPhoneNumber)) 
                UpdateOldUserMessage(fromPhoneNumber, userMessage ?? "0");// OldUserMessage se vuelve el último mensaje o 0 si fue null
            _receivedMessages.Add($"Retornando: {messageStatus}");
            UpdateOldUserMessage(fromPhoneNumber, "0");// eliminar más adelante
            return messageStatus;
        }
        private async Task<string> HandleEncryptedInteractiveFlowMessage(WebHookResponseModel webhookData)
        {
            _receivedMessages.Add("Flow message recibed");
            _lastMessageState = "";
            try
            {
                string decryptedJsonString = _decryptionEncryptionService.DecryptFlowData(
                    webhookData.encrypted_flow_data,
                    webhookData.encrypted_aes_key,
                    webhookData.initial_vector
                );
                _receivedMessages.Add($"Decrypted Flow Data: {decryptedJsonString}");

                // Parse JSON into a dynamic object
                var requestPayload = JsonSerializer.Deserialize<JsonElement>(decryptedJsonString);

                string responseJson;
                string? action = requestPayload.TryGetProperty("action", out var actionProp) ? actionProp.GetString() : null;
                string? screenTo = requestPayload.TryGetProperty("screen", out var screenProp) ? screenProp.GetString() : null;

                switch (action, screenTo)
                {
                    case ("ping", null) when requestPayload.GetProperty("version").GetString() == "3.0"://Este revisa por el Health Check
                        _receivedMessages.Add("Flow Health Check");
                        responseJson = JsonSerializer.Serialize(new { data = new { status = "active" } });
                        break;

                    case (not null, not null) when requestPayload.TryGetProperty("data", out _) &&
                                        requestPayload.TryGetProperty("flow_token", out _):// Este revisa una solicitud de intercambio de datos
                        _receivedMessages.Add($"Flow Solicitud de intercambio de datos");
                        responseJson = JsonSerializer.Serialize(new
                        {
                            screen = screenTo,
                            data = new { success = true }
                        });
                        break;

                    case (null, not null) when requestPayload.TryGetProperty("data", out var dataProp) &&
                                       dataProp.TryGetProperty("error_message", out _):// Este revisa por una solicitud de cambio de pantalla (no prioridad)
                        _receivedMessages.Add($"Flow solicitud de cambio de pantalla");
                        responseJson = JsonSerializer.Serialize(new { data = new { acknowledged = true } });
                        break;

                    case (null, "SUCCESS") when requestPayload.TryGetProperty("data", out var finalData) &&
                                        finalData.TryGetProperty("extension_message_response", out _):// Este revisa por un Flow respndido y es la funcion principal------------------------------
                        _receivedMessages.Add($"Flow respndido");
                        responseJson = JsonSerializer.Serialize(new { summary = "Process completed successfully." });
                        break;

                    case ("data_exchange" or "INIT", null) when requestPayload.TryGetProperty("flow_token", out _) && requestPayload.TryGetProperty("data", out var errorData) &&
                                                       errorData.TryGetProperty("error", out _) && errorData.TryGetProperty("error_message", out var errorMessageProp): // Este revisa si nos reporta un error WhatsApp
                        _receivedMessages.Add($"Logged Error: {errorMessageProp.GetString()}");
                        responseJson = JsonSerializer.Serialize(new { data = new { acknowledged = true } });
                        break;

                    default:
                        responseJson = JsonSerializer.Serialize(new { message = "Unknown request type" });
                        break;
                }
                // Se encripta la respuesta
                byte[] aesKeyBytes = Convert.FromBase64String(_decryptionEncryptionService.DecryptAESKey(webhookData.encrypted_aes_key));
                byte[] initialVectorBytes = Convert.FromBase64String(webhookData.initial_vector);

                // Se invierten los IV bits
                byte[] invertedIV = FlowEncryptionService.InvertBits(initialVectorBytes);
                string encryptedResponse = FlowEncryptionService.EncryptResponse(responseJson, aesKeyBytes, invertedIV);

                return encryptedResponse;
            }
            catch (Exception ex)
            {
                _receivedMessages.Add($"Flow Error: {ex}");
                return $"Error processing request {ex.Message}";

            }
        }
        private async Task<string> HandleDocumentMessage(WebHookResponseModel webhookData)
        {
            await _hanldeDocument.ProcesaRecibirDocumento(webhookData);

            //Aqui va la lógica de manejar documentos recibidos
            return "Documento";
        }
        //Normalizamos el número si es mexicano
        private string NormalizarNumeroMexico(string numeroTelefono)
        {
            if (numeroTelefono.StartsWith("52") && numeroTelefono.Length == 13 && numeroTelefono[2] == '1')
            {
                return "52" + numeroTelefono.Substring(3); // Remover el tercer carácter que posiblemente representa es hecho de ser un mensaje de whatsapp business
            }
            return numeroTelefono;
        }
        private async Task<string> HandleInteractiveFlowMessage(WebHookResponseModel webhookData, string fromPhoneNumber)
        {
            string messageStatus;
            //Flow_response_json_Models parsedResponseJson;
            try
            {
                _receivedMessages.Add("Procesando Flow");
                string jsonString = webhookData.entry[0].changes[0].value.messages[0].interactive.nfm_reply.response_json;

                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("Registrar_Persona_Fisica_Nombre_0", out _) && root.TryGetProperty("Registrar_Persona_Fisica_Apellido_Paterno_1", out _))
                    {
                        _receivedMessages.Add("Detected Model 637724539030495, Registrar Persona Física Simple");
                        messageStatus = await HandleInteractiveFlowMessageRegistraPersonaFísica(JsonSerializer.Deserialize<Flow_response_json_Model_637724539030495_RegistrarPersonaFisicaSimple>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("Registrar_Empresa_Nombre_0", out _) && root.TryGetProperty("Registrar_Empresa_NIT_1", out _))
                    {
                        _receivedMessages.Add("Detected Model 1187351356327089, Registrar Empresa");
                        messageStatus = await HandleInteractiveFlowMessageRegistraEmpresa(JsonSerializer.Deserialize<Flow_response_json_Model_1187351356327089_RegistrarEmpresa>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("Observaciones_y_Mail_ID", out _))
                    {
                        _receivedMessages.Add("Detected Model 1414924193269074, Información Factura");
                        messageStatus = await HandleInteractiveFlowMessageObservacionesYMail(JsonSerializer.Deserialize<Flow_response_json_Model_1414924193269074_Observaciones_y_Mail>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("RegistraCliente_Tipo_Cliente", out _))
                    {
                        _receivedMessages.Add("Detected Model 1584870855544061, Información del clente");
                        messageStatus = await HandleInteractiveFlowMessageRegistrarCliente(JsonSerializer.Deserialize<Flow_response_json_Model_1584870855544061_CrearCliente>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("ModificarCliente_Empresa_Juridical_Razon_Social", out _))
                    {
                        _receivedMessages.Add("Detected Model 1378725303264167, Modificar persona Jurídica");
                        messageStatus = await HandleInteractiveFlowMessageModificarClienteEmpresa(JsonSerializer.Deserialize<Flow_response_json_Model_1378725303264167_Modificar_Cliente_Persona_Jurídica>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("ModificarCliente_Natural_Nombre", out _))
                    {
                        _receivedMessages.Add("Detected Model 931945452349522, Modificar persona Natural");
                        messageStatus = await HandleInteractiveFlowMessageModificarClienteNatural(JsonSerializer.Deserialize<Flow_response_json_Model_931945452349522_Modificar_Cliente_Persona_Física>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("Agregar_Producto_Nombre", out _))
                    {
                        _receivedMessages.Add("Detected Model 1142951587576244, Crear Producto");
                        messageStatus = await HandleInteractiveFlowCearProducto(JsonSerializer.Deserialize<Flow_response_json_Model_1142951587576244_Crear_Producto>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("Modificar_Producto_Codigo", out _))
                    {
                        _receivedMessages.Add("Detected Model 572042871927108, Modificar Producto");
                        messageStatus = await HandleInteractiveFlowModificarProducto(JsonSerializer.Deserialize<Flow_response_json_Model_572042871927108_Modificar_Producto>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("PUyC_Cantidad", out _))
                    {
                        _receivedMessages.Add("Detected Model 624251470432760 or 651685330992197, Crear Producto");
                        messageStatus = await HandleInteractiveFlowPrecioUnitarioyCantidad(JsonSerializer.Deserialize<Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad>(jsonString), fromPhoneNumber);
                        return messageStatus;

                    }
                    else if (root.TryGetProperty("Registros_DIAN_Prefijo_0", out _))
                    {
                        _receivedMessages.Add("Detected Model 1232523891647775, Registro DIAN");
                        messageStatus = await HandleInteractiveFlowRegistroDIAN(JsonSerializer.Deserialize<Flow_response_json_Model_1232523891647775_Registro_DIAN>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else
                    {

                        messageStatus = "Unknown response format";
                        _receivedMessages.Add("Flow Error: Unknown response format");
                        throw new Exception("Unknown response format.");
                    }
                }

                messageStatus = await MainMenuButton(fromPhoneNumber, null);

                _receivedMessages.Add("Button Sent");
                _receivedMessages.Add($"Messages status: {messageStatus}");
                UpdateLastMessageState(fromPhoneNumber, "");
                UpdateOldUserMessage(fromPhoneNumber, "");
                return messageStatus;
            }
            catch (Exception parseEx)
            {
                _receivedMessages.Add($"Error parsing response_json: {parseEx.Message}. Original JSON: {webhookData.entry[0].changes[0].value.messages[0].interactive.nfm_reply.response_json}");
                messageStatus = await MainMenuButton(fromPhoneNumber, "Error: reiniciando");
                UpdateRestart(fromPhoneNumber);
                return $"Error parsing response_json: {parseEx.Message}";
            }
        }
        /*private async Task<string> HandleInteractiveFlowMessageInformaciónFactura(Flow_response_json_Model_682423707677994_InformacionFactura parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add("HandleInteractiveFlowMessageInformaciónFactura");
            _receivedMessages.Add("Parsed Factura Colombia response_json:");
            string messageStatus;
            _receivedMessages.Add(JsonSerializer.Serialize(parsedResponseJson, new JsonSerializerOptions { WriteIndented = true }));

            _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Factura", "No"), ("opcion_Anadir_Producto", "Sí"), ("opcion_Cancelar", "Reiniciar Proceso") };//{ ("opcion_Recibir_Registro_Flow", "Regístrame"), ("opcion_Registrar_Empresa_Flow", "Registrar Empresa"), ("opcion_Generar_Factura_Text ", "Generar factura") };
            messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Deseas añadir un producto?", "Selecciona opción", "Hay 2 opciones", _botones);
            _receivedMessages.Add(messageStatus);
            UpdateObservaciones(fromPhoneNumber, parsedResponseJson.screen_0_Telfono_0 ?? "", parsedResponseJson.screen_0_Direccin_1 ?? "", parsedResponseJson.screen_0_Monto_2 ?? "", parsedResponseJson.screen_0_Descripcin_3 ?? "");
            _receivedMessages.Add("Factura Updated");
            
            correosACopiar = parsedResponseJson.screen_0_Correos_4?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>();
            _receivedMessages.Add($"Correos ingresados: {string.Join(" | ", correosACopiar)}");
            return messageStatus;
        }*/
        
        private async Task<string> HandleInteractiveFlowMessageObservacionesYMail(Flow_response_json_Model_1414924193269074_Observaciones_y_Mail parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add("HandleInteractiveFlowMessageObservacionesYMail");
            _receivedMessages.Add("Parsed Factura Colombia response_json:");
            string messageStatus;
            _receivedMessages.Add(JsonSerializer.Serialize(parsedResponseJson, new JsonSerializerOptions { WriteIndented = true }));
            /*
            _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Factura", "No"), ("opcion_Anadir_Producto", "Sí"), ("opcion_Cancelar", "Reiniciar Proceso") };//{ ("opcion_Recibir_Registro_Flow", "Regístrame"), ("opcion_Registrar_Empresa_Flow", "Registrar Empresa"), ("opcion_Generar_Factura_Text ", "Generar factura") };
            messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Deseas añadir un producto?", "Selecciona opción", "Hay 3 opciones", _botones);
            */
            _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Factura ", "Finalizar factura"), ("opcion_Cancelar", "Reiniciar Proceso") };

            messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Que deseas hacer", "Selecciona opción", "¿Finalizar factura o reiniciar el proceso?", _botones);
            _receivedMessages.Add(messageStatus);
            UpdateObservaciones(fromPhoneNumber, parsedResponseJson.Observaciones_y_Mail_0_Observaciones_0 ?? "-");
            _receivedMessages.Add("Factura Updated");

            correosACopiar = parsedResponseJson.Observaciones_y_Mail_0_Correos_1?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>();
            _receivedMessages.Add($"Correos ingresados: {string.Join(" | ", correosACopiar)}");
            return messageStatus;
        }
        private async Task<string> HandleInteractiveFlowRegistroDIAN(Flow_response_json_Model_1232523891647775_Registro_DIAN parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add("HandleInteractiveFlowRegistroDIAN");
            string messageStatus;
            _receivedMessages.Add(JsonSerializer.Serialize(parsedResponseJson, new JsonSerializerOptions { WriteIndented = true }));

            messageStatus = await MainMenuButton(fromPhoneNumber, "Registrado");
            _receivedMessages.Add(messageStatus);
            UpdateRestart(fromPhoneNumber);
            return messageStatus;
        }

        private async Task<string> HandleInteractiveFlowMessageRegistraPersonaFísica(Flow_response_json_Model_637724539030495_RegistrarPersonaFisicaSimple parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add($"HandleInteractiveFlowMessageRegistraPersonaFísica");
            string messageStatus;
            string fullName = $"{parsedResponseJson.Registrar_Persona_Fisica_Nombre_0} {parsedResponseJson.Registrar_Persona_Fisica_Apellido_Paterno_1} {parsedResponseJson.Registrar_Persona_Fisica_Apellido_Materno_2}";
            bool existencia = _manejoDeComprador.CompradorExiste(fullName, parsedResponseJson.screen_1_NIT_0);
            if (existencia == null)
            {
                _receivedMessages.Add("Error, existencia is null");

                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "[Error] existencia is null");

                return "Error, existencia is null";
            }
            else if (existencia)
            {
                _receivedMessages.Add($"Comprador Existe");
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Usuario ya existe, modificando información");

                messageStatus = messageStatus + _manejoDeComprador.ModificarPersonaFísica(parsedResponseJson, fromPhoneNumber);
            }
            else
            {
                _receivedMessages.Add($"Comprador No Existe");
                messageStatus = _manejoDeComprador.CrearPersonaFísica(parsedResponseJson, fromPhoneNumber);
            }
            _receivedMessages.Add($"Comprador: {fullName}, {parsedResponseJson.screen_1_NIT_0}");

            if (parsedResponseJson.Registrar_Persona_Fisica_Tipo_Identificacin_5 != "4_NIT" &&
                parsedResponseJson.screen_1_documento == null)
            {
                _receivedMessages.Add("Error: Documento de identificación requerido");
            }

            if (GetLastMessageState(fromPhoneNumber) == "opcion_Generar_Factura_Registrar_Persona_Flow")
            {
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿A quién facturarás?\nPuedes escribir su NIT, Nombre o una parte de estos");
                _receivedMessages.Add(messageStatus);
                UpdateLastMessageState(fromPhoneNumber, "opcion_Generar_Factura_Cliente");
                UpdateNITComprador(fromPhoneNumber, parsedResponseJson.screen_1_NIT_0);
                UpdateNombreComprador(fromPhoneNumber, fullName);
                UpdateOldUserMessage(fromPhoneNumber, "HandleInteractiveFlowMessageRegistraPersonaFísica");
                return messageStatus;
            }

            messageStatus = await MainMenuButton(fromPhoneNumber, null);
            UpdateRestart(fromPhoneNumber);
            _receivedMessages.Add(messageStatus);

            return messageStatus;
        }
        private async Task<string> HandleInteractiveFlowMessageRegistraEmpresa(Flow_response_json_Model_1187351356327089_RegistrarEmpresa parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add($"HandleInteractiveFlowMessageRegistraEmpresa");
            string messageStatus;
            bool existencia = _manejoDeComprador.CompradorExiste(parsedResponseJson.Registrar_Empresa_Nombre_0, parsedResponseJson.Registrar_Empresa_NIT_1);
            if (existencia == null)
            {
                _receivedMessages.Add("Error, existencia is null");
                return "Error, existencia is null";
            }
            else if(existencia)
            {
                _receivedMessages.Add($"Empresa existe");
                messageStatus = _manejoDeComprador.ModificarEmpresa(parsedResponseJson, fromPhoneNumber);
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Usuario ya existe, modificando información");
            }
            else
            {
                _receivedMessages.Add($"Empresa no existe");
                messageStatus = _manejoDeComprador.CrearEmpresa(parsedResponseJson, fromPhoneNumber);
            }
            _receivedMessages.Add($"Empresa: {parsedResponseJson.Registrar_Empresa_Nombre_0}, {parsedResponseJson.Registrar_Empresa_NIT_1}");

            if (GetLastMessageState(fromPhoneNumber) == "opcion_Generar_Factura_Registrar_Empresa_Flow")
            {
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿A quién facturarás?\nPuedes escribir su NIT, Nombre o una parte de estos");
                _receivedMessages.Add(messageStatus);
                UpdateLastMessageState(fromPhoneNumber, "opcion_Generar_Factura_Cliente");
                UpdateNITComprador(fromPhoneNumber, parsedResponseJson.Registrar_Empresa_NIT_1);
                UpdateNombreComprador(fromPhoneNumber, parsedResponseJson.Registrar_Empresa_Nombre_0);
                UpdateOldUserMessage(fromPhoneNumber, "HandleInteractiveFlowMessageRegistraEmpresa");
                return messageStatus;
            }

            messageStatus = await MainMenuButton(fromPhoneNumber, null);
            UpdateRestart(fromPhoneNumber);
            _receivedMessages.Add(messageStatus);

            return messageStatus;
        }
        private async Task<string> HandleInteractiveFlowCearProducto(Flow_response_json_Model_1142951587576244_Crear_Producto parsedResponseJson, string fromPhoneNumber)
        {
            string messageStatus;
            _receivedMessages.Add("HandleInteractiveFlowCearProducto");
            if (string.IsNullOrEmpty(parsedResponseJson.Agregar_Producto_Codigo))
            {
                _receivedMessages.Add("Falta código de producto");
                messageStatus = await MainMenuButton(fromPhoneNumber, "Falta código de producto");
                _receivedMessages.Add(messageStatus);
                UpdateRestart(fromPhoneNumber);
                return "Producto Fallado";
            }else if (string.IsNullOrEmpty(parsedResponseJson.Agregar_Producto_Nombre)){
                _receivedMessages.Add("Falta nombre de producto");
                messageStatus = await MainMenuButton(fromPhoneNumber, "Falta nombre de producto");
                _receivedMessages.Add(messageStatus);
                UpdateRestart(fromPhoneNumber);
                return "Producto Fallado";
            }
            else if (string.IsNullOrEmpty(parsedResponseJson.Agregar_Producto_Precio_Unitario)){
                _receivedMessages.Add("Falta Precio Unitario del producto");
                messageStatus = await MainMenuButton(fromPhoneNumber, "Falta Precio Unitario del producto");
                _receivedMessages.Add(messageStatus);
                UpdateRestart(fromPhoneNumber);
                return "Producto Fallado";
            }


            if (_productos.Any(c => c.Agregar_Producto_Nombre.Equals(parsedResponseJson.Agregar_Producto_Nombre, StringComparison.OrdinalIgnoreCase)) ||
                _productos.Any(c => c.Agregar_Producto_Codigo.Equals(parsedResponseJson.Agregar_Producto_Codigo, StringComparison.OrdinalIgnoreCase)))
            {
                _receivedMessages.Add("Producto ya existe");
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Producto ya existe");
                UpdateRestart(fromPhoneNumber);
                return "Producto ya existe";
            }
            _productos.Add(parsedResponseJson);
            messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Producto añadido");
            if (GetLastMessageState(fromPhoneNumber) == "opcion_Configurar_Productos_agregar")
            {
                _receivedMessages.Add("HandleInteractiveFlowCearProducto: opcion_Configurar_Productos_agregar");

                messageStatus = messageStatus + await MainMenuButton(fromPhoneNumber, null);
                UpdateRestart(fromPhoneNumber);
            }
            else if (GetLastMessageState(fromPhoneNumber) == "opcion_Anadir_Producto_agregar")
            {
                _receivedMessages.Add("HandleInteractiveFlowCearProducto: opcion_Anadir_Producto_agregar");
                AddProductosEnFacturaList(fromPhoneNumber, parsedResponseJson.Agregar_Producto_Codigo);

                string precioString = parsedResponseJson.Agregar_Producto_Precio_Unitario;
                if (float.TryParse(precioString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsedFloat))
                {
                    UpdateLastMessageState(fromPhoneNumber, "opcion_Anadir_Producto");
                    var parameters = new { Precio_Unitario = parsedFloat, Precio_String = precioString };
                    messageStatus = await _messageSendService.EnviarFlowConDatos(fromPhoneNumber, "651685330992197", $"opcionFacturar{fromPhoneNumber}", "published", "Producto Registrado, proporciona Monto unitario y cantidad de unidades del producto", "Llenar", parameters, "DETAILS");
                    _receivedMessages.Add("Message Status: ");
                    _receivedMessages.Add(messageStatus);
                    return "Producto añadido";
                }

                _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Observaciones_Y_Mail", "No añadir"), ("opcion_Anadir_Producto", "Añadir"), ("opcion_Cancelar", "Reiniciar Proceso") };//{ ("opcion_Recibir_Registro_Flow", "Regístrame"), ("opcion_Registrar_Empresa_Flow", "Registrar Empresa"), ("opcion_Generar_Factura_Text ", "Generar factura") };
                messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Error al registrar", "Producto", "Hay 3 opciones", _botones);
            }
            else
            {
                _receivedMessages.Add("HandleInteractiveFlowCearProducto: else???");
                messageStatus = messageStatus + await MainMenuButton(fromPhoneNumber, "Error: reiniciando");
                UpdateRestart(fromPhoneNumber);
            }
            _receivedMessages.Add("Message Status: ");
            _receivedMessages.Add(messageStatus);
            return "Producto añadido";
        }
        private async Task<string> HandleInteractiveFlowModificarProducto(Flow_response_json_Model_572042871927108_Modificar_Producto parsedResponseJson, string fromPhoneNumber)
        {
            string messageStatus;

            string codigoModificar = _producto_a_modificar[fromPhoneNumber].productoCodigoModificar;
            string ViejoNombreProducto = _producto_a_modificar[fromPhoneNumber].productoNombreOld;

            _receivedMessages.Add("HandleInteractiveFlowModificarProducto");

            // Buscar el producto en la lista
            var producto = _productos.FirstOrDefault(p =>
                p.Agregar_Producto_Codigo == codigoModificar);

            _receivedMessages.Add($"Producto: {codigoModificar}");

            if (producto == null)
            {
                messageStatus = await MainMenuButton(fromPhoneNumber, "Producto no encontrado");
                _receivedMessages.Add(messageStatus);
                UpdateRestart(fromPhoneNumber);

                return messageStatus;
            }

            // Modificar solo los campos que vienen en parsedResponseJson
            if (parsedResponseJson.Modificar_Producto_Nombre != null && parsedResponseJson.Modificar_Producto_Nombre != "Nombre del producto")
            {
                producto.Agregar_Producto_Nombre = parsedResponseJson.Modificar_Producto_Nombre;
                _receivedMessages.Add("Nombre modificado");
            }

            if (parsedResponseJson.Modificar_Producto_Precio_Unitario != null && parsedResponseJson.Modificar_Producto_Precio_Unitario != "Precio Unitario")
            {
                producto.Agregar_Producto_Precio_Unitario = parsedResponseJson.Modificar_Producto_Precio_Unitario;
                _receivedMessages.Add("Precio Unitario modificado");
            }

            if (parsedResponseJson.Modificar_Producto_Info_Adicional != null && parsedResponseJson.Modificar_Producto_Info_Adicional != "Info")
            {
                producto.Agregar_Producto_Info_Adicional = parsedResponseJson.Modificar_Producto_Info_Adicional;
                _receivedMessages.Add("Info modificado");
            }

            if (parsedResponseJson.Modificar_Producto_Unidad_Medida != null && parsedResponseJson.Modificar_Producto_Unidad_Medida != "Unidad de medida")
            { 
                producto.Agregar_Producto_Unidad_Medida = parsedResponseJson.Modificar_Producto_Unidad_Medida;
                _receivedMessages.Add("Unidad de medida modificado");
            }

            if (parsedResponseJson.Modificar_Producto_Activo != null && parsedResponseJson.Modificar_Producto_Activo != "Activo")
            {
                producto.Agregar_Producto_Activo = parsedResponseJson.Modificar_Producto_Activo;
                _receivedMessages.Add("Activo modificado");
            }

            if (parsedResponseJson.Modificar_Producto_traslados != null && parsedResponseJson.Modificar_Producto_traslados != "Impuestos traslados")
            {
                producto.Agregar_Producto_traslados = parsedResponseJson.Modificar_Producto_traslados;
                _receivedMessages.Add("Impuestos traslados modificado");
            }

            if (parsedResponseJson.Modificar_Producto_Impuesto != null && parsedResponseJson.Modificar_Producto_Impuesto != "Impuesto")
            {
                producto.Agregar_Producto_Impuesto = parsedResponseJson.Modificar_Producto_Impuesto;
                _receivedMessages.Add("Impuesto modificado");
            }

            if (parsedResponseJson.Modificar_Producto_Tasa_cuota != null && parsedResponseJson.Modificar_Producto_Tasa_cuota != "Tasa o cuota")
            {
                producto.Agregar_Producto_Tasa_cuota = parsedResponseJson.Modificar_Producto_Tasa_cuota;
                _receivedMessages.Add("Tasa o cuota modificado");
            }

            if (parsedResponseJson.Modificar_Producto_Impuestos_Saludables != null && parsedResponseJson.Modificar_Producto_Impuestos_Saludables != "Impuestos Saludables")
            {
                producto.Agregar_Producto_Impuestos_Saludables = parsedResponseJson.Modificar_Producto_Impuestos_Saludables;
                _receivedMessages.Add("Impuestos Saludables modificado");
            }

            if (parsedResponseJson.Modificar_Producto_Impuestos_Saludables2 != null)
            {
                producto.Agregar_Producto_Impuestos_Saludables2 = parsedResponseJson.Modificar_Producto_Impuestos_Saludables2;
                _receivedMessages.Add("Impuestos Saludables2 modificado");
            }
            
            messageStatus = await MainMenuButton(fromPhoneNumber, "Producto Modificado");
            _receivedMessages.Add(messageStatus);
            UpdateRestart(fromPhoneNumber);

            return "Producto añadido";
        }
        private async Task<string> HandleInteractiveFlowMessageRegistrarCliente(Flow_response_json_Model_1584870855544061_CrearCliente parsedResponseJson, string fromPhoneNumber)
        {
            string messageStatus;
            string? NITCliente = null;

            if (string.IsNullOrEmpty(parsedResponseJson.RegistraCliente_Tipo_Cliente))
            {
                _receivedMessages.Add($"HandleInteractiveFlowMessageRegistrarCliente: RegistraCliente_Tipo_Cliente is null");
                messageStatus = await MainMenuButton(fromPhoneNumber, "Error: tipo de cliente null, reiniciando");
                UpdateRestart(fromPhoneNumber);
                _receivedMessages.Add($"HandleInteractiveFlowMessageRegistrarCliente {messageStatus}");
                return "Error: tipo de cliente null";
            }

            if (parsedResponseJson.flow_token == "pruebaconDataCrearCli")
            {
                _receivedMessages.Add("pruebaconDataCrearCli");
                _receivedMessages.Add(parsedResponseJson.RegisterClient_Juridical_Razon_Social);
                return "Prueba";
            }
            _receivedMessages.Add($"HandleInteractiveFlowMessageRegistrarCliente");
            _receivedMessages.Add(parsedResponseJson.RegistraCliente_Tipo_Cliente);
            messageStatus = _manejoDeComprador.AñadirClienteNaturalJudicialAComprador(GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber), parsedResponseJson);
            _receivedMessages.Add(messageStatus);

            /*NITCliente = !string.IsNullOrWhiteSpace(parsedResponseJson.RegisterClient_Juridical_NIT) && parsedResponseJson.RegistraCliente_Tipo_Cliente != "Tipo_Persona_Juridica"
                ? parsedResponseJson.RegisterClient_Juridical_NIT
                : (!string.IsNullOrWhiteSpace(parsedResponseJson.RegisterClient_Natural_NIT) && parsedResponseJson.RegistraCliente_Tipo_Cliente != "RegisterClient_Natural_NIT"
                ? parsedResponseJson.RegisterClient_Natural_NIT
                : null);*/
            if (parsedResponseJson.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Juridica")
            {
                NITCliente = parsedResponseJson.RegisterClient_Juridical_NIT;

                _receivedMessages.Add($"Nit: {NITCliente}");
                _receivedMessages.Add($"Razon social: {parsedResponseJson.RegisterClient_Juridical_Razon_Social}");
                _receivedMessages.Add($"Digito verificacion: {parsedResponseJson.RegisterClient_Juridical_Digito_Verificacion}");
                _receivedMessages.Add($"Direccion: {parsedResponseJson.RegisterClient_Juridical_Direccion}");
                _receivedMessages.Add($"Departamento: {parsedResponseJson.RegisterClient_Juridical_Departamento}");
                _receivedMessages.Add($"Ciudad: {parsedResponseJson.RegisterClient_Juridical_Ciudad}");
                _receivedMessages.Add($"Tipo régimen: {parsedResponseJson.RegisterClient_Juridical_Tipo_Regimen}");
                _receivedMessages.Add($"Obligaciones Fiscales: {parsedResponseJson.RegisterClient_Juridical_Obligaciones_Fiscales}");

                _receivedMessages.Add("Persona Judicial");
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Persona Judicial Registrada");
            }
            else if (parsedResponseJson.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Natural")
            {
                NITCliente = parsedResponseJson.RegisterClient_Natural_NIT;

                _receivedMessages.Add($"Nit: {NITCliente}");
                _receivedMessages.Add($"Nombre: {parsedResponseJson.RegisterClient_Natural_Nombre}");
                _receivedMessages.Add($"Apellido paterno: {parsedResponseJson.RegisterClient_Natural_Apellido_Paterno}");
                _receivedMessages.Add($"Apellido materno: {parsedResponseJson.RegisterClient_Natural_Apellido_Materno}");
                _receivedMessages.Add($"Correo: {parsedResponseJson.RegisterClient_Natural_Correo}");
                _receivedMessages.Add($"Teléfono: {parsedResponseJson.RegisterClient_Natural_Telefono}");
                _receivedMessages.Add($"Digito de Verificacion: {parsedResponseJson.RegisterClient_Natural_Digito_Verificacin}");
                _receivedMessages.Add($"Direccion: {parsedResponseJson.RegisterClient_Natural_Direccion}");
                _receivedMessages.Add($"Departamento: {parsedResponseJson.RegisterClient_Natural_Departamento}");
                _receivedMessages.Add($"Ciudad: {parsedResponseJson.RegisterClient_Natural_Ciudad}");
                _receivedMessages.Add($"Tipo régimen: {parsedResponseJson.RegisterClient_Natural_Tipo_Rgimen}");
                _receivedMessages.Add($"Obligaciones Fiscales: {parsedResponseJson.RegisterClient_Natural_Obligaciones_Fiscale}");

                _receivedMessages.Add($"Persona Natural");
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Persona Natural Registrada");
            }
            else
            {
                _receivedMessages.Add($"HandleInteractiveFlowMessageRegistrarCliente: RegistraCliente_Tipo_Cliente tipo de cliente desconocido: {parsedResponseJson.RegisterClient_Natural_NIT}");
                messageStatus = messageStatus + await MainMenuButton(fromPhoneNumber, "Error: tipo de cliente desconocido, reiniciando");
                UpdateRestart(fromPhoneNumber);
                _receivedMessages.Add($"HandleInteractiveFlowMessageRegistrarCliente {messageStatus}");
                return "Error con el tipo de cliente: reiniciando";
            }

            if (GetLastMessageState(fromPhoneNumber) == "opcion_Registrar_Cliente")
            {
                _receivedMessages.Add("opcion_Registrar_Cliente");

                //messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1414924193269074", $"opcionFacturar{fromPhoneNumber}", "published", "Todo parece estar listo, ya puedes facturar", "Facturar ahora");

                //_botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Anadir_Producto ", ""), ("opcion_Cancelar", "Reiniciar Proceso") };
                //messageStatus = messageStatus + await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Que deseas hacer", "Selecciona opción", "¿Regrezar al inicio o continuar?", _botones);

                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Favor de indicar nombre del producto a añadir a la factura");

                //_NITCliente = parsedResponseJson.screen_0_NIT_4;
                UpdateNITCliente(fromPhoneNumber, NITCliente);
                UpdateLastMessageState(fromPhoneNumber, "opcion_Anadir_Producto");
                UpdateOldUserMessage(fromPhoneNumber, "0");
                _receivedMessages.Add(messageStatus);
                return messageStatus;
            }
            else if (GetLastMessageState(fromPhoneNumber) == "opcion_Agregar_Clientes")
            {

                messageStatus = await MainMenuButton(fromPhoneNumber, null);
            }
            UpdateNITCliente(fromPhoneNumber, NITCliente);
            UpdateLastMessageState(fromPhoneNumber, "");
            UpdateOldUserMessage(fromPhoneNumber, "0");
            UpdateNITComprador(fromPhoneNumber, "");
            UpdateNombreComprador(fromPhoneNumber, "");

            _receivedMessages.Add(messageStatus);
            return messageStatus;
        }
        private async Task<string> HandleInteractiveFlowMessageModificarClienteEmpresa(Flow_response_json_Model_1378725303264167_Modificar_Cliente_Persona_Jurídica parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add($"HandleInteractiveFlowMessageModificarClienteEmpresa");
            string messageStatus;
            messageStatus = _manejoDeComprador.ModificarClienteJurídicaDeComprador(GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber), parsedResponseJson);
            _receivedMessages.Add($"Modificando: {messageStatus}");
            if (messageStatus.StartsWith("[ERROR]"))
            {
                messageStatus = messageStatus + await _messageSendService.EnviarTexto(fromPhoneNumber, "Error al actualizar");
                _receivedMessages.Add("Error al actualizar");
            }
            else
            {
            _receivedMessages.Add("Sin errores");
            messageStatus = messageStatus + await _messageSendService.EnviarTexto(fromPhoneNumber, "Empresa Cliente modificada correctamente");
            _receivedMessages.Add("Empresa Cliente modificada correctamente");
            }

            if (GetLastMessageState(fromPhoneNumber) == "opcion_Finalizar_Modificar_Cliente_Modificar")
            {
                _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Factura", "Ver Factura"), ("opcion_Cancelar", "Reiniciar Proceso") };
                messageStatus = messageStatus + await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que deseas hacer?", "Selecciona opción", "Hay 2 opcioned", _botones);
                UpdateNITCliente(fromPhoneNumber, parsedResponseJson.ModificarCliente_Empresa_Juridical_NIT);
                _receivedMessages.Add(messageStatus);
                return messageStatus;
            }
            messageStatus = messageStatus + await MainMenuButton(fromPhoneNumber, null);
            UpdateRestart(fromPhoneNumber);
            _receivedMessages.Add(messageStatus);
            return messageStatus;
        }
        private async Task<string> HandleInteractiveFlowMessageModificarClienteNatural(Flow_response_json_Model_931945452349522_Modificar_Cliente_Persona_Física parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add($"HandleInteractiveFlowMessageModificarClienteNatural");
            string messageStatus;
            messageStatus = _manejoDeComprador.ModificarClienteNaturalDeComprador(GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber), parsedResponseJson);
            if (messageStatus.StartsWith("[ERROR]"))
            {
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Error al actualizar");
                _receivedMessages.Add("Error al actualizar");
            }
            else
            {
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Cliente persona Natural modificado correctamente");
            _receivedMessages.Add("Cliente persona Natural modificado correctamente");
            }

            if (GetLastMessageState(fromPhoneNumber) == "opcion_Finalizar_Modificar_Cliente_Modificar")
            {
                _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Factura", "Ver Factura"), ("opcion_Cancelar", "Reiniciar Proceso") };
                messageStatus = messageStatus + await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que deseas hacer?", "Selecciona opción", "Hay 2 opcioned", _botones);
                UpdateNITCliente(fromPhoneNumber, parsedResponseJson.ModificarCliente_Natural_NIT);
                _receivedMessages.Add(messageStatus);
                return messageStatus;
            }

            messageStatus = messageStatus + await MainMenuButton(fromPhoneNumber, null);
            UpdateRestart(fromPhoneNumber);
            _receivedMessages.Add(messageStatus);
            return messageStatus;
        }
        private async Task<string> HandleInteractiveFlowPrecioUnitarioyCantidad(Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad parsedResponseJson, string fromPhoneNumber)
        {
            int monto = 1;
            _receivedMessages.Add($"HandleInteractiveFlowPrecioUnitarioyCantidad");

            _receivedMessages.Add($"Precio nuevo: {parsedResponseJson.PUyC_Precio_Unitario}");
            string messageStatus;
            AddProductosPrecioYCantidad(fromPhoneNumber, parsedResponseJson);


            string observaciones = GetObservaciones(fromPhoneNumber);
            var preciosYCantidades = GetPrecioUnitarioYCantidad(fromPhoneNumber);
            // Calculate the sum of PUyC_Precio_Unitario * PUyC_Cantidad
            float totalSum = preciosYCantidades.Sum(p =>
            {
                bool parsedPrecio = float.TryParse(p.PUyC_Precio_Unitario, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float precio);
                bool parsedCantidad = float.TryParse(p.PUyC_Cantidad, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float cantidad);
                return (parsedPrecio && parsedCantidad) ? precio * cantidad : 0f;
            });
            
            _receivedMessages.Add($"Total: {totalSum} = {totalSum}");

            messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, $"El total de los productos es {totalSum}");

            if (GetLastMessageState(fromPhoneNumber) == "opcion_Finalizar_Modificar_Productos_Anadir")
            {
                _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Finalizar_Modificar_Productos_Anadir", "Otro producto"), ("opcion_Cancelar", "Reiniciar Proceso"), ("opcion_Finalizar_Modificar_Factura", "Cancelar") };
                messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que deseas hacer?", "¿Añadir otro producto o Finalizar la factura?", "No puedes crear aquí", _botones);

            }
            else
            {
                _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Anadir_Producto", "Otro producto"), ("opcion_Observaciones_Y_Mail ", "Añadir Información"), ("opcion_Finalizar_Factura ", "Finalizar factura") };
                messageStatus = messageStatus + await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que deseas hacer?", "¿Añadir otro producto, Finalizar la factura o Añadir observaciones o correos a copiar?", "", _botones);
            }
            _receivedMessages.Add(messageStatus);
            _receivedMessages.Add("PrecioUnitarioyCantidad guardado");
            return messageStatus;
        }
        


        private async Task<string> MainMenuButton(string fromPhoneNumber, string? mainText)
        {
            string messageStatus;

            if (string.IsNullOrEmpty(mainText))
                mainText = "Soy Timbrame Bot, ¿En que puedo ayudarte?";
            int CompConCel = _manejoDeComprador.ContarCompradoresConTelefono(fromPhoneNumber);
            if (CompConCel <= 0)
            {
                _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Elegir_tipo_Registro", "Regístrame"), ("opcion_Iniciar_Sesión", "Iniciar Sesión") };
                messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, mainText, "No hay usuarios relacionados a tu teléfono", "Regístrate", _botones);
                _receivedMessages.Add(messageStatus);
                return messageStatus;
            }

            int CliPorCel = _manejoDeComprador.ContarClientesPorTelefono(fromPhoneNumber);
            if (CliPorCel <= 0)
            {
                _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Elegir_tipo_Registro", "Regístrame"), ("opcion_Iniciar_Sesión", "Iniciar Sesión"), ("opcion_configurar", "Configurar") };
                messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, mainText, "No tienes clientes relacionados a un usuario con tu teléfono", "Selecciona configurar para añadir cliente", _botones);
                _receivedMessages.Add(messageStatus);
                return messageStatus;
            }

            /*int ProdPorCel = _manejoDeComprador.ContarProductosPorTelefono(fromPhoneNumber);
            if (ProdPorCel <= 0)
            {
                _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Elegir_tipo_Registro", "Regístrame"), ("opcion_Iniciar_Sesión", "Iniciar Sesión"), ("opcion_configurar", "Configurar") };
                messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, mainText, "No tienes productos relacionados a un usuario con tu teléfono", "Selecciona configurar para añadir productos", _botones);
                _receivedMessages.Add(messageStatus);
                return messageStatus;
            }*/

            _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_configurar_usuario", "Configurar usuario"), ("opcion_Generar_Factura_Text ", "Generar factura"), ("opcion_configurar", "Configurar") };
            messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, mainText, "Estás listo para facturar", "Hay 3 opciones", _botones);
            return messageStatus;
        }

        private void UpdateLastMessageState(string fromPhoneNumber, string newMessageState)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    newMessageState,  // Update lastMessageState
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    //existingData.Factura_Telefono,
                    //existingData.Factura_dirección,
                    //existingData.Factura_Monto,
                    //existingData.Factura_Descripcón,
                    existingData.Factura_Observaciones,
                    existingData.ListaProductos,
                    existingData.ListaPreciosYCantidades
                );
            }
        }
        private void UpdateOldUserMessage(string fromPhoneNumber, string oldUserMessageNew)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    oldUserMessageNew,
                    existingData.montoFactura,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    //existingData.Factura_Telefono,
                    //existingData.Factura_dirección,
                    //existingData.Factura_Monto,
                    //existingData.Factura_Descripcón,
                    existingData.Factura_Observaciones,
                    existingData.ListaProductos,
                    existingData.ListaPreciosYCantidades
                );
            }
        }
        private void UpdateMontoFactura(string fromPhoneNumber, string MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    MessageChange,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    //existingData.Factura_Telefono,
                    //existingData.Factura_dirección,
                    //existingData.Factura_Monto,
                    //existingData.Factura_Descripcón,
                    existingData.Factura_Observaciones,
                    existingData.ListaProductos,
                    existingData.ListaPreciosYCantidades
                );
            }
        }
        private void UpdateNITCliente(string fromPhoneNumber, string MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    MessageChange,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    //existingData.Factura_Telefono,
                    //existingData.Factura_dirección,
                    //existingData.Factura_Monto,
                    //existingData.Factura_Descripcón,
                    existingData.Factura_Observaciones,
                    existingData.ListaProductos,
                    existingData.ListaPreciosYCantidades
                );
            }
        }
        private void UpdateNITComprador(string fromPhoneNumber, string MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.NITCliente,
                    MessageChange,
                    existingData.NombreComprador,
                    //existingData.Factura_Telefono,
                    //existingData.Factura_dirección,
                    //existingData.Factura_Monto,
                    //existingData.Factura_Descripcón,
                    existingData.Factura_Observaciones,
                    existingData.ListaProductos,
                    existingData.ListaPreciosYCantidades
                );
            }
        }
        private void UpdateNombreComprador(string fromPhoneNumber, string MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    MessageChange,
                    //existingData.Factura_Telefono,
                    //existingData.Factura_dirección,
                    //existingData.Factura_Monto,
                    //existingData.Factura_Descripcón,
                    existingData.Factura_Observaciones,
                    existingData.ListaProductos,
                    existingData.ListaPreciosYCantidades
                );
            }
        }
        /*private void UpdateFactura(string fromPhoneNumber, string Telefono,
                    string dirección,
                    string Monto,
                    string Descripcón)
        {

            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    Telefono,
                    dirección,
                    Monto,
                    Descripcón,
                    existingData.ListaProductos,
                    existingData.ListaPreciosYCantidades
                );
            }
        }*/
        private void UpdateObservaciones(string fromPhoneNumber, string MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    MessageChange,
                    existingData.ListaProductos,
                    existingData.ListaPreciosYCantidades
                );
            }
        }
        private void UpdateProductosEnFacturaList(string fromPhoneNumber, List<string> MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    //existingData.Factura_Telefono,
                    //existingData.Factura_dirección,
                    //existingData.Factura_Monto,
                    //existingData.Factura_Descripcón,
                    existingData.Factura_Observaciones,
                    MessageChange,
                    existingData.ListaPreciosYCantidades
                );
            }
        }
        private void UpdatePrecioUnitarioYCantidad(string fromPhoneNumber, List<Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad> MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    //existingData.Factura_Telefono,
                    //existingData.Factura_dirección,
                    //existingData.Factura_Monto,
                    //existingData.Factura_Descripcón,
                    existingData.Factura_Observaciones,
                    existingData.ListaProductos,
                    MessageChange
                );
            }
        }
        private void UpdateRestart(string fromPhoneNumber)
        {
            UpdateLastMessageState(fromPhoneNumber, "");
            UpdateMontoFactura(fromPhoneNumber, "");
            UpdateOldUserMessage(fromPhoneNumber, "0");
            UpdateNITCliente(fromPhoneNumber, "");
            UpdateNITComprador(fromPhoneNumber, "");
            UpdateNombreComprador(fromPhoneNumber, "");
            //UpdateFactura(fromPhoneNumber, "", "", "", "");
            UpdateObservaciones(fromPhoneNumber, "");
            UpdateProductosEnFacturaList(fromPhoneNumber, new List<string> { });
            UpdatePrecioUnitarioYCantidad(fromPhoneNumber, new List<Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad> { });
            correosACopiar = new List<string> { };
            _receivedMessages.Add("UpdateRestart");
            _producto_a_modificar[fromPhoneNumber] = new() { };
        }
        private void AddProductosEnFacturaList(string fromPhoneNumber, string NewProducto)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                existingData.ListaProductos.Add(NewProducto);
            }
        }
        private void AddProductosPrecioYCantidad(string fromPhoneNumber, Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad NewCantidadYPrecio)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                existingData.ListaPreciosYCantidades.Add(NewCantidadYPrecio);
            }
        }

        private string GetLastMessageState(string fromPhoneNumber) =>
    _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.lastMessageState : string.Empty;
        private string GetOldUserMessage(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.oldUserMessage : string.Empty;
        private string GetMontoFactura(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.montoFactura : string.Empty;
        private string GetNITCliente(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.NITCliente : string.Empty;
        private string GetNITComprador(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.NITComprador : string.Empty;
        private string GetNombreComprador(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.NombreComprador : string.Empty;
        /*private (string telefono, string direccion, string monto, string descripcion) GetFactura(string fromPhoneNumber)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data))
            {
                return (data.Factura_Telefono, data.Factura_dirección, data.Factura_Monto, data.Factura_Descripcón);
            }
            return (null, null, null, null);observaciones
        }*/
        private string GetObservaciones(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.Factura_Observaciones : string.Empty;
        private List<string> GetProductosEnFacturaList(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.ListaProductos : new List<string>();
        private List<Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad> GetPrecioUnitarioYCantidad(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.ListaPreciosYCantidades : new List<Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad>();

    }
}
