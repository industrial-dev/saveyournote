# 🐳 SaveYourNote - Guía de Docker

## 📋 Descripción

Este proyecto utiliza Docker para ejecutar:

- **API**: SaveYourNote API con hot reload para desarrollo
- **ngrok**: Túnel para exponer la API local a internet (necesario para webhooks de WhatsApp)

## 🚀 Inicio Rápido

### 1️⃣ Configurar variables de entorno

```bash
# Copia el archivo de ejemplo
cp .env.example .env

# Edita .env y añade tu NGROK_AUTHTOKEN
# Obtén tu token en: https://dashboard.ngrok.com/get-started/your-authtoken
```

El archivo `.env` debe contener:

```env
NGROK_AUTHTOKEN=tu_token_real_de_ngrok
WHATSAPP_VERIFY_TOKEN=your_verify_token_here
WHATSAPP_ACCESS_TOKEN=your_access_token_here
WHATSAPP_PHONE_NUMBER_ID=your_phone_number_id_here
```

### 2️⃣ Iniciar los contenedores

```bash
# Opción A: Usar el script de gestión (recomendado)
./docker.sh start

# Opción B: Usar docker-compose directamente
docker-compose up -d
```

Esto iniciará:

- ✅ API en `http://localhost:5001`
- ✅ Swagger en `http://localhost:5001`
- ✅ ngrok con URL pública
- ✅ Panel de ngrok en `http://localhost:4040`

### 3️⃣ Ver la URL de ngrok

```bash
# El script te mostrará la URL automáticamente, o usa:
./docker.sh url

# O visita el panel de ngrok:
open http://localhost:4040
```

### 4️⃣ Configurar webhook en Meta

Usa la URL de ngrok para configurar el webhook:

```
URL: https://tu-url.ngrok.io/api/whatsapp/webhook
Verify Token: tu_verify_token_aqui
```

## 🛠️ Comandos del Script

El script `docker.sh` facilita la gestión de los contenedores:

```bash
# Iniciar contenedores
./docker.sh start

# Detener contenedores
./docker.sh stop

# Reiniciar contenedores
./docker.sh restart

# Ver logs (todos los servicios)
./docker.sh logs

# Ver logs de un servicio específico
./docker.sh logs api
./docker.sh logs ngrok

# Ver estado de los contenedores
./docker.sh status

# Mostrar URL de ngrok
./docker.sh url

# Reconstruir contenedores (después de cambios en Dockerfile)
./docker.sh rebuild

# Abrir shell en un contenedor
./docker.sh shell api

# Limpiar todo (contenedores, volúmenes, imágenes)
./docker.sh clean
```

## 📁 Estructura de Archivos Docker

```
saveyournote/
├── docker-compose.yml          # Configuración de servicios
├── docker.sh                   # Script de gestión
├── .env                        # Variables de entorno (no en git)
├── .env.example                # Plantilla de .env
├── .dockerignore               # Archivos a ignorar en Docker
└── src/
    └── SaveYourNote.Api/
        └── Dockerfile          # Imagen de la API
```

## 🔥 Hot Reload (Desarrollo)

Los cambios en el código se reflejan automáticamente sin reiniciar:

1. Edita cualquier archivo `.cs`
2. Guarda los cambios
3. `dotnet watch` detecta los cambios y recompila automáticamente
4. La API se reinicia con los nuevos cambios

Ver logs en tiempo real:

```bash
./docker.sh logs api
```

## 🌐 Acceso a Servicios

| Servicio     | URL Local                                 | Descripción                |
| ------------ | ----------------------------------------- | -------------------------- |
| API          | http://localhost:5001                     | API principal              |
| Swagger      | http://localhost:5001                     | Documentación interactiva  |
| ngrok Panel  | http://localhost:4040                     | Panel de control de ngrok  |
| Health Check | http://localhost:5001/health              | Estado de la API           |
| Webhook      | https://xxx.ngrok.io/api/whatsapp/webhook | Endpoint público para Meta |

## 🐛 Debugging

### Ver logs en tiempo real:

```bash
# Todos los servicios
./docker.sh logs

# Solo API
./docker.sh logs api

# Solo ngrok
./docker.sh logs ngrok
```

### Inspeccionar contenedores:

```bash
# Ver estado
./docker.sh status

# Abrir shell en la API
./docker.sh shell api

# Ver procesos
docker-compose top
```

### Problemas comunes:

#### ❌ ngrok no se conecta:

```bash
# Verifica que tu NGROK_AUTHTOKEN esté en .env
cat .env | grep NGROK_AUTHTOKEN

# Reinicia ngrok
docker-compose restart ngrok

# Ver logs de ngrok
./docker.sh logs ngrok
```

#### ❌ API no responde:

```bash
# Ver logs de la API
./docker.sh logs api

# Verificar health check
curl http://localhost:5001/health

# Reiniciar API
docker-compose restart api
```

#### ❌ Cambios no se reflejan:

```bash
# Reconstruir e iniciar
./docker.sh rebuild
```

## 🔒 Seguridad

⚠️ **IMPORTANTE:**

- `.env` está en `.gitignore` - NUNCA lo subas a git
- Usa `.env.example` como plantilla sin valores reales
- Rota tokens periódicamente
- En producción, usa secretos de Docker o un gestor de secretos

## 🚀 Despliegue

### Desarrollo:

```bash
./docker.sh start
```

### Producción:

Para producción, necesitarás:

1. Crear un `docker-compose.prod.yml`
2. Usar un servicio como ngrok permanente o un dominio real
3. Configurar certificados SSL
4. Usar variables de entorno seguras

## 📊 Monitoreo

### Health Check:

```bash
# Verificar que la API esté saludable
curl http://localhost:5001/health
```

### Métricas del contenedor:

```bash
# Ver uso de recursos
docker stats

# Ver logs de contenedor
docker logs saveyournote-api -f
docker logs saveyournote-ngrok -f
```

## 🧹 Limpieza

### Detener contenedores:

```bash
./docker.sh stop
```

### Limpiar todo (contenedores, volúmenes, imágenes):

```bash
./docker.sh clean
```

### Limpiar recursos no utilizados de Docker:

```bash
# Limpiar todo lo no utilizado en el sistema
docker system prune -a --volumes
```

## 📝 Notas

- **Hot reload**: Los cambios en el código se reflejan automáticamente
- **Persistencia**: Los packages de NuGet se cachean en un volumen para builds más rápidos
- **Red**: Los contenedores se comunican entre sí a través de `saveyournote-network`
- **Restart**: Los contenedores se reinician automáticamente si fallan

## 🔗 Enlaces Útiles

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [ngrok Documentation](https://ngrok.com/docs)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
