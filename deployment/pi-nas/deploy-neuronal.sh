#!/bin/bash

# Script de déploiement du RAG Server avec modèle neuronal (Sentence Transformers)
# Pour Raspberry Pi NAS

set -e  # Arrêter en cas d'erreur

echo "🧠 Déploiement du RAG Server avec modèle neuronal"
echo "=================================================="
echo ""

# Couleurs pour les messages
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Fonction pour afficher les messages
info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Vérifier que le script est exécuté depuis le bon répertoire
if [ ! -f "docker-compose.neuronal.yml" ]; then
    error "Ce script doit être exécuté depuis le répertoire deployment/pi-nas/"
    exit 1
fi

# Étape 1 : Vérifier Python
info "Vérification de Python..."
if command -v python3 &> /dev/null; then
    PYTHON_VERSION=$(python3 --version)
    info "Python trouvé : $PYTHON_VERSION"
else
    error "Python 3 n'est pas installé !"
    echo ""
    echo "Installez Python avec :"
    echo "  sudo apt update && sudo apt install -y python3 python3-pip python3-venv"
    exit 1
fi

# Étape 2 : Vérifier l'espace disque
info "Vérification de l'espace disque..."
AVAILABLE_SPACE=$(df -BG . | tail -1 | awk '{print $4}' | sed 's/G//')
if [ "$AVAILABLE_SPACE" -lt 2 ]; then
    warn "Espace disque faible : ${AVAILABLE_SPACE}G disponible"
    warn "Le modèle nécessite ~500 MB + dépendances"
    read -p "Continuer quand même ? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
else
    info "Espace disque OK : ${AVAILABLE_SPACE}G disponible"
fi

# Étape 3 : Créer le répertoire de cache pour HuggingFace
info "Création du répertoire de cache pour le modèle..."
CACHE_DIR="/srv/dev-disk-by-uuid-8a747308-0fd1-4223-b1be-09ccfdf4bad1/docker/mo5-rag/huggingface-cache"
sudo mkdir -p "$CACHE_DIR"
sudo chown -R $(whoami):$(whoami) "$CACHE_DIR"
info "Répertoire de cache créé : $CACHE_DIR"

# Étape 4 : Arrêter les conteneurs existants
info "Arrêt des conteneurs existants..."
if sudo docker-compose -f docker-compose.yml ps -q 2>/dev/null | grep -q .; then
    sudo docker-compose -f docker-compose.yml down
    info "Conteneurs arrêtés"
else
    info "Aucun conteneur en cours d'exécution"
fi

# Étape 5 : Construire l'image avec Python
info "Construction de l'image Docker avec Python..."
warn "⚠️  Cette étape peut prendre 10-20 minutes (téléchargement de Python + dépendances)"
echo ""

if sudo docker-compose -f docker-compose.neuronal.yml build --no-cache; then
    info "Image construite avec succès !"
else
    error "Échec de la construction de l'image"
    exit 1
fi

# Étape 6 : Démarrer les conteneurs
info "Démarrage des conteneurs..."
if sudo docker-compose -f docker-compose.neuronal.yml up -d; then
    info "Conteneurs démarrés !"
else
    error "Échec du démarrage des conteneurs"
    exit 1
fi

# Étape 7 : Attendre que l'API soit prête
info "Attente du démarrage de l'API..."
warn "⚠️  Le premier démarrage peut prendre 2-5 minutes (téléchargement du modèle ~420 MB)"
echo ""

MAX_WAIT=300  # 5 minutes
WAIT_TIME=0
INTERVAL=10

while [ $WAIT_TIME -lt $MAX_WAIT ]; do
    if curl -s http://localhost:8080/health > /dev/null 2>&1; then
        info "API démarrée avec succès !"
        break
    fi
    
    echo -n "."
    sleep $INTERVAL
    WAIT_TIME=$((WAIT_TIME + INTERVAL))
done

echo ""

if [ $WAIT_TIME -ge $MAX_WAIT ]; then
    error "Timeout : L'API n'a pas démarré dans les 5 minutes"
    echo ""
    echo "Vérifiez les logs avec :"
    echo "  sudo docker-compose -f docker-compose.neuronal.yml logs api"
    exit 1
fi

# Étape 8 : Vérifier les logs
info "Dernières lignes des logs :"
echo ""
sudo docker-compose -f docker-compose.neuronal.yml logs --tail=20 api

# Étape 9 : Test de santé
info "Test de santé de l'API..."
HEALTH_RESPONSE=$(curl -s http://localhost:8080/health)
echo "Réponse : $HEALTH_RESPONSE"

# Étape 10 : Résumé
echo ""
echo "=================================================="
info "✅ Déploiement terminé avec succès !"
echo "=================================================="
echo ""
echo "📊 Informations :"
echo "  - API : http://localhost:8080"
echo "  - Swagger : http://localhost:8080/swagger"
echo "  - Provider : Sentence Transformers (modèle neuronal)"
echo "  - Modèle : paraphrase-multilingual-MiniLM-L12-v2"
echo ""
echo "🔧 Commandes utiles :"
echo "  - Voir les logs : sudo docker-compose -f docker-compose.neuronal.yml logs -f api"
echo "  - Arrêter : sudo docker-compose -f docker-compose.neuronal.yml down"
echo "  - Redémarrer : sudo docker-compose -f docker-compose.neuronal.yml restart api"
echo ""
echo "🧪 Test de recherche :"
echo "  curl \"http://localhost:8080/api/search?q=registre&maxResults=3\""
echo ""

