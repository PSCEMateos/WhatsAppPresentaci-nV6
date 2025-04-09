
using WhatsAppPresentacionV6.Modelos;
using WhatsAppPresentacionV6.Servicios;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text;

namespace WhatsAppPresentacionV6.Controllers
{
    //[ApiController]
    //[Route("api/chatbot-presentacion")]
    public class PresentacionController : ControllerBase
    {
        private readonly WhatsAppMessageService _messageSendService;
        private readonly HanldeDocument _hanldeDocument;
        private readonly WhatsAppFlow _manejoDeComprador;
        private static string _lastMessageState = string.Empty;
        private readonly FlowDecryptionEncryptionService _decryptionEncryptionService;
        private static readonly List<string> _nombres = new()
        {
            "Jesús Trejo", "Jesús Garza", "Jesús Zepeda", "Alejandro Trejo",
            "Alejandro Garza", "Javier Zepeda", "Carlos Mendoza", "María López",
            "Sofía Fernández", "Juan Hernández", "Luis Martínez", "Ana González",
            "Roberto Cruz", "Lucía Vargas", "Pablo Ramírez", "Gabriela Castillo",
            "Fernando Pérez", "Daniela Sánchez", "Héctor Morales", "Paola Jiménez"
        };
        private static readonly List<Flow_response_json_Model_1142951587576244_Crear_Producto> _productos = new(){};
        private static readonly List<string> _idNombres = new()
        {
            "TEDJ800706QA1", "TEDJ800706QA2", "TEDJ800706QA3", "TEDJ800706QA4",
            "TEDJ800706QA5", "TEDJ800706QA6", "TEDJ800706QA7", "TEDJ800706QA8",
            "TEDJ800706QA9", "TEDJ800706QB1", "TEDJ800706QB2", "TEDJ800706QB3",
            "TEDJ800706QB4", "TEDJ800706QB5", "TEDJ800706QB6", "TEDJ800706QB7",
            "TEDJ800706QB8", "TEDJ800706QB9", "TEDJ800706QC1", "TEDJ800706QC2"
        };
        private static List<string> _receivedMessages = new List<string>();
        private static List<(string ButtonId, string ButtonLabelText)> _botones = new();
        private static Dictionary<string, (string lastMessageState, string oldUserMessage, 
            string montoFactura, string tipoProgramación, string usoCFDI, 
            string usuarioAUsar, string usuarioAUsarID, string NITCliente, 
            string NITComprador, string NombreComprador, List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> clientExistance, byte valueInList)> _RegisteredPhoneDicitonary = new Dictionary<string, (string lastMessageState, string oldUserMessage,
            string montoFactura, string tipoProgramación, string usoCFDI,
            string usuarioAUsar, string usuarioAUsarID, string NITCliente,
            string NITComprador, string NombreComprador, List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> clientExistance, byte valueInList)>();

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
                _receivedMessages.Add("Raw JSON recibido:");
                _receivedMessages.Add(rawBody);
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
            _receivedMessages.Add("1");

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

            _receivedMessages.Add("Mensaje válido"); //Marca que está funcionando
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
                        "", // tipoProgramación
                        "", // usoCFDI
                        "", // usuarioAUsar
                        "", // usuarioAUsarID
                        "", // NITCliente
                        "", // NITComprador
                        "",  // _NombreComprador
                        new List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> { },
                        0
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
            // Return the list of received messages as the response 
            return _receivedMessages;
        }

        //Recabamos los Mensajes VIA GET 
        [HttpGet]
        //DENTRO DE LA RUTA webhook 
        [Route("Service/Logs")]
        public dynamic GetWhatsAppFlowServiceLog()
        {
            // Return the list of received messages as the response 
            return _manejoDeComprador.GetEventsList();
        }
        [HttpGet]
        //DENTRO DE LA RUTA webhook 
        [Route("Service/Compradores/List")]
        public dynamic GetCompradores()
        {
            // Return the list of received messages as the response 
            return _manejoDeComprador.ReturnCompradores();
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

            //if(GetLastMessageState(fromPhoneNumber) == )

            _receivedMessages.Add($"Selecting path");
            switch (selectedButtonId)
            {
                case "opcion_Generar_Factura_Text":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Qué NIT o nombre usarás para facturar?");
                    messageStatus = messageStatus + await _messageSendService.EnviarTexto(fromPhoneNumber, "Escribe el nombre o el NIT");
                    break;

                case "opcion_Enviar_Documento_Firmar":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Favor de enviar el documento");
                    break;

                case "opcion_Programación_Aplicaciones"://Paso 11: El usuario elige Programación de aplicaciones
                                                        //Paso 12: Bot: "Dame la cantidad a cobrar antes de impuestos."
                    
                    //_tipoProgramación = "Programación de Aplicaciones";
                    UpdatetTipoProgramación(fromPhoneNumber, "Programación de Aplicaciones");
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Dame la cantidad a cobrar antes de impuestos");
                    break;

                case "opcion_Programación_videojuegos"://Paso 11: El usuario elige Programación de aplicaciones
                                                       //Paso 12: Bot: "Dame la cantidad a cobrar antes de impuestos."

                    //_tipoProgramación = "Programación de Videojuegos";
                    UpdatetTipoProgramación(fromPhoneNumber, "Programación de Videojuegos");
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Dame la cantidad a cobrar antes de impuestos");
                    break;

                case "opcion_Recibir_Registro_Flow":
                    //messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "568198656271468", $"opcionRecibirRegistroFlow{fromPhoneNumber}", "published", $"Tu teléfono {fromPhoneNumber} aún no está registrado.", "¡Regístrate!");// sustituir por "published" al publicar flow
                    (string, string)? phoneExistance = _manejoDeComprador.TeléfonoExiste(fromPhoneNumber);

                    string messageText = phoneExistance != null
                        ? $"Tu teléfono {fromPhoneNumber} está registrado con {phoneExistance.Value.Item1} - {phoneExistance.Value.Item2}"
                        : $"Tu teléfono {fromPhoneNumber} aún no está registrado";
                    if (phoneExistance != null)
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, $"Para registrar una persona física deberás llenar el siguiente formulario.\n Para modificar una persona física existente, registrar usando mismo nombre y NIT.");
                    }
                    else
                    {
                        messageText = "Para registrar una persona física deberás llenar el siguiente formulario";
                    }
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "637724539030495", $"opcionRecibirRegistroFlow{fromPhoneNumber}", "published", messageText, "Registrar");
                    
                        break;
                case "opcion_Registrar_Empresa_Flow":

                    (string, string)? phoneCompanyExistance = _manejoDeComprador.TeléfonoExiste(fromPhoneNumber);

                    string messageCompanyText = phoneCompanyExistance != null
                        ? $"está registrado con {phoneCompanyExistance.Value.Item1} - {phoneCompanyExistance.Value.Item2}"
                        : "aún no está registrado";

                    string buttonCompanyText = phoneCompanyExistance != null
                        ? $"Modificar"
                        : "Registrar";

                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1187351356327089", $"opcionRegistrarEmpresaFlow{fromPhoneNumber}", "published", $"Para registrar una empresa deberás llenar el siguiente formulario"/*$"Tu teléfono {fromPhoneNumber} {messageCompanyText}."*/, "Registrar"/*buttonCompanyText*/);// sustituir flow correcto
                    break;
                case "opcion_Generar_Factura_Flow_0_1":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1877921042740421", $"opcionGenerarFacturaFlow{fromPhoneNumber}", "published", $"Registrando Cliente", "Registrar");
                    break;
                case "opcion_Registrar_Cliente":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1297640437985053", $"opcionRegistrarCliente{fromPhoneNumber}", "published", $"Registrando Cliente", "Registrar");
                    break;
                case "opcion_Continuar_Con_La_Factura":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "682423707677994", $"opcionFacturar{fromPhoneNumber}", "published", $"Desea facturar?", "Facturar");
                    break;
                case "opcion_Cancelar":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Elegir_tipo_Registro", "Regístrame"), ("opcion_Generar_Factura_Text ", "Generar factura"), ("opcion_configurar", "Configurar") };//{ ("opcion_Recibir_Registro_Flow", "Regístrame"), ("opcion_Registrar_Empresa_Flow", "Registrar Empresa"), ("opcion_Generar_Factura_Text ", "Generar factura") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Soy Timbrame Bot, ¿En que puedo ayudarte?", "Selecciona opción", "Hay 2 opciones", _botones);
                    break;
                case "opcion_configurar":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Configurar_Registros_DIAN", "Registros DIAN"), ("opcion_Configurar_Productos ", "Productos"), ("opcion_configurar_Clientes", "Clientes") };//{ ("opcion_Recibir_Registro_Flow", "Regístrame"), ("opcion_Registrar_Empresa_Flow", "Registrar Empresa"), ("opcion_Generar_Factura_Text ", "Generar factura") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que quieres configurar?", "Selecciona opción", "Hay 3 opciones", _botones);
                    break;
                case "opcion_configurar_Clientes":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Agregar_Clientes", "Agregar"), ("opcion_Modificar_Clientes ", "Modificar") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que quieres hacer con un cliente?", "Selecciona opción", "Hay 2 opciones", _botones);
                    UpdatetTipoProgramación(fromPhoneNumber, "");
                    break;
                case "opcion_Agregar_Clientes":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Que es tu NIT o nombre de usuario?");
                    break;
                case "opcion_Modificar_Clientes":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Que es tu NIT o nombre de usuario?");
                    break;
                case "opcion_Modificar_Clientes_Persona_Natural":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿A quien modificas? Puedes escribir su NIT, Nombre o una parte de estos");
                    break;
                case "opcion_Modificar_Clientes_Persona_Juridica":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Que empresa modificas? Puedes escribir su NIT, Razon social o una parte de estos");
                    break;
                case "opcion_Configurar_Productos":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Configurar_Productos_agregar", "Agregar"), ("opcion_Configurar_Productos_Modificar ", "Modificar") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que quieres hacer con productos?", "Selecciona opción", "Hay 3 opciones", _botones);
                    break;
                case "opcion_Configurar_Productos_agregar":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1142951587576244", $"opcionCrearProductoFlow{fromPhoneNumber}", "published", $"Creando Producto", "Crear");
                    break;
                case "opcion_Configurar_Productos_Modificar":
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿Que producto modificas? Puedes escribir su número, nombre o una parte de estos");
                    break;
                case "opcion_Configurar_Registros_DIAN":
                    UpdateRestart(fromPhoneNumber);

                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¡Registros DIAN configurados!");
                    break;
                case "opcion_Recibir_Elegir_tipo_Registro":
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Registro_Flow", "Persona física"), ("opcion_Registrar_Empresa_Flow", "Empresa") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Registrando, ¿A quien quieres registrar?", "Selecciona opción", "Hay 2 opciones", _botones);
                    break;
                case "opcion_Generar_Factura_Flow_0":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1277233923341533", $"opcionGenerarIdentidadFlow{fromPhoneNumber}", "published", $"Favor de comprobar identidad", "Responder");
                    break;
                default:

                    if (_idNombres.Contains(selectedButtonId))
                    {
                        //_usuarioAUsar = _nombres[_idNombres.IndexOf(selectedButtonId)];// Paso 7: El bot recibe nombre
                        UpdateUsuarioAUsar(fromPhoneNumber, _nombres[_idNombres.IndexOf(selectedButtonId)]);
                        //_usuarioAUsarID = selectedButtonId;
                        UpdateUsuarioAUsarID(fromPhoneNumber, selectedButtonId);
                        //Paso 8: Bot: responde con un mensaje "¡Bien! ¿Que producto vamos a facturar a {nombre} ({ID})?"
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, $"¡Bien! ¿Que producto vamos a facturar a {GetUsuarioAUsar(fromPhoneNumber)/*_usuarioAUsar*/} ({GetUsuarioAUsarID(fromPhoneNumber)/*_usuarioAUsarID*/})?");
                        selectedButtonId = "ID_De_Nombre";
                    }
                    else
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Opción no reconocida.");
                    }
                    
                    break;
            }

            _receivedMessages.Add($"Updating _lastMessageState");
            //_lastMessageState = selectedButtonId;
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
            if (selectedListId == "opcion_Registrar_Cliente")
            {
                messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1297640437985053", $"opcionRegistrarCliente{fromPhoneNumber}", "published", $"Registrando Cliente", "Registrar");
                //_lastMessageState = selectedListId;
                UpdateLastMessageState(fromPhoneNumber, selectedListId);
                _receivedMessages.Add($"List opcion_Registrar_Cliente");
                return messageStatus;
            }
            switch (GetLastMessageState(fromPhoneNumber))//(_lastMessageState)
            {
                case "ruta_Factura_programación_3":
                    switch (selectedListId)
                    {
                        case "Gastos_en_General_ID":
                            //_usoCFDI = "Gastos en general ";
                            UpdateUsoCFDI(fromPhoneNumber, "Gastos en general ");
                            messageStatus = await SendInvoiceMessagesAsync(fromPhoneNumber);
                            break;

                        case "Adquisicion_de_Mercancia_ID":
                            //_usoCFDI = "Adquisición de mercancía ";
                            UpdateUsoCFDI(fromPhoneNumber, "Adquisición de mercancía ");
                            messageStatus = await SendInvoiceMessagesAsync(fromPhoneNumber);
                            break;

                        case "Honorarios_Medicos_Dentales_y_Gastos_Hospitalarios_ID":
                            //_usoCFDI = "Honorarios médicos, dentales y gastos hospitalarios";
                            UpdateUsoCFDI(fromPhoneNumber, "Honorarios médicos, dentales y gastos hospitalarios");
                            messageStatus = await SendInvoiceMessagesAsync(fromPhoneNumber);
                            break;

                        case "Pagos_Por_Servicios_Educativos_ID":
                            //_usoCFDI = "Pagos por servicios educativos (colegiaturas)";
                            UpdateUsoCFDI(fromPhoneNumber, "Pagos por servicios educativos (colegiaturas)");
                            messageStatus = await SendInvoiceMessagesAsync(fromPhoneNumber);
                            break;

                        case "Devoluciones_Descuentos_o_Bonificaciones_ID":
                            //_usoCFDI = "Devoluciones, descuentos o bonificaciones ";
                            UpdateUsoCFDI(fromPhoneNumber, "Devoluciones, descuentos o bonificaciones ");
                            messageStatus = await SendInvoiceMessagesAsync(fromPhoneNumber);
                            break;
                        default:
                            messageStatus = await _messageSendService.EnviarMensaje(fromPhoneNumber, "Seleccionar otra opción");
                            //selectedListId = _lastMessageState;
                            UpdateLastMessageState(fromPhoneNumber, selectedListId);
                            break;
                    }
                    _receivedMessages.Add($"Updating _lastMessageState");
                    //_lastMessageState = selectedListId;
                    UpdateLastMessageState(fromPhoneNumber, selectedListId);
                    break;
                case "opcion_Generar_Factura_Cliente":
                    messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "682423707677994", $"opcionFacturar{fromPhoneNumber}", "published", "Cliente Identificado, ¿Desea facturar?", "Facturar");
                    //_NITCliente = selectedListId;
                    UpdateNITCliente(fromPhoneNumber, selectedListId);
                    UpdateLastMessageState(fromPhoneNumber, "");
                    //_lastMessageState = "";
                    //_oldUserMessage = "0";
                    UpdateOldUserMessage(fromPhoneNumber, "0");
                    _receivedMessages.Add($"List Registrado, A facturar");
                    break;
            }
            //switch (selectedListId)
            //{
            //    case "opcion1":
            //        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Seleccionó de una lista");
            //        break;
            //}
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
                UpdateRestart(fromPhoneNumber);
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¡Reiniciando todo!");
                return messageStatus;
            }
            switch (GetLastMessageState(fromPhoneNumber))
            {
                case "":  
                         
                    if (userMessage != GetOldUserMessage(fromPhoneNumber)/*_oldUserMessage*/)
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¡Hola!");
                        _receivedMessages.Add("Mensaje inicial: " + messageStatus);
                        //Construye la lista del botón a enviar.
                        //_botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Generar_Factura_Text ", "Generar factura"), ("opcion_Enviar_Documento_Firmar", "Enviar documento"), ("opcion_Recibir_Registro_Flow", "Proceso de registro") };
                        //_botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Registro_Flow", "Regístrame"), ("opcion_Registrar_Empresa_Flow", "Registrar Empresa"), ("opcion_Generar_Factura_Text ", "Generar factura") };

                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Elegir_tipo_Registro", "Regístrame"), ("opcion_Generar_Factura_Text ", "Generar factura"), ("opcion_configurar", "Configurar") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Soy Timbrame Bot, ¿En que puedo ayudarte?", "Selecciona opción", "Hay 2 opciones", _botones);
                        
                        _receivedMessages.Add("Mensaje: " + userMessage + " De: " + fromPhoneNumber);
                    }
                    break;

                case "opcion_Generar_Factura_Text"://Paso 5: El usuario da un nombre
                                                   //Paso 6: Bot muestra lista: "encontré a:"

                    _receivedMessages.Add($"Text opcion_Generar_Factura_Text");
                    if (!string.IsNullOrWhiteSpace(userMessage) && GetOldUserMessage(fromPhoneNumber)/*_oldUserMessage*/ != userMessage)
                    {
                        (string, string)? phoneExistance = _manejoDeComprador.CompradorExisteKeys(userMessage);
                        if (phoneExistance != null)
                        {
                            messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, $"{phoneExistance.Value.Item1} - {phoneExistance.Value.Item2}\n¿A quién facturarás?\nPuedes escribir su NIT, Nombre o una parte de estos");
                            //_lastMessageState = "opcion_Generar_Factura_Cliente";
                            UpdateLastMessageState(fromPhoneNumber, "opcion_Generar_Factura_Cliente");
                            //_NITComprador = phoneExistance.Value.Item2;
                            UpdateNITComprador(fromPhoneNumber, phoneExistance.Value.Item2);
                            //_NombreComprador = phoneExistance.Value.Item1;
                            UpdateNombreComprador(fromPhoneNumber, phoneExistance.Value.Item1);
                            _receivedMessages.Add($"Text Nit Reconocido");
                        }
                        else
                        {
                            _receivedMessages.Add($"Text Nit No Reconocido");
                            //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Este NIT no lo tienes registrado, por favor completa tu registro");
                            //_receivedMessages.Add($"Simple message: {messageStatus}");
                            _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Registro_Flow", "Persona física"), ("opcion_Registrar_Empresa_Flow", "Empresa"), ("opcion_Cancelar", "Cancelar") };
                            messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "NIT no registrado, por favor completa tu registro", "Selecciona opción", "2 opciones para registrar", _botones);
                            _receivedMessages.Add($"Button message: {messageStatus}");
                        }
                        //messageStatus = await EnviarListaCurada(userMessage, fromPhoneNumber);
                    }
                    else
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Por favor, envía un VALOR válido.");
                    }
                    break;

                case "opcion_Generar_Factura_Cliente":
                    _receivedMessages.Add($"Text opcion_Generar_Factura_Cliente");
                    if (!string.IsNullOrWhiteSpace(userMessage) && GetOldUserMessage(fromPhoneNumber)/*_oldUserMessage*/ != userMessage)
                    {

                        List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> clientExistance = _manejoDeComprador.BuscarListaClientes(GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber),/*_NombreComprador, _NITComprador,*/ userMessage );
                        _receivedMessages.Add($"Hay lista de clientes? {clientExistance.ToString()}");
                        if (clientExistance.Any())
                        {
                            _receivedMessages.Add($"Text opcion_Generar_Factura_Cliente Cliente existe");
                            var totalClients = clientExistance.Count;
                            int totalSets = (int)Math.Ceiling(totalClients / 10.0);
                            var listaSeccionesClientes = new List<(string, List<(string, string, string)>)>();
                            for (int i = 0; i < totalSets; i++)
                            {
                                string setTitle = $"Set {i + 1}";

                                var opciones = clientExistance
                                    .Skip(i * 10) // Skip previous sets
                                    .Take(10)     // Take up to 10 clients per set
                                    .Select((client, index) => (
                                        client.screen_0_NIT_4 ?? $"NIT_{i}_{index}", // OptionId (Fallback in case NIT is null)
                                        (i * 10 + index + 1).ToString(), // OptionTitle (1, 2, 3, ...)
                                        $"{client.screen_0_Primer_Nombre_0} {client.screen_0_Segundo_Nombre_1} {client.screen_0_Apellido_Paterno_2} {client.screen_0_Apellido_Materno_3}".Trim() // OptionDescription
                                    ))
                                    .ToList();

                                listaSeccionesClientes.Add((setTitle, opciones));
                            }

                            listaSeccionesClientes.Add(("Other", new List<(string, string, string)>
                            {
                                ("opcion_Registrar_Cliente", "Registrar", "Elejir opción para registrar nuevo cliente")
                            }));
                            messageStatus = await _messageSendService.EnviarListaDeOpciones(fromPhoneNumber, "Lista de Clientes", "Seleccionar una coincidencia o mandar nombre diferente", $"Hay {totalClients} coincidencias", "Expandir opciones", listaSeccionesClientes);
                        }
                        else
                        {
                            _receivedMessages.Add("No existe Cliente en comprador");
                            _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Registrar_Cliente"/*"opcion_Agregar_Clientes"*/, "Registrar Cliente"), ("opcion_Generar_Factura_Text", "Volver a intentar") };
                            messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "No encontré este NIT en tus clientes", "Selecciona opción", "Lo vaz a registrar?", _botones);
                            UpdatetTipoProgramación(fromPhoneNumber, "opcion_Generar_Factura_Cliente");
                            _receivedMessages.Add(messageStatus);
                            UpdateOldUserMessage(fromPhoneNumber, "No existe Cliente en comprador");
                            return messageStatus;
                        }
                    }
                    else
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Por favor, envía un VALOR válido.");
                    }
                    break;

                case "opcion_Enviar_Documento_Firmar":
                    if (userMessage == "documento")
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Aquí está el documento firmado");
                        //_lastMessageState = "";// Reset del estado después de procesar
                        UpdateLastMessageState(fromPhoneNumber, "");
                        _receivedMessages.Add("Ruta opcion_Enviar_Documento_Firmar: " + messageStatus);
                        messageStatus = await _messageSendService.EnviarDocumentoPorUrl(fromPhoneNumber, "https://test-timbrame.azurewebsites.net/Ejemplo/24a8a905-f155-4726-a27f-1451a8bf5388.pdf", "DocumentoFirmado.pdf");
                    }
                    else { messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Eso no es documento"); }
                    break;

                case "ID_De_Nombre"://Paso 9: El bot recibe mensaje progra
                                    //Paso 8: Bot: muestra lista de productos
                    if (userMessage.StartsWith("prog", StringComparison.OrdinalIgnoreCase))
                    {
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Programación_Aplicaciones", "aplicaciones"), ("opcion_Programación_videojuegos", "videojuegos"), };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Productos de", "Programación", "de", _botones);
                    }
                    else
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "No tenemos ese producto");
                    }
                    break;

                case "opcion_Programación_Aplicaciones": case "opcion_Programación_videojuegos": //Paso 13: El bot recibe mensaje cantidad
                                                                                                 //Paso 14: Bot: muestra lista de productos
                    //_montoFactura = userMessage;
                    UpdateMontoFactura(fromPhoneNumber, userMessage);
                    //_lastMessageState = "Mandando lista";
                    UpdateLastMessageState(fromPhoneNumber, "Mandando lista");
                    var listaSecciones = new List<(string, List<(string, string, string)>)>
                    {
                        ("CFDI para factura", new List<(string, string, string)>
                        {
                            ("Gastos_en_General_ID", "1", "Gastos en general"),
                            ("Adquisicion_de_Mercancia_ID", "2", "Adquisicion de mercancia"),
                            ("Honorarios_Medicos_Dentales_y_Gastos_Hospitalarios_ID", "3", "Honorarios medicos, dentales y gastos hospitalarios"),
                            ("Pagos_Por_Servicios_Educativos_ID", "4", "Pagos por servicios educativos"),
                            ("Devoluciones_Descuentos_o_Bonificaciones_ID", "5", "Devoluciones, descuentos o bonificaciones")
                        }),
                        ("Seccion 2", new List<(string, string, string)>
                        {
                            ("No_Seleccionar_1", "No seleccionar", "Esto está para que no lo selecciones"),
                            ("No_Seleccionar_2", "No seleccionar", "NO LO ELIJAS")
                        })
                    };
                    messageStatus = await _messageSendService.EnviarListaDeOpciones(fromPhoneNumber, "Uso de CFDI", "Favor de indicar el uso de CFDI para esta factura", "Hay 5 opciones", "Expandir opciones",listaSecciones);

                    //await _messageSendService.EnviarTexto(fromPhoneNumber, "Marca el número para definir el uso de CFDI para esta factura");
                    //await _messageSendService.EnviarTexto(fromPhoneNumber, "1 para Gastos en general \n2 para Adquisición de mercancía \n3 para Honorarios médicos, dentales y gastos hospitalarios \n4 para Pagos por servicios educativos (colegiaturas) \n5 para Devoluciones, descuentos o bonificaciones");
                    //_lastMessageState = "ruta_Factura_programación_3";
                    UpdateLastMessageState(fromPhoneNumber, "ruta_Factura_programación_3");
                    break;
                case "opcion_Agregar_Clientes":
                    if(_manejoDeComprador.CompradorExiste(userMessage, userMessage))
                    {
                        UpdateNITComprador(fromPhoneNumber, userMessage);
                        UpdateNombreComprador(fromPhoneNumber, userMessage);
                        messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1584870855544061", $"opcionAgregarClienteFlow{fromPhoneNumber}", "published", $"Registrando Cliente", "Registrar");
                    }
                    else
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Nombre o NIT no reconocido, favor de volver a intentar");
                    }
                        break;
                case "opcion_Modificar_Clientes":
                    if (_manejoDeComprador.CompradorExiste(userMessage, userMessage))
                    {
                        UpdateNITComprador(fromPhoneNumber, userMessage);
                        UpdateNombreComprador(fromPhoneNumber, userMessage);
                        _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Modificar_Clientes_Persona_Natural", "Natural"), ("opcion_Modificar_Clientes_Persona_Juridica", "Jurídica") };
                        messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "¿Que tipo de persona es tu cliente?", "Selecciona opción", "Hay 2 opciones", _botones);
                    }
                    else
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Nombre o NIT no reconocido, favor de volver a intentar");
                    }
                    break;
                case "opcion_Modificar_Clientes_Persona_Natural":
                    if (_manejoDeComprador.ClienteExisteEnComprador(userMessage, GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber)))
                    {
                        messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "931945452349522", $"opcionModCliNaturalFlow{fromPhoneNumber}", "published", $"Modificando Cliente", "Modificar");
                    }
                    else
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Nombre o NIT no reconocido, favor de volver a intentar");
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
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Nombre o NIT no reconocido, favor de volver a intentar");
                    }
                    UpdateLastMessageState(fromPhoneNumber, "opcion_Modificar_Clientes_Persona_Juridica");
                    break;
                case "opcion_Configurar_Productos_Modificar":
                    if (_productos.Any(c => c.Agregar_Producto_Nombre.Contains(userMessage, StringComparison.OrdinalIgnoreCase)) || _productos.Any(c => c.Agregar_Producto_Codigo.Contains(userMessage, StringComparison.OrdinalIgnoreCase)))
                    {
                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Producto encontrado");
                        UpdateRestart(fromPhoneNumber);
                    }
                    else
                    {

                        messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Producto no encontrado");
                        UpdateRestart(fromPhoneNumber);
                    }
                    break;
                default:
                    await _messageSendService.EnviarTexto(fromPhoneNumber, "Accion no reconocida");
                    //_botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Registro_Flow", "Regístrame"), ("opcion_Registrar_Empresa_Flow", "Registrar Empresa"), ("opcion_Generar_Factura_Text ", "Generar factura") };

                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Elegir_tipo_Registro", "Regístrame"), ("opcion_Generar_Factura_Text ", "Generar factura"), ("opcion_configurar", "Configurar") };
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Soy Timbrame Bot, ¿En que puedo ayudarte?", "Selecciona opción", "Hay 2 opciones", _botones);
                    break;
            }
            if (userMessage != GetOldUserMessage(fromPhoneNumber)/*_oldUserMessage*/) { UpdateOldUserMessage(fromPhoneNumber, userMessage);/*_oldUserMessage = userMessage;*/ }
            _receivedMessages.Add($"Retornando: {messageStatus}");
            UpdateOldUserMessage(fromPhoneNumber, "0");
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
        private async Task<string> EnviarListaCurada(string userMessage, string fromPhoneNumber)
        {
            string messageStatus = "";
            var listaCurada = _nombres
                .Where(r => r.StartsWith(userMessage, StringComparison.OrdinalIgnoreCase))
                .Take(3).ToList();//Compara el nombre recibido sin importar mayusculas y regresa 3 nombres. 
            //El boton interactivo que se usa en esta prueba está limitado a 3 botonoe, cada botón con 20 caracteres incluyendo espacios

            if (listaCurada.Any())//Revisa si el nombre tiene coincidencias
            {
                //Construye la lista de botones, cada boton tiene un {id} y un {body_text}
                _botones = listaCurada.Select(nombre =>
                {
                    int index = _nombres.IndexOf(nombre);
                    string id = _idNombres[index];
                    return (id, nombre);
                }).ToList();

                //Construye el JSON que se usa para enviar el botón interactivo y lo manda por la función EnviarBotonInteractivo del objeto _messageSendService
                messageStatus = await _messageSendService.EnviarBotonInteractivo(
                    fromPhoneNumber,
                    "Selecciona un nombre",
                    "Opciones encontradas",
                    "Haz clic en uno de los nombres:",
                    _botones
                    );
                _receivedMessages.Add("Lista curada: " + messageStatus);
                //_lastMessageState = "ruta_Factura_Nombre_Elegido";
                UpdateLastMessageState(fromPhoneNumber, "ruta_Factura_Nombre_Elegido");
            }
            else
            {
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "No hay coincidencias.");
                _receivedMessages.Add("Sin coincidencias: " + messageStatus);
            }
            _receivedMessages.Add("Empty 1" + messageStatus);
            return messageStatus;
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
        public async Task<string> SendInvoiceMessagesAsync(string fromPhoneNumber/*, string userMessage*/)
        {
            // Send the initial "Generando factura" message
            await _messageSendService.EnviarTexto(fromPhoneNumber, "Generando factura");

            // Send detailed invoice information
            await _messageSendService.EnviarTexto(fromPhoneNumber, $"Facturando a {GetUsuarioAUsarID(fromPhoneNumber)/*_usuarioAUsar*/} ({GetUsuarioAUsarID(fromPhoneNumber)/*_usuarioAUsarID*/}), el producto {GetTipoProgramacion(fromPhoneNumber)/*_tipoProgramación*/}, con la cantidad a cobrar antes de impuestos {GetMontoFactura(fromPhoneNumber)/*_montoFactura*/}, con el uso de CFDI {GetUsoCFDI(fromPhoneNumber)/*_usoCFDI*/}");

            // Save the old user message (optional, depends on your logic)
            //_oldUserMessage = userMessage;

            // Send PDF document via URL
            await _messageSendService.EnviarDocumentoPorUrl(fromPhoneNumber, "https://test-timbrame.azurewebsites.net/Ejemplo/24a8a905-f155-4726-a27f-1451a8bf5388.pdf", "Factura.pdf");
            // Reset the last message state
            //_lastMessageState = "";
            UpdateLastMessageState(fromPhoneNumber, "");
            // Send XML document via URL
            return await _messageSendService.EnviarDocumentoPorUrl(fromPhoneNumber, "https://test-timbrame.azurewebsites.net/Ejemplo/24a8a905-f155-4726-a27f-1451a8bf5388.xml", "Factura.xml");

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
                    if (root.TryGetProperty("Registrar_Persona_Fisica_Nombre_0", out _) && root.TryGetProperty("Registrar_Persona_Fisica_Apellido_Paterno_1", out _) && root.TryGetProperty("Registrar_Persona_Fisica_Apellido_Materno_2", out _))
                    {
                        _receivedMessages.Add("Detected Model 637724539030495, Registrar Persona Física Simple");
                        messageStatus = await HandleInteractiveFlowMessageRegistraPersonaFísica(JsonSerializer.Deserialize<Flow_response_json_Model_637724539030495_RegistrarPersonaFisicaSimple>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("Registrar_Empresa_Nombre_0", out _) && root.TryGetProperty("Registrar_Empresa_NIT_1", out _) && root.TryGetProperty("Registrar_Empresa_Digito_Verificacin_1", out _))
                    {
                        _receivedMessages.Add("Detected Model 1187351356327089, Registrar Empresa");
                        messageStatus = await HandleInteractiveFlowMessageRegistraEmpresa(JsonSerializer.Deserialize<Flow_response_json_Model_1187351356327089_RegistrarEmpresa>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("screen_0_Nombres_0", out _) && root.TryGetProperty("screen_1_NIT_0", out _) && root.TryGetProperty("screen_0_Apellidos_1", out _))
                    {
                        _receivedMessages.Add("Detected Model 568198656271468, Regístrate");
                        messageStatus = await HandleInteractiveFlowMessageRegistrate(JsonSerializer.Deserialize<Flow_response_json_Model_568198656271468_Registrate>(jsonString), fromPhoneNumber);
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("screen_0_Primer_Nombre_0", out _) && root.TryGetProperty("screen_0_NIT_4", out _) && root.TryGetProperty("screen_1_Apellido_Paterno_2", out _))
                    {
                        _receivedMessages.Add("Detected Model 1877921042740421, Factura Colombia");
                        messageStatus = await HandleInteractiveFlowMessageFacturaColombia(JsonSerializer.Deserialize<Flow_response_json_Model_1877921042740421_FacturaColombia>(jsonString), fromPhoneNumber);
                    }
                    else if (root.TryGetProperty("screen_0_Nombre_0", out _) && root.TryGetProperty("screen_0_NIT_1", out _) && root.TryGetProperty("screen_0_NIT_2", out _))
                    {
                        _receivedMessages.Add("Detected Model 1277233923341533, Comprobante NIT Cliente y Combrador");
                        messageStatus = await HandleInteractiveFlowMessageComprobanteNitClienteYCombrador(JsonSerializer.Deserialize<Flow_response_json_Model_1277233923341533_ComprobanteNitClienteYCombrador>(jsonString), fromPhoneNumber);
                        //_lastMessageState = "";
                        //_oldUserMessage = "";
                        return messageStatus;
                    }
                    else if (root.TryGetProperty("screen_0_Telfono_0", out _) && root.TryGetProperty("screen_0_Direccin_1", out _) && root.TryGetProperty("screen_0_Monto_2", out _) && root.TryGetProperty("screen_0_Descripcin_3", out _))
                    {
                        _receivedMessages.Add("Detected Model 682423707677994, Información Factura");
                        messageStatus = await HandleInteractiveFlowMessageInformaciónFactura(JsonSerializer.Deserialize<Flow_response_json_Model_682423707677994_InformacionFactura>(jsonString), fromPhoneNumber);
                        return messageStatus;

                    }
                    else if (root.TryGetProperty("screen_0_Primer_Nombre_0", out _) && root.TryGetProperty("screen_0_Apellido_Paterno_2", out _) && root.TryGetProperty("screen_0_Apellido_Materno_3", out _) && root.TryGetProperty("screen_0_NIT_4", out _))
                    {
                        _receivedMessages.Add("Detected Model 1297640437985053, Información del clente");
                        messageStatus = await HandleInteractiveFlowMessageInformaciónDelCliente(JsonSerializer.Deserialize<Flow_response_json_Model_1297640437985053_InformacionDelCliente>(jsonString), fromPhoneNumber);
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
                    else
                    {
                        
                        messageStatus = "Unknown response format";
                        _receivedMessages.Add("Flow Error: Unknown response format");
                        throw new Exception("Unknown response format.");
                    }
                }

                //_botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Registro_Flow", "Regístrame"), ("opcion_Registrar_Empresa_Flow", "Registrar Empresa"), ("opcion_Generar_Factura_Text ", "Generar factura") };
                _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Elegir_tipo_Registro", "Regístrame"), ("opcion_Generar_Factura_Text ", "Generar factura"), ("opcion_configurar", "Configurar") };
                _receivedMessages.Add("Button Created");
                messageStatus = messageStatus + "\n" + await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Soy Timbrame Bot, ¿En que puedo ayudarte?", "Selecciona opción", "Hay 2 opciones", _botones);
                _receivedMessages.Add("Button Sent");
                _receivedMessages.Add($"Messages status: {messageStatus}");
                //_lastMessageState = "";
                UpdateLastMessageState(fromPhoneNumber, "");
                //_oldUserMessage = "";
                UpdateOldUserMessage(fromPhoneNumber, "");
                return messageStatus;
            }
            catch (Exception parseEx)
            {
                _receivedMessages.Add($"Error parsing response_json: {parseEx.Message}");
                return $"Error parsing response_json: {parseEx.Message}";
            }
        }
        private async Task<string> HandleInteractiveFlowMessageRegistrate(Flow_response_json_Model_568198656271468_Registrate parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add("Parsed Regístrate response_json:");
            _receivedMessages.Add(JsonSerializer.Serialize(parsedResponseJson, new JsonSerializerOptions { WriteIndented = true }));
            //Mandar el Summary
            string summary = $"*Resumen del Formulario Recibido:*\n" +
                     $"*Nombre:* {parsedResponseJson.screen_0_Nombres_0} {parsedResponseJson.screen_0_Apellidos_1}\n"; 
            /*if (parsedResponseJson.screen_0_Correo_2 != "UNKNOWN")
                summary = summary + $"*Correo:* {parsedResponseJson.screen_0_Correo_2}\n";*/

            summary = summary +/* $"*Teléfono:* {parsedResponseJson.screen_0_Telfono_3}\n" +*/
                     $"*NIT:* {parsedResponseJson.screen_1_NIT_0} - {parsedResponseJson.screen_1_Digito_Verificacin_1}\n" +
                     $"*Departamento:* {parsedResponseJson.screen_1_Departamento_3}\n Ciudad: {parsedResponseJson.screen_1_Ciudad_4}\n";

            switch (parsedResponseJson.screen_0_Tipo_de_persona_4)
            {
                case "0_Persona_Jurídica_y_asimiladas":
                    summary = summary + $"*Tipo de Persona:* Persona Jurídica y asimiladas\n";
                    break;
                case "1_Persona_Natural_y_asimiladas":
                    summary = summary + $"*Tipo de Persona:* Persona Natural y asimiladas\n";
                    break;
            }
            switch (parsedResponseJson.screen_0_Tipo_Identificacin_5)
            {
                case "0_Registro_Civil":
                    summary = summary + $"*Tipo de Identificación:* Registro Civil\n";
                    break;
                case "1_Cédula_de_Ciudadanía":
                    summary = summary + $"*Tipo de Identificación:* Cédula de Ciudadanía\n";
                    break;
                case "2_Tarjeta_de_extrangería":
                    summary = summary + $"*Tipo de Identificación:* Tarjeta de extrangería\n";
                    break;
                case "3_Cédula_de_extranjería":
                    summary = summary + $"*Tipo de Identificación:* Cédula de extranjería\n";
                    break;
                case "4_NIT":
                    summary = summary + $"*Tipo de Identificación:* NIT\n";
                    break;
                case "5_Pasaporte":
                    summary = summary + $"*Tipo de Identificación:* Pasaporte\n";
                    break;
                case "6_Documento_de_identificación_extranjero":
                    summary = summary + $"*Tipo de Identificación:* Documento de identificación extranjero\n";
                    break;
                case "7_PEP_(Permiso_Especial_de_Permanencia)":
                    summary = summary + $"*Tipo de Identificación:* PEP (Permiso Especial de Permanencia)\n";
                    break;
            }
            switch (parsedResponseJson.screen_1_Tipo_Rgimen_5)
            {
                case "0_Impuesto_sobre_ventas_-_IVA":
                    summary = summary + $"*Régimen Tributario:* Impuesto sobre ventas\n";
                    break;
                case "1_No_responsable_de_IVA":
                    summary = summary + $"*Régimen Tributario:* No responsable de IVA\n";
                    break;
            }
            switch (parsedResponseJson.screen_1_Obligaciones_Fiscale_6)
            {
                case "0_Gran_contribuyente":
                    summary = summary + $"*Obligaciones Fiscales:* Gran contribuyente\n";
                    break;
                case "1_Autorretenedor":
                    summary = summary + $"*Obligaciones Fiscales:* Autorretenedor\n";
                    break;
                case "2_Agente_de_retención_IVA":
                    summary = summary + $"*Obligaciones Fiscales:* Agente de retención IVA\n";
                    break;
                case "3_Régimen_simple_tributación":
                    summary = summary + $"*Obligaciones Fiscales:* Régimen simple tributación\n";
                    break;
                case "4_No_aplica-_Otros":
                    summary = summary + $"*Obligaciones Fiscales:* No aplica\n";
                    break;
            }
            _receivedMessages.Add("Summary: ");
            _receivedMessages.Add(summary);
            _receivedMessages.Add("Flow Token: ");
            _receivedMessages.Add(parsedResponseJson.flow_token);
            string messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, summary);
            _receivedMessages.Add("Summary sent");
            string compradpr = _manejoDeComprador.CrearComprador(parsedResponseJson, fromPhoneNumber);
            _receivedMessages.Add("Comprador registrado");
            _receivedMessages.Add(compradpr);

            _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Generar_Factura_Text ", "Generar factura"), ("opcion_Cancelar", "Regresar") };
            messageStatus = messageStatus + "\n" + await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Gracias por registrarte, ¿Deseas proceder con la factura?", "Selecciona opción", "Facturar o regresar", _botones);

            _receivedMessages.Add("Solicitud generar factura");
            _receivedMessages.Add(messageStatus);

            return messageStatus + "; " + compradpr;
        }
        private async Task<string> HandleInteractiveFlowMessageFacturaColombia(Flow_response_json_Model_1877921042740421_FacturaColombia parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add("Parsed Factura Colombia response_json:");
            _receivedMessages.Add(JsonSerializer.Serialize(parsedResponseJson, new JsonSerializerOptions { WriteIndented = true }));
            //Mandar el Summary
            string summary = $"*Resumen del Formulario Recibido:*\n" +
                     $"\n*Información del Vendedor*\n" +
                     $"*Nombre:* {parsedResponseJson.screen_0_Primer_Nombre_0} {parsedResponseJson.screen_0_Segundo_nombre_1} {parsedResponseJson.screen_0_Apellido_Paterno_2} {parsedResponseJson.screen_0_Apellido_Materno_3}\n" +
                     $"*NIT:* {parsedResponseJson.screen_0_NIT_4}\n";

            if (parsedResponseJson.screen_0_Correo_Electrnico_5 != "UNKNOWN")
                summary = summary + $"*Correo:* {parsedResponseJson.screen_0_Correo_Electrnico_5}\n";
            
            summary= summary +  $"\n*Información del Comprador*\n" +
                     $"*Nombre:* {parsedResponseJson.screen_1_Primer_Nombre_0} {parsedResponseJson.screen_1_Segundo_Nombre_1} {parsedResponseJson.screen_1_Apellido_Paterno_2} {parsedResponseJson.screen_1_Apellido_Materno_3}\n" +
                     $"*NIT:* {parsedResponseJson.screen_1_NIT_4}\n" +
                     $"*Razón social:* {parsedResponseJson.screen_1_Razn_social_5}\n" +

                     $"\n*Información de Factura*\n" +
                     $"*Dirección:* {parsedResponseJson.screen_2_Direccin_0}\n" +
                     $"*Teléfono:* {parsedResponseJson.screen_2_Telfono_1}\n" +
                     $"*Descripción:* {parsedResponseJson.screen_2_Descripcin_2}\n" +
                     $"*Cantidad:* {parsedResponseJson.screen_2_Cantidad_3}";

            _receivedMessages.Add("Summary: ");
            _receivedMessages.Add(summary);
            _receivedMessages.Add("Flow Token: ");
            _receivedMessages.Add(parsedResponseJson.flow_token);
            string messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, summary);
            _receivedMessages.Add("Summary sent");
            return messageStatus;
        }
        private async Task<string> HandleInteractiveFlowMessageComprobanteNitClienteYCombrador(Flow_response_json_Model_1277233923341533_ComprobanteNitClienteYCombrador parsedResponseJson, string fromPhoneNumber)
        {
            try
            {
                string messageStatus;
                _receivedMessages.Add("Parsed Comprobante NIT cliente y combrador response_json:");
                _receivedMessages.Add(JsonSerializer.Serialize(parsedResponseJson, new JsonSerializerOptions { WriteIndented = true }));

                _receivedMessages.Add("Flow Token: ");
                _receivedMessages.Add(parsedResponseJson.flow_token);

                //Comprobar si el comprador está registrado
                bool existenciaComprador = _manejoDeComprador.CompradorExiste(parsedResponseJson.screen_0_Nombre_0, parsedResponseJson.screen_0_NIT_1);
                if (!existenciaComprador)
                {
                    _receivedMessages.Add("No existe comprador");
                    messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Comprador no registrado o nombre incorrecto");
                    _receivedMessages.Add("Comprador no registrado o nombre incorrecto");
                    _receivedMessages.Add(parsedResponseJson.screen_0_NIT_1);

                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Recibir_Registro_Flow", "Regístrame"), ("opcion_Generar_Factura_Text", "Generar factura") /*,("opcion_Enviar_Documento_Firmar", "Enviar documento")*/ };
                    messageStatus = messageStatus + await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Favor de Registrarse o volver a intentar", "Selecciona opción", "Hay 2 opciones", _botones);
                    _receivedMessages.Add("Favor de Registrarse o volver a intentar");
                    //_lastMessageState = "";
                    UpdateLastMessageState(fromPhoneNumber, "");
                    _receivedMessages.Add(messageStatus);
                    return messageStatus;
                }
                //_NITComprador = parsedResponseJson.screen_0_NIT_1;
                UpdateNITComprador(fromPhoneNumber, parsedResponseJson.screen_0_NIT_1);
                //_NombreComprador = parsedResponseJson.screen_0_Nombre_0;
                UpdateNombreComprador(fromPhoneNumber, parsedResponseJson.screen_0_Nombre_0);

                //comprobar si el comprador tiene el cliente
                bool existenciaCliente = _manejoDeComprador.ClienteExisteEnComprador(parsedResponseJson.screen_0_NIT_2, parsedResponseJson.screen_0_NIT_1, parsedResponseJson.screen_0_Nombre_0);
                if (!existenciaCliente)
                {
                    _receivedMessages.Add("No existe Cliente en comprador");
                    _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Registrar_Cliente", "Registrar Cliente"), ("opcion_Generar_Factura_Flow_0", "Volver a intentar")};
                    messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "No encontré este NIT en tus clientes", "Selecciona opción", "Lo vaz a registrar?", _botones);
                    //_lastMessageState = parsedResponseJson.screen_0_NIT_1; // nit comprador
                    //_oldUserMessage = parsedResponseJson.screen_0_Nombre_0; // nombre comprador
                    _receivedMessages.Add(messageStatus);
                    return messageStatus;
                }
                //_NITCliente = parsedResponseJson.screen_0_NIT_2;
                UpdateNITCliente(fromPhoneNumber, parsedResponseJson.screen_0_NIT_2);

                //Continuar con la factura si Cliente existe en el comprador
                _receivedMessages.Add("Cliente existe en el comprador");

                _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Continuar_Con_La_Factura", "Facturar"), ("opcion_Cancelar", "Cancelar") };
                messageStatus = await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Encontré este NIT en tus clientes", "Selecciona opción", "Facturar?", _botones);
                //_lastMessageState = parsedResponseJson.screen_0_NIT_2;// cliente
                //_oldUserMessage = parsedResponseJson.screen_0_NIT_1; // comprador
                //_lastMessageState = "";
                UpdateLastMessageState(fromPhoneNumber, "");
                //_oldUserMessage = "0";
                UpdateOldUserMessage(fromPhoneNumber, "0");
                _receivedMessages.Add("Options sent");
                _receivedMessages.Add(messageStatus);
                return messageStatus;
            }
            catch (Exception ex)
            {
                _receivedMessages.Add($"Error en ComprobanteNitClienteYCombrador: {ex.Message}");
                return $"Error en ComprobanteNitClienteYCombrador: {ex.Message}";
            }
        }
        private async Task<string> HandleInteractiveFlowMessageInformaciónFactura(Flow_response_json_Model_682423707677994_InformacionFactura parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add("Parsed Factura Colombia response_json:");
            _receivedMessages.Add(JsonSerializer.Serialize(parsedResponseJson, new JsonSerializerOptions { WriteIndented = true }));

            //Extraer información
            string infoClienteComprador = _manejoDeComprador.ReturnCompradorAndClientFromNit(GetNITCliente(fromPhoneNumber), GetNITComprador(fromPhoneNumber), GetNombreComprador(fromPhoneNumber)/*_NITCliente, _NITComprador, _NombreComprador*/);
            if (infoClienteComprador.StartsWith("[ERROR]"))
            {
                _receivedMessages.Add($"ReturnCompradorAndClientFromNit Failed: {infoClienteComprador}");
                return infoClienteComprador;
            }
            //Mandar el Summary
            string summary = $"*Resumen del Formulario Recibido:*\n" + infoClienteComprador +
                     $"\n*Información de Factura*\n" +
                     $"*Dirección:* {parsedResponseJson.screen_0_Direccin_1}\n" +
                     $"*Teléfono:* {parsedResponseJson.screen_0_Telfono_0}\n" +
                     $"*Descripción:* {parsedResponseJson.screen_0_Descripcin_3}\n" +
                     $"*Cantidad:* {parsedResponseJson.screen_0_Monto_2}";

            _receivedMessages.Add("Summary: ");
            _receivedMessages.Add(summary);
            _receivedMessages.Add("Flow Token: ");
            _receivedMessages.Add(parsedResponseJson.flow_token);
            string messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, summary);
            _receivedMessages.Add("Summary sent");
            _receivedMessages.Add("Summary MessageStatus: ");
            _receivedMessages.Add(messageStatus);

            string lastMessageState = GetLastMessageState(fromPhoneNumber);
            //messageStatus = messageStatus + "; " + await _messageSendService.EnviarDocumentoPorUrl(fromPhoneNumber, "https://test-timbrame.azurewebsites.net/Ejemplo/24a8a905-f155-4726-a27f-1451a8bf5388.pdf", $"Factura{fromPhoneNumber}{lastMessageState}.pdf");
            messageStatus = messageStatus + "; " + await _messageSendService.EnviarDocumentoPorUrl(fromPhoneNumber, "https://drive.google.com/file/d/11sRf6l3n7fRI6J5vexuiKZKTDIYuDaKM/view?usp=sharing", $"FacturaEjemplo{fromPhoneNumber}{lastMessageState}.pdf");
            _receivedMessages.Add("Zip sent");
            _receivedMessages.Add("Zip MessageStatus: ");
            _receivedMessages.Add(messageStatus);

            _botones = new List<(string ButtonId, string ButtonLabelText)> { ("opcion_Cancelar", "Regresar al inicio") };
            messageStatus = messageStatus + "; " + await _messageSendService.EnviarBotonInteractivo(fromPhoneNumber, "Factura creada", "Gracias por usar Timbrabot", "Desea regrezar al inicio", _botones);
            _receivedMessages.Add("Return button sent");
            _receivedMessages.Add("Return button MessageStatus: ");
            _receivedMessages.Add(messageStatus);

            //_NITCliente = "";
            UpdateNITCliente(fromPhoneNumber, "");
            //_lastMessageState = "";
            UpdateLastMessageState(fromPhoneNumber, "");
            //_oldUserMessage = "0";
            UpdateOldUserMessage(fromPhoneNumber, "0");
            return messageStatus;
        }
        private async Task<string> HandleInteractiveFlowMessageInformaciónDelCliente(Flow_response_json_Model_1297640437985053_InformacionDelCliente parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add("HandleInteractiveFlowMessageInformaciónDelCliente");
            bool existenciaCliente = _manejoDeComprador.ClienteExisteEnComprador(parsedResponseJson.screen_0_NIT_4, GetNITComprador(fromPhoneNumber), GetNombreComprador(fromPhoneNumber)/*_NITComprador, _NombreComprador*/);
            string messageStatus = "";
            string errorMessage = "Error desconocido";
            string messageToSend = "";
            if (existenciaCliente)
            {
                _receivedMessages.Add("Cliente ya existe en comprador");
                //messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Cliente ya existe en comprador");
                messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1297640437985053", $"opcionRegistrarCliente{fromPhoneNumber}", "published", "Cliente ya existe en comprador", "Registrar");
                return messageStatus;
            }
            _receivedMessages.Add($"Comprador {GetNombreComprador(fromPhoneNumber)/*_NombreComprador*/}, {GetNITComprador(fromPhoneNumber)/*_NITComprador*/}");

            messageStatus = _manejoDeComprador.AñadirClienteAComprador(GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber),/*_NombreComprador, _NITComprador,*/ parsedResponseJson);

            if (messageStatus == "[ERROR] Comprador no existe")
                errorMessage = "Comprador no existe, Volver a intentar";
            else if (messageStatus == "[ERROR] Cliente ya existe en comprador")
                errorMessage = "Cliente ya existe en comprador, Volver a intentar";
            else if (messageStatus.StartsWith("[ERROR]"))
                errorMessage = "De compilación";
            if (messageStatus.StartsWith("[ERROR]"))
            {
                messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "1297640437985053", $"opcionRegistrarCliente{fromPhoneNumber}", "published", errorMessage, "Registrar");
                return messageStatus;
            }
            
            messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, messageStatus);

            messageStatus = await _messageSendService.EnviarFlow(fromPhoneNumber, "682423707677994", $"opcionFacturar{fromPhoneNumber}", "published", "Todo parece estar listo, ya puedes facturar", "Facturar ahora");
            //_NITCliente = parsedResponseJson.screen_0_NIT_4;
            UpdateNITCliente(fromPhoneNumber, parsedResponseJson.screen_0_NIT_4);
            //_lastMessageState = "";
            UpdateLastMessageState(fromPhoneNumber, "");
            //_oldUserMessage = "0";
            UpdateOldUserMessage(fromPhoneNumber, "0");
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
                return "Error, existencia is null";
            }
            else if (existencia)
            {
                _receivedMessages.Add($"Comprador Existe");
                messageStatus = _manejoDeComprador.ModificarPersonaFísica(parsedResponseJson, fromPhoneNumber);
            }
            else
            {
                _receivedMessages.Add($"Comprador No Existe");
                messageStatus = _manejoDeComprador.CrearPersonaFísica(parsedResponseJson, fromPhoneNumber);
            }
            _receivedMessages.Add($"Comprador: {fullName}, {parsedResponseJson.screen_1_NIT_0}");

            messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿A quién facturarás?\nPuedes escribir su NIT, Nombre o una parte de estos");
            _receivedMessages.Add(messageStatus);
            //_lastMessageState = "opcion_Generar_Factura_Cliente";
            UpdateLastMessageState(fromPhoneNumber, "opcion_Generar_Factura_Cliente");
            //_NITComprador = parsedResponseJson.screen_1_NIT_0;
            UpdateNITComprador(fromPhoneNumber, parsedResponseJson.screen_1_NIT_0);
            //_NombreComprador = fullName;
            UpdateNombreComprador(fromPhoneNumber, fullName);
            UpdateOldUserMessage(fromPhoneNumber, "HandleInteractiveFlowMessageRegistraPersonaFísica");
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
            }
            else
            {
                _receivedMessages.Add($"Empresa no existe");
                messageStatus = _manejoDeComprador.CrearEmpresa(parsedResponseJson, fromPhoneNumber);
            }
            _receivedMessages.Add($"Empresa: {parsedResponseJson.Registrar_Empresa_Nombre_0}, {parsedResponseJson.Registrar_Empresa_NIT_1}");

            messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "¿A quién facturarás?\nPuedes escribir su NIT, Nombre o una parte de estos");
            _receivedMessages.Add(messageStatus);
            //_lastMessageState = "opcion_Generar_Factura_Cliente";
            UpdateLastMessageState(fromPhoneNumber, "opcion_Generar_Factura_Cliente");
            //_NITComprador = parsedResponseJson.Registrar_Empresa_NIT_1;
            UpdateNITComprador(fromPhoneNumber, parsedResponseJson.Registrar_Empresa_NIT_1);
            //_NombreComprador = parsedResponseJson.Registrar_Empresa_Nombre_0;
            UpdateNombreComprador(fromPhoneNumber, parsedResponseJson.Registrar_Empresa_Nombre_0);
            UpdateOldUserMessage(fromPhoneNumber, "HandleInteractiveFlowMessageRegistraEmpresa");
            return messageStatus;
        }
        private async Task<string> HandleInteractiveFlowCearProducto(Flow_response_json_Model_1142951587576244_Crear_Producto parsedResponseJson, string fromPhoneNumber)
        {
            string messageStatus;
            _receivedMessages.Add("HandleInteractiveFlowCearProducto");
            if (_productos.Any(c => c.Agregar_Producto_Nombre.Contains(parsedResponseJson.Agregar_Producto_Nombre, StringComparison.OrdinalIgnoreCase)) ||
                _productos.Any(c => c.Agregar_Producto_Codigo.Contains(parsedResponseJson.Agregar_Producto_Codigo, StringComparison.OrdinalIgnoreCase)))
            {
                _receivedMessages.Add("Producto ya existe");
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Producto ya existe");
                UpdateRestart(fromPhoneNumber);
                return "Producto ya existe";
            }
            _productos.Add(parsedResponseJson);
            messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Producto añadido");
            UpdateRestart(fromPhoneNumber);
            return "Producto añadido";
        }
        private async Task<string> HandleInteractiveFlowMessageRegistrarCliente(Flow_response_json_Model_1584870855544061_CrearCliente parsedResponseJson, string fromPhoneNumber)
        {
            _receivedMessages.Add($"HandleInteractiveFlowMessageRegistrarCliente");
            string messageStatus;
            messageStatus = _manejoDeComprador.AñadirClienteNaturalJudicialAComprador(GetNombreComprador(fromPhoneNumber), GetNITComprador(fromPhoneNumber), parsedResponseJson);


            if (parsedResponseJson.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Juridica")
            {
                _receivedMessages.Add("Persona Judicial");
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Persona Judicial Registrada");
            }
            else if (parsedResponseJson.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Natural")
            {
                _receivedMessages.Add($"Persona Natural");
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Persona Natural Registrada");
            }
            else
            {
                _receivedMessages.Add($"Error, tipo de persona desconocido");
                messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Error, tipo de persona desconocido");
            }
            if (GetTipoProgramacion(fromPhoneNumber) == "opcion_Generar_Factura_Cliente")
            {

            }
            //_NITCliente = "";
            UpdateNITCliente(fromPhoneNumber, "");
            //_lastMessageState = "";
            UpdateLastMessageState(fromPhoneNumber, "");
            //_oldUserMessage = "0";
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
                _receivedMessages.Add(messageStatus);
                UpdateRestart(fromPhoneNumber);
                return messageStatus;
            }
            _receivedMessages.Add("Sin errores");
            UpdateRestart(fromPhoneNumber);

            messageStatus = messageStatus + await _messageSendService.EnviarTexto(fromPhoneNumber, "Empresa Cliente modificada correctamente");

            _receivedMessages.Add(messageStatus);
            _receivedMessages.Add("Empresa Cliente modificada correctamente");
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
                _receivedMessages.Add(messageStatus);
                UpdateRestart(fromPhoneNumber);
                return messageStatus;
            }
            UpdateRestart(fromPhoneNumber);

            messageStatus = await _messageSendService.EnviarTexto(fromPhoneNumber, "Cliente persona Natural modificado correctamente");

            _receivedMessages.Add(messageStatus);
            _receivedMessages.Add("Cliente persona Natural modificado correctamente");
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
                    existingData.tipoProgramación,
                    existingData.usoCFDI,
                    existingData.usuarioAUsar,
                    existingData.usuarioAUsarID,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    existingData.clientExistance,
                    existingData.valueInList
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
                    existingData.tipoProgramación,
                    existingData.usoCFDI,
                    existingData.usuarioAUsar,
                    existingData.usuarioAUsarID,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    existingData.clientExistance,
                    existingData.valueInList
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
                    existingData.tipoProgramación,
                    existingData.usoCFDI,
                    existingData.usuarioAUsar,
                    existingData.usuarioAUsarID,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    existingData.clientExistance,
                    existingData.valueInList
                );
            }
        }
        private void UpdatetTipoProgramación(string fromPhoneNumber, string MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    MessageChange,
                    existingData.usoCFDI,
                    existingData.usuarioAUsar,
                    existingData.usuarioAUsarID,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    existingData.clientExistance,
                    existingData.valueInList
                );
            }
        }
        private void UpdateUsoCFDI(string fromPhoneNumber, string MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.tipoProgramación,
                    MessageChange,
                    existingData.usuarioAUsar,
                    existingData.usuarioAUsarID,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    existingData.clientExistance,
                    existingData.valueInList
                );
            }
        }
        private void UpdateUsuarioAUsar(string fromPhoneNumber, string MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.tipoProgramación,
                    existingData.usoCFDI,
                    MessageChange,
                    existingData.usuarioAUsarID,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    existingData.clientExistance,
                    existingData.valueInList
                );
            }
        }
        private void UpdateUsuarioAUsarID(string fromPhoneNumber, string MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.tipoProgramación,
                    existingData.usoCFDI,
                    existingData.usuarioAUsar,
                    MessageChange,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    existingData.clientExistance,
                    existingData.valueInList
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
                    existingData.tipoProgramación,
                    existingData.usoCFDI,
                    existingData.usuarioAUsar,
                    existingData.usuarioAUsarID,
                    MessageChange,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    existingData.clientExistance,
                    existingData.valueInList
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
                    existingData.tipoProgramación,
                    existingData.usoCFDI,
                    existingData.usuarioAUsar,
                    existingData.usuarioAUsarID,
                    existingData.NITCliente,
                    MessageChange,
                    existingData.NombreComprador,
                    existingData.clientExistance,
                    existingData.valueInList
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
                    existingData.tipoProgramación,
                    existingData.usoCFDI,
                    existingData.usuarioAUsar,
                    existingData.usuarioAUsarID,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    MessageChange,
                    existingData.clientExistance,
                    existingData.valueInList
                );
            }
        }
        private void UpdateClientExistanceList(string fromPhoneNumber, List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.tipoProgramación,
                    existingData.usoCFDI,
                    existingData.usuarioAUsar,
                    existingData.usuarioAUsarID,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    MessageChange,
                    existingData.valueInList
                );
            }
        }
        private void UpdateValueInList(string fromPhoneNumber, byte MessageChange)
        {
            if (_RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var existingData))
            {
                _RegisteredPhoneDicitonary[fromPhoneNumber] = (
                    existingData.lastMessageState,
                    existingData.oldUserMessage,
                    existingData.montoFactura,
                    existingData.tipoProgramación,
                    existingData.usoCFDI,
                    existingData.usuarioAUsar,
                    existingData.usuarioAUsarID,
                    existingData.NITCliente,
                    existingData.NITComprador,
                    existingData.NombreComprador,
                    existingData.clientExistance,
                    MessageChange
                );
            }
        }
        private void  UpdateRestart(string fromPhoneNumber)
        {
                //_lastMessageState = "";
                UpdateLastMessageState(fromPhoneNumber, "");
                //_montoFactura = "";
                UpdateMontoFactura(fromPhoneNumber, "");
                //_tipoProgramación = "";
                UpdatetTipoProgramación(fromPhoneNumber, "");
                //_oldUserMessage = "0";
                UpdateOldUserMessage(fromPhoneNumber, "0");
                //_usoCFDI = "";
                UpdateUsoCFDI(fromPhoneNumber, "");
                //_usuarioAUsar = "";
                UpdateUsuarioAUsar(fromPhoneNumber, "");
                //_usuarioAUsarID = "";
                UpdateUsuarioAUsarID(fromPhoneNumber, "");
                //_NITCliente = "";
                UpdateNITCliente(fromPhoneNumber, "");
                //_NITComprador = "";
                UpdateNITComprador(fromPhoneNumber, "");
                //_NombreComprador = "";
                UpdateNombreComprador(fromPhoneNumber, "");
        }
        private string GetLastMessageState(string fromPhoneNumber) =>
    _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.lastMessageState : string.Empty;
        private string GetOldUserMessage(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.oldUserMessage : string.Empty;
        private string GetMontoFactura(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.montoFactura : string.Empty;
        private string GetTipoProgramacion(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.tipoProgramación : string.Empty;
        private string GetUsoCFDI(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.usoCFDI : string.Empty;
        private string GetUsuarioAUsar(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.usuarioAUsar : string.Empty;
        private string GetUsuarioAUsarID(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.usuarioAUsarID : string.Empty;
        private string GetNITCliente(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.NITCliente : string.Empty;
        private string GetNITComprador(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.NITComprador : string.Empty;
        private string GetNombreComprador(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.NombreComprador : string.Empty;
        private List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> GetClientExistance(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.clientExistance : new List<Flow_response_json_Model_1297640437985053_InformacionDelCliente>();
        private byte GetValueInList(string fromPhoneNumber) =>
            _RegisteredPhoneDicitonary.TryGetValue(fromPhoneNumber, out var data) ? data.valueInList : (byte)0;
    }
}