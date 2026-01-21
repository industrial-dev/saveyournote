# Pre-commit Hook Configuration

## 🎯 ¿Qué hace?

El hook de pre-commit ejecuta automáticamente antes de cada commit y realiza:

1. **🎨 Formateo** - Formatea todo el código C# con CSharpier
2. **📝 Staging** - Agrega los archivos formateados al commit
3. **🧹 Limpieza** - Limpia el proyecto de archivos temporales
4. **🔨 Compilación** - Verifica que el proyecto compila sin errores

Si algún paso falla, el commit se cancela automáticamente.

## ✅ Multiplataforma

El script está escrito en **Node.js**, por lo que funciona en:

- ✅ Windows
- ✅ macOS
- ✅ Linux

## 📋 Requisitos

- Node.js (ya lo tienes si instalaste Husky)
- .NET SDK 10.0
- Git

## 🚀 Instalación

Ya está todo configurado. Los hooks se instalan automáticamente cuando ejecutas:

```bash
npm install
```

## 🧪 Probar manualmente

Puedes ejecutar el script de pre-commit sin hacer commit:

```bash
# Usando Node.js directamente
node .husky/pre-commit.js

# O usando npm
npm run pre-commit
```

## ⚙️ Archivos configurados

- `.husky/pre-commit` - Hook de Git
- `.husky/pre-commit.js` - Script multiplataforma en Node.js
- `.husky/pre-commit.ps1` - Script de PowerShell (legacy, solo Windows)
- `.csharpierrc.json` - Configuración de CSharpier
- `dotnet-tools.json` - CSharpier como herramienta local

## 🔧 Configuración de CSharpier

El archivo `.csharpierrc.json` contiene la configuración del formateador:

```json
{
  "printWidth": 100,
  "useTabs": false,
  "tabWidth": 4,
  "endOfLine": "lf"
}
```

## 🛠️ Personalización

Para modificar el comportamiento del hook, edita `.husky/pre-commit.js`:

```javascript
// Ejemplo: Desactivar la compilación en pre-commit
// Comenta o elimina esta sección:
if (!exec("dotnet build --no-incremental --verbosity quiet", "")) {
  log("❌ Error al compilar el proyecto", colors.red);
  process.exit(1);
}
```

## 🚫 Saltarse el hook (no recomendado)

En casos excepcionales, puedes saltarte el hook con:

```bash
git commit --no-verify -m "mensaje"
```

**⚠️ Advertencia:** Esto puede resultar en código sin formatear o que no compile en el repositorio.

## 📊 Salida esperada

```
🔍 Ejecutando pre-commit checks...

🎨 Formateando código con CSharpier...
Formatted 5 files in 324ms.
✅ Código formateado correctamente

📝 Agregando archivos formateados...
✅ Archivos agregados

🧹 Limpiando proyecto...
✅ Proyecto limpiado

🔨 Compilando proyecto...
✅ Proyecto compilado correctamente

🎉 Pre-commit checks completados exitosamente!
```

## 🐛 Troubleshooting

### El hook no se ejecuta

```bash
# Reinstalar los hooks
npm run prepare
```

### Error de permisos en Linux/Mac

```bash
# Dar permisos de ejecución
chmod +x .husky/pre-commit
chmod +x .husky/pre-commit.js
```

### CSharpier no se encuentra

```bash
# Restaurar herramientas locales
dotnet tool restore
```
