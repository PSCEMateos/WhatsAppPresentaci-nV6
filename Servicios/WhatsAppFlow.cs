using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using WhatsAppPresentacionV6.Modelos;
using System.Net.Http;
using Swashbuckle.AspNetCore.SwaggerGen;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Win32;
using System;

namespace WhatsAppPresentacionV6.Servicios
{
    public class WhatsAppFlow
    {
        private readonly string _idTelefono;
        private readonly string _tokenAcceso;
        private readonly string _facebookGraphVersion = "v21.0";
        private static Dictionary<(string Nombre, string NIT), CompradorInfo> _compradores = new Dictionary<(string Nombre, string NIT), CompradorInfo>();
        private static List<string> _EventsList = new List<string>();

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
            //_compradores = new Dictionary<(string Nombre, string NIT), CompradorInfo>();


            //Agregar un ejemplo de comprador
            var key = ("ejemplo prueba", "15083930");
            var emjemploRegistro = new Flow_response_json_Model_568198656271468_Registrate
            {
                screen_0_Nombres_0 = "ejemplo",
                screen_0_Apellidos_1 = "prueba",
                screen_0_Telfono_3 = "No",
                screen_0_Correo_2 = "ejemplo@correo.com",
                screen_1_NIT_0 = "15083930",
                screen_1_Digito_Verificacin_1 = "98765432",
                screen_1_Label_2 = "Ejemplo Dirección",
                screen_1_Departamento_3 = "Ejemplo depto",
                screen_1_Ciudad_4 = "Ciudad ejemplo",
                screen_1_Tipo_Rgimen_5 = "1_No_responsable_de_IVA",
                screen_1_Obligaciones_Fiscale_6 = "4_No_aplica-_Otros",
                flow_token = "00000000",
            };
            var clientesEjemplo = new List<Flow_response_json_Model_1297640437985053_InformacionDelCliente>
            {
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    ModificarCliente_Empresa_Juridical_Razon_Social = "Empresa",
                    ModificarCliente_Empresa_Tipo_Cliente = "Tipo_Persona_Juridica",
                    ModificarCliente_Empresa_Juridical_Digito_Verificacion = "1234",
                    ModificarCliente_Empresa_Juridical_Direccion = "Lopez Mega",
                    ModificarCliente_Empresa_Juridical_Departamento = "Depto",
                    ModificarCliente_Empresa_Juridical_Ciudad = "México",
                    ModificarCliente_Empresa_Juridical_Tipo_Regimen = "Tipo",
                    ModificarCliente_Empresa_Juridical_Obligaciones_Fiscales = "Obligación",

                    screen_0_NIT_4 = "866866866866",
                    flow_token = "10100000",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {

                    screen_0_NIT_4 = "92925757",

                    ModificarCliente_Natural_Nombre = "Uno",
                    ModificarCliente_Natural_Apellido_Paterno = "Dos",
                    ModificarCliente_Natural_Correo = "Tres@c.s",
                    ModificarCliente_Natural_Telefono = "5666",
                    ModificarCliente_Natural_Tipo_Identificacion = "wwe",
                    ModificarCliente_Tipo_Cliente = "Tipo_Persona_Natural",
                    ModificarCliente_Natural_Digito_Verificacin = "4654",
                    ModificarCliente_Natural_Label = "fsew",
                    ModificarCliente_Natural_Departamento = "dsagd",
                    ModificarCliente_Natural_Ciudad = "sgd",
                    ModificarCliente_Natural_Tipo_Rgimen = "sad",
                    ModificarCliente_Natural_Obligaciones_Fiscale = "dtse",

                    flow_token = "20100000",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Carlos",
                    screen_0_Segundo_Nombre_1 = "Alejandro",
                    screen_0_Apellido_Paterno_2 = "Gomez",
                    screen_0_Apellido_Materno_3 = "Fernandez",
                    screen_0_NIT_4 = "1029384756",
                    flow_token = "00000000",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Carlos",
                    screen_0_Segundo_Nombre_1 = "Alejandro",
                    screen_0_Apellido_Paterno_2 = "Perez",
                    screen_0_Apellido_Materno_3 = "Sánches",
                    screen_0_NIT_4 = "1029999756",
                    flow_token = "00000000",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Carlos",
                    screen_0_Segundo_Nombre_1 = "Pumas",
                    screen_0_Apellido_Paterno_2 = "Vs",
                    screen_0_Apellido_Materno_3 = "Chivas",
                    screen_0_NIT_4 = "1029366666",
                    flow_token = "00000000",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Lucía",
                    screen_0_Segundo_Nombre_1 = "María",
                    screen_0_Apellido_Paterno_2 = "Perez",
                    screen_0_Apellido_Materno_3 = "Lopez",
                    screen_0_NIT_4 = "2233445566",
                    flow_token = "00000000",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Roberto",
                    screen_0_Segundo_Nombre_1 = "Carlos",
                    screen_0_Apellido_Paterno_2 = "Mendez",
                    screen_0_Apellido_Materno_3 = "Santos",
                    screen_0_NIT_4 = "3344556677",
                    flow_token = "00000000",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Andrea",
                    screen_0_Segundo_Nombre_1 = "Fernanda",
                    screen_0_Apellido_Paterno_2 = "Martinez",
                    screen_0_Apellido_Materno_3 = "Ramirez",
                    screen_0_NIT_4 = "4455667788",
                    flow_token = "00000000",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Jorge",
                    screen_0_Segundo_Nombre_1 = "Luis",
                    screen_0_Apellido_Paterno_2 = "Serrano",
                    screen_0_Apellido_Materno_3 = "Navarro",
                    screen_0_NIT_4 = "5566778899",
                    flow_token = "00000000",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Sofía",
                    screen_0_Segundo_Nombre_1 = "Gabriela",
                    screen_0_Apellido_Paterno_2 = "Ortiz",
                    screen_0_Apellido_Materno_3 = "Herrera",
                    screen_0_NIT_4 = "6677889900",
                    flow_token = "00000000",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Miguel",
                    screen_0_Segundo_Nombre_1 = "Angel",
                    screen_0_Apellido_Paterno_2 = "Vargas",
                    screen_0_Apellido_Materno_3 = "Figueroa",
                    screen_0_NIT_4 = "7788990011",
                    flow_token = "00000000",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Daniela",
                    screen_0_Segundo_Nombre_1 = "Isabel",
                    screen_0_Apellido_Paterno_2 = "Rodriguez",
                    screen_0_Apellido_Materno_3 = "Castro",
                    screen_0_NIT_4 = "8899001122",
                    flow_token = "00000000",
                },
            };
            /*
            var ejemploCliente = new Flow_response_json_Model_1297640437985053_InformacionDelCliente 
            {
                screen_0_Primer_Nombre_0 = "ejemplo",
                screen_0_Segundo_Nombre_1 = "cliente",
                screen_0_Apellido_Paterno_2 = "ejemplo2",
                screen_0_Apellido_Materno_3 = "cliente2",
                screen_0_NIT_4 = "1297640437985053",
                flow_token = "00000000",
            };*/
            _compradores[key] = new CompradorInfo
            {
                Datos = emjemploRegistro,
                Telefonos = new List<string> { "525526903132" },
                Correos = new List<string> { "ejemplo@correo.com" },
                Clientes = clientesEjemplo//new List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> { ejemploCliente }
            };
            
             key = ("Probador PSC", "11800");
             var emjemploRegistroEmpresa = new Flow_response_json_Model_1187351356327089_RegistrarEmpresa
             {
                Registrar_Empresa_Nombre_0 = "PSC World",
                Registrar_Empresa_NIT_1 = "11800",
                Registrar_Empresa_Digito_Verificacin_1 = "111111",
                Registrar_Empresa_Direccion_2 = "Ejemplo Dirección",
                 Registrar_Empresa_Departamento_3 = "Ejemplo depto",
                 Registrar_Empresa_Ciudad_4 = "Ciudad ejemplo",
                 Registrar_Empresa_Tipo_Rgimen_5 = "1_No_responsable_de_IVA",
                 Registrar_Empresa_Obligaciones_Fiscales_6 = "4_No_aplica-_Otros",
                flow_token = "11111111",
            };
            clientesEjemplo = new List<Flow_response_json_Model_1297640437985053_InformacionDelCliente>
            {
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "PSC",
                    screen_0_Apellido_Paterno_2 = "World",
                    screen_0_Apellido_Materno_3 = "1",
                    screen_0_NIT_4 = "11800",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "PSC",
                    screen_0_Apellido_Paterno_2 = "Chip",
                    screen_0_Apellido_Materno_3 = "2",
                    screen_0_NIT_4 = "00002",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "PSC",
                    screen_0_Apellido_Paterno_2 = "View",
                    screen_0_Apellido_Materno_3 = "3",
                    screen_0_NIT_4 = "00003",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "World",
                    screen_0_Apellido_Paterno_2 = "PSC",
                    screen_0_Apellido_Materno_3 = "4",
                    screen_0_NIT_4 = "00004",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Roberto",
                    screen_0_Segundo_Nombre_1 = "Carlos",
                    screen_0_Apellido_Paterno_2 = "Mendez",
                    screen_0_Apellido_Materno_3 = "Santos",
                    screen_0_NIT_4 = "3344556677",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Andrea",
                    screen_0_Segundo_Nombre_1 = "Fernanda",
                    screen_0_Apellido_Paterno_2 = "Martinez",
                    screen_0_Apellido_Materno_3 = "Ramirez",
                    screen_0_NIT_4 = "4455667788",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Jorge",
                    screen_0_Segundo_Nombre_1 = "Luis",
                    screen_0_Apellido_Paterno_2 = "Serrano",
                    screen_0_Apellido_Materno_3 = "Navarro",
                    screen_0_NIT_4 = "5566778899",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Sofía",
                    screen_0_Segundo_Nombre_1 = "Gabriela",
                    screen_0_Apellido_Paterno_2 = "Ortiz",
                    screen_0_Apellido_Materno_3 = "Herrera",
                    screen_0_NIT_4 = "6677889900",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Miguel",
                    screen_0_Segundo_Nombre_1 = "Angel",
                    screen_0_Apellido_Paterno_2 = "Vargas",
                    screen_0_Apellido_Materno_3 = "Figueroa",
                    screen_0_NIT_4 = "7788990011",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Daniela",
                    screen_0_Segundo_Nombre_1 = "Isabel",
                    screen_0_Apellido_Paterno_2 = "Rodriguez",
                    screen_0_Apellido_Materno_3 = "Castro",
                    screen_0_NIT_4 = "8899001122",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Felipe",
                    screen_0_Segundo_Nombre_1 = "Manuel",
                    screen_0_Apellido_Paterno_2 = "Rojas",
                    screen_0_Apellido_Materno_3 = "Suarez",
                    screen_0_NIT_4 = "9900112233",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Victoria",
                    screen_0_Segundo_Nombre_1 = "Beatriz",
                    screen_0_Apellido_Paterno_2 = "Morales",
                    screen_0_Apellido_Materno_3 = "Luna",
                    screen_0_NIT_4 = "1011122334",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Ricardo",
                    screen_0_Segundo_Nombre_1 = "David",
                    screen_0_Apellido_Paterno_2 = "Gonzalez",
                    screen_0_Apellido_Materno_3 = "Torres",
                    screen_0_NIT_4 = "1122334455",
                    flow_token = "11111111",
                },
                new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_Primer_Nombre_0 = "Paula",
                    screen_0_Segundo_Nombre_1 = "Natalia",
                    screen_0_Apellido_Paterno_2 = "Cordero",
                    screen_0_Apellido_Materno_3 = "Delgado",
                    screen_0_NIT_4 = "2233445566",
                    flow_token = "11111111",
                }
            };

            _compradores[key] = new CompradorInfo
            {
                DatosRegistrarEmpresa = emjemploRegistroEmpresa,
                Telefonos = new List<string> { "525526903132" }, //{ "525568982422" },
                Correos = new List<string> { "ejemplo@correo.com" },
                Clientes = clientesEjemplo//new List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> { ejemploCliente }
            };
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
        //Manejo de comprador
        public string CrearComprador(Flow_response_json_Model_568198656271468_Registrate registro, string telefono)
        {
            if (string.IsNullOrEmpty(registro.screen_0_Nombres_0) || string.IsNullOrEmpty(registro.screen_0_Apellidos_1))
            {
                return $"Nombre o apellido no válido";
            }
            if (string.IsNullOrEmpty(registro.screen_1_NIT_0))
            {
                return $"NIT Requerido";

            }

            string nombre = $"{registro.screen_0_Nombres_0.ToLowerInvariant()} {registro.screen_0_Apellidos_1.ToLowerInvariant()}".Trim();
            string nit = registro.screen_1_NIT_0;

            var key = (nombre, nit);


            if (!_compradores.ContainsKey(key))
            {
                List<string> phoneList;
                if (!string.IsNullOrEmpty(registro.screen_0_Telfono_3) && registro.screen_0_Telfono_3 != telefono)
                { 
                    phoneList = new List<string> { registro.screen_0_Telfono_3, telefono };
                    _EventsList.Add("Dos teléfonos diferentes");
                }
                else
                {
                    phoneList = new List<string> { telefono };
                    _EventsList.Add("Un sólo teléfono");
                }

                _compradores[key] = new CompradorInfo
                {
                    Datos = registro,
                    Telefonos = phoneList,
                    Correos = new List<string> { registro.screen_0_Correo_2 ?? "UNKNOWN" },
                    Clientes = new List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> { },
                };
                _EventsList.Add($"[INFO] Comprador '{nombre}' with NIT '{nit}' created successfully at {DateTime.Now}");
                return $"Comprador '{nombre}' with NIT '{nit}' created successfully at {DateTime.Now}";
            }
            else
            {
                _EventsList.Add($"[WARNING] Comprador '{nombre}' with NIT '{nit}' already exists at {DateTime.Now}");
                return $"Comprador '{nombre}' with NIT '{nit}' already exists";
            }
        }
        public string CrearEmpresa(Flow_response_json_Model_1187351356327089_RegistrarEmpresa registro, string telefono)
        {
            if (string.IsNullOrEmpty(registro.Registrar_Empresa_Nombre_0))
            {
                return $"Nombre o apellido no válido";
            }
            if (string.IsNullOrEmpty(registro.Registrar_Empresa_NIT_1))
            {
                return $"NIT Requerido";

            }
            string nombre = $"{registro.Registrar_Empresa_Nombre_0.ToLowerInvariant()}".Trim();
            string nit = registro.Registrar_Empresa_NIT_1;

            var key = (nombre, nit);


            if (!_compradores.ContainsKey(key))
            {

                _compradores[key] = new CompradorInfo
                {
                    DatosRegistrarEmpresa = registro,
                    Telefonos = new List<string> { telefono },
                    Correos = new List<string> { "UNKNOWN" },
                    Clientes = new List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> { },
                };
                _EventsList.Add($"[INFO] Empresa '{nombre}' with NIT '{nit}' created successfully at {DateTime.Now}");
                return $"Empresa '{nombre}' with NIT '{nit}' created successfully at {DateTime.Now}";
            }
            else
            {
                _EventsList.Add($"[WARNING] Empresa '{nombre}' with NIT '{nit}' already exists at {DateTime.Now}");
                return $"Empresa '{nombre}' with NIT '{nit}' already exists";
            }
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


            if (!_compradores.ContainsKey(key))
            {
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
                    Clientes = new List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> { },
                };
                _EventsList.Add($"[INFO] Comprador '{nombre}' with NIT '{nit}' created successfully at {DateTime.Now}");
                return $"Comprador '{nombre}' with NIT '{nit}' created successfully at {DateTime.Now}";
            }
            else
            {
                _EventsList.Add($"[WARNING] Comprador '{nombre}' with NIT '{nit}' already exists at {DateTime.Now}");
                return $"Comprador '{nombre}' with NIT '{nit}' already exists";
            }
        }
        public string ModificarComprador(Flow_response_json_Model_568198656271468_Registrate registro, string telefono)
        {
            if (string.IsNullOrEmpty(registro.screen_0_Nombres_0) || string.IsNullOrEmpty(registro.screen_0_Apellidos_1))
            {
                return $"Nombre o apellido no válido";
            }
            if (string.IsNullOrEmpty(registro.screen_1_NIT_0))
            {
                return $"NIT Requerido";

            }

            string nombre = $"{registro.screen_0_Nombres_0.ToLowerInvariant} {registro.screen_0_Apellidos_1.ToLowerInvariant}".Trim();
            string nit = registro.screen_1_NIT_0;
            var key = (nombre, nit);

            if (_compradores.ContainsKey(key))
            {
                _compradores[key].Datos = registro;
                if (!string.IsNullOrEmpty(registro.screen_0_Correo_2) && !_compradores[key].Correos.Contains(registro.screen_0_Correo_2))
                    _compradores[key].Correos.Add(registro.screen_0_Correo_2);
                if (!string.IsNullOrEmpty(registro.screen_0_Telfono_3) && !_compradores[key].Telefonos.Contains(registro.screen_0_Telfono_3))
                    _compradores[key].Telefonos.Add(registro.screen_0_Telfono_3);
                return $"Comprador '{nombre}' with NIT '{nit}' modified successfully at {DateTime.Now}";
            }
            else
            {
                _EventsList.Add($"[ERROR] Comprador '{nombre}' with NIT '{nit}' not found to update at {DateTime.Now}");
                return $"[ERROR] Comprador '{nombre}' with NIT '{nit}' not found";
            }
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

            if (_compradores.ContainsKey(key))
            {
                _compradores[key].DatosRegistrarEmpresa = registro;
                if (_compradores[key].Telefonos.Contains(telefono))
                    _compradores[key].Telefonos.Add(telefono);
                return $"Empresa '{nombre}' with NIT '{nit}' modified successfully at {DateTime.Now}";
            }
            else
            {
                _EventsList.Add($"[ERROR] Empresa '{nombre}' with NIT '{nit}' not found to update at {DateTime.Now}");
                return $"[ERROR] Empresa '{nombre}' with NIT '{nit}' not found";
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
                    _EventsList.Add($"Clientes: {entry.Value.Clientes.Count()}");
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
                    _EventsList.Add($"Clientes:");
                    sb.AppendLine("Clientes:");
                    if (InfoADesplegar.Clientes.Count() > 1)
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

                    if ( coincideNombre && entry.Value.Clientes.Any(c => c.screen_0_NIT_4 == clientNIT))
                    {
                        CompradorInfo InfoADesplegar = entry.Value;
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"Comprador: {nombre}, NIT: {nit}");
                        //sb.AppendLine($"Teléfonos: {string.Join(", ", InfoADesplegar.Telefonos)}");
                        //sb.AppendLine($"Correos: {string.Join(", ", InfoADesplegar.Correos)}");
                        sb.AppendLine("Clientes:");
                        foreach (var cliente in InfoADesplegar.Clientes)
                        {
                            if(cliente.screen_0_NIT_4 == clientNIT)
                            {
                                string fullCliente = $"{cliente.screen_0_Primer_Nombre_0} {cliente.screen_0_Segundo_Nombre_1} " +
                                    $"{cliente.screen_0_Apellido_Materno_3} {cliente.screen_0_Apellido_Paterno_2}, {cliente.screen_0_NIT_4}";
                                sb.AppendLine($"- {fullCliente.Trim()}");
                                _EventsList.Add($"[INFO] PrintComprador executed successfully at {DateTime.Now}");
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
            /*
            var key = (compradorNIT, compradorNombre);
            if (_compradores.ContainsKey(key))
            {
                foreach (var entry in _compradores[key].Clientes)
                    if (entry.screen_0_NIT_4 == clientNIT)
                        return true;
            }*/

            foreach (var entry in _compradores)
            {
                var (nombre, nit) = entry.Key;
                if ((nombre.Contains(compradorNombre, StringComparison.OrdinalIgnoreCase) || nit.Contains(compradorNIT, StringComparison.OrdinalIgnoreCase)) &&
            entry.Value.Clientes.Any(c => c.screen_0_NIT_4 == clientNIT))
                    return true;
            }
            return false;
        }
        public bool CompradorExiste(string compradorNombre, string compradorNIT)
        {
            if (string.IsNullOrWhiteSpace(compradorNombre) && string.IsNullOrWhiteSpace(compradorNIT))
            {
                _EventsList.Add("CompradorExiste called with empty values!");
                return false;
            }

            foreach (var comprador in _compradores.Keys)
            {
                _EventsList.Add($"Existing Comprador: {comprador.Nombre}, {comprador.NIT}");
            }
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
                    return entry.Key;
            }
            return null;
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
        public string AñadirClienteAComprador(string compradorNombre, string compradorNIT, Flow_response_json_Model_1297640437985053_InformacionDelCliente cliente)
        {
            _EventsList.Add($"AñadirClienteAComprador");
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
            if (ClienteExisteEnComprador(cliente.screen_0_NIT_4, compradorNIT, compradorNombre))
            {
                _EventsList.Add($"[ERROR] Cliente ya existe en comprador");
                return $"[ERROR] Cliente ya existe en comprador";
            }

            foreach (var entry in _compradores)
            {
                var (nombre, nit) = entry.Key;
                if (nombre.Contains(compradorNombre, StringComparison.OrdinalIgnoreCase) || nit == compradorNIT)
                {
                    var compradoresList = entry.Value;
                    if (!compradoresList.Clientes.Any(c => c.screen_0_NIT_4 == cliente.screen_0_NIT_4))
                    {
                        compradoresList.Clientes.Add(cliente);
                        _compradores[entry.Key].Clientes.Add(cliente); // = compradoresList;
                        _EventsList.Add($"[INFO] Cliente '{cliente.screen_0_Primer_Nombre_0}' added to Comprador '{nombre}' at {DateTime.Now}");
                        return $"El Cliente '{cliente.screen_0_Primer_Nombre_0}' se registró correctamente";
                    }
                }
            }
            _EventsList.Add($"[ERROR] Comprador '{compradorNombre}' with NIT '{compradorNIT}' not found at {DateTime.Now}");

            return $"[ERROR] Comprador '{compradorNombre}' with NIT '{compradorNIT}' not found at {DateTime.Now}";
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
            if (ClienteExisteEnComprador(cliente.RegisterClient_Natural_NIT, compradorNIT, compradorNombre))
            {
                _EventsList.Add($"[ERROR] Cliente ya existe en comprador");
                return $"[ERROR] Cliente ya existe en comprador";
            }
            var clienteInfo = new Flow_response_json_Model_1297640437985053_InformacionDelCliente { };
            if (cliente.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Juridica")
            {
                clienteInfo = new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_NIT_4 = cliente.RegisterClient_Juridical_NIT,
                    RegistraCliente_Tipo_Cliente = cliente.RegistraCliente_Tipo_Cliente,

                    RegisterClient_Juridical_Razon_Social = cliente.RegisterClient_Juridical_Razon_Social,
                    RegisterClient_Juridical_Digito_Verificacion = cliente.RegisterClient_Juridical_Digito_Verificacion,
                    RegisterClient_Juridical_Direccion = cliente.RegisterClient_Juridical_Direccion,
                    RegisterClient_Juridical_Departamento = cliente.RegisterClient_Juridical_Departamento,
                    RegisterClient_Juridical_Ciudad = cliente.RegisterClient_Juridical_Ciudad,
                    RegisterClient_Juridical_Tipo_Regimen = cliente.RegisterClient_Juridical_Tipo_Regimen,
                    RegisterClient_Juridical_Obligaciones_Fiscales = cliente.RegisterClient_Juridical_Obligaciones_Fiscales,
                };
            }
            else if (cliente.RegistraCliente_Tipo_Cliente == "Tipo_Persona_Natural")
            {
                clienteInfo = new Flow_response_json_Model_1297640437985053_InformacionDelCliente
                {
                    screen_0_NIT_4 = cliente.RegisterClient_Natural_NIT,
                    RegistraCliente_Tipo_Cliente = cliente.RegistraCliente_Tipo_Cliente,

                    RegisterClient_Natural_Nombre = cliente.RegisterClient_Natural_Nombre,
                    RegisterClient_Natural_Apellido_Paterno = cliente.RegisterClient_Natural_Apellido_Paterno,
                    RegisterClient_Natural_Apellido_Materno = cliente.RegisterClient_Natural_Apellido_Materno,
                    RegisterClient_Natural_Correo = cliente.RegisterClient_Natural_Correo,
                    RegisterClient_Natural_Telefono = cliente.RegisterClient_Natural_Telefono,
                    RegisterClient_Natural_Tipo_Identificacion = cliente.RegisterClient_Natural_Tipo_Identificacion,
                    RegisterClient_Natural_Digito_Verificacin = cliente.RegisterClient_Natural_Digito_Verificacin,
                    RegisterClient_Natural_Label = cliente.RegisterClient_Natural_Label,
                    RegisterClient_Natural_Departamento = cliente.RegisterClient_Natural_Departamento,
                    RegisterClient_Natural_Ciudad = cliente.RegisterClient_Natural_Ciudad,
                    RegisterClient_Natural_Tipo_Rgimen = cliente.RegisterClient_Natural_Tipo_Rgimen,
                    RegisterClient_Natural_Obligaciones_Fiscale = cliente.RegisterClient_Natural_Obligaciones_Fiscale,
                    RegisterClient_Natural_documento = cliente.RegisterClient_Natural_documento,
                };
            }
            else
            {
                return $"[ERROR] Tipo Persona Not identified";
            }

                foreach (var entry in _compradores)
                {
                    var (nombre, nit) = entry.Key;
                    if (nombre.Contains(compradorNombre, StringComparison.OrdinalIgnoreCase) || nit.Contains(compradorNIT, StringComparison.OrdinalIgnoreCase))
                    {
                        var compradoresList = entry.Value;
                        if (!compradoresList.Clientes.Any(c => c.screen_0_NIT_4 == clienteInfo.screen_0_NIT_4))
                        {
                            compradoresList.Clientes.Add(clienteInfo);
                            _compradores[entry.Key].Clientes.Add(clienteInfo); // = compradoresList;
                            _EventsList.Add($"El Cliente '{clienteInfo.screen_0_NIT_4}' se registró correctamente");
                            return $"El Cliente '{clienteInfo.screen_0_NIT_4}' se registró correctamente";
                        }
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
                _EventsList.Add($"Comprador {compradorNombre}, {compradorNIT}");
                _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT}");

                var clienteInfo = new Flow_response_json_Model_1297640437985053_InformacionDelCliente 
                {
                    screen_0_NIT_4 = clienteNIT,

                    ModificarCliente_Empresa_Juridical_Razon_Social = clienteNombre,
                    ModificarCliente_Empresa_Tipo_Cliente = cliente.ModificarCliente_Empresa_Tipo_Cliente,
                    ModificarCliente_Empresa_Juridical_Digito_Verificacion = cliente.ModificarCliente_Empresa_Juridical_Digito_Verificacion,
                    ModificarCliente_Empresa_Juridical_Direccion = cliente.ModificarCliente_Empresa_Juridical_Direccion,
                    ModificarCliente_Empresa_Juridical_Departamento = cliente.ModificarCliente_Empresa_Juridical_Departamento,
                    ModificarCliente_Empresa_Juridical_Ciudad = cliente.ModificarCliente_Empresa_Juridical_Ciudad,
                    ModificarCliente_Empresa_Juridical_Tipo_Regimen = cliente.ModificarCliente_Empresa_Juridical_Tipo_Regimen,
                    ModificarCliente_Empresa_Juridical_Obligaciones_Fiscales = cliente.ModificarCliente_Empresa_Juridical_Obligaciones_Fiscales
                };

                foreach (var entry in _compradores)
                {
                    var (nombre, nit) = entry.Key;
                    if (nombre.Contains(compradorNombre, StringComparison.OrdinalIgnoreCase) || nit == compradorNIT)
                    {
                        var index = _compradores[entry.Key].Clientes.FindIndex(c => c.screen_0_NIT_4.Contains(clienteNIT, StringComparison.OrdinalIgnoreCase));
                        if (index >= 0)
                        {
                            _compradores[entry.Key].Clientes[index] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        /*var matchConditions = new Func<dynamic, bool>[]
                        {
                            c => !string.IsNullOrEmpty(c.ModificarCliente_Empresa_Juridical_Razon_Social) &&
                                 c.ModificarCliente_Empresa_Juridical_Razon_Social.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase),
                        
                            c => !string.IsNullOrEmpty(c.screen_0_Primer_Nombre_0) &&
                                 c.screen_0_Primer_Nombre_0.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase),
                        
                            c => !string.IsNullOrEmpty(c.screen_0_Segundo_Nombre_1) &&
                                 c.screen_0_Segundo_Nombre_1.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase),
                        
                            c => !string.IsNullOrEmpty(c.screen_0_Apellido_Paterno_2) &&
                                 c.screen_0_Apellido_Paterno_2.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase),
                        
                            c => !string.IsNullOrEmpty(c.screen_0_Apellido_Materno_3) &&
                                 c.screen_0_Apellido_Materno_3.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase),
                        
                            c => !string.IsNullOrEmpty(c.RegisterClient_Natural_Nombre) &&
                                 c.RegisterClient_Natural_Nombre.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase),
                        
                            c => !string.IsNullOrEmpty(c.RegisterClient_Natural_Apellido_Paterno) &&
                                 c.RegisterClient_Natural_Apellido_Paterno.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase),
                        
                            c => !string.IsNullOrEmpty(c.RegisterClient_Natural_Apellido_Materno) &&
                                 c.RegisterClient_Natural_Apellido_Materno.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase),
                        
                            c => !string.IsNullOrEmpty(c.RegisterClient_Juridical_Razon_Social) &&
                                 c.RegisterClient_Juridical_Razon_Social.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase),
                        
                            c => !string.IsNullOrEmpty(c.ModificarCliente_Natural_Apellido_Materno) &&
                                 c.ModificarCliente_Natural_Apellido_Materno.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase),
                        
                            c => !string.IsNullOrEmpty(c.ModificarCliente_Natural_Nombre) &&
                                 c.ModificarCliente_Natural_Nombre.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase),
                        
                            c => !string.IsNullOrEmpty(c.ModificarCliente_Natural_Apellido_Paterno) &&
                                 c.ModificarCliente_Natural_Apellido_Paterno.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase)
                        };
                        foreach (var match in matchConditions)
                        {
                            var matchIndex = _compradores[entry.Key].Clientes.FindIndex(match);
                            if (matchIndex >= 0)
                            {
                                _compradores[entry.Key].Clientes[matchIndex] = clienteInfo;
                                _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                                return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                            }
                        }*/
                        var index2 = _compradores[entry.Key].Clientes.FindIndex(c => 
                            !string.IsNullOrEmpty(c.ModificarCliente_Empresa_Juridical_Razon_Social) &&
                            c.ModificarCliente_Empresa_Juridical_Razon_Social.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index2 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index2] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index3 = _compradores[entry.Key].Clientes.FindIndex(c =>
                            !string.IsNullOrEmpty(c.screen_0_Primer_Nombre_0) &&
                            c.screen_0_Primer_Nombre_0.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index3 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index3] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index4 = _compradores[entry.Key].Clientes.FindIndex(c =>
                            !string.IsNullOrEmpty(c.screen_0_Segundo_Nombre_1) &&
                            c.screen_0_Segundo_Nombre_1.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index4 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index4] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index5 = _compradores[entry.Key].Clientes.FindIndex(c =>
                            !string.IsNullOrEmpty(c.screen_0_Apellido_Paterno_2) &&
                            c.screen_0_Apellido_Paterno_2.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index5 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index5] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index6 = _compradores[entry.Key].Clientes.FindIndex(c =>
                            !string.IsNullOrEmpty(c.screen_0_Apellido_Materno_3) &&
                            c.screen_0_Apellido_Materno_3.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index6 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index6] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index7 = _compradores[entry.Key].Clientes.FindIndex(c =>
                            !string.IsNullOrEmpty(c.RegisterClient_Natural_Nombre) &&
                            c.RegisterClient_Natural_Nombre.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index7 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index7] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index8 = _compradores[entry.Key].Clientes.FindIndex(c =>
                            !string.IsNullOrEmpty(c.RegisterClient_Natural_Apellido_Paterno) &&
                            c.RegisterClient_Natural_Apellido_Paterno.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index8 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index8] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index9 = _compradores[entry.Key].Clientes.FindIndex(c =>
                            !string.IsNullOrEmpty(c.RegisterClient_Natural_Apellido_Materno) &&
                            c.RegisterClient_Natural_Apellido_Materno.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index9 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index9] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index10 = _compradores[entry.Key].Clientes.FindIndex(c =>
                            !string.IsNullOrEmpty(c.RegisterClient_Juridical_Razon_Social) &&
                            c.RegisterClient_Juridical_Razon_Social.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index10 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index10] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index11 = _compradores[entry.Key].Clientes.FindIndex(c =>
                            !string.IsNullOrEmpty(c.ModificarCliente_Natural_Apellido_Materno) &&
                            c.ModificarCliente_Natural_Apellido_Materno.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index11 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index11] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index12 = _compradores[entry.Key].Clientes.FindIndex(c =>
                            !string.IsNullOrEmpty(c.ModificarCliente_Natural_Nombre) &&
                            c.ModificarCliente_Natural_Nombre.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index12 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index12] = clienteInfo;
                            _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                            return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                        }
                        var index13 = _compradores[entry.Key].Clientes.FindIndex(c =>
                            !string.IsNullOrEmpty(c.ModificarCliente_Natural_Apellido_Paterno) &&
                            c.ModificarCliente_Natural_Apellido_Paterno.Contains(clienteNombre, StringComparison.OrdinalIgnoreCase));
                        if (index13 >= 0)
                        {
                            _compradores[entry.Key].Clientes[index13] = clienteInfo;
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

            var clienteInfo = new Flow_response_json_Model_1297640437985053_InformacionDelCliente
            {
                screen_0_NIT_4 = clienteNIT,

                ModificarCliente_Natural_Nombre = cliente.ModificarCliente_Natural_Nombre,
                ModificarCliente_Natural_Apellido_Paterno = cliente.ModificarCliente_Natural_Apellido_Paterno,
                ModificarCliente_Natural_Correo = cliente.ModificarCliente_Natural_Correo,
                ModificarCliente_Natural_Telefono = cliente.ModificarCliente_Natural_Telefono,
                ModificarCliente_Natural_Tipo_Identificacion = cliente.ModificarCliente_Natural_Tipo_Identificacion,
                ModificarCliente_Tipo_Cliente = cliente.ModificarCliente_Tipo_Cliente,
                ModificarCliente_Natural_Digito_Verificacin = cliente.ModificarCliente_Natural_Digito_Verificacin,
                ModificarCliente_Natural_Label = cliente.ModificarCliente_Natural_Label,
                ModificarCliente_Natural_Departamento = cliente.ModificarCliente_Natural_Departamento,
                ModificarCliente_Natural_Ciudad = cliente.ModificarCliente_Natural_Ciudad,
                ModificarCliente_Natural_Tipo_Rgimen = cliente.ModificarCliente_Natural_Tipo_Rgimen,
                ModificarCliente_Natural_Obligaciones_Fiscale = cliente.ModificarCliente_Natural_Obligaciones_Fiscale
            };

            foreach (var entry in _compradores)
            {
                var (nombre, nit) = entry.Key;
                if (nombre.Contains(compradorNombre, StringComparison.OrdinalIgnoreCase) || nit.Contains(compradorNIT, StringComparison.OrdinalIgnoreCase))
                {
                    var index = _compradores[entry.Key].Clientes.FindIndex(c => c.screen_0_NIT_4.Contains(clienteNIT, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        _compradores[entry.Key].Clientes[index] = clienteInfo;
                        _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                        return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                    }
                    index = _compradores[entry.Key].Clientes.FindIndex(c => c.ModificarCliente_Natural_Nombre.Contains(cliente.ModificarCliente_Natural_Nombre, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        _compradores[entry.Key].Clientes[index] = clienteInfo;
                        _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                        return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                    }
                    index = _compradores[entry.Key].Clientes.FindIndex(c => c.ModificarCliente_Natural_Apellido_Paterno.Contains(cliente.ModificarCliente_Natural_Apellido_Paterno, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        _compradores[entry.Key].Clientes[index] = clienteInfo;
                        _EventsList.Add($"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente");
                        return $"Empresa Cliente {clienteNombre}, {clienteNIT} modificada correctamente";
                    }
                    index = _compradores[entry.Key].Clientes.FindIndex(c => c.ModificarCliente_Natural_Apellido_Materno.Contains(cliente.ModificarCliente_Natural_Apellido_Materno, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        _compradores[entry.Key].Clientes[index] = clienteInfo;
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
        public List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> BuscarListaClientes(string compradorNombre, string compradorNIT, string clientPice)
        {
            _EventsList.Add("BuscarListaClientes");
            _EventsList.Add($"Comprador:{compradorNombre}, ");
            _EventsList.Add($"BuscarListaClientes");
            var resultados = new List<Flow_response_json_Model_1297640437985053_InformacionDelCliente>();

            clientPice = clientPice.ToLower();

            if (!_compradores.TryGetValue((compradorNombre, compradorNIT), out var compradorInfo))
                return resultados; // Return empty list if the buyer is not found

            foreach (var cliente in compradorInfo.Clientes)
            {
                if (cliente != null && (cliente.screen_0_Primer_Nombre_0?.ToLower().Contains(clientPice) == true ||
             cliente.screen_0_Segundo_Nombre_1?.ToLower().Contains(clientPice) == true ||
             cliente.screen_0_Apellido_Paterno_2?.ToLower().Contains(clientPice) == true ||
             cliente.screen_0_Apellido_Materno_3?.ToLower().Contains(clientPice) == true ||
             cliente.screen_0_NIT_4?.ToLower().Contains(clientPice) == true))
                    resultados.Add(cliente);
            }

            return resultados;
        }
        private bool ContieneCoincidencia(Flow_response_json_Model_1297640437985053_InformacionDelCliente cliente, string clientPice)
        {
            if (string.IsNullOrWhiteSpace(clientPice))
                return false;

            clientPice = clientPice.ToLower();

            return cliente.screen_0_Primer_Nombre_0?.ToLower().Contains(clientPice) == true ||
                   cliente.screen_0_Segundo_Nombre_1?.ToLower().Contains(clientPice) == true ||
                   cliente.screen_0_Apellido_Paterno_2?.ToLower().Contains(clientPice) == true ||
                   cliente.screen_0_Apellido_Materno_3?.ToLower().Contains(clientPice) == true ||
                   cliente.screen_0_NIT_4?.ToLower().Contains(clientPice) == true ||
                   cliente.RegisterClient_Natural_Nombre?.ToLower().Contains(clientPice) == true ||
                   cliente.RegisterClient_Natural_Apellido_Paterno?.ToLower().Contains(clientPice) == true ||
                   cliente.RegisterClient_Natural_Apellido_Materno?.ToLower().Contains(clientPice) == true ||
                   cliente.ModificarCliente_Natural_Apellido_Materno?.ToLower().Contains(clientPice) == true ||
                   cliente.ModificarCliente_Natural_Nombre?.ToLower().Contains(clientPice) == true ||
                   cliente.ModificarCliente_Natural_Apellido_Paterno?.ToLower().Contains(clientPice) == true ||
                   cliente.ModificarCliente_Empresa_Juridical_Razon_Social?.ToLower().Contains(clientPice) == true;
        }

        //Faltan: Añadir telefono y Añadir Correo
        public dynamic GetEventsList()
        {
            // Return the list of received messages as the response 
            return _EventsList;
        }
    }
}