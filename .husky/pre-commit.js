#!/usr/bin/env node

const { execSync } = require('child_process');
const path = require('path');

// Colores para la consola
const colors = {
  reset: '\x1b[0m',
  cyan: '\x1b[36m',
  yellow: '\x1b[33m',
  green: '\x1b[32m',
  red: '\x1b[31m',
};

function log(message, color = colors.reset) {
  console.log(`${color}${message}${colors.reset}`);
}

function exec(command, description) {
  try {
    log(description, colors.yellow);
    execSync(command, { stdio: 'inherit', cwd: path.resolve(__dirname, '..') });
    return true;
  } catch (error) {
    return false;
  }
}

function main() {
  log('\n🔍 Ejecutando pre-commit checks...\n', colors.cyan);

  // 1. Formatear código con CSharpier
  log('🎨 Formateando código con CSharpier...', colors.yellow);
  if (!exec('dotnet csharpier format .', '')) {
    log('❌ Error al formatear el código', colors.red);
    process.exit(1);
  }
  log('✅ Código formateado correctamente\n', colors.green);

  // 2. Agregar archivos formateados al staging area
  log('📝 Agregando archivos formateados...', colors.yellow);
  exec('git add -u', '');
  log('✅ Archivos agregados\n', colors.green);

  // 3. Limpiar proyecto
  log('🧹 Limpiando proyecto...', colors.yellow);
  exec('dotnet clean --verbosity quiet', '');
  log('✅ Proyecto limpiado\n', colors.green);

  // 4. Compilar proyecto
  log('🔨 Compilando proyecto...', colors.yellow);
  if (!exec('dotnet build --no-incremental --verbosity quiet', '')) {
    log('❌ Error al compilar el proyecto', colors.red);
    log('   Por favor, corrija los errores antes de hacer commit\n', colors.red);
    process.exit(1);
  }
  log('✅ Proyecto compilado correctamente\n', colors.green);

  log('🎉 Pre-commit checks completados exitosamente!\n', colors.green);
  process.exit(0);
}

main();
