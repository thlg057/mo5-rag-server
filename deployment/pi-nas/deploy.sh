#!/bin/bash
# Script de déploiement pour Raspberry Pi NAS avec OpenMediaVault
# Usage: ./deploy.sh [build|start|stop|restart|logs|status]

set -e

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
COMPOSE_FILE="docker-compose.yml"
DOCKER_DATA_DIR="/srv/dev-disk-by-uuid-8a747308-0fd1-4223-b1be-09ccfdf4bad1/docker/mo5-rag"

# Fonction pour afficher les messages
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Déterminer la commande Docker Compose à utiliser
DOCKER_COMPOSE_CMD=""
DOCKER_CMD="docker"

get_docker_compose_cmd() {
    # Vérifier si l'utilisateur a accès à Docker sans sudo
    if ! docker ps &> /dev/null; then
        if groups | grep -q docker; then
            log_error "Vous êtes dans le groupe docker mais la session n'est pas à jour. Déconnectez-vous et reconnectez-vous."
            exit 1
        else
            log_warn "L'utilisateur n'est pas dans le groupe docker. Utilisation de sudo."
            DOCKER_CMD="sudo docker"
        fi
    fi

    if command -v docker-compose &> /dev/null; then
        DOCKER_COMPOSE_CMD="$DOCKER_CMD-compose"
    elif $DOCKER_CMD compose version &> /dev/null 2>&1; then
        DOCKER_COMPOSE_CMD="$DOCKER_CMD compose"
    else
        log_error "Docker Compose n'est pas installé"
        exit 1
    fi
}

# Vérifier que Docker est installé
check_docker() {
    if ! command -v docker &> /dev/null; then
        log_error "Docker n'est pas installé"
        exit 1
    fi

    get_docker_compose_cmd

    log_info "Docker et Docker Compose sont installés ($DOCKER_COMPOSE_CMD)"
}

# Créer les répertoires nécessaires
create_directories() {
    log_info "Création des répertoires de données..."
    sudo mkdir -p "$DOCKER_DATA_DIR/postgres-data"
    sudo chown -R 999:999 "$DOCKER_DATA_DIR/postgres-data"  # UID/GID de postgres
    log_info "Répertoires créés: $DOCKER_DATA_DIR"
}

# Compiler les images Docker
build() {
    log_info "Compilation de l'image Docker pour ARM64..."
    log_warn "Cette opération peut prendre 10-20 minutes sur Raspberry Pi..."

    $DOCKER_COMPOSE_CMD -f "$COMPOSE_FILE" build --no-cache

    log_info "✓ Compilation terminée avec succès"
}

# Démarrer les services
start() {
    log_info "Démarrage des services..."

    create_directories

    $DOCKER_COMPOSE_CMD -f "$COMPOSE_FILE" up -d

    log_info "✓ Services démarrés"
    log_info "Attente de l'initialisation (40 secondes)..."
    sleep 40

    status
}

# Arrêter les services
stop() {
    log_info "Arrêt des services..."
    $DOCKER_COMPOSE_CMD -f "$COMPOSE_FILE" down
    log_info "✓ Services arrêtés"
}

# Redémarrer les services
restart() {
    stop
    start
}

# Afficher les logs
logs() {
    $DOCKER_COMPOSE_CMD -f "$COMPOSE_FILE" logs -f --tail=100
}

# Afficher le statut
status() {
    log_info "Statut des services:"
    $DOCKER_COMPOSE_CMD -f "$COMPOSE_FILE" ps
    
    echo ""
    log_info "Vérification de la santé de l'API..."
    
    if curl -f http://localhost:8080/health 2>/dev/null; then
        log_info "✓ API est en ligne et répond correctement"
        echo ""
        log_info "Accès à l'API: http://$(hostname -I | awk '{print $1}'):8080"
        log_info "Documentation: http://$(hostname -I | awk '{print $1}'):8080/swagger"
    else
        log_warn "L'API ne répond pas encore (peut nécessiter plus de temps)"
    fi
}

# Nettoyer tout (ATTENTION: supprime les données)
clean() {
    log_warn "ATTENTION: Cette opération va supprimer tous les conteneurs et les données!"
    read -p "Êtes-vous sûr? (yes/no): " -r
    if [[ $REPLY == "yes" ]]; then
        $DOCKER_COMPOSE_CMD -f "$COMPOSE_FILE" down -v
        sudo rm -rf "$DOCKER_DATA_DIR"
        log_info "✓ Nettoyage terminé"
    else
        log_info "Opération annulée"
    fi
}

# Menu principal
case "${1:-help}" in
    build)
        check_docker
        build
        ;;
    start)
        check_docker
        start
        ;;
    stop)
        check_docker
        stop
        ;;
    restart)
        check_docker
        restart
        ;;
    logs)
        check_docker
        logs
        ;;
    status)
        check_docker
        status
        ;;
    clean)
        check_docker
        clean
        ;;
    help|*)
        echo "Usage: $0 {build|start|stop|restart|logs|status|clean}"
        echo ""
        echo "Commandes:"
        echo "  build   - Compiler l'image Docker (première fois ou après modifications)"
        echo "  start   - Démarrer les services"
        echo "  stop    - Arrêter les services"
        echo "  restart - Redémarrer les services"
        echo "  logs    - Afficher les logs en temps réel"
        echo "  status  - Afficher le statut des services"
        echo "  clean   - Nettoyer tout (SUPPRIME LES DONNÉES)"
        echo ""
        echo "Déploiement initial:"
        echo "  1. ./deploy-nas.sh build"
        echo "  2. ./deploy-nas.sh start"
        echo "  3. ./deploy-nas.sh status"
        ;;
esac

