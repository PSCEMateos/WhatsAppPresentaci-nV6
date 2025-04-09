namespace WhatsAppPresentacionV6.Modelos
{
    public class WhatsAppInteractive
    {
        public class InteractiveButton
        {
            public string ButtonId { get; set; }
            public string ButtonLabel { get; set; }
        }
        public class WhatsAppMessage
        {
            public string From { get; set; }
            public string Body { get; set; }
            public string MessageType { get; set; }
            public InteractiveButton Interactive { get; set; }
        }
    }
}
/*
{
    "routing_model": {
        "Persona_Natural_Info_Base":["Persona_Natural_Identificacion"],
        "Persona_Natural_Identificacion":[]
    },
  "screens": [
    {
        "id": "Persona_Natural_Info_Base",
      "title": "Modificar Persona Natural",
      "data": { "ModificarCliente_Tipo_Cliente": { "__example__":"Example", "type": "string"} },
      "layout": {
            "type": "SingleColumnLayout",
        "children": [
          {
                "name": "flow_path",
            "type": "Form",
            "children": [
              {
                    "text": "Favor de Indicar su nombre y apellido",
                "type": "TextBody"
              },
              {
                    "input-type": "text",
                "label": "Nombre(s)",
                "name": "nombres",
                "required": true,
                "type": "TextInput"
              },
              {
                    "input-type": "text",
                "label": "Apellido Paterno",
                "name": "apellido_paterno",
                "required": true,
                "type": "TextInput"
              },
              {
                    "input-type": "text",
                "label": "Apellido Materno",
                "name": "apellido_materno",
                "required": true,
                "type": "TextInput"
              },
              {
                    "input-type": "email",
                "label": "Correo",
                "name": "correo",
                "required": true,
                "type": "TextInput"
              },
              {
                    "input-type": "phone",
                "label": "Teléfono",
                "name": "telefono",
                "required": true,
                "type": "TextInput"
              },
              {
                    "label": "Tipo Identificación",
                "name": "tipo_identificacion",
                "type": "Dropdown",
                "required": true,
                "data-source": [
                  { "id": "0", "title": "Registro Civil" },
                  { "id": "1", "title": "Cédula de Ciudadanía" },
                  { "id": "2", "title": "Tarjeta de extranjería" },
                  { "id": "3", "title": "Cédula de extranjería" },
                  { "id": "4", "title": "NIT" },
                  { "id": "5", "title": "Pasaporte" },
                  { "id": "6", "title": "Doc. identificación extranjero" },
                  { "id": "7", "title": "PEP (Permiso Especial de Permanencia)" }
                ]
              },
              {
                    "label": "Continuar",
                "type": "Footer",
                "on-click-action": {
                        "name": "navigate",
                  "next": {
                            "name": "Persona_Natural_Identificacion",
                      "type": "screen"
                  },
                  "payload": {
                            "ModificarCliente_Tipo_Cliente": "${data.ModificarCliente_Tipo_Cliente}",

                      "ModificarCliente_Natural_Nombre": "${form.nombres}",
                      "ModificarCliente_Natural_Apellido_Paterno": "${form.apellido_paterno}",
                      "ModificarCliente_Natural_Apellido_Materno": "${form.apellido_materno}",
                      "ModificarCliente_Natural_Correo": "${form.correo}",
                      "ModificarCliente_Natural_Telefono": "${form.telefono}",
                      "ModificarCliente_Natural_Tipo_Identificacion": "${form.tipo_identificacion}"
                  }
                    }
                }
            ]
          }
        ]
      }
    },
    {
        "id": "Persona_Natural_Identificacion",
      "terminal": true,
      "layout": {
            "children": [
              {
                "children": [
                  {
                    "type": "If",
                "condition":"${data.ModificarCliente_Natural_Tipo_Identificacion} == '4'",
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
                            "ModificarCliente_Tipo_Cliente": "${data.ModificarCliente_Tipo_Cliente}",

                      "ModificarCliente_Natural_Nombre": "${data.ModificarCliente_Natural_Nombre}",
                      "ModificarCliente_Natural_Apellido_Paterno": "${data.ModificarCliente_Natural_Apellido_Paterno}",
                      "ModificarCliente_Natural_Apellido_Materno": "${data.ModificarCliente_Natural_Apellido_Materno}",
                      "ModificarCliente_Natural_Correo": "${data.ModificarCliente_Natural_Correo}",
                      "ModificarCliente_Natural_Telefono": "${data.ModificarCliente_Natural_Telefono}",
                      "ModificarCliente_Natural_Tipo_Identificacion": "${data.ModificarCliente_Natural_Tipo_Identificacion}",

                    "ModificarCliente_Natural_NIT": "${form.NIT_dbaa4f}",
                    "ModificarCliente_Natural_Digito_Verificacin": "${form.Digito_Verificacin_7025c8}",
                    "ModificarCliente_Natural_Label": "${form.Direccion_265b50}",
                    "ModificarCliente_Natural_Departamento": "${form.Departamento_601ceb}",
                    "ModificarCliente_Natural_Ciudad": "${form.Ciudad_69892a}",
                    "ModificarCliente_Natural_Tipo_Rgimen": "${form.Tipo_Rgimen_16b36b}",
                    "ModificarCliente_Natural_Obligaciones_Fiscale": "${form.Obligaciones_Fiscale_070a24}",
                    "ModificarCliente_Natural_documento": "${form.Documento_Subido}"
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
      "data": {
            "ModificarCliente_Natural_Apellido_Materno": { "__example__": "Example","type": "string"},
        "ModificarCliente_Natural_Nombre": { "__example__": "Example","type": "string"},
        "ModificarCliente_Natural_Apellido_Paterno": { "__example__": "Example","type": "string"},
        "ModificarCliente_Natural_Correo": { "__example__": "Example","type": "string"},
        "ModificarCliente_Natural_Telefono": { "__example__": "Example","type": "string"},
        "ModificarCliente_Natural_Tipo_Identificacion": { "__example__": "Example","type": "string"},
        "ModificarCliente_Tipo_Cliente": { "__example__":"Example", "type": "string"}
        },
      "title": "Modificar Persona Natural"
    }
  ],
  "version": "6.3"
}
*/