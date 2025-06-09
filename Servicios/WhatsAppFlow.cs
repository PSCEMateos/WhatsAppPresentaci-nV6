using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using WhatsAppPresentacionV11.Modelos;
using System.Net.Http;
using Swashbuckle.AspNetCore.SwaggerGen;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Win32;
using System;
using System.Globalization;

namespace WhatsAppPresentacionV11.Servicios
{
    public class WhatsAppFlow
    {
        private readonly string _idTelefono;
        private readonly string _tokenAcceso;
        private readonly string _facebookGraphVersion = "v21.0";

        private static Dictionary<(string Nombre, string NIT), CompradorInfo> _compradores = new Dictionary<(string Nombre, string NIT), CompradorInfo>();
        
        private static List<string> _EventsList = new List<string>();
        private static readonly object _lock = new(); // Para thread-safety

        /// <summary>
        /// Constructor de la clase WhatsAppMessageService. Inicializa el servicio con el ID de teléfono y el token de acceso.
        /// </summary>
        /// <param name="idTelefono"> ID del número de WhatsApp Business. Actualmente se usa número test que da whatsapp.</param>
        /// <param name="tokenAcceso">Token de acceso para autenticación en la API de Facebook Graph. Se tiene que generar cada cierto tiempo y modificar en elprograma.</param>
        /// <param name="_compradores">Diccionario para manejar compradores, su NIT, teléfonos que manejan y clientes (con primer y segundo nombre, apellido paterno y materno y NIT del cliente) que manejan</param>
        public WhatsAppFlow(string idTelefono, string tokenAcceso)
        {
            _idTelefono = idTelefono;
            _tokenAcceso = tokenAcceso;

            //Agregar un ejemplo de comprador
             /*var key = ("Probador PSC", "11800");
             var emjemploRegistroEmpresa = new Flow_response_json_Model_1187351356327089_RegistrarEmpresa
             {
                 Registrar_Empresa_Nombre_0 = "Probador PSC",
                 Registrar_Empresa_NIT_1 = "11800",
                 Registrar_Empresa_Digito_Verificacin_1 = "111111",
                 Registrar_Empresa_Direccion_2 = "Ejemplo Dirección",
                 Registrar_Empresa_Departamento_3 = "Ejemplo depto",
                 Registrar_Empresa_Ciudad_4 = "Ciudad ejemplo",
                 Registrar_Empresa_Tipo_Rgimen_5 = "1_No_responsable_de_IVA",
                 Registrar_Empresa_Obligaciones_Fiscales_6 = "4_No_aplica-_Otros",
                 flow_token = "11111111",
            };
            var clientesEjemplo = new List<Flow_response_json_Model_1584870855544061_CrearCliente>
            {
                new Flow_response_json_Model_1584870855544061_CrearCliente
                {
                    RegistraCliente_Tipo_Cliente = "Tipo_Persona_Natural",
                    RegisterClient_Natural_Nombre = "PSC",
                    RegisterClient_Natural_NIT = "118001",
                    RegisterClient_Natural_Apellido_Paterno = "World",
                    RegisterClient_Natural_Apellido_Materno = "World",
                    RegisterClient_Natural_Correo = "info@pscworld.com.mx",
                    RegisterClient_Natural_Telefono = "525553396600",
                    RegisterClient_Natural_Tipo_Identificacion = "4",
                    RegisterClient_Natural_Digito_Verificacin = "11800",
                    RegisterClient_Natural_Direccion = "Avenida Patriotismo 48-6, Escandón I Secc, Miguel Hidalgo, 11800 Ciudad de México, CDMX",
                    RegisterClient_Natural_Departamento = "48-6",
                    RegisterClient_Natural_Ciudad = "CDMX",
                    RegisterClient_Natural_Tipo_Rgimen = "1_No_responsable_de_IVA",
                    RegisterClient_Natural_Obligaciones_Fiscale = "4_No_aplica-_Otros",

                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1584870855544061_CrearCliente
                {
                    RegistraCliente_Tipo_Cliente = "Tipo_Persona_Juridica",
                    RegisterClient_Juridical_Razon_Social = "PSC",
                    RegisterClient_Juridical_NIT = "Chip",
                    RegisterClient_Juridical_Email = "2",
                    RegisterClient_Juridical_Digito_Verificacion = "00002",
                    RegisterClient_Juridical_Direccion = "Presente",
                    RegisterClient_Juridical_Departamento = "Pasado",
                    RegisterClient_Juridical_Ciudad = "Futuro",
                    RegisterClient_Juridical_Tipo_Regimen = "1_No_responsable_de_IVA",
                    RegisterClient_Juridical_Obligaciones_Fiscales = "4_No_aplica-_Otros",

                    flow_token = "11111112",
                }
            };

            _compradores[key] = new CompradorInfo
            {
                DatosRegistrarEmpresa = emjemploRegistroEmpresa,
                Telefonos = new List<string> { "525526903132" }, // 525526903132
                Correos = new List<string> { "ejemplo@correo.com" },
                Clientes2 = clientesEjemplo
            };*/
        }
        private string BuildApiUrl() =>
            $"https://graph.facebook.com/{_facebookGraphVersion}/{_idTelefono}/flows";
        public async Task<string> CreateWhatsAppFlowAsync(string flowDefinitionJson)
        {
            using HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, BuildApiUrl());

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAcceso);

            request.Content = new StringContent(JsonSerializer.Serialize(flowDefinitionJson), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                return $"Error creating flow: {errorResponse}";
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return $"Flow created successfully: {responseBody}";
        }
        public async Task<string> CrearFormularioConStringAsync(string accessToken)
        {
            string flowDefinitionJson = @"
            {
                ""name"": ""pokemon_survey"",
                ""language"": ""en_US"",
                ""content"": {
                    ""actions"": [
                        {
                            ""title"": ""Survey"",
                            ""description"": ""Help us know you better!"",
                            ""type"": ""QUESTION"",
                            ""question"": {
                                ""text"": ""What's your name?"",
                                ""input"": {
                                    ""type"": ""TEXT"",
                                    ""next_action"": ""ask_group""
                                }
                            }
                        },
                        {
                            ""id"": ""ask_group"",
                            ""type"": ""QUESTION"",
                            ""question"": {
                                ""text"": ""Which group do you belong to?"",
                                ""input"": {
                                    ""type"": ""TEXT"",
                                    ""next_action"": ""ask_pokemon""
                                }
                            }
                        },
                        {
                            ""id"": ""ask_pokemon"",
                            ""type"": ""QUESTION"",
                            ""question"": {
                                ""text"": ""What's your favorite Pokémon?"",
                                ""input"": {
                                    ""type"": ""TEXT"",
                                    ""next_action"": ""thank_user""
                                }
                            }
                        },
                        {
                            ""id"": ""thank_user"",
                            ""type"": ""MESSAGE"",
                            ""message"": {
                                ""text"": ""Thank you! Your information has been recorded.""
                            }
                        }
                    ]
                }
            }";

            return await CreateWhatsAppFlowAsync(flowDefinitionJson);
        }


        public string CrearEmpresa(Flow_response_json_Model_1187351356327089_RegistrarEmpresa registro, string telefono)
        {
            if (string.IsNullOrEmpty(registro.Registrar_Empresa_Nombre_0))
            {
                return $"[Error] Nombre o apellido no válido";
            }
            if (string.IsNullOrEmpty(registro.Registrar_Empresa_NIT_1))
            {
                return $"[Error] NIT Requerido";

            }
            string nombre = $"{registro.Registrar_Empresa_Nombre_0.ToLowerInvariant()}".Trim();
            string nit = registro.Registrar_Empresa_NIT_1;

            var key = (nombre, nit);


            if (_compradores.ContainsKey(key))
            {
                _EventsList.Add($"[WARNING] Empresa '{nombre}' with NIT '{nit}' already exists at {DateTime.Now}");
                return $"Empresa '{nombre}' with NIT '{nit}' already exists";
            }

            _compradores[key] = new CompradorInfo
            {
                DatosRegistrarEmpresa = registro,
                Telefonos = new List<string> { telefono },
                Correos = new List<string> { registro.Registrar_Empresa_Correo_2 ?? "UNKNOWN" },
                Clientes2 = new List<Flow_response_json_Model_1584870855544061_CrearCliente> { },
            };
            _EventsList.Add($"[INFO] Empresa '{nombre}' with NIT '{nit}' created successfully at {DateTime.Now}");
            return $"Empresa '{nombre}' with NIT '{nit}' created successfully at {DateTime.Now}";
        }
        public string ModificarEmpresa(Flow_response_json_Model_1187351356327089_RegistrarEmpresa registro, string telefono)
        {
            if (string.IsNullOrEmpty(registro.Registrar_Empresa_Nombre_0))
            {
                return $"Nombre o apellido no válido";
            }
            if (string.IsNullOrEmpty(registro.Registrar_Empresa_NIT_1))
            {
                return $"NIT Requerido";

            }

            string nombre = $"{registro.Registrar_Empresa_Nombre_0.ToLowerInvariant}".Trim();
            string nit = registro.Registrar_Empresa_NIT_1;
            var key = (nombre, nit);

            if (!_compradores.ContainsKey(key))
            {
                _EventsList.Add($"[ERROR] Empresa '{nombre}' with NIT '{nit}' not found to update at {DateTime.Now}");
                return $"[ERROR] Empresa '{nombre}' with NIT '{nit}' not found";
            }
            _compradores[key].DatosRegistrarEmpresa = registro;
            if (!_compradores[key].Telefonos.Contains(telefono))
                _compradores[key].Telefonos.Add(telefono);
            return $"Empresa '{nombre}' with NIT '{nit}' modified successfully at {DateTime.Now}";
        }
        public string CrearPersonaFísica(Flow_response_json_Model_637724539030495_RegistrarPersonaFisicaSimple registro, string telefono)
        {
            if (string.IsNullOrEmpty(registro.Registrar_Persona_Fisica_Nombre_0) || string.IsNullOrEmpty(registro.Registrar_Persona_Fisica_Apellido_Paterno_1) || string.IsNullOrEmpty(registro.Registrar_Persona_Fisica_Apellido_Materno_2))
            {
                return $"Nombre o apellido no válido";
            }
            if (string.IsNullOrEmpty(registro.screen_1_NIT_0))
            {
                return $"NIT Requerido";

            }

            string nombre = $"{registro.Registrar_Persona_Fisica_Nombre_0.ToLowerInvariant()} {registro.Registrar_Persona_Fisica_Apellido_Paterno_1.ToLowerInvariant()} {registro.Registrar_Persona_Fisica_Apellido_Materno_2.ToLowerInvariant()}".Trim();
            string nit = registro.screen_1_NIT_0;

            var key = (nombre, nit);


            if (_compradores.ContainsKey(key))
            {
                _EventsList.Add($"[WARNING] Comprador '{nombre}' with NIT '{nit}' already exists at {DateTime.Now}");
                return $"Comprador '{nombre}' with NIT '{nit}' already exists";
            }

            List<string> phoneList;

            if (!string.IsNullOrEmpty(registro.Registrar_Persona_Fisica_Telfono_4) && registro.Registrar_Persona_Fisica_Telfono_4 != telefono)
            {
                phoneList = new List<string> { registro.Registrar_Persona_Fisica_Telfono_4, telefono };
                _EventsList.Add("Dos teléfonos diferentes");
            }
            else
            {
                phoneList = new List<string> { telefono };
                _EventsList.Add("Un sólo teléfono");
            }

            _compradores[key] = new CompradorInfo
            {
                DatosPersonaFisicaSimple = registro,
                Telefonos = phoneList,
                Correos = new List<string> { registro.Registrar_Persona_Fisica_Correo_3 ?? "UNKNOWN" },
                Clientes2 = new List<Flow_response_json_Model_1584870855544061_CrearCliente> { },
            };

            _EventsList.Add($"[INFO] Comprador '{nombre}' with NIT '{nit}' created successfully at {DateTime.Now}");
            return $"Comprador '{nombre}' with NIT '{nit}' created successfully at {DateTime.Now}";
        }
        public string ModificarPersonaFísica(Flow_response_json_Model_637724539030495_RegistrarPersonaFisicaSimple registro, string telefono)
        {
            if (string.IsNullOrEmpty(registro.Registrar_Persona_Fisica_Nombre_0) || string.IsNullOrEmpty(registro.Registrar_Persona_Fisica_Apellido_Paterno_1) || string.IsNullOrEmpty(registro.Registrar_Persona_Fisica_Apellido_Materno_2))
            {
                return $"Nombre o apellido no válido";
            }
            if (string.IsNullOrEmpty(registro.screen_1_NIT_0))
            {
                return $"NIT Requerido";

            }

            string nombre = $"{registro.Registrar_Persona_Fisica_Nombre_0.ToLowerInvariant()} {registro.Registrar_Persona_Fisica_Apellido_Paterno_1.ToLowerInvariant()} {registro.Registrar_Persona_Fisica_Apellido_Materno_2.ToLowerInvariant()}".Trim();
            string nit = registro.screen_1_NIT_0;

            var key = (nombre, nit);

            if (_compradores.ContainsKey(key))
            {
                _compradores[key].DatosPersonaFisicaSimple = registro;
                if (!_compradores[key].Telefonos.Contains(telefono))
                    _compradores[key].Telefonos.Add(telefono);
                return $"Comprador '{nombre}' with NIT '{nit}' modified successfully at {DateTime.Now}";
            }
            else
            {
                _EventsList.Add($"[ERROR] Comprador '{nombre}' with NIT '{nit}' not found to update at {DateTime.Now}");
                return $"[ERROR] Comprador '{nombre}' with NIT '{nit}' not found";
            }
        }
        public List<string> ReturnCompradores()
        {
            List<string> compradoresInfo = new List<string>();

            try
            {
                foreach (var entry in _compradores)
                {

                    var (nombre, nit) = entry.Key;
                    _EventsList.Add($"Comprador: {nombre}, con NIT {nit}");
                    //_EventsList.Add($"Clientes: {entry.Value.Clientes.Count()}");
                    CompradorInfo InfoADesplegar = entry.Value;

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Comprador: {nombre}, NIT: {nit}");
                    if (InfoADesplegar.Telefonos.Count() > 1)
                    {
                        sb.AppendLine($"Teléfonos: {string.Join(", ", InfoADesplegar.Telefonos)}");
                        _EventsList.Add($"Teléfonos: {string.Join(", ", InfoADesplegar.Telefonos)}");
                    }
                    switch (InfoADesplegar.Correos.Count())
                    {
                        case 0:
                            _EventsList.Add($"Correo: 0");
                            break;
                        case 1:
                            _EventsList.Add($"Correo: 1");
                            if (InfoADesplegar.Correos[0] != "UNKNNOWN")
                                sb.AppendLine($"Correos: {InfoADesplegar.Correos[0]}");
                            break;
                        default:
                            sb.AppendLine($"Correos: {string.Join(", ", InfoADesplegar.Correos)}");
                            _EventsList.Add($"Correo: Muchos");
                            break;
                    }
                    /*_EventsList.Add($"Clientes:");
                    sb.AppendLine("Clientes:");
                    if (InfoADesplegar.Clientes.Count() >= 1)
                    {
                        foreach (var cliente in InfoADesplegar.Clientes)
                        {
                            _EventsList.Add($"Cliente: {cliente.screen_0_NIT_4}");
                            if (!string.IsNullOrEmpty(cliente.screen_0_Primer_Nombre_0))
                            {
                                string fullCliente = $"{cliente.screen_0_Primer_Nombre_0} {cliente.screen_0_Segundo_Nombre_1} " +
                                                 $"{cliente.screen_0_Apellido_Materno_3} {cliente.screen_0_Apellido_Paterno_2}, {cliente.screen_0_NIT_4}";
                                sb.AppendLine($"--- {fullCliente.Trim()}");
                                _EventsList.Add("Cliente: screen_0_Primer_Nombre_0");
                            }
                            else if (!string.IsNullOrEmpty(cliente.ModificarCliente_Natural_Nombre))
                            {
                                string fullCliente = $"{cliente.ModificarCliente_Natural_Nombre} " +
                                                 $"{cliente.ModificarCliente_Natural_Apellido_Paterno} {cliente.ModificarCliente_Natural_Apellido_Materno}, {cliente.screen_0_NIT_4}";
                                sb.AppendLine($"--- {fullCliente.Trim()}");
                                _EventsList.Add("Cliente: ModificarCliente_Natural_Nombre");
                            }
                            else if (!string.IsNullOrEmpty(cliente.ModificarCliente_Empresa_Juridical_Razon_Social))
                            {
                                string fullCliente = $"{cliente.ModificarCliente_Empresa_Juridical_Razon_Social}, {cliente.screen_0_NIT_4}";
                                sb.AppendLine($"--- {fullCliente.Trim()}");
                                _EventsList.Add("Cliente: ModificarCliente_Empresa_Juridical_Razon_Social");
                            }
                        }
                        
                    }*/
                    if (InfoADesplegar.Clientes2.Count() >= 1)
                    {
                        _EventsList.Add($"Clientes2: {InfoADesplegar.Clientes2.Count()}");
                        sb.AppendLine($"Clientes2:{InfoADesplegar.Clientes2.Count()}");
                        foreach (var cliente in InfoADesplegar.Clientes2)
                        {
                            _EventsList.Add($"Cliente: {cliente.RegistraCliente_Tipo_Cliente}"); 
                            if (!string.IsNullOrEmpty(cliente.RegistraCliente_Tipo_Cliente) && cliente.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Natural")
                            {
                                string fullCliente = $"{cliente.RegisterClient_Natural_Nombre} {cliente.RegisterClient_Natural_Apellido_Paterno} {cliente.RegisterClient_Natural_Apellido_Materno}";
                                sb.AppendLine($"--- {fullCliente.Trim()} _ {cliente.RegisterClient_Natural_NIT}");
                                _EventsList.Add("Cliente: Tipo_Persona_Natural");
                            }
                            else if (!string.IsNullOrEmpty(cliente.RegistraCliente_Tipo_Cliente) && cliente.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Juridica")
                            {
                                string fullCliente = $"{cliente.RegisterClient_Juridical_Razon_Social}";
                                sb.AppendLine($"--- {fullCliente.Trim()} _ {cliente.RegisterClient_Juridical_NIT}");
                                _EventsList.Add("Cliente: Tipo_Persona_Juridica");
                            }
                            else
                            {
                                sb.AppendLine($"--- [Error] Falló añadir cliente");
                                _EventsList.Add("Cliente:");
                            }
                        }
                    }

                    compradoresInfo.Add(sb.ToString());
                }

                _EventsList.Add($"[INFO] PrintCompradores executed successfully at {DateTime.Now}");

            }
            catch (Exception ex)
            {
                string errorMsg = $"[ERROR] PrintCompradores failed at {DateTime.Now}: {ex.Message}";
                _EventsList.Add(errorMsg);
                compradoresInfo.Add(errorMsg);
            }

            return compradoresInfo;


        }
        public bool CompradorExiste(string compradorNombre, string compradorNIT)
        {
            if (string.IsNullOrWhiteSpace(compradorNombre) && string.IsNullOrWhiteSpace(compradorNIT))
            {
                _EventsList.Add("[Error] CompradorExiste called with empty values!");
                return false;
            }
            _EventsList.Add($"CompradorExiste: {compradorNombre} - {compradorNIT}");
            bool exists =_compradores.Keys.Any(k =>
            k.Nombre.Contains(compradorNombre, StringComparison.OrdinalIgnoreCase) || k.NIT == compradorNIT
            );
            _EventsList.Add($"Checking: {compradorNombre} - {compradorNIT} | Exists: {exists}");

            return exists;
        }
        public (string, string)? TeléfonoExiste(string fromPhoneNumber)
        {
            foreach (var entry in _compradores)
            {
                if (entry.Value.Telefonos.Contains(fromPhoneNumber))
                {
                    return entry.Key;
                }
            }
            return null;
        }
        public string AñadirTeléfonoAComprador(string phoneNumber, (string, string)key)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                _EventsList.Add($"[Error] Falta teléfono");
                return $"[Error] Falta teléfono";
            }
            if(_compradores[key] == null)
            {
                _EventsList.Add($"[Error] {key.Item1}, {key.Item2} no existe");
                return $"[Error] {key.Item1}, {key.Item2} no existe";
            }

            _compradores[key].Telefonos.Add(phoneNumber);

            return $"Teléfono {phoneNumber} añadido";
        }
        // Esta función cuenta cuántos compradores tienen un número de teléfono específico registrado.
        public int ContarCompradoresConTelefono(string fromPhoneNumber)
        {
            // Inicializa el contador en cero
            int contador = 0;

            // Recorre cada entrada del diccionario de compradores
            foreach (var entry in _compradores)
            {
                // Verifica si la lista de teléfonos del comprador contiene el número dado
                if (entry.Value.Telefonos.Contains(fromPhoneNumber))
                {
                    // Si lo contiene, incrementa el contador
                    contador++;
                }
            }

            // Devuelve el número total de coincidencias encontradas
            return contador;
        }
        public string AñadirProductoAComprador(string compradorNombre, string compradorNIT, Flow_response_json_Model_1142951587576244_Crear_Producto producto)
        {
            //Solo funciona si recibe el nombre y nit del comprador exactos

            _EventsList.Add($"AñadirProductoAComprador");
            _EventsList.Add($"Comprador {compradorNombre}, {compradorNIT}");
            if (string.IsNullOrEmpty(compradorNombre) || string.IsNullOrEmpty(compradorNIT))
            {
                _EventsList.Add($"[ERROR] Se requiere nombre y NIT del comprador");
                return "[ERROR] Se requiere nombre y NIT del comprador";
            }

            /*bool exists = _compradores.Keys.Any(k =>
            k.Nombre.Equals(compradorNombre, StringComparison.OrdinalIgnoreCase) && k.NIT.Equals(compradorNIT, StringComparison.OrdinalIgnoreCase)
            );*/

            if (!_compradores.TryGetValue((compradorNombre, compradorNIT), out var compradorInfo))
            {
                _EventsList.Add($"[ERROR] Comprador no existe");
                return $"[ERROR] Comprador no existe";
            }

            if (string.IsNullOrEmpty(producto.Agregar_Producto_Nombre) || string.IsNullOrEmpty(producto.Agregar_Producto_Codigo))
            {
                _EventsList.Add($"[ERROR] Falta Nombre o Código de producto");
                return $"[ERROR] Falta Nombre o Código de producto";
            }
            var existenciaProducto = ProductoExisteEnComprador(producto.Agregar_Producto_Nombre, producto.Agregar_Producto_Codigo, compradorNIT, compradorNombre);
            if (existenciaProducto == null)
            {
                _EventsList.Add($"[ERROR] Comprador no existe");
                return $"[ERROR] Comprador no existe";
            }
            if (existenciaProducto == true)
            {
                _EventsList.Add($"[ERROR] Producto ya existe en comprador");
                return $"[ERROR] Producto ya existe en comprador";
            }


            var productInfo = new Flow_response_json_Model_1142951587576244_Crear_Producto { };
            productInfo = new Flow_response_json_Model_1142951587576244_Crear_Producto
             {
                Agregar_Producto_Nombre = producto.Agregar_Producto_Nombre,
                Agregar_Producto_Precio_Unitario = producto.Agregar_Producto_Precio_Unitario,

                Agregar_Producto_Info_Adicional = producto.Agregar_Producto_Info_Adicional,
                Agregar_Producto_Codigo = producto.Agregar_Producto_Codigo,
                Agregar_Producto_Unidad_Medida = producto.Agregar_Producto_Unidad_Medida,
                Agregar_Producto_Activo = producto.Agregar_Producto_Activo,
                Agregar_Producto_traslados = producto.Agregar_Producto_traslados,
                Agregar_Producto_Impuesto = producto.Agregar_Producto_Impuesto,
                Agregar_Producto_Tasa_cuota = producto.Agregar_Producto_Tasa_cuota,
                Agregar_Producto_Impuestos_Saludables = producto.Agregar_Producto_Impuestos_Saludables,
                Agregar_Producto_Impuestos_Saludables2 = producto.Agregar_Producto_Impuestos_Saludables2
            };
         

            foreach (var entry in _compradores)
            {
                var (nombre, nit) = entry.Key;
                if (nombre.Contains(compradorNombre, StringComparison.OrdinalIgnoreCase) || nit.Contains(compradorNIT, StringComparison.OrdinalIgnoreCase))
                {
                    var compradoresList = entry.Value;
                    if (!compradoresList.Productos.Any(c => c.Agregar_Producto_Nombre == productInfo.Agregar_Producto_Nombre))
                    {
                        compradoresList.Productos.Add(productInfo);
                        _compradores[entry.Key].Productos.Add(productInfo); // = compradoresList;
                        _EventsList.Add($"El Cliente '{productInfo.Agregar_Producto_Nombre}' se registró correctamente");
                        return $"El Cliente '{productInfo.Agregar_Producto_Nombre}' se registró correctamente";
                    }
                }
            }
            _EventsList.Add($"[ERROR] Comprador '{compradorNombre}' with NIT '{compradorNIT}' not found at {DateTime.Now}");

            return $"[ERROR] Comprador '{compradorNombre}' with NIT '{compradorNIT}' not found at {DateTime.Now}";
        }
        // Esta función cuenta cuántos productos tienen compradores con un número de teléfono específico registrado.
        public int ContarProductosPorTelefono(string fromPhoneNumber)
        {
            // Usamos un HashSet para evitar contar productos duplicados
            HashSet<string> productosUnicos = new();

            // Recorremos todos los compradores
            foreach (var entry in _compradores)
            {
                // Si el comprador tiene el número de teléfono especificado
                if (entry.Value.Telefonos.Contains(fromPhoneNumber))
                {
                    // Recorremos la lista de productos de ese comprador
                    foreach (var producto in entry.Value.Productos)
                    {
                        // Usamos algún identificador único del producto
                        string? idUnico = producto.Agregar_Producto_Codigo ?? producto.Agregar_Producto_Nombre;

                        if (!string.IsNullOrEmpty(idUnico))
                        {
                            productosUnicos.Add(idUnico);
                        }
                    }
                }
            }

            // Devuelve el número total de coincidencias encontradas
            return productosUnicos.Count;
        }
        public bool? ProductoExisteEnComprador(string nombreProducto, string codigoProducto, string compradorNIT, string compradorNombre)
        {
            //Solo funciona si recibe el nombre y nit del comprador exactos
            _EventsList.Add("ProductoExisteEnComprador");

            if (!_compradores.TryGetValue((compradorNombre, compradorNIT), out var compradorInfo))
            {
                _EventsList.Add($"[ERROR] key comprador no existe, nombre: {compradorNombre}, Nit: {compradorNIT}");
                return null;
            }

            return compradorInfo.Productos.Any(p =>
                p.Agregar_Producto_Nombre == nombreProducto ||
                p.Agregar_Producto_Codigo == codigoProducto);
        }
        public (string, string)? CompradorExisteKeys(string textMessage)
        {
            foreach (var entry in _compradores)
            {
                if (entry.Key.Nombre.Contains(textMessage, StringComparison.OrdinalIgnoreCase) ||
                    entry.Key.NIT.Equals(textMessage, StringComparison.OrdinalIgnoreCase))
                    return entry.Key;
            }
            return null;
        }
        public List<(string TipoPersona, string Nombre, string NIT)> ListaDeCoincidenciasCompradores(string messagePiece)
        {
            var coincidencias = new List<(string TipoPersona, string Nombre, string NIT)>(); // lista a retornar

            foreach (var comp in _compradores)
            {

                string nombre = comp.Key.Nombre;
                string nit = comp.Key.Nombre;

                if (nombre.Contains(messagePiece, StringComparison.OrdinalIgnoreCase) ||
                    nit.Contains(messagePiece, StringComparison.OrdinalIgnoreCase))
                {
                    string tipoPersona = "";
                    if (comp.Value.DatosPersonaFisicaSimple != null)
                    {
                        tipoPersona = "Natural";
                        coincidencias.Add((tipoPersona, nombre, nit));
                    }
                    else if (comp.Value.DatosRegistrarEmpresa != null)
                    {
                        tipoPersona = "Juridica";
                        coincidencias.Add((tipoPersona, nombre, nit));
                    }
                }
            }

            return coincidencias;
        }
        public string ReturnCompradorAndClientFromNit(string clientNIT, string compradorNIT, string compradorNombre)
        {
            try
            {
                string nombreMinusculas = compradorNombre.ToLowerInvariant().Trim();

                foreach (var entry in _compradores)
                {
                    var (nombre, nit) = entry.Key;

                    var nombrePartes = nombre.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    bool coincideNombre = nombre.Contains(nombreMinusculas, StringComparison.OrdinalIgnoreCase) || nombrePartes.Any(parte => parte.Contains(nombreMinusculas)) || nit == compradorNIT;

                    if ( coincideNombre && entry.Value.Clientes2.Any(c => (c.RegisterClient_Natural_NIT == clientNIT) || (c.RegisterClient_Juridical_NIT == clientNIT) ))
                    {
                        CompradorInfo InfoADesplegar = entry.Value;
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"Comprador: {nombre}, NIT: {nit}");
                        //sb.AppendLine($"Teléfonos: {string.Join(", ", InfoADesplegar.Telefonos)}");
                        //sb.AppendLine($"Correos: {string.Join(", ", InfoADesplegar.Correos)}");
                        sb.AppendLine("Clientes:");
                        foreach (var cliente in InfoADesplegar.Clientes2)
                        {
                            if(cliente.RegisterClient_Natural_NIT == clientNIT)
                            {
                                string fullCliente = $"{cliente.RegisterClient_Natural_Nombre} {cliente.RegisterClient_Natural_Apellido_Paterno} " +
                                    $"{cliente.RegisterClient_Natural_Apellido_Materno}, {cliente.RegisterClient_Natural_NIT}";
                                sb.AppendLine($"- {fullCliente.Trim()}");
                                _EventsList.Add($"[INFO] PrintComprador natural executed successfully at {DateTime.Now}");
                                return (sb.ToString());
                            }
                            else if (cliente.RegisterClient_Juridical_NIT == clientNIT)
                            {
                                string fullCliente = $"{cliente.RegisterClient_Juridical_Razon_Social}, {cliente.RegisterClient_Juridical_NIT}";
                                sb.AppendLine($"- {fullCliente.Trim()}");
                                _EventsList.Add($"[INFO] PrintComprador Juridical executed successfully at {DateTime.Now}");
                                return (sb.ToString());
                            }
                        }
                    }
                }
                throw new Exception("CompradorAndClient not found");
            }
            catch (Exception ex)
            {
                string errorMsg = $"[ERROR] PrintComprador failed at {DateTime.Now}: {ex.Message}";
                _EventsList.Add(errorMsg);
                return errorMsg;
            }
        }

        public bool ClienteExisteEnComprador(string clientNIT, string compradorNIT, string compradorNombre)
        {
            foreach (var entry in _compradores)
            {
                var (nombre, nit) = entry.Key;
                if ((nombre.Contains(compradorNombre, StringComparison.OrdinalIgnoreCase) || nit.Contains(compradorNIT, StringComparison.OrdinalIgnoreCase)) &&
                    (/*entry.Value.Clientes.Any(c => c.screen_0_NIT_4 == clientNIT) || */
                    entry.Value.Clientes2.Any(c => c.RegisterClient_Natural_NIT == clientNIT) || 
                    entry.Value.Clientes2.Any(c => c.RegisterClient_Juridical_NIT == clientNIT)))
                    return true;
            }
            return false;
        }
        // Esta función cuenta cuántos clientes tienen compradores con un número de teléfono específico registrado.
        public int ContarClientesPorTelefono(string fromPhoneNumber)
        {
            // Usamos un HashSet para evitar contar clientes duplicados
            HashSet<string> clientesUnicos = new();

            // Recorremos todos los compradores
            foreach (var entry in _compradores)
            {
                // Si el comprador tiene el número de teléfono especificado
                if (entry.Value.Telefonos.Contains(fromPhoneNumber))
                {
                    foreach (var cliente in entry.Value.Clientes2)
                    {
                        // Si el cliente tiene NIT o algún identificador único, lo usamos para evitar duplicados
                        string? idUnico = cliente.RegisterClient_Natural_NIT ?? cliente.RegisterClient_Juridical_NIT;

                        if (!string.IsNullOrEmpty(idUnico))
                        {
                            clientesUnicos.Add(idUnico);
                        }
                    }
                }
            }

            // Devuelve el número total de coincidencias encontradas
            return clientesUnicos.Count;
        }
        public string AñadirClienteNaturalJudicialAComprador(string compradorNombre, string compradorNIT, Flow_response_json_Model_1584870855544061_CrearCliente cliente)
        {
            _EventsList.Add($"AñadirClienteNaturalJudicialAComprador");
            _EventsList.Add($"Comprador {compradorNombre}, {compradorNIT}");
            if (string.IsNullOrEmpty(compradorNombre) && string.IsNullOrEmpty(compradorNIT))
            {
                _EventsList.Add($"[ERROR] Se requiere una de las 2");
                return "[ERROR] Se requiere una de las 2";
            }
            if (string.IsNullOrEmpty(compradorNombre) && !CompradorExiste(compradorNombre, compradorNIT))
            {
                _EventsList.Add($"[ERROR] Comprador no existe 1");
                return $"[ERROR] Comprador no existe";
            }
            if (!string.IsNullOrEmpty(cliente.RegisterClient_Natural_NIT) && ClienteExisteEnComprador(cliente.RegisterClient_Natural_NIT, compradorNIT, compradorNombre))
            {
                _EventsList.Add($"[ERROR] Cliente Natural ya existe en comprador");
                return $"[ERROR] Cliente ya existe en comprador";
            }
            if (!string.IsNullOrEmpty(cliente.RegisterClient_Juridical_NIT) && ClienteExisteEnComprador(cliente.RegisterClient_Juridical_NIT, compradorNIT, compradorNombre))
            {
                _EventsList.Add($"[ERROR] Cliente Empreza ya existe en comprador");
                return $"[ERROR] Cliente ya existe en comprador";
            }

            var clienteInfo = new Flow_response_json_Model_1584870855544061_CrearCliente { };
            if (cliente.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Juridica")
            {
                _EventsList.Add($"Tipo_Persona_Juridica");

                _EventsList.Add($"Nit: {cliente.RegisterClient_Juridical_NIT}");
                _EventsList.Add($"Razon social: {cliente.RegisterClient_Juridical_Razon_Social}");
                _EventsList.Add($"Digito verificacion: {cliente.RegisterClient_Juridical_Digito_Verificacion}");
                _EventsList.Add($"Direccion: {cliente.RegisterClient_Juridical_Direccion}");
                _EventsList.Add($"Departamento: {cliente.RegisterClient_Juridical_Departamento}");
                _EventsList.Add($"Ciudad: {cliente.RegisterClient_Juridical_Ciudad}");
                _EventsList.Add($"Tipo régimen: {cliente.RegisterClient_Juridical_Tipo_Regimen}");
                _EventsList.Add($"Obligaciones Fiscales: {cliente.RegisterClient_Juridical_Obligaciones_Fiscales}");
                _EventsList.Add($"Correo: {cliente.RegisterClient_Juridical_Email}");

                clienteInfo = new Flow_response_json_Model_1584870855544061_CrearCliente
                {
                    RegisterClient_Juridical_NIT = cliente.RegisterClient_Juridical_NIT,
                    RegistraCliente_Tipo_Cliente = cliente.RegistraCliente_Tipo_Cliente,

                    RegisterClient_Juridical_Razon_Social = cliente.RegisterClient_Juridical_Razon_Social,
                    RegisterClient_Juridical_Digito_Verificacion = cliente.RegisterClient_Juridical_Digito_Verificacion,
                    RegisterClient_Juridical_Direccion = cliente.RegisterClient_Juridical_Direccion,
                    RegisterClient_Juridical_Departamento = cliente.RegisterClient_Juridical_Departamento,
                    RegisterClient_Juridical_Ciudad = cliente.RegisterClient_Juridical_Ciudad,
                    RegisterClient_Juridical_Tipo_Regimen = cliente.RegisterClient_Juridical_Tipo_Regimen,
                    RegisterClient_Juridical_Obligaciones_Fiscales = cliente.RegisterClient_Juridical_Obligaciones_Fiscales,
                    RegisterClient_Juridical_Email = cliente.RegisterClient_Juridical_Email
                };
            }
            else if (cliente.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Natural")
            {
                _EventsList.Add($"Tipo_Persona_Natural");

                _EventsList.Add($"Nit: {cliente.RegisterClient_Natural_NIT}");
                _EventsList.Add($"Nombre: {cliente.RegisterClient_Natural_Nombre}");
                _EventsList.Add($"Apellido paterno: {cliente.RegisterClient_Natural_Apellido_Paterno}");
                _EventsList.Add($"Apellido materno: {cliente.RegisterClient_Natural_Apellido_Materno}");
                _EventsList.Add($"Correo: {cliente.RegisterClient_Natural_Correo}");
                _EventsList.Add($"Teléfono: {cliente.RegisterClient_Natural_Telefono}");
                _EventsList.Add($"Digito de Verificacion: {cliente.RegisterClient_Natural_Digito_Verificacin}");
                _EventsList.Add($"Direccion: {cliente.RegisterClient_Natural_Direccion}");
                _EventsList.Add($"Departamento: {cliente.RegisterClient_Natural_Departamento}");
                _EventsList.Add($"Ciudad: {cliente.RegisterClient_Natural_Ciudad}");
                _EventsList.Add($"Tipo régimen: {cliente.RegisterClient_Natural_Tipo_Rgimen}");
                _EventsList.Add($"Obligaciones Fiscales: {cliente.RegisterClient_Natural_Obligaciones_Fiscale}");

                clienteInfo = new Flow_response_json_Model_1584870855544061_CrearCliente
                {
                    RegisterClient_Natural_NIT = cliente.RegisterClient_Natural_NIT,
                    RegistraCliente_Tipo_Cliente = cliente.RegistraCliente_Tipo_Cliente,

                    RegisterClient_Natural_Nombre = cliente.RegisterClient_Natural_Nombre,
                    RegisterClient_Natural_Apellido_Paterno = cliente.RegisterClient_Natural_Apellido_Paterno,
                    RegisterClient_Natural_Apellido_Materno = cliente.RegisterClient_Natural_Apellido_Materno,
                    RegisterClient_Natural_Correo = cliente.RegisterClient_Natural_Correo,
                    RegisterClient_Natural_Telefono = cliente.RegisterClient_Natural_Telefono,
                    RegisterClient_Natural_Tipo_Identificacion = cliente.RegisterClient_Natural_Tipo_Identificacion,
                    RegisterClient_Natural_Digito_Verificacin = cliente.RegisterClient_Natural_Digito_Verificacin,
                    RegisterClient_Natural_Direccion = cliente.RegisterClient_Natural_Direccion,
                    RegisterClient_Natural_Departamento = cliente.RegisterClient_Natural_Departamento,
                    RegisterClient_Natural_Ciudad = cliente.RegisterClient_Natural_Ciudad,
                    RegisterClient_Natural_Tipo_Rgimen = cliente.RegisterClient_Natural_Tipo_Rgimen,
                    RegisterClient_Natural_Obligaciones_Fiscale = cliente.RegisterClient_Natural_Obligaciones_Fiscale
                };
            }
            else
            {
                _EventsList.Add($"[ERROR] Tipo Persona Not identified");
                return $"[ERROR] Tipo Persona Not identified";
            }

            /*var superKey = (compradorNombre, compradorNIT);
            if (_compradores[superKey] != null)
            {
                if(cliente.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Juridica")
                {
                    if (!_compradores[superKey].Clientes2.Any(c =>
                     c.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Juridica" &&
                     c.RegisterClient_Juridical_NIT == clienteInfo.RegisterClient_Juridical_NIT))
                    {
                        _compradores[superKey].Clientes2.Add(clienteInfo);
                        string NIT = clienteInfo.RegisterClient_Juridical_NIT;
                        _EventsList.Add($"El Cliente empresa '{NIT}' se registró correctamente en {compradorNombre}, {compradorNIT}");
                        return $"El Cliente '{NIT}' se registró correctamente";
                    }

                }
                else if (clienteInfo.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Natural")
                {
                    if (!_compradores[superKey].Clientes2.Any(c =>
                     c.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Natural" &&
                     c.RegisterClient_Natural_NIT == clienteInfo.RegisterClient_Natural_NIT))
                    {
                        _compradores[superKey].Clientes2.Add(clienteInfo);
                        string NIT = clienteInfo.RegisterClient_Natural_NIT;
                        _EventsList.Add($"Key, El Cliente tipo persona '{NIT}' se registró correctamente en {compradorNombre}, {compradorNIT}");
                        return $"El Cliente '{NIT}' se registró correctamente";
                    }

                }
                /*
                if (!_compradores[superKey].Clientes2.Any(c => c.RegisterClient_Juridical_NIT == clienteInfo.RegisterClient_Juridical_NIT) &&
                !_compradores[superKey].Clientes2.Any(c => c.RegisterClient_Natural_NIT == clienteInfo.RegisterClient_Natural_NIT))
                {
                    _compradores[superKey].Clientes2.Add(clienteInfo);
                    string NIT = clienteInfo.RegisterClient_Juridical_NIT ?? clienteInfo.RegisterClient_Natural_NIT ?? "Sin nombre";
                    _EventsList.Add($"Key, El Cliente '{NIT}' se registró correctamente en {compradorNombre}, {compradorNIT}");
                    return $"El Cliente '{NIT}' se registró correctamente";
                }*//*

                _EventsList.Add($"Key El Cliente '{clienteInfo.RegisterClient_Juridical_NIT ?? clienteInfo.RegisterClient_Natural_NIT}' Ya existe en {compradorNombre}, {compradorNIT} at {DateTime.Now}");
                return $"El Cliente '{clienteInfo.RegisterClient_Juridical_NIT ?? clienteInfo.RegisterClient_Natural_NIT}' Ya existe en {compradorNombre}, {compradorNIT} at {DateTime.Now}";
            }*/

            foreach (var entry in _compradores)
            {
                var (nombre, nit) = entry.Key;
                if (nombre.Contains(compradorNombre, StringComparison.OrdinalIgnoreCase) || nit.Contains(compradorNIT, StringComparison.OrdinalIgnoreCase) ||
                    nombre.Equals(compradorNombre, StringComparison.OrdinalIgnoreCase) || nit.Equals(compradorNIT, StringComparison.OrdinalIgnoreCase))
                {

                    var compradoresList = entry.Value;

                    bool nitJuridicoYaExiste = !string.IsNullOrEmpty(clienteInfo.RegisterClient_Juridical_NIT) &&
                        compradoresList.Clientes2.Any(c => c.RegisterClient_Juridical_NIT == clienteInfo.RegisterClient_Juridical_NIT);

                    bool nitNaturalYaExiste = !string.IsNullOrEmpty(clienteInfo.RegisterClient_Natural_NIT) &&
                        compradoresList.Clientes2.Any(c => c.RegisterClient_Natural_NIT == clienteInfo.RegisterClient_Natural_NIT);

                    if (!nitJuridicoYaExiste && !nitNaturalYaExiste)
                    {
                        var countAntes = _compradores[(compradorNombre, compradorNIT)].Clientes2.Count;

                        //compradoresList.Clientes2.Add(clienteInfo);
                        _compradores[entry.Key].Clientes2.Add(clienteInfo); // = compradoresList;

                        string NIT = clienteInfo.RegisterClient_Juridical_NIT ?? clienteInfo.RegisterClient_Natural_NIT ?? "Sin nombre";
                        _EventsList.Add($"El Cliente '{NIT}' se registró correctamente en {nombre}, {nit}");

                        var countDespues = _compradores[(compradorNombre, compradorNIT)].Clientes2.Count;
                        _EventsList.Add($"Antes: {countAntes}, Después: {countDespues}");
                        return $"El Cliente '{NIT}' se registró correctamente";
                    }
                    _EventsList.Add($"El Cliente '{clienteInfo.RegisterClient_Juridical_NIT ?? clienteInfo.RegisterClient_Natural_NIT}' Ya existe en {nombre}, {nit} at {DateTime.Now}");
                    return $"El Cliente '{clienteInfo.RegisterClient_Juridical_NIT ?? clienteInfo.RegisterClient_Natural_NIT}' Ya existe en {nombre}, {nit} at {DateTime.Now}";
                }
            }

            _EventsList.Add($"[ERROR] Comprador '{compradorNombre}' with NIT '{compradorNIT}' not found at {DateTime.Now}");

            return $"[ERROR] Comprador '{compradorNombre}' with NIT '{compradorNIT}' not found at {DateTime.Now}";
        }
        public string ModificarClienteJurídicaDeComprador(string compradorNombre, string compradorNIT, Flow_response_json_Model_1378725303264167_Modificar_Cliente_Persona_Jurídica cliente)
        {
            _EventsList.Add($"ModificarClienteJudicialDeComprador");
            if (string.IsNullOrEmpty(cliente.ModificarCliente_Empresa_Juridical_Razon_Social) || string.IsNullOrEmpty(cliente.ModificarCliente_Empresa_Juridical_NIT))
            {
                _EventsList.Add($"[ERROR] Rason social o NIT incorrecto");
                return $"[ERROR] Rason social o NIT incorrecto";
            }
            if (string.IsNullOrEmpty(compradorNombre) && string.IsNullOrEmpty(compradorNIT))
            {
                _EventsList.Add($"[ERROR] Se requiere nombre o nit de comprador");
                return "[ERROR] Se requiere nombre o nit de comprador";
            }
            if (string.IsNullOrEmpty(compradorNombre) && !CompradorExiste(compradorNombre, compradorNIT))
            {
                _EventsList.Add($"[ERROR] Comprador no existe 1");
                return $"[ERROR] Comprador no existe";
            }

            try {
                var clienteNIT = cliente.ModificarCliente_Empresa_Juridical_NIT;
                var clienteNombre = cliente.ModificarCliente_Empresa_Juridical_Razon_Social;
                var clienteEmail = cliente.ModificarCliente_Empresa_Juridical_Email;
                _EventsList.Add($"Comprador {compradorNombre}, {compradorNIT}");
                _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT}");

                var clienteInfo = new Flow_response_json_Model_1584870855544061_CrearCliente
                {
                    RegistraCliente_Tipo_Cliente = cliente.ModificarCliente_Empresa_Tipo_Cliente,
                    RegisterClient_Juridical_Razon_Social = clienteNombre,
                    RegisterClient_Juridical_NIT = clienteNIT,
                    RegisterClient_Juridical_Email = clienteEmail,

                    RegisterClient_Juridical_Digito_Verificacion = cliente.ModificarCliente_Empresa_Juridical_Digito_Verificacion,
                    RegisterClient_Juridical_Direccion = cliente.ModificarCliente_Empresa_Juridical_Direccion,
                    RegisterClient_Juridical_Departamento = cliente.ModificarCliente_Empresa_Juridical_Departamento,
                    RegisterClient_Juridical_Ciudad = cliente.ModificarCliente_Empresa_Juridical_Ciudad,
                    RegisterClient_Juridical_Tipo_Regimen = cliente.ModificarCliente_Empresa_Juridical_Tipo_Regimen,
                    RegisterClient_Juridical_Obligaciones_Fiscales = cliente.ModificarCliente_Empresa_Juridical_Obligaciones_Fiscales
                };

                foreach (var entry in _compradores)
                {
                    var (nombre, nit) = entry.Key;
                    if (/*nombre.Contains(compradorNombre, StringComparison.OrdinalIgnoreCase) ||*/ nit == compradorNIT)
                    {
                        var index = _compradores[entry.Key].Clientes2.FindIndex(c => c.RegisterClient_Juridical_NIT.Equals(clienteNIT, StringComparison.OrdinalIgnoreCase));
                        if (index >= 0)
                        {
                            _compradores[entry.Key].Clientes2[index] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index2 = _compradores[entry.Key].Clientes2.FindIndex(c => 
                            !string.IsNullOrEmpty(c.RegisterClient_Juridical_Razon_Social) &&
                            c.RegisterClient_Juridical_Razon_Social.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index2 >= 0)
                        {
                            _compradores[entry.Key].Clientes2[index2] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index3 = _compradores[entry.Key].Clientes2.FindIndex(c =>
                            !string.IsNullOrEmpty(c.RegisterClient_Natural_Nombre) &&
                            c.RegisterClient_Natural_Nombre.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index3 >= 0)
                        {
                            _compradores[entry.Key].Clientes2[index3] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index4 = _compradores[entry.Key].Clientes2.FindIndex(c =>
                            !string.IsNullOrEmpty(c.RegisterClient_Natural_Apellido_Paterno) &&
                            c.RegisterClient_Natural_Apellido_Paterno.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index4 >= 0)
                        {
                            _compradores[entry.Key].Clientes2[index4] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index5 = _compradores[entry.Key].Clientes2.FindIndex(c =>
                            !string.IsNullOrEmpty(c.RegisterClient_Natural_Apellido_Materno) &&
                            c.RegisterClient_Natural_Apellido_Materno.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index5 >= 0)
                        {
                            _compradores[entry.Key].Clientes2[index5] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                    }
                }
                _EventsList.Add($"[ERROR] Empresa Cliente {clienteNombre}, {clienteNIT} No encontrada");
                    return $"[ERROR] Empresa Cliente {clienteNombre}, {clienteNIT} No encontrada";
            }catch(Exception ex)
            {
                _EventsList.Add($"[ERROR] Empresa Cliente: {ex}");
                return $"[ERROR] Empresa Cliente: {ex}";
            }
        }
        public string ModificarClienteNaturalDeComprador(string compradorNombre, string compradorNIT, Flow_response_json_Model_931945452349522_Modificar_Cliente_Persona_Física cliente)
        {
            try{
            _EventsList.Add($"ModificarClienteNaturalDeComprador");
            if ((string.IsNullOrEmpty(cliente.ModificarCliente_Natural_Nombre) && string.IsNullOrEmpty(cliente.ModificarCliente_Natural_Apellido_Paterno) && string.IsNullOrEmpty(cliente.ModificarCliente_Natural_Apellido_Materno)) || string.IsNullOrEmpty(cliente.ModificarCliente_Natural_NIT))
            {
                _EventsList.Add($"[ERROR] Nombre, apellido o NIT incorrecto");
                return $"[ERROR] Rason social o NIT incorrecto";
            }
            if (string.IsNullOrEmpty(compradorNombre) && string.IsNullOrEmpty(compradorNIT))
            {
                _EventsList.Add($"[ERROR] Se requiere nombre o nit de comprador");
                return "[ERROR] Se requiere nombre o nit de comprador";
            }
            if (string.IsNullOrEmpty(compradorNombre) && !CompradorExiste(compradorNombre, compradorNIT))
            {
                _EventsList.Add($"[ERROR] Comprador no existe 1");
                return $"[ERROR] Comprador no existe";
            }
            string clienteNIT = cliente.ModificarCliente_Natural_NIT;
            string clienteNombre = cliente.ModificarCliente_Natural_Nombre + " " +cliente.ModificarCliente_Natural_Apellido_Paterno + " " + cliente.ModificarCliente_Natural_Apellido_Materno;
            _EventsList.Add($"Comprador {compradorNombre}, {compradorNIT}");
            _EventsList.Add($"Persona Natural {clienteNombre}, {clienteNIT}");

            var clienteInfo = new Flow_response_json_Model_1584870855544061_CrearCliente
            {
                RegistraCliente_Tipo_Cliente = cliente.ModificarCliente_Tipo_Cliente,
                RegisterClient_Natural_NIT = clienteNIT,

                RegisterClient_Natural_Nombre = cliente.ModificarCliente_Natural_Nombre,
                RegisterClient_Natural_Apellido_Paterno = cliente.ModificarCliente_Natural_Apellido_Paterno,
                RegisterClient_Natural_Apellido_Materno = cliente.ModificarCliente_Natural_Apellido_Materno,
                RegisterClient_Natural_Correo = cliente.ModificarCliente_Natural_Correo,

                RegisterClient_Natural_Telefono = cliente.ModificarCliente_Natural_Telefono,
                RegisterClient_Natural_Tipo_Identificacion = cliente.ModificarCliente_Natural_Tipo_Identificacion,
                RegisterClient_Natural_Digito_Verificacin = cliente.ModificarCliente_Natural_Digito_Verificacin,
                RegisterClient_Natural_Direccion = cliente.ModificarCliente_Natural_Label,
                RegisterClient_Natural_Departamento = cliente.ModificarCliente_Natural_Departamento,
                RegisterClient_Natural_Ciudad = cliente.ModificarCliente_Natural_Ciudad,
                RegisterClient_Natural_Tipo_Rgimen = cliente.ModificarCliente_Natural_Tipo_Rgimen,
                RegisterClient_Natural_Obligaciones_Fiscale = cliente.ModificarCliente_Natural_Obligaciones_Fiscale
            };

            foreach (var entry in _compradores)
            {
                var (nombre, nit) = entry.Key;
                if (nombre.Contains(compradorNombre, StringComparison.OrdinalIgnoreCase) || nit.Contains(compradorNIT, StringComparison.OrdinalIgnoreCase))
                {
                    var index = _compradores[entry.Key].Clientes2.FindIndex(c => c.RegisterClient_Natural_NIT.Contains(clienteNIT, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        _compradores[entry.Key].Clientes2[index] = clienteInfo;
                        _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                        return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                    }
                    index = _compradores[entry.Key].Clientes2.FindIndex(c => c.RegisterClient_Natural_Nombre.Contains(cliente.ModificarCliente_Natural_Nombre, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        _compradores[entry.Key].Clientes2[index] = clienteInfo;
                        _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                        return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                    }
                    index = _compradores[entry.Key].Clientes2.FindIndex(c => c.RegisterClient_Natural_Apellido_Paterno.Contains(cliente.ModificarCliente_Natural_Apellido_Paterno, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        _compradores[entry.Key].Clientes2[index] = clienteInfo;
                        _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                        return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                    }
                    index = _compradores[entry.Key].Clientes2.FindIndex(c => c.RegisterClient_Natural_Apellido_Materno.Contains(cliente.ModificarCliente_Natural_Apellido_Materno, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        _compradores[entry.Key].Clientes2[index] = clienteInfo;
                        _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                        return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                    }
                }
            }
            _EventsList.Add($"[ERROR] Persona Natural {clienteNombre}, {clienteNIT} No encontrada");
                return $"[ERROR] Persona Natural {clienteNombre}, {clienteNIT} No encontrada";
            }catch(Exception ex)
            {
                _EventsList.Add($"[ERROR] Cliente Natural: {ex}");
                return $"[ERROR] Cliente Natural: {ex}";
            }
        }
        public List<Flow_response_json_Model_1584870855544061_CrearCliente> BuscarListaClientes2(string compradorNombre, string compradorNIT, string clientPice)
        {
            _EventsList.Add("BuscarListaClientes2");
            _EventsList.Add($"Comprador:{compradorNombre}, ");
            _EventsList.Add($"BuscarListaClientes2");
            var resultados = new List<Flow_response_json_Model_1584870855544061_CrearCliente>();

            clientPice = clientPice.ToLower();

            if (!_compradores.TryGetValue((compradorNombre, compradorNIT), out var compradorInfo))
                return resultados; // Return empty list if the buyer is not found

            foreach (var cliente in compradorInfo.Clientes2)
            {
                if (cliente != null && (
             cliente.RegisterClient_Natural_Nombre?.ToLower().Contains(clientPice) == true ||
             cliente.RegisterClient_Natural_Apellido_Materno?.ToLower().Contains(clientPice) == true ||
             cliente.RegisterClient_Natural_Apellido_Paterno?.ToLower().Contains(clientPice)== true ||
             cliente.RegisterClient_Natural_NIT?.ToLower().Contains(clientPice) == true || 
             cliente.RegisterClient_Juridical_Razon_Social?.ToLower().Contains(clientPice) == true ||
             cliente.RegisterClient_Juridical_NIT?.ToLower().Contains(clientPice) == true ) ) 
                    resultados.Add(cliente);
            }

            return resultados;
        }
        public string ReturnClientType(string compradorNombre, string compradorNIT, string clienteNIT)
        {

            if (!_compradores.TryGetValue((compradorNombre, compradorNIT), out var compradorInfo))
                return "[Error] no hay comprador";
            var clienteFind = compradorInfo.Clientes2.FirstOrDefault(c => (!string.IsNullOrWhiteSpace(c.RegisterClient_Juridical_NIT) && c.RegisterClient_Juridical_NIT.Equals(clienteNIT))||
                (!string.IsNullOrWhiteSpace(c.RegisterClient_Natural_NIT) && c.RegisterClient_Natural_NIT.Equals(clienteNIT)));

            if (clienteFind == null || clienteFind.RegistraCliente_Tipo_Cliente == null)
            {
                return "[Error] no hay cliente";
            }

            return clienteFind.RegistraCliente_Tipo_Cliente;
        }

        public List<(string Nombre, string Codigo)> ListaDeCoincidenciasProductos(string compradorNombre, string compradorNIT, string messagePiece)
        {
            var coincidencias = new List<(string Nombre, string Codigo)>(); // lista a retornar

            if (_compradores.TryGetValue((compradorNombre, compradorNIT), out var comprador))
            {
                foreach (var producto in comprador.Productos)
                {
                    string nombre = producto.Agregar_Producto_Nombre ?? "";
                    string Codigo = producto.Agregar_Producto_Codigo ?? "";

                    if (nombre.Contains(messagePiece, StringComparison.OrdinalIgnoreCase) ||
                        Codigo.Contains(messagePiece, StringComparison.OrdinalIgnoreCase))
                    {
                        coincidencias.Add((nombre, Codigo));
                    }
                }
            }
            return coincidencias;
        }
        private static CompradorInfo2? BuscarYTransformarComprador(string? nombre, string? nit)
        {
            if (string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(nit)) // Si no recibe alguno de los 2 retorna null
                return null;

            // Buscar coincidencia exacta en el diccionario
            var compradorEntry = _compradores
                .FirstOrDefault(c =>
                (string.IsNullOrWhiteSpace(nombre) || c.Key.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(nit) || c.Key.NIT.Equals(nit, StringComparison.OrdinalIgnoreCase)));

            if (compradorEntry.Equals(default(KeyValuePair<(string, string), CompradorInfo>)))
                return null;

            var comprador = compradorEntry.Value;
            var infoComprador = new CompradorInfo2();

            //Registro viejo para los ejemplos
            /*if (comprador.Datos != null)
            {
                string apellidopaterno;
                string apellidomaterno = "";
                string[] words = comprador.Datos.screen_0_Apellidos_1.Split(' ');
                if (words.Count() > 2)
                {
                    apellidopaterno = words[0];
                    for (int i = 1; i < words.Count(); i++)
                        apellidomaterno = apellidomaterno + words[i];
                }
                else if (words.Count() == 2)
                {
                    apellidopaterno = words[0];
                    apellidomaterno = words[1];
                }
                else if (words.Count() == 1)
                {
                    apellidopaterno = words[0];
                    apellidomaterno = "-";
                }
                else
                {
                    apellidopaterno = "-";
                    apellidomaterno = "-";
                }
                
                infoComprador.TipoPersona = "Natural";
                infoComprador.Nombre = comprador.Datos.screen_0_Nombres_0;
                infoComprador.ApellidoPaterno = apellidopaterno;
                infoComprador.ApellidoMaterno = apellidomaterno;
                infoComprador.NIT = comprador.Datos.screen_1_NIT_0;
                infoComprador.DigitoVerificacion = comprador.Datos.screen_1_Digito_Verificacin_1;
                infoComprador.Direccion = comprador.Datos.screen_1_Label_2;
                infoComprador.Departamento = comprador.Datos.screen_1_Departamento_3;
                infoComprador.Ciudad = comprador.Datos.screen_1_Ciudad_4;
                infoComprador.TipoRegimen = comprador.Datos.screen_1_Tipo_Rgimen_5;
                infoComprador.ObligacionesFiscales = comprador.Datos.screen_1_Obligaciones_Fiscale_6;


                infoComprador.Correos = comprador.Correos;
            }*/

            // Datos persona natural
            if (comprador.DatosPersonaFisicaSimple != null)
            {
                infoComprador.TipoPersona = "Natural";
                infoComprador.Nombre = comprador.DatosPersonaFisicaSimple.Registrar_Persona_Fisica_Nombre_0;
                infoComprador.ApellidoPaterno = comprador.DatosPersonaFisicaSimple.Registrar_Persona_Fisica_Apellido_Paterno_1;
                infoComprador.ApellidoMaterno = comprador.DatosPersonaFisicaSimple.Registrar_Persona_Fisica_Apellido_Materno_2;
                infoComprador.NIT = comprador.DatosPersonaFisicaSimple.screen_1_NIT_0;
                infoComprador.DigitoVerificacion = comprador.DatosPersonaFisicaSimple.screen_1_Digito_Verificacin_1;
                infoComprador.Direccion = comprador.DatosPersonaFisicaSimple.screen_1_Label_2;
                infoComprador.Departamento = comprador.DatosPersonaFisicaSimple.screen_1_Departamento_3;
                infoComprador.Ciudad = comprador.DatosPersonaFisicaSimple.screen_1_Ciudad_4;
                infoComprador.TipoRegimen = comprador.DatosPersonaFisicaSimple.screen_1_Tipo_Rgimen_5;
                infoComprador.ObligacionesFiscales = comprador.DatosPersonaFisicaSimple.screen_1_Obligaciones_Fiscale_6;

                infoComprador.Correos = comprador.Correos;
            }

            // Datos empresa
            else if (comprador.DatosRegistrarEmpresa != null)
            {
                infoComprador.TipoPersona = "Juridica";
                infoComprador.RazonSocial = comprador.DatosRegistrarEmpresa.Registrar_Empresa_Nombre_0;
                infoComprador.NIT = comprador.DatosRegistrarEmpresa.Registrar_Empresa_NIT_1;
                //infoComprador.Correos = comprador.DatosRegistrarEmpresa.Registrar_Empresa_Correo_2;
                infoComprador.DigitoVerificacion = comprador.DatosRegistrarEmpresa.Registrar_Empresa_Digito_Verificacin_1;
                infoComprador.Direccion = comprador.DatosRegistrarEmpresa.Registrar_Empresa_Direccion_2;
                infoComprador.Departamento = comprador.DatosRegistrarEmpresa.Registrar_Empresa_Departamento_3;
                infoComprador.Ciudad = comprador.DatosRegistrarEmpresa.Registrar_Empresa_Ciudad_4;
                infoComprador.TipoRegimen = comprador.DatosRegistrarEmpresa.Registrar_Empresa_Tipo_Rgimen_5;
                infoComprador.ObligacionesFiscales = comprador.DatosRegistrarEmpresa.Registrar_Empresa_Obligaciones_Fiscales_6;


                infoComprador.Correos = comprador.Correos;

                if (!string.IsNullOrEmpty(comprador.DatosRegistrarEmpresa.Registrar_Empresa_Correo_2))
                    infoComprador.Correos.Add(comprador.DatosRegistrarEmpresa.Registrar_Empresa_Correo_2);
            }

            // Correos y teléfonos
            infoComprador.Telefonos = comprador.Telefonos;
            //infoComprador.Correos = comprador.Correos;
            //Clientes
            /*foreach (var cliente in comprador.Clientes)
            {
                // Registrar Natural
                if ((cliente.RegistraCliente_Tipo_Cliente != null)&& (cliente.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Natural"))
                {
                    infoComprador.Clientes.Add(new ClienteInfo2
                    {
                        NIT = cliente.screen_0_NIT_4,
                        TipoCliente = "Natural",
                        Nombre = cliente.RegisterClient_Natural_Nombre,
                        ApellidoPaterno = cliente.RegisterClient_Natural_Apellido_Paterno,
                        ApellidoMaterno = cliente.RegisterClient_Natural_Apellido_Materno,
                        Correo = cliente.RegisterClient_Natural_Correo,
                        Telefono = cliente.RegisterClient_Natural_Telefono,
                        TipoIdentificacion = cliente.RegisterClient_Natural_Tipo_Identificacion,
                        DigitoVerificacion = cliente.RegisterClient_Natural_Digito_Verificacin,
                        Departamento = cliente.RegisterClient_Natural_Departamento,
                        Ciudad = cliente.RegisterClient_Natural_Ciudad,
                        Direccion = cliente.RegisterClient_Natural_Label,
                        TipoRegimen = cliente.RegisterClient_Natural_Tipo_Rgimen,
                        ObligacionesFiscales = cliente.RegisterClient_Natural_Obligaciones_Fiscale,
                        RazonSocial = "-"
                    });
                }
                // Registrar Jurídico
                else if ((cliente.RegistraCliente_Tipo_Cliente != null) && (cliente.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Juridica"))
                {
                    infoComprador.Clientes.Add(new ClienteInfo2
                    {
                        NIT = cliente.screen_0_NIT_4,
                        TipoCliente = "Juridica",
                        Nombre = "-",
                        ApellidoPaterno = "-",
                        ApellidoMaterno = "-",
                        Correo = "-",
                        Telefono = "-",
                        TipoIdentificacion = "-",
                        DigitoVerificacion = cliente.RegisterClient_Juridical_Digito_Verificacion,
                        Departamento = cliente.RegisterClient_Juridical_Departamento,
                        Ciudad = cliente.RegisterClient_Juridical_Ciudad,
                        Direccion = cliente.RegisterClient_Juridical_Direccion,
                        TipoRegimen = cliente.RegisterClient_Juridical_Tipo_Regimen,
                        ObligacionesFiscales = cliente.RegisterClient_Juridical_Obligaciones_Fiscales,
                        RazonSocial = cliente.RegisterClient_Juridical_Razon_Social
                    });
                }
                // Modificar Natural
                else if (!string.IsNullOrEmpty(cliente.ModificarCliente_Tipo_Cliente))
                {
                    infoComprador.Clientes.Add(new ClienteInfo2
                    {
                        NIT = cliente.screen_0_NIT_4,
                        TipoCliente = "Natural",
                        Nombre = cliente.ModificarCliente_Natural_Nombre,
                        ApellidoPaterno = cliente.ModificarCliente_Natural_Apellido_Paterno,
                        ApellidoMaterno = cliente.ModificarCliente_Natural_Apellido_Materno,
                        Correo = cliente.ModificarCliente_Natural_Correo,
                        Telefono = cliente.ModificarCliente_Natural_Telefono,
                        TipoIdentificacion = cliente.ModificarCliente_Natural_Tipo_Identificacion,
                        DigitoVerificacion = cliente.ModificarCliente_Natural_Digito_Verificacin,
                        Departamento = cliente.ModificarCliente_Natural_Departamento,
                        Ciudad = cliente.ModificarCliente_Natural_Ciudad,
                        Direccion = cliente.ModificarCliente_Natural_Label,
                        TipoRegimen = cliente.ModificarCliente_Natural_Tipo_Rgimen,
                        ObligacionesFiscales = cliente.ModificarCliente_Natural_Obligaciones_Fiscale,
                        RazonSocial = "-"
                    });
                }
                // Modificar Jurídico
                else if (!string.IsNullOrEmpty(cliente.ModificarCliente_Empresa_Tipo_Cliente))
                {
                    infoComprador.Clientes.Add(new ClienteInfo2
                    {
                        NIT = cliente.screen_0_NIT_4,
                        TipoCliente = "Juridica",
                        Nombre = "-",
                        ApellidoPaterno = "-",
                        ApellidoMaterno = "-",
                        Correo = "-",
                        Telefono = "-",
                        TipoIdentificacion = "-",
                        DigitoVerificacion = cliente.ModificarCliente_Empresa_Juridical_Digito_Verificacion,
                        Departamento = cliente.ModificarCliente_Empresa_Juridical_Departamento,
                        Ciudad = cliente.ModificarCliente_Empresa_Juridical_Ciudad,
                        Direccion = cliente.ModificarCliente_Empresa_Juridical_Direccion,
                        TipoRegimen = cliente.ModificarCliente_Empresa_Juridical_Tipo_Regimen,
                        ObligacionesFiscales = cliente.ModificarCliente_Empresa_Juridical_Obligaciones_Fiscales,
                        RazonSocial = cliente.ModificarCliente_Empresa_Juridical_Razon_Social
                    });
                }
                // flow viejo
                else if (cliente.screen_0_Primer_Nombre_0 != null)
                {
                    infoComprador.Clientes.Add(new ClienteInfo2
                    {
                        NIT = cliente.screen_0_NIT_4,
                        TipoCliente = "Natural",
                        Nombre = cliente.screen_0_Primer_Nombre_0 + " " + cliente.screen_0_Segundo_Nombre_1,
                        ApellidoPaterno = cliente.screen_0_Apellido_Paterno_2,
                        ApellidoMaterno = cliente.screen_0_Apellido_Materno_3,
                        Correo = "-",
                        Telefono = "-",
                        TipoIdentificacion = "-",
                        DigitoVerificacion = "-",
                        Departamento = "-",
                        Ciudad = "-",
                        Direccion = "-",
                        TipoRegimen = "-",
                        ObligacionesFiscales = "-",
                        RazonSocial = "-"
                    });

                }
            }*/

            foreach (var cliente in comprador.Clientes2)
            {
                // Registrar Natural
                if (!string.IsNullOrEmpty(cliente.RegistraCliente_Tipo_Cliente) && (cliente.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Natural"))
                {
                    infoComprador.Clientes.Add(new ClienteInfo2
                    {
                        NIT = cliente.RegisterClient_Natural_NIT,
                        TipoCliente = "Tipo_Persona_Natural",
                        Nombre = cliente.RegisterClient_Natural_Nombre,
                        ApellidoPaterno = cliente.RegisterClient_Natural_Apellido_Paterno,
                        ApellidoMaterno = cliente.RegisterClient_Natural_Apellido_Materno,
                        Correo = cliente.RegisterClient_Natural_Correo,
                        Telefono = cliente.RegisterClient_Natural_Telefono,
                        TipoIdentificacion = cliente.RegisterClient_Natural_Tipo_Identificacion,
                        DigitoVerificacion = cliente.RegisterClient_Natural_Digito_Verificacin,
                        Departamento = cliente.RegisterClient_Natural_Departamento,
                        Ciudad = cliente.RegisterClient_Natural_Ciudad,
                        Direccion = cliente.RegisterClient_Natural_Direccion,
                        TipoRegimen = cliente.RegisterClient_Natural_Tipo_Rgimen,
                        ObligacionesFiscales = cliente.RegisterClient_Natural_Obligaciones_Fiscale,
                        RazonSocial = "-"
                    });
                }
                // Registrar Jurídico
                else if (!string.IsNullOrEmpty(cliente.RegistraCliente_Tipo_Cliente) && (cliente.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Juridica"))
                {
                    infoComprador.Clientes.Add(new ClienteInfo2
                    {
                        NIT = cliente.RegisterClient_Juridical_NIT,
                        TipoCliente = "Tipo_Persona_Juridica",
                        Nombre = "-",
                        ApellidoPaterno = "-",
                        ApellidoMaterno = "-",
                        Correo = cliente.RegisterClient_Juridical_Email,
                        Telefono = "-",
                        TipoIdentificacion = "-",
                        DigitoVerificacion = cliente.RegisterClient_Juridical_Digito_Verificacion,
                        Departamento = cliente.RegisterClient_Juridical_Departamento,
                        Ciudad = cliente.RegisterClient_Juridical_Ciudad,
                        Direccion = cliente.RegisterClient_Juridical_Direccion,
                        TipoRegimen = cliente.RegisterClient_Juridical_Tipo_Regimen,
                        ObligacionesFiscales = cliente.RegisterClient_Juridical_Obligaciones_Fiscales,
                        RazonSocial = cliente.RegisterClient_Juridical_Razon_Social
                    });
                }
            }
            // Productos
            foreach (var producto in comprador.Productos)
            {
                infoComprador.Productos.Add(new ProductoInfo2
                {
                    Nombre = producto.Agregar_Producto_Nombre ?? "",
                    PrecioUnitario = producto.Agregar_Producto_Precio_Unitario ?? "",
                    InfoAdicional = producto.Agregar_Producto_Info_Adicional ?? "",
                    Codigo = producto.Agregar_Producto_Codigo ?? "",
                    UnidadMedida = producto.Agregar_Producto_Unidad_Medida ?? "",
                    Activo = producto.Agregar_Producto_Activo ?? "",
                    Traslados = producto.Agregar_Producto_traslados ?? "",
                    Impuesto = producto.Agregar_Producto_Impuesto ?? "",
                    TasaCuota = producto.Agregar_Producto_Tasa_cuota ?? "",
                    ImpuestosSaludables = producto.Agregar_Producto_Impuestos_Saludables ?? "",
                    ImpuestosSaludables2 = string.Join(",", producto.Agregar_Producto_Impuestos_Saludables2 ?? new())
                });
            }
            return infoComprador;
        }

        public string CrearFacturaJson(
            string compradorNombre, string compradorNIT,
            string clienteNIT,string observacionesFactura,
            List<Flow_response_json_Model_1142951587576244_Crear_Producto> ListaProductos,
            List<Flow_response_json_Model_624251470432760_o_651685330992197_Precio_Unitario_Y_Cantidad> UnitarioYCantidad,
            List<string> correosACopiar)
        {
            var comprador = BuscarYTransformarComprador(compradorNombre, compradorNIT);
            if (comprador == null)
                throw new Exception("[Error] Comprador no encontrado.");
            
            var cliente = comprador.Clientes.FirstOrDefault(c => c.NIT == clienteNIT);
            if (cliente == null)
                throw new Exception("[Error] Cliente no encontrado en comprador.");
            // Armar info comprador
            var compradorJson = new Dictionary<string, object?>
            {
                ["TipoPersona"] = comprador.TipoPersona,
                ["Natural"] = comprador.TipoPersona == "Natural" ? new
                {
                    comprador.Nombre,
                    comprador.ApellidoPaterno,
                    comprador.ApellidoMaterno
                } : null,
                ["Juridica"] = comprador.TipoPersona == "Juridica" ? new
                {
                    comprador.RazonSocial
                } : null,
                ["NIT"] = comprador.NIT,
                ["DigitoVerificacion"] = comprador.DigitoVerificacion,
                //["Direccion"] = direccionFactura ?? comprador.Direccion,
                ["Departamento"] = comprador.Departamento,
                ["Ciudad"] = comprador.Ciudad,
                ["TipoRegimen"] = comprador.TipoRegimen,
                ["ObligacionesFiscales"] = comprador.ObligacionesFiscales,
                //["Telefono"] = telefonoFactura ?? comprador.Telefonos.FirstOrDefault(),
                ["observaciones"] = observacionesFactura ?? "-",
                ["Correo"] = comprador.Correos.FirstOrDefault()
            };

            // Armar info cliente
            compradorJson["Cliente"] = new Dictionary<string, object?>
            {
                ["NIT"] = cliente.NIT,
                ["TipoCliente"] = cliente.TipoCliente,
                ["Juridica"] = cliente.TipoCliente == "Tipo_Persona_Juridica" ? new
                {
                    cliente.RazonSocial
                } : null,
                ["Natural"] = cliente.TipoCliente == "Tipo_Persona_Natural" ? new
                {
                    cliente.Nombre,
                    cliente.ApellidoPaterno,
                    cliente.ApellidoMaterno
                } : null,
                ["Correo"] = cliente.Correo,
                ["Telefono"] = cliente.Telefono,
                ["TipoIdentificacion"] = cliente.TipoIdentificacion,
                ["DigitoVerificacion"] = cliente.DigitoVerificacion,
                ["Departamento"] = cliente.Departamento,
                ["Ciudad"] = cliente.Ciudad,
                ["TipoRegimen"] = cliente.TipoRegimen,
                ["ObligacionesFiscales"] = cliente.ObligacionesFiscales,
                ["Direccion"] = cliente.Direccion
            };

            // Armar productos
            var productos = new List<Dictionary<string, object>>();
            var cantidadCantidades = UnitarioYCantidad.Count;
            for (int i = 0; i < ListaProductos.Count; i++)
            {
                var prod = ListaProductos[i];
                var cantidad = UnitarioYCantidad[i];

                float floatCantidad = float.Parse(cantidad.PUyC_Cantidad, CultureInfo.InvariantCulture.NumberFormat);
                float floatPrecio = float.Parse(cantidad.PUyC_Precio_Unitario, CultureInfo.InvariantCulture.NumberFormat);
                float floatTasaCuota = float.Parse(prod.Agregar_Producto_Tasa_cuota, CultureInfo.InvariantCulture.NumberFormat);
                string impuesto = (floatCantidad * floatPrecio * floatTasaCuota / 100).ToString("0.00");

                productos.Add(new Dictionary<string, object>
                {
                    ["Nombre"] = prod.Agregar_Producto_Nombre,
                    ["PrecioUnitario"] = cantidad.PUyC_Precio_Unitario,
                    ["InfoAdicional"] = string.IsNullOrEmpty(prod.Agregar_Producto_Info_Adicional) ? null : prod.Agregar_Producto_Info_Adicional,
                    ["Codigo"] = prod.Agregar_Producto_Codigo,
                    ["UnidadMedida"] = prod.Agregar_Producto_Unidad_Medida,
                    ["Activo"] = prod.Agregar_Producto_Activo,
                    ["Traslados"] = prod.Agregar_Producto_traslados,
                    ["Impuesto"] = impuesto,
                    ["TasaCuota"] = prod.Agregar_Producto_Tasa_cuota,
                    ["ImpuestosSaludables"] = prod.Agregar_Producto_Impuestos_Saludables ?? "0",
                    ["ImpuestosSaludables2"] = (prod.Agregar_Producto_Impuestos_Saludables2 != null && prod.Agregar_Producto_Impuestos_Saludables2.Any())
                        ? string.Join(", ", prod.Agregar_Producto_Impuestos_Saludables2)
                        : "0",
                    ["Cantidad"] = cantidad.PUyC_Cantidad
                });
            }
            // Agregar Lista de productos
            compradorJson["Productos"] = productos;

            // Agregar correos a copiar
            //compradorJson["CopiarCorreos"] = correosACopiar;
            compradorJson["CopiarCorreos"] = string.Join("; ", correosACopiar);
            // Serializar con opciones para ignorar nulos
            var options = new JsonSerializerOptions
            {
                WriteIndented = false, //  This removes \r\n and indentation
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };
            string json = JsonSerializer.Serialize(compradorJson, options);
            _EventsList.Add(json);
            return json;
        }

        public async Task<string> MandarFacturaJson(string message)
        {
            using HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://chatbot-f1.chingon.solutions/Factura");

            request.Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }
        public async Task<string> PedirPreviewJson(string message)
        {
            using HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://chatbot-f1.chingon.solutions/Preview");

            request.Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }
        //Faltan: Añadir telefono y Añadir Correo
        public dynamic GetEventsList()
        {
            // Return the list of received messages as the response 
            return _EventsList;
        }
    }
}
