# SaveYourNote API - Guía de Configuración

API para integración con WhatsApp Business usando .NET 10 y Caddy reverse proxy.

## 🚀 Inicio Rápido

### Prerrequisitos

- .NET 10 SDK
- Docker y Docker Compose
- Cuenta de WhatsApp Business API (Meta for Developers)

### 1. Clonar y configurar

```bash
# Clonar el repositorio
git clone <tu-repo>
cd saveyournote

# Copiar archivo de configuración
cp .env.example .env
```

### 2. Configurar WhatsApp Business API

1. Ve a [Meta for Developers](https://developers.facebook.com/apps)
2. Crea una app de WhatsApp Business
3. Obtén las credenciales necesarias
4. Actualiza el archivo `.env`:

```env
WHATSAPP_VERIFY_TOKEN=mi_token_secreto_123
WHATSAPP_ACCESS_TOKEN=tu_token_de_meta
WHATSAPP_PHONE_NUMBER_ID=123456789
WHATSAPP_BUSINESS_ACCOUNT_ID=987654321
```

### 3. Configurar Webhook en Meta

1. En tu app de WhatsApp Business, ve a **Configuration** > **Webhooks**
2. Configura la URL del webhook:
   - **Callback URL**: `https://tu-dominio.com/api/whatsapp`
   - **Verify Token**: El mismo que pusiste en `WHATSAPP_VERIFY_TOKEN`
3. Suscríbete a los eventos: `messages`, `messaging_postbacks`

## 🐳 Ejecución con Docker

### Desarrollo Local

```bash
# Construir y ejecutar
docker-compose up --build

# La API estará disponible en:
# http://localhost/api/whatsapp
# http://localhost/health
```

### Producción

1. Actualiza el `Caddyfile` con tu dominio:

```caddy
api.tudominio.com {
    reverse_proxy api:8080
    encode gzip
}
```

2. Ejecuta:

```bash
docker-compose up -d
```

Caddy obtendrá automáticamente certificados SSL de Let's Encrypt.

## 🛠️ Desarrollo sin Docker

```bash
cd src/SaveYourNote.Api
dotnet run
```

La API correrá en `http://localhost:5000`

## 📡 Endpoints

### Health Check

```http
GET /health
```

### Webhook de WhatsApp

**Verificación (GET):**

```http
GET /api/whatsapp?hub.mode=subscribe&hub.verify_token=tu_token&hub.challenge=1234
```

**Recepción de mensajes (POST):**

```http
POST /api/whatsapp
Content-Type: application/json

{
  "object": "whatsapp_business_account",
  "entry": [...]
}
```

## 📝 Logs

Los mensajes recibidos se loguean automáticamente en la consola con formato:

```
=== WEBHOOK RECEIVED ===
From: +1234567890 (John Doe)
Message ID: wamid.xxx
Type: text
📱 TEXT MESSAGE: Hola, este es un mensaje de prueba
=== WEBHOOK PROCESSED ===
```

### Tipos de mensajes soportados:

- 📱 **Text**: Mensajes de texto
- 🖼️ **Image**: Imágenes con caption opcional
- 🎵 **Audio**: Audios y notas de voz
- 🎥 **Video**: Videos con caption opcional
- 📄 **Document**: Documentos (PDF, DOCX, etc.)
- 📍 **Location**: Ubicaciones compartidas

## 🔧 Estructura del Proyecto

```
saveyournote/
├── src/
│   └── SaveYourNote.Api/
│       ├── Controllers/
│       │   └── WhatsAppController.cs      # Controlador del webhook
│       ├── Models/
│       │   └── WhatsApp/
│       │       └── WhatsAppWebhook.cs     # Modelos de WhatsApp
│       ├── Program.cs                      # Configuración de la API
│       ├── appsettings.json               # Configuración base
│       ├── appsettings.Development.json   # Configuración dev
│       ├── appsettings.Production.json    # Configuración prod
│       └── Dockerfile                      # Docker para la API
├── docker-compose.yml                      # Orquestación de contenedores
├── Caddyfile                               # Configuración del reverse proxy
└── .env.example                            # Plantilla de variables de entorno
```

## 🔐 Seguridad

- ✅ HTTPS automático con Let's Encrypt (vía Caddy)
- ✅ Headers de seguridad configurados
- ✅ Verificación de token en webhook
- ✅ Variables de entorno para secretos
- ✅ .gitignore para archivos sensibles

## 📚 Recursos

- [WhatsApp Business API Docs](https://developers.facebook.com/docs/whatsapp/cloud-api)
- [Webhook Payload Examples](https://developers.facebook.com/docs/whatsapp/cloud-api/webhooks/payload-examples)
- [Caddy Documentation](https://caddyserver.com/docs/)
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)

## 🤝 Próximos Pasos

1. [ ] Implementar envío de mensajes (respuestas automáticas)
2. [ ] Agregar base de datos para almacenar mensajes
3. [ ] Implementar procesamiento de comandos
4. [ ] Agregar pruebas unitarias
5. [ ] Configurar CI/CD

## 📄 Licencia

Ver archivo LICENSE
