using System.Text.Json.Serialization;

namespace WhatsAppPresentacionV6.Modelos
{
    public class Flow_response_json_Models
    {
        public string flow_token { get; set; }
    }
    public class Flow_response_json_Model_568198656271468_Registrate : Flow_response_json_Models //Regístrate
    {
        public string? screen_0_Apellidos_1 { get; set; }
        public string? screen_0_Telfono_3 { get; set; }
        public string? screen_0_Nombres_0 { get; set; }
        public string? screen_0_Tipo_de_persona_4 { get; set; }
        public string? screen_0_Tipo_Identificacin_5 { get; set; }
        public string? screen_0_Correo_2 { get; set; }
        public string? screen_1_NIT_0 { get; set; }
        public string? screen_1_Departamento_3 { get; set; }
        public string? screen_1_Tipo_Rgimen_5 { get; set; }
        public string? screen_1_Obligaciones_Fiscale_6 { get; set; }
        public string? screen_1_Label_2 { get; set; }
        public string? screen_1_Ciudad_4 { get; set; }
        public string? screen_1_Digito_Verificacin_1 { get; set; }
        public string? screen_2_Razn_social_0 { get; set; }
    }
    public class Flow_response_json_Model_1877921042740421_FacturaColombia : Flow_response_json_Models//Factura Colombia
    {
        public string? screen_0_Primer_Nombre_0 { get; set; }
        public string? screen_0_Segundo_nombre_1 { get; set; }
        public string? screen_0_Apellido_Paterno_2 { get; set; }
        public string? screen_0_Apellido_Materno_3 { get; set; }
        public string? screen_0_NIT_4 { get; set; }
        public string? screen_0_Correo_Electrnico_5 { get; set; }
        public string? screen_1_Primer_Nombre_0 { get; set; }
        public string? screen_1_Segundo_Nombre_1 { get; set; }
        public string? screen_1_Apellido_Paterno_2 { get; set; }
        public string? screen_1_Apellido_Materno_3 { get; set; }
        public string? screen_1_NIT_4 { get; set; }
        public string? screen_1_Razn_social_5 { get; set; }
        public string? screen_2_Direccin_0 { get; set; }
        public string? screen_2_Telfono_1 { get; set; }
        public string? screen_2_Descripcin_2 { get; set; }
        public string? screen_2_Cantidad_3 { get; set; }
    }
    public class Flow_response_json_Model_1277233923341533_ComprobanteNitClienteYCombrador : Flow_response_json_Models//Comprobante NIT Cliente y Combrador
    {
        public string? screen_0_Nombre_0 { get; set; }//Nombre Comprador
        public string? screen_0_NIT_1 { get; set; }//NIT Comprador
        public string? screen_0_NIT_2 { get; set; }// NIT Cliente

    }
    public class Flow_response_json_Model_1297640437985053_InformacionDelCliente : Flow_response_json_Models//Información del cliente
    {
        public string? screen_0_Primer_Nombre_0 { get; set; }
        public string? screen_0_Segundo_Nombre_1 { get; set; }
        public string? screen_0_Apellido_Paterno_2 { get; set; }
        public string? screen_0_Apellido_Materno_3 { get; set; }
        public string? screen_0_NIT_4 { get; set; }

        //Nueva lógica


        public string? RegistraCliente_Tipo_Cliente { get; set; }
        public string? RegisterClient_Natural_Nombre { get; set; }
        public string? RegisterClient_Natural_Apellido_Paterno { get; set; }
        public string? RegisterClient_Natural_Apellido_Materno { get; set; }
        public string? RegisterClient_Natural_Correo { get; set; }
        public string? RegisterClient_Natural_Telefono { get; set; }
        public string? RegisterClient_Natural_Tipo_Identificacion { get; set; }
        public string? RegisterClient_Natural_Digito_Verificacin { get; set; }
        public string? RegisterClient_Natural_Label { get; set; }
        public string? RegisterClient_Natural_Departamento { get; set; }
        public string? RegisterClient_Natural_Ciudad { get; set; }
        public string? RegisterClient_Natural_Tipo_Rgimen { get; set; }
        public string? RegisterClient_Natural_Obligaciones_Fiscale { get; set; }
        public string? RegisterClient_Natural_documento { get; set; }

        public string? RegisterClient_Juridical_Razon_Social { get; set; }
        public string? RegisterClient_Juridical_Digito_Verificacion { get; set; }
        public string? RegisterClient_Juridical_Direccion { get; set; }
        public string? RegisterClient_Juridical_Departamento { get; set; }
        public string? RegisterClient_Juridical_Ciudad { get; set; }
        public string? RegisterClient_Juridical_Tipo_Regimen { get; set; }
        public string? RegisterClient_Juridical_Obligaciones_Fiscales { get; set; }

        //Natural
        public string? ModificarCliente_Natural_Apellido_Materno { get; set; }
        public string? ModificarCliente_Natural_Nombre { get; set; }
        public string? ModificarCliente_Natural_Apellido_Paterno { get; set; }
        public string? ModificarCliente_Natural_Correo { get; set; }
        public string? ModificarCliente_Natural_Telefono { get; set; }
        public string? ModificarCliente_Natural_Tipo_Identificacion { get; set; }
        public string? ModificarCliente_Tipo_Cliente { get; set; }
        public string? ModificarCliente_Natural_Digito_Verificacin { get; set; }
        public string? ModificarCliente_Natural_Label { get; set; }
        public string? ModificarCliente_Natural_Departamento { get; set; }
        public string? ModificarCliente_Natural_Ciudad { get; set; }
        public string? ModificarCliente_Natural_Tipo_Rgimen { get; set; }
        public string? ModificarCliente_Natural_Obligaciones_Fiscale { get; set; }
        public Array? ModificarCliente_Natural_documento { get; set; }

        //Juridica
        public string? ModificarCliente_Empresa_Juridical_Razon_Social { get; set; }
        public string? ModificarCliente_Empresa_Tipo_Cliente { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Digito_Verificacion { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Direccion { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Departamento { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Ciudad { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Tipo_Regimen { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Obligaciones_Fiscales { get; set; }
    }
    public class Flow_response_json_Model_682423707677994_InformacionFactura : Flow_response_json_Models//Información Factura
    {
        public string? screen_0_Telfono_0 { get; set; }
        public string? screen_0_Direccin_1 { get; set; }
        public string? screen_0_Monto_2 { get; set; }
        public string? screen_0_Descripcin_3 { get; set; }
    }
    public class CompradorInfo
    {
        public Flow_response_json_Model_568198656271468_Registrate? Datos { get; set; }
        public Flow_response_json_Model_637724539030495_RegistrarPersonaFisicaSimple? DatosPersonaFisicaSimple { get; set; }
        public Flow_response_json_Model_1187351356327089_RegistrarEmpresa? DatosRegistrarEmpresa { get; set; }
        public List<string> Telefonos { get; set; } = new();
        public List<string> Correos { get; set; } = new();
        public List<Flow_response_json_Model_1297640437985053_InformacionDelCliente> Clientes { get; set; } = new();
    }
    public class Flow_response_json_Model_647884018138514_FacturaColombiaComplejo : Flow_response_json_Models
    {
        // Screen 0 - Registro
        public string screen_0_Nombres_647884018138514 { get; set; }
        public string screen_0_Apellidos_647884018138514 { get; set; }
        public string screen_0_Telefono_647884018138514 { get; set; }
        public string screen_0_Tipo_de_Persona_647884018138514 { get; set; }
        public string screen_0_Tipo_identificacion_647884018138514 { get; set; }

        // Screen 1 - Informacion_Condicional
        public string? screen_1_Razon_Social_647884018138514 { get; set; }
        public DocumentData document { get; set; } // Might have to be "DocumentPicker"
        public string? screen_1_NIT_647884018138514 { get; set; }
        public string? screen_1_Digito_Verificacion_647884018138514 { get; set; }
        public string? screen_1_Direccion_647884018138514 { get; set; }
        public string? screen_1_Departamento_647884018138514 { get; set; }
        public string? screen_1_Ciudad_647884018138514 { get; set; }
        public string? screen_1_Tipo_de_Regimen_647884018138514 { get; set; }
        public string? screen_1_Obligacion_Fiscal_647884018138514 { get; set; }

        // Screen 2 - Registrar_Cliente
        public string screen_2_Nombres_Cliente_647884018138514 { get; set; }
        public string screen_2_Apellidos_Cliente_647884018138514 { get; set; }
        public string screen_2_NIT_Cliente_647884018138514 { get; set; }

        // Screen 3 - Informacion_Factura
        public string screen_3_Telefono_Factura_647884018138514 { get; set; }
        public string screen_3_Direccion_Factura_647884018138514 { get; set; }
        public string screen_3_Monto_647884018138514 { get; set; }
        public string screen_3_Descripcion_647884018138514 { get; set; }

        // Screen 4 - Confirmacion (terminal screen)
        // No additional fields needed as it just displays collected data
    }

    public class DocumentData
    {
        public string? media_id { get; set; }
        public string? file_name { get; set; }
        public string? url { get; set; }
        public string? cdn_url { get; set; }
        public long? size { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public EncriptedDocument? encryption_metadata { get; set; }
    }
    public class EncriptedDocument
    {
        public string encrypted_hash { get; set; }
        public string iv { get; set; }
        public string encryption_key { get; set; }
        public string hmac_key { get; set; }
        public string plaintext_hash { get; set; }
    }
    public class Flow_response_json_Model_637724539030495_RegistrarPersonaFisicaSimple : Flow_response_json_Models//Registrar persona física simple
    {
        public string? Registrar_Persona_Fisica_Nombre_0 { get; set; }
        public string? Registrar_Persona_Fisica_Apellido_Paterno_1 { get; set; }
        public string? Registrar_Persona_Fisica_Apellido_Materno_2 { get; set; }
        public string? Registrar_Persona_Fisica_Correo_3 { get; set; }
        public string? Registrar_Persona_Fisica_Telfono_4 { get; set; }
        public string? Registrar_Persona_Fisica_Tipo_Identificacin_5 { get; set; }

        public DocumentData? screen_1_documento { get; set; } // Might have to be "DocumentPicker"
        public string? screen_1_NIT_0 { get; set; }
        public string? screen_1_Digito_Verificacin_1 { get; set; }
        public string? screen_1_Label_2 { get; set; }
        public string? screen_1_Departamento_3 { get; set; }
        public string? screen_1_Ciudad_4 { get; set; }
        public string? screen_1_Tipo_Rgimen_5 { get; set; }
        public string? screen_1_Obligaciones_Fiscale_6 { get; set; }
        /*
        {
  "screens": [
    {
      "data": {},
      "id": "Info_Base",
      "layout": {
        "children": [
          {
            "children": [
              {
                "text": "Favor de Indicar su nombre y apellido",
                "type": "TextBody"
              },
              {
                "input-type": "text",
                "label": "Nombre(s)",
                "name": "Nombres_d6e59c",
                "required": true,
                "type": "TextInput"
              },
              {
                "input-type": "text",
                "label": "Apellido Paterno",
                "name": "Apellido_Paterno_64151d",
                "required": true,
                "type": "TextInput"
              },
              {
                "input-type": "text",
                "label": "Apellido Materno",
                "name": "Apellido_Materno_0ea6c3",
                "required": true,
                "type": "TextInput"
              },
              {
                "input-type": "email",
                "label": "Correo",
                "name": "Correo_21222c",
                "required": true,
                "type": "TextInput"
              },
              {
                "input-type": "phone",
                "label": "Teléfono",
                "name": "Telfono_0d9cb2",
                "required": true,
                "type": "TextInput"
              },
              {
                "data-source": [
                  {
                    "id": "0_Registro_Civil",
                    "title": "Registro Civil"
                  },
                  {
                    "id": "1_Cédula_de_Ciudadanía",
                    "title": "Cédula de Ciudadanía"
                  },
                  {
                    "id": "2_Tarjeta_de_extrangería",
                    "title": "Tarjeta de extrangería"
                  },
                  {
                    "id": "3_Cédula_de_extranjería",
                    "title": "Cédula de extranjería"
                  },
                  {
                    "id": "4_NIT",
                    "title": "NIT"
                  },
                  {
                    "id": "5_Pasaporte",
                    "title": "Pasaporte"
                  },
                  {
                    "id": "6_Documento_de_identificación_extranjero",
                    "title": "Documento de identificación extranjero"
                  },
                  {
                    "id": "7_PEP_(Permiso_Especial_de_Permanencia)",
                    "title": "PEP (Permiso Especial de Permanencia)"
                  }
                ],
                "label": "Tipo Identificación",
                "name": "Tipo_Identificacin_da9f14",
                "required": true,
                "type": "Dropdown"
              },
              {
                "label": "Continuar",
                "on-click-action": {
                  "name": "navigate",
                  "next": {
                    "name": "Identificacion",
                    "type": "screen"
                  },
                  "payload": {
                    "Registrar_Persona_Fisica_Nombre_0": "${form.Nombres_d6e59c}",
                    "Registrar_Persona_Fisica_Apellido_Paterno_1": "${form.Apellido_Paterno_64151d}",
                    "Registrar_Persona_Fisica_Apellido_Materno_2": "${form.Apellido_Materno_0ea6c3}",
                    "Registrar_Persona_Fisica_Correo_3": "${form.Correo_21222c}",
                    "Registrar_Persona_Fisica_Telfono_4": "${form.Telfono_0d9cb2}",
                    "Registrar_Persona_Fisica_Tipo_Identificacin_5": "${form.Tipo_Identificacin_da9f14}"
                  }
                },
                "type": "Footer"
              }
            ],
            "name": "flow_path",
            "type": "Form"
          }
        ],
        "type": "SingleColumnLayout"
      },
      "title": "Registro"
    },
    {
      "data": {
        "Registrar_Persona_Fisica_Apellido_Materno_2": {
          "__example__": "Example",
          "type": "string"
        },
        "Registrar_Persona_Fisica_Nombre_0": {
          "__example__": "Example",
          "type": "string"
        },
        "Registrar_Persona_Fisica_Apellido_Paterno_1": {
          "__example__": "Example",
          "type": "string"
        },
        "Registrar_Persona_Fisica_Correo_3": {
          "__example__": "Example",
          "type": "string"
        },
        "Registrar_Persona_Fisica_Telfono_4": {
          "__example__": "Example",
          "type": "string"
        },
        "Registrar_Persona_Fisica_Tipo_Identificacin_5": {
          "__example__": "Example",
          "type": "string"
        }
      },
      "id": "Identificacion",
      "layout": {
        "children": [
          {
            "children": [
              {
                "type": "If",
                "condition":"${data.Registrar_Persona_Fisica_Tipo_Identificacin_5} == '4_NIT'",
                "then": [
                  {
                    "input-type": "text",
                    "label": "NIT",
                    "name": "NIT_dbaa4f",
                    "required": true,
                    "type": "TextInput"
                  }
                ],
                "else": [
                  {
                    "type": "DocumentPicker",
                    "label": "Subir Documento",
                    "name": "Documento_Subido",
                    "description": "Suba el documento de identificación en formato PDF",
                    "min-uploaded-documents": 1,
                    "max-uploaded-documents": 1
                  }
                ]
              },
              {
                "input-type": "number",
                "label": "Digito Verificación",
                "name": "Digito_Verificacin_7025c8",
                "required": true,
                "type": "TextInput"
              },
              {
                "label": "Dirección",
                "name": "Direccion_265b50",
                "required": true,
                "type": "TextArea",
                "helper-text": "Dirección"
              },
              {
                "input-type": "text",
                "label": "Departamento",
                "name": "Departamento_601ceb",
                "required": true,
                "type": "TextInput"
              },
              {
                "input-type": "text",
                "label": "Ciudad",
                "name": "Ciudad_69892a",
                "required": true,
                "type": "TextInput"
              },
              {
                "data-source": [
                  {
                    "id": "0_Impuesto_sobre_ventas_-_IVA",
                    "title": "Impuesto sobre ventas - IVA"
                  },
                  {
                    "id": "1_No_responsable_de_IVA",
                    "title": "No responsable de IVA"
                  }
                ],
                "label": "Tipo Régimen",
                "name": "Tipo_Rgimen_16b36b",
                "required": true,
                "type": "Dropdown"
              },
              {
                "data-source": [
                  {
                    "id": "0_Gran_contribuyente",
                    "title": "Gran contribuyente"
                  },
                  {
                    "id": "1_Autorretenedor",
                    "title": "Autorretenedor"
                  },
                  {
                    "id": "2_Agente_de_retención_IVA",
                    "title": "Agente de retención IVA"
                  },
                  {
                    "id": "3_Régimen_simple_tributación",
                    "title": "Régimen simple tributación"
                  },
                  {
                    "id": "4_No_aplica-_Otros",
                    "title": "No aplica- Otros"
                  }
                ],
                "label": "Obligaciones Fiscale",
                "name": "Obligaciones_Fiscale_070a24",
                "required": true,
                "type": "Dropdown"
              },
              {
                "label": "Guardar",
                "on-click-action": {
                  "name": "complete",
                  "payload": {
                    "screen_1_NIT_0": "${form.NIT_dbaa4f}",
                    "screen_1_Digito_Verificacin_1": "${form.Digito_Verificacin_7025c8}",
                    "screen_1_Label_2": "${form.Direccion_265b50}",
                    "screen_1_Departamento_3": "${form.Departamento_601ceb}",
                    "screen_1_Ciudad_4": "${form.Ciudad_69892a}",
                    "screen_1_Tipo_Rgimen_5": "${form.Tipo_Rgimen_16b36b}",
                    "screen_1_Obligaciones_Fiscale_6": "${form.Obligaciones_Fiscale_070a24}",
                    "screen_1_documento": "${form.Documento_Subido}",

                    "Registrar_Persona_Fisica_Nombre_0": "${data.Registrar_Persona_Fisica_Nombre_0}",
                    "Registrar_Persona_Fisica_Apellido_Paterno_1": "${data.Registrar_Persona_Fisica_Apellido_Paterno_1}",
                    "Registrar_Persona_Fisica_Apellido_Materno_2": "${data.Registrar_Persona_Fisica_Apellido_Materno_2}",
                    "Registrar_Persona_Fisica_Correo_3": "${data.Registrar_Persona_Fisica_Correo_3}",
                    "Registrar_Persona_Fisica_Telfono_4": "${data.Registrar_Persona_Fisica_Telfono_4}",
                    "Registrar_Persona_Fisica_Tipo_Identificacin_5": "${data.Registrar_Persona_Fisica_Tipo_Identificacin_5}"
                  }
                },
                "type": "Footer"
              }
            ],
            "name": "flow_path",
            "type": "Form"
          }
        ],
        "type": "SingleColumnLayout"
      },
      "terminal": true,
      "title": "Registro"
    }
  ],
  "version": "6.3"
}
        */
    }
    public class Flow_response_json_Model_1187351356327089_RegistrarEmpresa : Flow_response_json_Models//Registrar empresa
    {
        public string? Registrar_Empresa_Nombre_0 { get; set; }//Nombre Empresa
        public string? Registrar_Empresa_NIT_1 { get; set; }//NIT Empresa

        public string? Registrar_Empresa_Digito_Verificacin_1 { get; set; }
        public string? Registrar_Empresa_Direccion_2 { get; set; }
        public string? Registrar_Empresa_Departamento_3 { get; set; }
        public string? Registrar_Empresa_Ciudad_4 { get; set; }
        public string? Registrar_Empresa_Tipo_Rgimen_5 { get; set; }
        public string? Registrar_Empresa_Obligaciones_Fiscales_6 { get; set; }
        /*
        {
          "screens": [
            {
              "data": {},
              "id": "Info_Base",
              "layout": {
                "children": [
                  {
                    "children": [
                      {
                        "text": "Favor de Indicar la Razón Social y NIT de la Empresa",
                        "type": "TextBody"
                      },
                      {
                        "input-type": "text",
                        "label": "Razón Social",
                        "name": "razon_social_rtr56x3a",
                        "required": true,
                        "type": "TextInput"
                      },
                      {
                        "input-type": "text",
                        "label": "NIT",
                        "name": "NIT_mkj68jb",
                        "required": true,
                        "type": "TextInput"
                      },
                      {
                        "label": "Continuar",
                        "on-click-action": {
                          "name": "navigate",
                          "next": {
                            "name": "Identificacion",
                            "type": "screen"
                          },
                          "payload": {
                            "Registrar_Empresa_Nombre_0": "${form.razon_social_rtr56x3a}",
                            "Registrar_Empresa_NIT_1": "${form.NIT_mkj68jb}"
                          }
                        },
                        "type": "Footer"
                      }
                    ],
                    "name": "flow_path",
                    "type": "Form"
                  }
                ],
                "type": "SingleColumnLayout"
              },
              "title": "Registro"
            },
            {
              "data": {
                "Registrar_Empresa_Nombre_0": {
                  "__example__": "Example",
                  "type": "string"
                },
                "Registrar_Empresa_NIT_1": {
                  "__example__": "Example",
                  "type": "string"
                }
              },
              "id": "Identificacion",
              "layout": {
                "children": [
                  {
                    "children": [
                      {
                        "input-type": "number",
                        "label": "Digito Verificación",
                        "name": "Digito_Verificacin_7025c8",
                        "required": true,
                        "type": "TextInput"
                      },
                      {
                        "label": "Dirección",
                        "name": "Direccion_265b50",
                        "required": true,
                        "type": "TextArea",
                        "helper-text": "Dirección"
                      },
                      {
                        "input-type": "text",
                        "label": "Departamento",
                        "name": "Departamento_601ceb",
                        "required": true,
                        "type": "TextInput"
                      },
                      {
                        "input-type": "text",
                        "label": "Ciudad",
                        "name": "Ciudad_69892a",
                        "required": true,
                        "type": "TextInput"
                      },
                      {
                        "data-source": [
                          {
                            "id": "0_Impuesto_sobre_ventas_-_IVA",
                            "title": "Impuesto sobre ventas - IVA"
                          },
                          {
                            "id": "1_No_responsable_de_IVA",
                            "title": "No responsable de IVA"
                          }
                        ],
                        "label": "Tipo Régimen",
                        "name": "Tipo_Rgimen_16b36b",
                        "required": true,
                        "type": "Dropdown"
                      },
                      {
                        "data-source": [
                          {
                            "id": "0_Gran_contribuyente",
                            "title": "Gran contribuyente"
                          },
                          {
                            "id": "1_Autorretenedor",
                            "title": "Autorretenedor"
                          },
                          {
                            "id": "2_Agente_de_retención_IVA",
                            "title": "Agente de retención IVA"
                          },
                          {
                            "id": "3_Régimen_simple_tributación",
                            "title": "Régimen simple tributación"
                          },
                          {
                            "id": "4_No_aplica-_Otros",
                            "title": "No aplica- Otros"
                          }
                        ],
                        "label": "Obligaciones Fiscal",
                        "name": "Obligaciones_Fiscales_070a24",
                        "required": true,
                        "type": "Dropdown"
                      },
                      {
                        "label": "Guardar",
                        "on-click-action": {
                          "name": "complete",
                          "payload": {
                            "Registrar_Empresa_Digito_Verificacin_1": "${form.Digito_Verificacin_7025c8}",
                            "Registrar_Empresa_Label_2": "${form.Direccion_265b50}",
                            "Registrar_Empresa_Departamento_3": "${form.Departamento_601ceb}",
                            "Registrar_Empresa_Ciudad_4": "${form.Ciudad_69892a}",
                            "Registrar_Empresa_Tipo_Rgimen_5": "${form.Tipo_Rgimen_16b36b}",
                            "Registrar_Empresa_Obligaciones_Fiscales_6": "${form.Obligaciones_Fiscales_070a24}",

                            "Registrar_Empresa_Nombre_0": "${data.Registrar_Empresa_Nombre_0}",
                            "Registrar_Empresa_NIT_1": "${data.Registrar_Empresa_NIT_1}"
                          }
                        },
                        "type": "Footer"
                      }
                    ],
                    "name": "flow_path",
                    "type": "Form"
                  }
                ],
                "type": "SingleColumnLayout"
              },
              "terminal": true,
              "title": "Registro"
            }
          ],
          "version": "6.3"
        }
        */
    }
    public class Flow_response_json_Model_1584870855544061_CrearCliente : Flow_response_json_Models //CrearCliente
    {
        public string? RegistraCliente_Tipo_Cliente { get; set; }
        public string? RegisterClient_Natural_Nombre { get; set; }
        public string? RegisterClient_Natural_Apellido_Paterno { get; set; }
        public string? RegisterClient_Natural_Apellido_Materno { get; set; }
        public string? RegisterClient_Natural_Correo { get; set; }
        public string? RegisterClient_Natural_Telefono { get; set; }
        public string? RegisterClient_Natural_Tipo_Identificacion { get; set; }
        public string? RegisterClient_Natural_NIT { get; set; }
        public string? RegisterClient_Natural_Digito_Verificacin { get; set; }
        public string? RegisterClient_Natural_Label { get; set; }
        public string? RegisterClient_Natural_Departamento { get; set; }
        public string? RegisterClient_Natural_Ciudad { get; set; }
        public string? RegisterClient_Natural_Tipo_Rgimen { get; set; }
        public string? RegisterClient_Natural_Obligaciones_Fiscale { get; set; }
        public string? RegisterClient_Natural_documento { get; set; }

        public string? RegisterClient_Juridical_Razon_Social { get; set; }
        public string? RegisterClient_Juridical_NIT { get; set; }
        public string? RegisterClient_Juridical_Digito_Verificacion { get; set; }
        public string? RegisterClient_Juridical_Direccion { get; set; }
        public string? RegisterClient_Juridical_Departamento { get; set; }
        public string? RegisterClient_Juridical_Ciudad { get; set; }
        public string? RegisterClient_Juridical_Tipo_Regimen { get; set; }
        public string? RegisterClient_Juridical_Obligaciones_Fiscales { get; set; }
    }
    public class Flow_response_json_Model_931945452349522_Modificar_Cliente_Persona_Física : Flow_response_json_Models //
    {
        public string? ModificarCliente_Natural_Apellido_Materno { get; set; }
        public string? ModificarCliente_Natural_Nombre { get; set; }
        public string? ModificarCliente_Natural_Apellido_Paterno { get; set; }
        public string? ModificarCliente_Natural_Correo { get; set; }
        public string? ModificarCliente_Natural_Telefono { get; set; }
        public string? ModificarCliente_Natural_Tipo_Identificacion { get; set; }
        public string? ModificarCliente_Tipo_Cliente { get; set; }
        public string? ModificarCliente_Natural_NIT { get; set; }
        public string? ModificarCliente_Natural_Digito_Verificacin { get; set; }
        public string? ModificarCliente_Natural_Label { get; set; }
        public string? ModificarCliente_Natural_Departamento { get; set; }
        public string? ModificarCliente_Natural_Ciudad { get; set; }
        public string? ModificarCliente_Natural_Tipo_Rgimen { get; set; }
        public string? ModificarCliente_Natural_Obligaciones_Fiscale { get; set; }
        public Array? ModificarCliente_Natural_documento { get; set; }

    }
    public class Flow_response_json_Model_1378725303264167_Modificar_Cliente_Persona_Jurídica : Flow_response_json_Models //
    {
        public string? ModificarCliente_Empresa_Juridical_Razon_Social { get; set; }
        public string? ModificarCliente_Empresa_Juridical_NIT { get; set; }
        public string? ModificarCliente_Empresa_Tipo_Cliente { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Digito_Verificacion { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Direccion { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Departamento { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Ciudad { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Tipo_Regimen { get; set; }
        public string? ModificarCliente_Empresa_Juridical_Obligaciones_Fiscales { get; set; }
    }
    public class Flow_response_json_Model_1142951587576244_Crear_Producto : Flow_response_json_Models
    {
        public string Agregar_Producto_Nombre { get; set; }
        public string Agregar_Producto_Precio_Unitario { get; set; }
        public string Agregar_Producto_Info_Adicional { get; set; }
        public string Agregar_Producto_Codigo { get; set; }
        public string Agregar_Producto_Unidad_Medida { get; set; }
        public string Agregar_Producto_Activo { get; set; }
        public string Agregar_Producto_traslados { get; set; }
        public string Agregar_Producto_Impuesto { get; set; }
        public string Agregar_Producto_Tasa_cuota { get; set; }
        public string Agregar_Producto_Impuestos_Saludables { get; set; }
        public string Agregar_Producto_Impuestos_Saludables2 { get; set; }
    }
}
/*Ejemplo de respuesta:

 {\"response_json\":\"{
    "screen_0_Apellidos_1\\\":\\\"Si\\\",
    "screen_0_Telfono_3\\\":\\\"5151\\\",
    "screen_1_Tipo_Rgimen_5\\\":\\\"0_Impuesto_sobre_ventas_-_IVA\\\",
    "screen_0_Nombres_0\\\":\\\"Si\\\",
    "screen_1_NIT_0\\\":\\\"Si\\\",
    "screen_1_Departamento_3\\\":\\\"Si\\\",
    "screen_0_Tipo_de_persona_4\\\":\\\"0_Persona_Jur\\\\u00eddica_y_asimiladas\\\",
    "screen_0_Tipo_Identificacin_5\\\":\\\"0_Registro_Civil\\\",
    "screen_2_Razn_social_0\\\":\\\"Si\\\",
    "screen_0_Correo_2\\\":\\\"Si\\\\u0040si.si\\\",
    "screen_1_Digito_Verificacin_1\\\":\\\"5151\\\",
    "flow_token\\\":\\\"Token123\\\",
    "screen_1_Obligaciones_Fiscale_6\\\":\\\"0_Gran_contribuyente\\\",
    "screen_1_Label_2\\\":\\\"Si\\\",
    "screen_1_Ciudad_4\\\":\\\"Si\\\"}\",
    \"body\":\"Sent\",
    \"name\":\"flow\"
   }
}
 */