# 🛠️ Comandos de Desarrollo

Este proyecto proporciona **tres formas** de ejecutar comandos de desarrollo, todas multiplataforma:

1. **📦 Scripts npm** (Recomendado) - Funciona en todos los SO
2. **🟢 Node.js** - Script JavaScript multiplataforma
3. **🐚 Bash** - Para usuarios de Linux/Mac/WSL

## 🎯 Características

- ✅ **Multiplataforma**: Funciona en Windows, macOS y Linux
- ✅ **Múltiples interfaces**: npm, Node.js o Bash
- ✅ **Interfaz amigable**: Menú interactivo con emojis
- ✅ **Manejo de errores**: Feedback claro de éxito/error

## 📋 Comandos Disponibles

### API Local

```bash
# Iniciar la API en modo desarrollo
npm start
# o
npm run dev start-api
# o
node dev-commands.js start-api
```

### Compilación

```bash
# Compilar el proyecto
npm run build
# o
npm run dev build

# Limpiar y compilar
npm run dev clean-build
```

### Docker

```bash
# Iniciar con Docker Compose
npm run docker:up
# o
npm run dev docker-up

# Detener contenedores
npm run docker:down
# o
npm run dev docker-down

# Ver logs
npm run docker:logs
# o
npm run dev docker-logs
```

### Formateo de Código

```bash
# Formatear todo el código
npm run format
# o
npm run dev format

# Verificar formato sin modificar
npm run format:check
# o
npm run dev format-check
```

### Testing

```bash
# Ejecutar tests
npm test
# o
npm run dev test
```

### Dependencias

```bash
# Restaurar todas las dependencias
npm run restore
# o
npm run dev restore
```

## 📖 Menú de Ayuda

Para ver todos los comandos disponibles:

```bash
# Con npm
npm run dev

# Con Node.js
node dev-commands.js

# Con Bash (Linux/Mac/WSL)
./dev-commands.sh
```

Salida:

```
╔═══════════════════════════════════════════════════════╗
║         SaveYourNote API - Comandos                   ║
╠═══════════════════════════════════════════════════════╣
║  🚀 start-api      → Ejecutar la API localmente       ║
║  🔨 build          → Compilar el proyecto             ║
║  🧹 clean-build    → Limpiar y recompilar             ║
║  🐳 docker-up      → Ejecutar con Docker Compose      ║
║  🛑 docker-down    → Detener contenedores de Docker   ║
║  📋 docker-logs    → Ver logs de Docker               ║
║  🎨 format         → Formatear código con CSharpier   ║
║  🔍 format-check   → Verificar formato del código     ║
║  🧪 test           → Ejecutar tests                   ║
║  📦 restore        → Restaurar dependencias           ║
╚═══════════════════════════════════════════════════════╝
```

## 🔄 Migración desde PowerShell

Si anteriormente usabas `dev-commands.ps1`:

| PowerShell (Windows only)             | Node.js (Multiplataforma)                |
| ------------------------------------- | ---------------------------------------- |
| `. .\dev-commands.ps1`<br>`Start-Api` | `npm start`<br>o `npm run dev start-api` |
| `Build-Api`                           | `npm run build`                          |
| `Clean-Build`                         | `npm run dev clean-build`                |
| `Start-Docker`                        | `npm run docker:up`                      |
| `Stop-Docker`                         | `npm run docker:down`                    |
| `Show-Logs`                           | `npm run docker:logs`                    |

## 🚀 Inicio Rápido

1. **Instalar dependencias:**

   ```bash
   npm install
   npm run restore
   ```

2. **Iniciar desarrollo:**

   ```bash
   npm start
   ```

3. **Con Docker:**
   ```bash
   npm run docker:up
   ```

## 🐛 Troubleshooting

### Node.js no encontrado

```bash
# Verificar instalación
node --version

# Debe mostrar v18+ o superior
```

### .NET no encontrado

```bash
# Verificar instalación
dotnet --version

# Debe mostrar 10.0+
```

### Docker no encontrado

```bash
# Verificar instalación
docker --version
docker-compose --version
```

## 💡 Consejos

- **Usar `npm run dev` sin argumentos** para ver el menú de ayuda
- **Interrumpir procesos con Ctrl+C** (se maneja gracefully)
- **Todos los comandos npm** también funcionan directamente con el script:
  ```bash
  node dev-commands.js <comando>
  ```

## 📝 Añadir Nuevos Comandos

Edita `dev-commands.js` y agrega en el objeto `commands`:

```javascript
'mi-comando': {
  description: 'Descripción del comando',
  emoji: '🎯',
  async run() {
    log('Ejecutando mi comando...', colors.green);
    await exec('comando', ['arg1', 'arg2']);
  },
},
```

---

**Nota:** El archivo `dev-commands.ps1` se mantiene para compatibilidad con scripts legacy de Windows, pero se recomienda usar `dev-commands.js` para desarrollo multiplataforma.
