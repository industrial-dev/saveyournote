#!/bin/bash

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
NC='\033[0m' # No Color

show_menu() {
    echo ""
    echo -e "${CYAN}╔═══════════════════════════════════════════════════════╗${NC}"
    echo -e "${CYAN}║         SaveYourNote API - Comandos                   ║${NC}"
    echo -e "${CYAN}╠═══════════════════════════════════════════════════════╣${NC}"
    echo -e "${CYAN}║  🚀 start-api      → Ejecutar la API localmente       ║${NC}"
    echo -e "${CYAN}║  🔨 build          → Compilar el proyecto             ║${NC}"
    echo -e "${CYAN}║  🧹 clean-build    → Limpiar y recompilar             ║${NC}"
    echo -e "${CYAN}║  🐳 docker-up      → Ejecutar con Docker Compose      ║${NC}"
    echo -e "${CYAN}║  🛑 docker-down    → Detener contenedores de Docker   ║${NC}"
    echo -e "${CYAN}║  📋 docker-logs    → Ver logs de Docker               ║${NC}"
    echo -e "${CYAN}║  🎨 format         → Formatear código con CSharpier   ║${NC}"
    echo -e "${CYAN}║  🔍 format-check   → Verificar formato del código     ║${NC}"
    echo -e "${CYAN}║  🧪 test           → Ejecutar tests                   ║${NC}"
    echo -e "${CYAN}║  📦 restore        → Restaurar dependencias           ║${NC}"
    echo -e "${CYAN}╚═══════════════════════════════════════════════════════╝${NC}"
    echo ""
    echo -e "${YELLOW}Uso:${NC}"
    echo "  ./dev-commands.sh <comando>"
    echo ""
    echo -e "${YELLOW}Ejemplo:${NC}"
    echo -e "  ${GREEN}./dev-commands.sh start-api${NC}"
    echo ""
}

start_api() {
    echo -e "${GREEN}🚀 Iniciando SaveYourNote API...${NC}"
    cd src/SaveYourNote.Api && dotnet run
}

build_project() {
    echo -e "${YELLOW}🔨 Compilando proyecto...${NC}"
    dotnet build
}

clean_build() {
    echo -e "${CYAN}🧹 Limpiando proyecto...${NC}"
    dotnet clean
    echo -e "${YELLOW}🔨 Compilando...${NC}"
    dotnet build
}

docker_up() {
    echo -e "${BLUE}🐳 Iniciando con Docker Compose...${NC}"
    docker-compose up --build
}

docker_down() {
    echo -e "${RED}🛑 Deteniendo contenedores...${NC}"
    docker-compose down
}

docker_logs() {
    echo -e "${MAGENTA}📋 Mostrando logs...${NC}"
    docker-compose logs -f
}

format_code() {
    echo -e "${YELLOW}🎨 Formateando código...${NC}"
    dotnet csharpier format .
}

format_check() {
    echo -e "${CYAN}🔍 Verificando formato...${NC}"
    dotnet csharpier check .
}

run_tests() {
    echo -e "${GREEN}🧪 Ejecutando tests...${NC}"
    dotnet test
}

restore_deps() {
    echo -e "${CYAN}📦 Restaurando dependencias...${NC}"
    dotnet restore
    dotnet tool restore
}

# Main script
case "$1" in
    start-api)
        start_api
        ;;
    build)
        build_project
        ;;
    clean-build)
        clean_build
        ;;
    docker-up)
        docker_up
        ;;
    docker-down)
        docker_down
        ;;
    docker-logs)
        docker_logs
        ;;
    format)
        format_code
        ;;
    format-check)
        format_check
        ;;
    test)
        run_tests
        ;;
    restore)
        restore_deps
        ;;
    help|--help|-h|"")
        show_menu
        ;;
    *)
        echo -e "${RED}❌ Comando desconocido: $1${NC}"
        show_menu
        exit 1
        ;;
esac

if [ $? -eq 0 ]; then
    echo ""
    echo -e "${GREEN}✅ Comando completado exitosamente!${NC}"
else
    echo ""
    echo -e "${RED}❌ Error al ejecutar el comando${NC}"
    exit 1
fi
