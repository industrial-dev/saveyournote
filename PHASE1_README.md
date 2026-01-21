# SaveYourNote - Fase 1.1: WhatsApp Webhook Integration

## ✅ Implementación Completada

Esta es la **Fase 1.1** del proyecto SaveYourNote: una API que se comunica con WhatsApp y recibe mensajes (texto y audio), mostrándolos en consola.

### 🎯 Objetivos Cumplidos

- ✅ API REST con ASP.NET Core (.NET 10)
- ✅ Arquitectura limpia (Clean Architecture) con 4 capas
- ✅ Principios SOLID respetados
- ✅ Dominio agnóstico de tecnología (no atado a WhatsApp)
- ✅ Manejo de errores funcional con **ErrorOr**
- ✅ Recepción de mensajes de texto desde WhatsApp
- ✅ Recepción de mensajes de audio desde WhatsApp
- ✅ Salida formateada en consola con colores
- ✅ Validación de firmas HMAC-SHA256 (seguridad)
- ✅ Endpoint de verificación de webhook

## 🏗️ Arquitectura

El proyecto sigue **Clean Architecture** con separación clara de responsabilidades:

```
src/
├── SaveYourNote.Domain/              # Capa de Dominio (puro, sin dependencias)
│   ├── Entities/                     # Message (entidad principal)
│   ├── ValueObjects/                 # MessageId, SenderId, TextContent, AudioContent
│   ├── Enums/                        # MessageType, MessageSource
│   ├── Errors/                       # DomainErrors
│   └── Interfaces/                   # IMessageProcessor
│
├── SaveYourNote.Application/         # Capa de Aplicación (casos de uso)
│   ├── UseCases/ProcessMessage/      # ProcessMessageCommand, ProcessMessageHandler
│   ├── DTOs/                         # MessageDto
│   ├── Errors/                       # ApplicationErrors
│   └── Interfaces/                   # IMessageService, IMessageLogger
│
├── SaveYourNote.Infrastructure/      # Capa de Infraestructura (implementaciones)
│   ├── WhatsApp/                     # Código específico de WhatsApp
│   │   ├── DTOs/                     # WhatsAppWebhookDto
│   │   ├── Mappers/                  # WhatsAppMessageMapper
│   │   └── Validators/               # WhatsAppSignatureValidator
│   └── Logging/                      # ConsoleMessageLogger
│
└── SaveYourNote.Api/                 # Capa de API (controladores)
    ├── Controllers/                  # WhatsAppWebhookController
    ├── Middleware/                   # ExceptionHandlingMiddleware
    └── Extensions/                   # ServiceCollectionExtensions
```

### 🔑 Características Clave

1. **Dominio Agnóstico**: La entidad `Message` no está atada a WhatsApp. Puede recibir mensajes de cualquier fuente (WhatsApp, WebApp, MobileApp, API).

2. **ErrorOr Pattern**: Manejo de errores funcional sin excepciones, usando `ErrorOr<T>` para comunicación entre capas.

3. **Value Objects**: Validación en el dominio con objetos de valor inmutables.

4. **Dependency Injection**: Configuración limpia de DI siguiendo SOLID.

5. **Seguridad**: Validación de firmas HMAC-SHA256 para webhooks de WhatsApp.

## 🚀 Inicio Rápido

### Requisitos

- .NET SDK 10.0
- curl (para pruebas)
- jq (opcional, para formatear JSON)

### Instalación

```bash
# Clonar el repositorio
cd /Users/uningeniero/Documents/Repos/saveyournote

# Restaurar dependencias
dotnet restore

# Compilar el proyecto
dotnet build
```

### Ejecución

```bash
# Ejecutar la API
dotnet run --project src/SaveYourNote.Api/SaveYourNote.Api.csproj

# La API estará disponible en http://localhost:5001
```

## 🧪 Pruebas

### 1. Health Check

```bash
curl http://localhost:5001/health
```

**Respuesta esperada:**

```json
{
  "status": "healthy",
  "timestamp": "2026-01-21T19:18:30Z",
  "service": "SaveYourNote API"
}
```

### 2. Verificación de Webhook (GET)

```bash
curl "http://localhost:5001/api/whatsapp/webhook?hub.mode=subscribe&hub.verify_token=dev_verify_token_12345&hub.challenge=test_challenge_123"
```

**Respuesta esperada:**

```
test_challenge_123
```

### 3. Mensaje de Texto (POST)

```bash
curl -X POST http://localhost:5001/api/whatsapp/webhook \
  -H "Content-Type: application/json" \
  -d @test-text-message.json
```

**Respuesta esperada:**

```json
{
  "status": "success",
  "message": "Message received and logged",
  "data": {
    "messageId": "wamid.test123",
    "senderId": "34612345678",
    "content": "Hola, este es un mensaje de prueba desde WhatsApp",
    "type": 0,
    "source": 0,
    "timestamp": "2021-12-01T12:00:00Z"
  }
}
```

**Salida en consola:**

```
[2026-01-21 20:18:33] Message Received
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Message ID: wamid.test123
Source: 📱 WhatsApp
From: 34612345678
Type: Text
Timestamp: 2021-12-01 12:00:00 UTC
Content: Hola, este es un mensaje de prueba desde WhatsApp
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

### 4. Mensaje de Audio (POST)

```bash
curl -X POST http://localhost:5001/api/whatsapp/webhook \
  -H "Content-Type: application/json" \
  -d @test-audio-message.json
```

**Respuesta esperada:**

```json
{
  "status": "success",
  "message": "Message received and logged",
  "data": {
    "messageId": "wamid.audio123",
    "senderId": "34612345678",
    "content": "Audio: audio_id_123 (audio/ogg; codecs=opus)",
    "type": 1,
    "source": 0,
    "timestamp": "2021-12-01T12:00:00Z"
  }
}
```

**Salida en consola:**

```
[2026-01-21 20:18:51] Message Received
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Message ID: wamid.audio123
Source: 📱 WhatsApp
From: 34612345678
Type: Audio
Timestamp: 2021-12-01 12:00:00 UTC
Audio ID: audio_id_123
MIME Type: audio/ogg; codecs=opus
SHA256: abc123hash
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## ⚙️ Configuración

### appsettings.Development.json

```json
{
  "WhatsApp": {
    "VerifyToken": "dev_verify_token_12345",
    "AppSecret": ""
  }
}
```

- **VerifyToken**: Token para verificación de webhook (usado en GET /webhook)
- **AppSecret**: Secret de la app de WhatsApp para validar firmas (opcional en desarrollo)

## 📦 Dependencias

- **ErrorOr** (2.0.1): Manejo funcional de errores
- **Swashbuckle.AspNetCore** (10.1.0): Documentación Swagger/OpenAPI

## 🔄 Próximas Fases

- **Fase 1.2**: Descarga de archivos de audio desde WhatsApp
- **Fase 1.3**: Transcripción de audio con Whisper
- **Fase 2**: Análisis y clasificación con IA (Ollama)
- **Fase 3**: Almacenamiento en Google Sheets

## 📝 Notas Técnicas

### Clean Architecture

El proyecto respeta estrictamente Clean Architecture:

- **Domain**: No tiene dependencias externas (puro C#)
- **Application**: Solo depende de Domain + ErrorOr
- **Infrastructure**: Implementa interfaces de Application
- **API**: Orquesta todo y expone endpoints

### SOLID Principles

- **Single Responsibility**: Cada clase tiene una única responsabilidad
- **Open/Closed**: Extensible sin modificar código existente
- **Liskov Substitution**: Las interfaces son sustituibles
- **Interface Segregation**: Interfaces pequeñas y específicas
- **Dependency Inversion**: Dependencias hacia abstracciones

### ErrorOr Pattern

En lugar de excepciones, usamos `ErrorOr<T>`:

```csharp
// En lugar de:
try {
    var result = DoSomething();
} catch (Exception ex) {
    // manejar error
}

// Usamos:
var result = DoSomething(); // ErrorOr<T>
if (result.IsError) {
    // manejar result.FirstError
}
```

## 🎉 Verificación de Éxito

✅ **Todos los criterios de la Fase 1.1 cumplidos:**

1. ✅ API se ejecuta correctamente en local (puerto 5001)
2. ✅ Recibe mensajes de WhatsApp (texto y audio)
3. ✅ Muestra mensajes en consola con formato estructurado
4. ✅ Clean Architecture implementada
5. ✅ Principios SOLID respetados
6. ✅ ErrorOr integrado para manejo de errores
7. ✅ Dominio agnóstico de tecnología
8. ✅ Código limpio y bien documentado

---

**Desarrollado con Clean Architecture, SOLID y buenas prácticas de programación** 🚀
