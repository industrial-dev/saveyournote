#!/usr/bin/env node

const { spawn } = require('child_process');
const path = require('path');
const readline = require('readline');

// Colores para la consola
const colors = {
  reset: '\x1b[0m',
  cyan: '\x1b[36m',
  yellow: '\x1b[33m',
  green: '\x1b[32m',
  red: '\x1b[31m',
  blue: '\x1b[34m',
  magenta: '\x1b[35m',
};

function log(message, color = colors.reset) {
  console.log(`${color}${message}${colors.reset}`);
}

function exec(command, args = [], options = {}) {
  return new Promise((resolve, reject) => {
    // En Windows, algunos comandos necesitan shell, pero sin pasar args directamente
    const isWindows = process.platform === 'win32';
    const needsShell = ['docker-compose', 'npm'].includes(command);
    
    const spawnOptions = {
      stdio: 'inherit',
      cwd: options.cwd || process.cwd(),
      ...options,
    };

    // Solo usar shell cuando es absolutamente necesario y de forma segura
    if (isWindows && needsShell) {
      spawnOptions.shell = true;
    }

    const child = spawn(command, args, spawnOptions);

    child.on('close', (code) => {
      if (code === 0) {
        resolve();
      } else {
        reject(new Error(`Process exited with code ${code}`));
      }
    });

    child.on('error', (err) => {
      reject(err);
    });
  });
}

const commands = {
  'start-api': {
    description: 'Ejecutar la API localmente',
    emoji: '🚀',
    async run() {
      log('Iniciando SaveYourNote API...', colors.green);
      await exec('dotnet', ['run'], {
        cwd: path.join(process.cwd(), 'src', 'SaveYourNote.Api'),
      });
    },
  },
  'build': {
    description: 'Compilar el proyecto',
    emoji: '🔨',
    async run() {
      log('Compilando proyecto...', colors.yellow);
      await exec('dotnet', ['build']);
    },
  },
  'clean-build': {
    description: 'Limpiar y recompilar',
    emoji: '🧹',
    async run() {
      log('Limpiando proyecto...', colors.cyan);
      await exec('dotnet', ['clean']);
      log('Compilando...', colors.yellow);
      await exec('dotnet', ['build']);
    },
  },
  'docker-up': {
    description: 'Ejecutar con Docker Compose',
    emoji: '🐳',
    async run() {
      log('Iniciando con Docker Compose...', colors.blue);
      await exec('docker-compose', ['up', '--build']);
    },
  },
  'docker-down': {
    description: 'Detener contenedores de Docker',
    emoji: '🛑',
    async run() {
      log('Deteniendo contenedores...', colors.red);
      await exec('docker-compose', ['down']);
    },
  },
  'docker-logs': {
    description: 'Ver logs de Docker',
    emoji: '📋',
    async run() {
      log('Mostrando logs...', colors.magenta);
      await exec('docker-compose', ['logs', '-f']);
    },
  },
  'format': {
    description: 'Formatear código con CSharpier',
    emoji: '🎨',
    async run() {
      log('Formateando código...', colors.yellow);
      await exec('dotnet', ['csharpier', 'format', '.']);
    },
  },
  'format-check': {
    description: 'Verificar formato del código',
    emoji: '🔍',
    async run() {
      log('Verificando formato...', colors.cyan);
      await exec('dotnet', ['csharpier', 'check', '.']);
    },
  },
  'test': {
    description: 'Ejecutar tests',
    emoji: '🧪',
    async run() {
      log('Ejecutando tests...', colors.green);
      await exec('dotnet', ['test']);
    },
  },
  'restore': {
    description: 'Restaurar dependencias',
    emoji: '📦',
    async run() {
      log('Restaurando dependencias...', colors.cyan);
      await exec('dotnet', ['restore']);
      await exec('dotnet', ['tool', 'restore']);
    },
  },
};

function showMenu() {
  console.log('\n');
  log('╔═══════════════════════════════════════════════════════╗', colors.cyan);
  log('║         SaveYourNote API - Comandos                   ║', colors.cyan);
  log('╠═══════════════════════════════════════════════════════╣', colors.cyan);
  
  Object.entries(commands).forEach(([key, cmd]) => {
    const padding = ' '.repeat(Math.max(0, 15 - key.length));
    log(`║  ${cmd.emoji} ${key}${padding}→ ${cmd.description.padEnd(30)} ║`, colors.cyan);
  });
  
  log('╚═══════════════════════════════════════════════════════╝', colors.cyan);
  console.log('\n');
  log('Uso:', colors.yellow);
  log('  node dev-commands.js <comando>', colors.white);
  log('  npm run dev <comando>', colors.white);
  console.log('\n');
  log('Ejemplo:', colors.yellow);
  log('  node dev-commands.js start-api', colors.green);
  log('  npm run dev start-api', colors.green);
  console.log('\n');
}

async function main() {
  const args = process.argv.slice(2);
  const commandName = args[0];

  if (!commandName || commandName === 'help' || commandName === '--help' || commandName === '-h') {
    showMenu();
    return;
  }

  const command = commands[commandName];

  if (!command) {
    log(`❌ Comando desconocido: ${commandName}`, colors.red);
    showMenu();
    process.exit(1);
  }

  try {
    await command.run();
    log(`\n✅ Comando completado exitosamente!`, colors.green);
  } catch (error) {
    log(`\n❌ Error: ${error.message}`, colors.red);
    process.exit(1);
  }
}

// Manejar Ctrl+C gracefully
process.on('SIGINT', () => {
  log('\n\n👋 Proceso interrumpido por el usuario', colors.yellow);
  process.exit(0);
});

main();
