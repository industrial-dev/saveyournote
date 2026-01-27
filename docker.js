#!/usr/bin/env node
/**
 * SaveYourNote Docker Manager
 * Script multiplataforma para gestionar los servicios de Docker
 */

const { spawn } = require('child_process');
const { existsSync, copyFileSync, readFileSync } = require('fs');

// Colores para la consola
const colors = {
  reset: '\x1b[0m',
  green: '\x1b[32m',
  blue: '\x1b[34m',
  yellow: '\x1b[33m',
  red: '\x1b[31m'
};

// Funciones de logging
const info = (msg) => console.log(`${colors.blue}ℹ️  ${msg}${colors.reset}`);
const success = (msg) => console.log(`${colors.green}✅ ${msg}${colors.reset}`);
const warn = (msg) => console.log(`${colors.yellow}⚠️  ${msg}${colors.reset}`);
const error = (msg) => console.log(`${colors.red}❌ ${msg}${colors.reset}`);

// Argumentos de línea de comandos
const command = process.argv[2];
const env = process.argv[3] || 'development';
const service = process.argv[3];

// Verificar archivo .env
function checkEnv() {
  if (env !== 'production' && env !== 'prod') {
    if (!existsSync('.env')) {
      warn('.env no encontrado, copiando desde .env.example...');
      if (existsSync('.env.example')) {
        copyFileSync('.env.example', '.env');
      } else {
        error('No se encontró .env.example');
        process.exit(1);
      }
    }

    // Leer y verificar NGROK_AUTHTOKEN
    const envContent = readFileSync('.env', 'utf-8');
    const ngrokToken = envContent.match(/NGROK_AUTHTOKEN=(.+)/);
    
    if (!ngrokToken || !ngrokToken[1].trim()) {
      error('NGROK_AUTHTOKEN no configurado en .env');
      info('Token en: https://dashboard.ngrok.com/get-started/your-authtoken');
      process.exit(1);
    }
  }
}

// Mostrar URL de ngrok
async function showUrl() {
  if (env === 'production' || env === 'prod') {
    return;
  }

  console.log('');
  success('SaveYourNote iniciado');
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
  console.log('📱 API Local: http://localhost:5001');
  console.log('📚 Swagger: http://localhost:5001/swagger');
  console.log('❤️  Health: http://localhost:5001/health');
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');

  info('Esperando a que ngrok esté listo...');

  // Intentar obtener la URL de ngrok con reintentos
  const http = require('http');
  const maxRetries = 20;
  let retries = 0;

  while (retries < maxRetries) {
    try {
      await new Promise((resolve, reject) => {
        const options = {
          hostname: 'localhost',
          port: 4040,
          path: '/api/tunnels',
          method: 'GET'
        };

        const req = http.request(options, (res) => {
          let data = '';
          res.on('data', (chunk) => { data += chunk; });
          res.on('end', () => {
            try {
              const tunnels = JSON.parse(data);
              if (tunnels.tunnels && tunnels.tunnels.length > 0) {
                const url = tunnels.tunnels[0].public_url;
                console.log('');
                success('ngrok corriendo');
                console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
                console.log(`🌐 URL Pública: ${url}`);
                console.log(`🪝 Webhook: ${url}/api/whatsapp/webhook`);
                console.log('🎛️  Panel ngrok: http://localhost:4040');
                console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
                resolve(true);
              } else {
                reject(new Error('No tunnels yet'));
              }
            } catch (e) {
              reject(e);
            }
          });
        });

        req.on('error', reject);
        req.end();
      });

      return; // Éxito, salir de la función
    } catch (e) {
      retries++;
      if (retries < maxRetries) {
        await new Promise(resolve => setTimeout(resolve, 1000)); // Esperar 1 segundo antes de reintentar
      }
    }
  }

  warn('No se pudo obtener la URL de ngrok. Verifica el panel en http://localhost:4040');
}

// Ejecutar comando de docker compose
function runDockerCompose(args) {
  return new Promise((resolve, reject) => {
    const cmd = 'docker';
    const fullArgs = ['compose', ...args];

    const proc = spawn(cmd, fullArgs, {
      stdio: 'inherit',
      shell: false
    });

    proc.on('close', (code) => {
      if (code === 0) {
        resolve();
      } else {
        reject(new Error(`Proceso terminó con código ${code}`));
      }
    });

    proc.on('error', (err) => {
      reject(err);
    });
  });
}

// Comandos principales
async function start() {
  try {
    info('Iniciando SaveYourNote...');
    checkEnv();

    if (env === 'production' || env === 'prod') {
      await runDockerCompose(['-f', 'docker-compose.yml', '-f', 'docker-compose.prod.yml', 'up', '-d']);
      success('Iniciado en PRODUCCIÓN');
    } else {
      // En desarrollo: iniciar API y ngrok (con profile development)
      await runDockerCompose(['--profile', 'development', 'up', '-d', 'api', 'ngrok']);
      success('Iniciado en DESARROLLO');
      await showUrl();
    }
  } catch (err) {
    error(`Error al iniciar: ${err.message}`);
    process.exit(1);
  }
}

async function stop() {
  try {
    info('Deteniendo...');
    
    if (env === 'production' || env === 'prod') {
      await runDockerCompose(['-f', 'docker-compose.yml', '-f', 'docker-compose.prod.yml', 'down']);
    } else {
      await runDockerCompose(['down']);
    }
    
    success('Detenido');
  } catch (err) {
    error(`Error al detener: ${err.message}`);
    process.exit(1);
  }
}

async function logs() {
  try {
    const serviceArgs = service ? [service] : [];
    await runDockerCompose(['logs', '-f', ...serviceArgs]);
  } catch (err) {
    error(`Error al ver logs: ${err.message}`);
    process.exit(1);
  }
}

async function status() {
  try {
    await runDockerCompose(['ps']);
  } catch (err) {
    error(`Error al obtener estado: ${err.message}`);
    process.exit(1);
  }
}

// Menú de ayuda
function showHelp() {
  console.log('Uso: node docker.js [comando] [entorno]');
  console.log('');
  console.log('Comandos:');
  console.log('  start [dev|prod] - Iniciar');
  console.log('  stop [dev|prod]  - Detener');
  console.log('  logs [service]   - Ver logs');
  console.log('  status           - Estado');
  console.log('  url              - Mostrar URL ngrok');
}

// Ejecutar comando
(async () => {
  switch (command) {
    case 'start':
      await start();
      break;
    case 'stop':
      await stop();
      break;
    case 'logs':
      await logs();
      break;
    case 'status':
      await status();
      break;
    case 'url':
      await showUrl();
      break;
    default:
      showHelp();
      break;
  }
})();
