# 🏗️ Arquitectura y Flujo de SaveYourNote

Este documento explica la estructura completa de la aplicación SaveYourNote, cómo se comunican sus componentes y el flujo de arranque.

---

## 📑 Índice

1. [Visión General](#visión-general)
2. [Estructura de Directorios](#estructura-de-directorios)
3. [Arquitectura de Capas](#arquitectura-de-capas-clean-architecture)
4. [Flujo de Arranque](#flujo-de-arranque)
5. [Configuración Docker](#configuración-docker)
6. [Flujo de Mensajes WhatsApp](#flujo-de-mensajes-whatsapp)
7. [Variables de Entorno](#variables-de-entorno)
8. [Tecnologías y Patrones](#tecnologías-y-patrones)

---

## 🎯 Visión General

SaveYourNote es una API que recibe mensajes de WhatsApp, los procesa y los almacena organizados. La arquitectura sigue el patrón **Clean Architecture** con separación clara entre capas.

### Diagrama de Alto Nivel

```mermaid
graph TB
    Internet["🌐 INTERNET"]
    WhatsApp["📱 WhatsApp Cloud API<br/>(Meta)"]

    subgraph Development["💻 DESARROLLO"]
        ngrok["🔗 ngrok<br/>túnel HTTPS<br/>:4040"]
        API_Dev["🚀 SaveYourNote API<br/>localhost:5001"]
        API_Layer_Dev["API Layer"]
        App_Layer_Dev["Application Layer"]
        Infra_Dev["Infrastructure"]
        Domain_Dev["Domain Layer"]

        ngrok --> API_Dev
        API_Dev --> API_Layer_Dev
        API_Dev --> Infra_Dev
        API_Layer_Dev --> App_Layer_Dev
        Infra_Dev --> Domain_Dev
    end

    subgraph Production["🏭 PRODUCCIÓN"]
        nginx["🔒 Nginx<br/>Reverse Proxy<br/>:80/:443"]
        API_Prod["🚀 SaveYourNote API<br/>imagen optimizada"]

        nginx --> API_Prod
    end

    Internet --> WhatsApp
    WhatsApp -->|Webhook POST| ngrok
    WhatsApp -->|Webhook POST| nginx

    style Development fill:#e3f2fd
    style Production fill:#fff3e0
    style WhatsApp fill:#25D366
```

---

## 📁 Estructura de Directorios

```
saveyournote/
├── docker.sh                    # Script de gestión Docker
├── docker-compose.yml           # Configuración desarrollo
├── docker-compose.prod.yml      # Override producción
├── SaveYourNote.sln             # Solución .NET
├── .env                         # Variables de entorno (no en git)
├── .env.example                 # Plantilla de variables
│
├── nginx/                       # Configuración Nginx
│   ├── nginx.conf               # Config reverse proxy
│   └── certs/                   # Certificados SSL
│
└── src/                         # Código fuente
    │
    ├── SaveYourNote.Api/        # 🌐 CAPA DE PRESENTACIÓN
    │   ├── Controllers/
    │   │   └── WhatsAppWebhookController.cs
    │   ├── Extensions/
    │   │   └── ServiceCollectionExtensions.cs
    │   ├── Middleware/
    │   │   └── ExceptionHandlingMiddleware.cs
    │   ├── Program.cs           # ⭐ Entry Point
    │   ├── Dockerfile           # Multi-stage build
    │   └── appsettings.json
    │
    ├── SaveYourNote.Application/ # 📋 CAPA DE APLICACIÓN
    │   ├── DTOs/
    │   │   └── MessageDto.cs
    │   ├── Interfaces/
    │   │   ├── IMessageService.cs
    │   │   └── IMessageLogger.cs
    │   ├── UseCases/
    │   │   └── ProcessMessage/
    │   │       ├── ProcessMessageCommand.cs
    │   │       └── ProcessMessageHandler.cs
    │   └── Errors/
    │       └── ApplicationErrors.cs
    │
    ├── SaveYourNote.Domain/      # 💎 CAPA DE DOMINIO
    │   ├── Entities/
    │   │   └── Message.cs
    │   ├── ValueObjects/
    │   │   ├── MessageId.cs
    │   │   ├── SenderId.cs
    │   │   ├── TextContent.cs
    │   │   └── AudioContent.cs
    │   ├── Enums/
    │   │   ├── MessageType.cs
    │   │   └── MessageSource.cs
    │   └── Errors/
    │       └── DomainErrors.cs
    │
    └── SaveYourNote.Infrastructure/ # 🔧 CAPA DE INFRAESTRUCTURA
        ├── WhatsApp/
        │   ├── DTOs/
        │   │   └── WhatsAppWebhookDto.cs
        │   ├── Mappers/
        │   │   └── WhatsAppMessageMapper.cs
        │   ├── Validators/
        │   │   └── WhatsAppSignatureValidator.cs
        │   └── Services/
        │       └── WhatsAppService.cs
        └── Logging/
            └── ConsoleMessageLogger.cs
```

---

## 🏛️ Arquitectura de Capas (Clean Architecture)

### Diagrama de Dependencias

```mermaid
graph TB
    subgraph API["🌐 API LAYER"]
        Controllers["Controllers"]
        Middleware["Middleware"]
        Extensions["Extensions<br/>(DI Config)"]
    end

    subgraph Application["📋 APPLICATION LAYER"]
        UseCases["Use Cases<br/>(Handlers)"]
        DTOs["DTOs"]
        Interfaces["Interfaces<br/>(IMessageService)"]
    end

    subgraph Domain["💎 DOMAIN LAYER<br/>⭐ SIN DEPENDENCIAS EXTERNAS"]
        Entities["Entities<br/>(Message)"]
        ValueObjects["Value Objects<br/>(MessageId)"]
        DomainErrors["Domain Errors"]
    end

    subgraph Infrastructure["🔧 INFRASTRUCTURE LAYER"]
        WhatsAppService["WhatsApp Service<br/>(implementación)"]
        Mapper["Message Mapper<br/>(DTO → Domain)"]
        Logger["Console Logger<br/>(implements)"]
    end

    Controllers -->|usa interfaces de| UseCases
    UseCases -->|usa entidades de| Entities
    Infrastructure -->|implementa| Interfaces
    Infrastructure -->|conoce| Domain

    style Domain fill:#ffd700,stroke:#333,stroke-width:3px
    style API fill:#e3f2fd
    style Application fill:#f3e5f5
    style Infrastructure fill:#e8f5e9
```

### Regla de Dependencia

```mermaid
graph TB
    Domain["💎 Domain<br/>Núcleo: sin dependencias"]
    Application["📋 Application"]
    Infrastructure["🔧 Infrastructure"]
    API["🌐 API<br/>Punto de entrada"]

    Domain --> Application
    Domain --> Infrastructure
    Application --> API
    Infrastructure --> API

    style Domain fill:#ffd700,stroke:#333,stroke-width:3px
    style API fill:#90caf9
```

**Principio**: Las capas internas NO conocen a las externas. Domain no sabe nada de HTTP, bases de datos, ni WhatsApp.

---

## 🚀 Flujo de Arranque

### Secuencia de Inicio

```
Terminal          docker.sh         Docker Compose        Dockerfile         Program.cs
   │                  │                   │                   │                  │
   │ ./docker.sh      │                   │                   │                  │
   │ start            │                   │                   │                  │
   │─────────────────►│                   │                   │                  │
   │                  │                   │                   │                  │
   │                  │ 1. Verifica .env  │                   │                  │
   │                  │ 2. Check NGROK    │                   │                  │
   │                  │                   │                   │                  │
   │                  │ docker compose    │                   │                   │
   │                  │ --profile dev up  │                   │                  │
   │                  │──────────────────►│                   │                  │
   │                  │                   │                   │                  │
   │                  │                   │ 3. Build API      │                  │
   │                  │                   │ (target: dev)     │                  │
   │                  │                   │──────────────────►│                  │
   │                  │                   │                   │                  │
   │                  │                   │                   │ 4. dotnet restore│
   │                  │                   │                   │ 5. dotnet watch  │
   │                  │                   │                   │─────────────────►│
   │                  │                   │                   │                  │
   │                  │                   │                   │                  │ 6. Configure
   │                  │                   │                   │                  │    Services
   │                  │                   │                   │                  │ 7. Build App
   │                  │                   │                   │                  │ 8. Listen :5001
   │                  │                   │                   │                  │
   │                  │                   │ 9. Start ngrok    │                  │
   │                  │                   │──────────────────►│                  │
   │                  │                   │                   │                  │
   │                  │ 10. Show URL      │                   │                  │
   │◄─────────────────│                   │                   │                  │
```

### Detalle de Cada Componente

#### 1. docker.sh (Script de entrada)

```mermaid
flowchart TD
    Start(["🚀 Inicio docker.sh"])
    DetectEnv["1. Detecta entorno<br/>ENV = $2 o 'development'"]
    CheckEnv{"2. check_env_file()"}
    EnvExists{"¿Existe .env?"}
    CopyEnv["Copia .env.example → .env"]
    IsDev{"¿ENV = dev?"}
    CheckNgrok["Verifica NGROK_AUTHTOKEN"]
    RunCompose["3. Ejecuta docker compose"]
    DevMode["Dev: docker compose<br/>--profile development up"]
    ProdMode["Prod: docker compose<br/>-f ... -f prod.yml up"]
    ShowURL["4. show_url()<br/>Obtiene URL ngrok :4040"]
    End(["✅ Listo"])

    Start --> DetectEnv
    DetectEnv --> CheckEnv
    CheckEnv --> EnvExists
    EnvExists -->|No| CopyEnv
    EnvExists -->|Sí| IsDev
    CopyEnv --> IsDev
    IsDev -->|Sí| CheckNgrok
    IsDev -->|No| RunCompose
    CheckNgrok --> RunCompose
    RunCompose --> DevMode
    RunCompose --> ProdMode
    DevMode --> ShowURL
    ProdMode --> End
    ShowURL --> End
```

#### 2. docker-compose.yml (Orquestación)

```
┌─────────────────────────────────────────────────────────┐
│                  docker-compose.yml                     │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  services:                                              │
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │  api:                                            │   │
│  │    build:                                        │   │
│  │      context: .                                  │   │
│  │      dockerfile: src/.../Dockerfile              │   │
│  │      target: development ◄── Stage Dockerfile    │   │
│  │    ports: ["5001:5001"]                          │   │
│  │    volumes:                                      │   │
│  │      - ./src:/app/src  ◄── Hot reload            │   │
│  │    environment:                                  │   │
│  │      - ASPNETCORE_ENVIRONMENT=Development        │   │
│  │      - WhatsApp__VerifyToken=${...}              │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │  ngrok:  (profiles: [development])               │   │
│  │    image: ngrok/ngrok:latest                     │   │
│  │    command: http api:5001                        │   │
│  │    ports: ["4040:4040"]                          │   │
│  │    depends_on: [api]                             │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

#### 3. Dockerfile (Multi-stage Build)

```mermaid
flowchart TB
    Base["⚙️ Stage: base<br/>FROM dotnet/sdk:10.0<br/>COPY *.csproj<br/>dotnet restore<br/>COPY . ."]

    Development["💻 Stage: development<br/>dotnet watch<br/>(hot reload)"]
    Build["🔨 Stage: build<br/>dotnet build<br/>-c Release"]
    Publish["📦 Stage: publish<br/>dotnet publish"]
    Production["🏭 Stage: production<br/>FROM dotnet/aspnet:10.0<br/>USER appuser"]

    UseDev(["Usado en desarrollo"])
    UseProd(["Usado en producción"])

    Base --> Development
    Base --> Build
    Base --> Publish
    Publish --> Production

    Development -.-> UseDev
    Production -.-> UseProd

    style Base fill:#e3f2fd
    style Development fill:#c8e6c9
    style Production fill:#ffecb3
    style UseDev fill:#a5d6a7
    style UseProd fill:#ffe082
```

#### 4. Program.cs (Configuración de la App)

```mermaid
flowchart TD
    Start(["📝 Program.cs"])
    CreateBuilder["var builder =<br/>WebApplication.CreateBuilder(args)"]

    subgraph Services["1️⃣ REGISTRAR SERVICIOS"]
        AddControllers["builder.Services.AddControllers()"]
        AddSwagger["builder.Services.AddSwaggerGen()"]
        AddApp["builder.Services.AddApplicationServices()"]
        AddCors["builder.Services.AddCorsConfiguration()"]
    end

    subgraph Kestrel["2️⃣ CONFIGURAR KESTREL"]
        ListenPort["options.ListenAnyIP(5001)"]
    end

    Build["var app = builder.Build()"]

    subgraph Pipeline["3️⃣ CONFIGURAR PIPELINE"]
        UseSwagger["app.UseSwagger()"]
        UseMiddleware["app.UseMiddleware<br/>&lt;ExceptionHandling&gt;()"]
        UseCors["app.UseCors()"]
        MapControllers["app.MapControllers()"]
        MapHealth["app.MapGet('/health', ...)"]
    end

    Run(["🚀 app.Run()<br/>Inicia servidor HTTP"])

    Start --> CreateBuilder
    CreateBuilder --> Services
    Services --> Kestrel
    Kestrel --> Build
    Build --> Pipeline
    Pipeline --> Run

    style Services fill:#e3f2fd
    style Kestrel fill:#f3e5f5
    style Pipeline fill:#fff3e0
```

---

## 🐳 Configuración Docker

### Desarrollo vs Producción

| Aspecto        | Desarrollo           | Producción                |
| -------------- | -------------------- | ------------------------- |
| **Archivo**    | docker-compose.yml   | + docker-compose.prod.yml |
| **Target**     | development          | production                |
| **Comando**    | dotnet watch         | dotnet dll                |
| **Hot reload** | ✅ Sí                | ❌ No                     |
| **Túnel**      | ngrok                | Nginx                     |
| **Usuario**    | root                 | appuser (no-root)         |
| **URL**        | https://xxx.ngrok.io | https://tu-dominio.com    |

### Comandos de docker.sh

| Comando                      | Descripción                    |
| ---------------------------- | ------------------------------ |
| `./docker.sh start`          | Inicia en desarrollo con ngrok |
| `./docker.sh start prod`     | Inicia en producción con nginx |
| `./docker.sh stop`           | Detiene contenedores           |
| `./docker.sh logs [service]` | Muestra logs                   |
| `./docker.sh status`         | Estado de contenedores         |
| `./docker.sh url`            | Muestra URL de ngrok           |

---

## 📨 Flujo de Mensajes WhatsApp

### Secuencia de Procesamiento

```mermaid
sequenceDiagram
    participant WA as WhatsApp<br/>Cloud API
    participant ngrok
    participant Controller
    participant Validator
    participant Handler
    participant Logger

    WA->>ngrok: POST webhook
    ngrok->>Controller: forward :5001
    activate Controller
    Note over Controller: 1. Read body<br/>2. Validate
    Controller->>Validator: Validate signature
    Validator-->>Controller: ✅ OK
    Note over Controller: 3. Deserialize<br/>4. Map to Command<br/>5. Process
    Controller->>Handler: ProcessMessageCommand
    activate Handler
    Note over Handler: 6. Create Entity<br/>7. Validate<br/>8. Log
    Handler->>Logger: Log message
    Logger-->>Handler: Logged
    Handler-->>Controller: MessageDto
    deactivate Handler
    Controller-->>ngrok: 200 OK
    deactivate Controller
    ngrok-->>WA: 200 OK
```

### Transformación de Datos

```mermaid
flowchart LR
    JSON["📄 JSON Payload<br/>(WhatsApp API)"]
    DTO["📦 WhatsAppDto<br/>Webhook"]
    Command["⚡ ProcessMessage<br/>Command"]
    Entity["💎 Message<br/>Entity"]
    Response["📤 MessageDto<br/>(response)"]

    JSON -->|Deserialize| DTO
    DTO -->|Mapper| Command
    Command -->|Create| Entity
    Entity -->|Transform| Response

    style JSON fill:#fff3e0
    style DTO fill:#e8f5e9
    style Command fill:#f3e5f5
    style Entity fill:#ffd700
    style Response fill:#e3f2fd
```

---

## 🔐 Variables de Entorno

### Flujo de Configuración

```mermaid
flowchart TD
    Example["📋 .env.example<br/>Plantilla en Git"]
    Env["🔐 .env<br/>Valores reales<br/>NO en Git"]
    Compose["🐳 docker-compose.yml<br/>environment:<br/>WhatsApp__VerifyToken=${...}<br/>WhatsApp__AccessToken=${...}<br/>WhatsApp__PhoneNumberId=${...}"]
    Container["📦 API Container<br/>IConfiguration['WhatsApp:VerifyToken']<br/>IConfiguration['WhatsApp:AccessToken']<br/>IConfiguration['WhatsApp:AppSecret']"]

    Example -->|copiar| Env
    Env -->|leído por| Compose
    Compose -->|inyectado en| Container

    style Example fill:#e3f2fd
    style Env fill:#ffebee
    style Compose fill:#e8f5e9
    style Container fill:#fff3e0
```

### Tabla de Variables

| Variable                   | Requerida | Usado en         | Propósito               |
| -------------------------- | --------- | ---------------- | ----------------------- |
| `NGROK_AUTHTOKEN`          | Dev       | docker.sh, ngrok | Autenticación ngrok     |
| `WHATSAPP_VERIFY_TOKEN`    | Sí        | API              | Verificación webhook    |
| `WHATSAPP_ACCESS_TOKEN`    | Sí        | API              | Llamadas a WhatsApp API |
| `WHATSAPP_PHONE_NUMBER_ID` | Sí        | API              | ID del número           |
| `WHATSAPP_APP_SECRET`      | Prod      | API              | Validar firmas HMAC     |
| `ASPNETCORE_ENVIRONMENT`   | No        | API              | Development/Production  |

---

## 🛠️ Tecnologías y Patrones

### Stack Tecnológico

| Categoría        | Tecnología     | Propósito                    |
| ---------------- | -------------- | ---------------------------- |
| **Runtime**      | .NET 10.0      | Plataforma de ejecución      |
| **Framework**    | ASP.NET Core   | Web API                      |
| **Lenguaje**     | C#             | Código fuente                |
| **Containers**   | Docker         | Contenedorización            |
| **Orquestación** | Docker Compose | Multi-container              |
| **Túnel**        | ngrok          | Exponer localhost            |
| **Errores**      | ErrorOr        | Railway-oriented programming |
| **Docs API**     | Swashbuckle    | Swagger/OpenAPI              |

### Patrones de Diseño

| Patrón                         | Aplicación                        |
| ------------------------------ | --------------------------------- |
| **Clean Architecture**         | Separación en 4 capas             |
| **Dependency Injection**       | ServiceCollectionExtensions       |
| **Railway-Oriented (ErrorOr)** | Manejo de errores sin excepciones |
| **Value Objects**              | MessageId, SenderId, TextContent  |
| **Command Pattern**            | ProcessMessageCommand → Handler   |
| **Repository Pattern**         | (Futuro: persistencia)            |

---

## 📚 Referencias Rápidas

| Necesito...           | Archivo                          |
| --------------------- | -------------------------------- |
| Iniciar la aplicación | `./docker.sh start`              |
| Añadir un servicio DI | `ServiceCollectionExtensions.cs` |
| Configurar la API     | `Program.cs`                     |
| Manejar webhooks      | `WhatsAppWebhookController.cs`   |
| Procesar mensajes     | `ProcessMessageHandler.cs`       |
| Definir entidades     | `Domain/Entities/`               |
| Configurar Docker     | `docker-compose.yml`             |
| Variables sensibles   | `.env`                           |

---

## 🔄 Resumen del Flujo Completo

```mermaid
flowchart TD
    Dev["👨‍💻 1. DESARROLLADOR<br/>./docker.sh start"]
    Compose["🐳 2. DOCKER COMPOSE<br/>lee docker-compose.yml + .env"]
    Dockerfile["📦 3. DOCKERFILE<br/>construye imagen<br/>(stage: development)"]
    Container["🚀 4. CONTENEDOR API<br/>ejecuta dotnet watch run"]
    Program["⚙️ 5. PROGRAM.CS<br/>configura DI, middleware, endpoints"]
    Ngrok["🔗 6. NGROK<br/>crea túnel HTTPS → localhost:5001"]
    WhatsApp["📱 7. WHATSAPP<br/>envía mensaje al webhook"]
    Controller["🎯 8. CONTROLLER<br/>recibe, valida, mapea"]
    Handler["⚡ 9. HANDLER<br/>procesa y crea entidad de dominio"]
    Response["✅ 10. RESPONSE<br/>200 OK a WhatsApp"]

    Dev --> Compose
    Compose --> Dockerfile
    Dockerfile --> Container
    Container --> Program
    Program --> Ngrok
    Ngrok --> WhatsApp
    WhatsApp --> Controller
    Controller --> Handler
    Handler --> Response

    style Dev fill:#e3f2fd
    style Compose fill:#e8f5e9
    style Dockerfile fill:#fff3e0
    style Container fill:#f3e5f5
    style Program fill:#ffebee
    style Ngrok fill:#e0f2f1
    style WhatsApp fill:#25D366,color:#fff
    style Controller fill:#fce4ec
    style Handler fill:#f1f8e9
    style Response fill:#c8e6c9
```

---

_Documento generado para SaveYourNote - Última actualización: Enero 2026_
