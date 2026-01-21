# Guía de Uso de Docker - SaveYourNote

Docker permite ejecutar la aplicación SaveYourNote en cualquier entorno de forma consistente, asegurando que todas las dependencias y configuraciones sean las mismas tanto en tu máquina local como en producción.

## 🐳 ¿Qué es Docker?

Docker es una plataforma que "empaqueta" la aplicación y todo lo necesario para que funcione (librerías, ambiente de ejecución, etc.) en un **contenedor**. Esto evita el clásico problema de "en mi máquina funciona, pero en el servidor no".

## 🚀 Requisitos Previos

1. **Docker Desktop**: Descarga e instala Docker para Mac desde [docker.com](https://www.docker.com/products/docker-desktop/).
2. Asegúrate de que Docker esté corriendo (verás un pequeño icono de una ballena en la barra superior de tu Mac).

## 🛠️ Cómo ejecutar el proyecto con Docker

### 1. Desde la Terminal (Recomendado)

En la raíz del proyecto, ejecuta:

```bash
docker compose up --build
```

- `--build`: Fuerza a Docker a reconstruir la imagen (necesario si has hecho cambios en el código).
- `up`: Enciende el contenedor.

La API estará disponible en: <http://localhost:5001>

Para detener la aplicación:
Presiona `Ctrl + C` o ejecuta `docker compose down`.

### 2. Desde Antigravity (Botón "Play")

Hemos configurado un perfil de ejecución especial para que puedas usar el botón "Play" del IDE:

1. En la barra superior, busca el menú desplegable de "Perfiles de Ejecución" (donde suele poner `http`).
2. Selecciona el perfil **Docker**.
3. Pulsa el botón **Play**.

Esto arrancará el contenedor automáticamente y lo detendrá cuando detengas la ejecución en el IDE.

## 📂 Archivos de Configuración

- **Dockerfile**: Contiene las instrucciones para construir la imagen de la API.
- **docker-compose.yml**: Orquesta los servicios (en el futuro aquí también configuraremos la base de datos).
- **.dockerignore**: Evita que archivos innecesarios (como los que genera .NET al compilar localmente) entren en el contenedor, haciendo que la construcción sea más rápida.

## 💡 Tips Útiles

- **Ver logs**: Si usas `docker compose up -d` (modo oculto), puedes ver los mensajes de la API con `docker compose logs -f`.
- **Limpiar**: Si sientes que algo no va bien, puedes limpiar todo con `docker system prune` (cuidado, esto borra imágenes no usadas).

---

¡Ahora tu entorno de desarrollo es profesional y está listo para escalar! 🚀
