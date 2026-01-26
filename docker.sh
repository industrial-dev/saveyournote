#!/bin/bash
# SaveYourNote Docker Manager
set -e

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

ENV=${2:-development}

info() { echo -e "${BLUE}ℹ️  $1${NC}"; }
success() { echo -e "${GREEN}✅ $1${NC}"; }
warn() { echo -e "${YELLOW}⚠️  $1${NC}"; }
error() { echo -e "${RED}❌ $1${NC}"; }

check_env() {
    if [ "$ENV" != "production" ] && [ "$ENV" != "prod" ]; then
        if [ ! -f .env ]; then
            warn ".env no encontrado, copiando desde .env.example..."
            cp .env.example .env
        fi
        source .env
        if [ -z "$NGROK_AUTHTOKEN" ]; then
            error "NGROK_AUTHTOKEN no configurado en .env"
            info "Token en: https://dashboard.ngrok.com/get-started/your-authtoken"
            exit 1
        fi
    fi
}

show_url() {
    if [ "$ENV" = "production" ] || [ "$ENV" = "prod" ]; then
        return
    fi
    sleep 5
    echo ""
    success "SaveYourNote iniciado"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "📱 API Local: http://localhost:5001"
    echo "📚 Swagger: http://localhost:5001/swagger"
    echo "❤️  Health: http://localhost:5001/health"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    if command -v jq >/dev/null 2>&1; then
        URL=$(curl -s http://localhost:4040/api/tunnels 2>/dev/null | jq -r '.tunnels[0].public_url // empty')
    else
        warn "jq no está instalado; no se puede obtener la URL pública de ngrok"
        return
    fi
    if [ -n "$URL" ]; then
        echo ""
        success "ngrok corriendo"
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo "🌐 URL Pública: $URL"
        echo "🪝 Webhook: ${URL}/api/whatsapp/webhook"
        echo "🎛️  Panel ngrok: http://localhost:4040"
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    fi
}

case "$1" in
    start)
        info "Iniciando SaveYourNote..."
        check_env
        if [ "$ENV" = "production" ] || [ "$ENV" = "prod" ]; then
            docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
            success "Iniciado en PRODUCCIÓN"
        else
            docker compose --profile development up -d
            success "Iniciado en DESARROLLO"
            show_url
        fi
        ;;
    stop)
        info "Deteniendo..."
        if [ "$ENV" = "production" ] || [ "$ENV" = "prod" ]; then
            docker compose -f docker-compose.yml -f docker-compose.prod.yml down
        else
            docker compose down
        fi
        success "Detenido"
        ;;
    logs)
        docker compose logs -f ${3:-}
        ;;
    status)
        docker compose ps
        ;;
    url)
        show_url
        ;;
    *)
        echo "Uso: ./docker.sh [comando] [entorno]"
        echo ""
        echo "Comandos:"
        echo "  start [dev|prod] - Iniciar"
        echo "  stop [dev|prod]  - Detener"
        echo "  logs [service]   - Ver logs"
        echo "  status           - Estado"
        echo "  url              - Mostrar URL ngrok"
        ;;
esac
