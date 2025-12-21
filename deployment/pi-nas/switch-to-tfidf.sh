#!/bin/bash

# Script pour basculer vers le mode TF-IDF (rapide)
# Usage: ./switch-to-tfidf.sh

set -e  # Arrêter en cas d'erreur

echo "🔄 Basculement vers le mode TF-IDF (rapide)..."
echo ""

# Couleurs pour l'affichage
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Répertoire du script
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

echo -e "${BLUE}📍 Répertoire de travail: $SCRIPT_DIR${NC}"
echo ""

# Étape 1: Arrêter le mode neuronal
echo -e "${YELLOW}[1/5] Arrêt du mode neuronal...${NC}"
sudo docker compose -f docker-compose.neuronal.yml down
echo -e "${GREEN}✅ Mode neuronal arrêté${NC}"
echo ""

# Étape 2: Démarrer le mode TF-IDF
echo -e "${YELLOW}[2/5] Démarrage du mode TF-IDF...${NC}"
sudo docker compose up -d
echo -e "${GREEN}✅ Mode TF-IDF démarré${NC}"
echo ""

# Étape 3: Attendre que l'API démarre
echo -e "${YELLOW}[3/5] Attente du démarrage de l'API (10 secondes)...${NC}"
sleep 10

# Vérifier que l'API est en ligne
MAX_RETRIES=12
RETRY_COUNT=0
while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    if curl -s http://localhost:8080/health > /dev/null 2>&1; then
        echo -e "${GREEN}✅ API démarrée et opérationnelle${NC}"
        break
    fi
    RETRY_COUNT=$((RETRY_COUNT + 1))
    if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
        echo -e "${YELLOW}⚠️  L'API ne répond pas encore, mais on continue...${NC}"
    else
        echo "   Tentative $RETRY_COUNT/$MAX_RETRIES..."
        sleep 5
    fi
done
echo ""

# Étape 4: Supprimer les anciens chunks (embeddings neuronaux)
echo -e "${YELLOW}[4/5] Suppression des anciens chunks (embeddings neuronaux)...${NC}"
sudo docker exec -it mo5-rag-postgres psql -U mo5_user -d mo5_rag -c 'DELETE FROM "DocumentChunks"; DELETE FROM "DocumentTags"; DELETE FROM "Documents";'
echo -e "${GREEN}✅ Anciens chunks supprimés${NC}"
echo ""

# Étape 5: Ré-indexer avec TF-IDF
echo -e "${YELLOW}[5/5] Ré-indexation avec TF-IDF (prend ~10 secondes)...${NC}"
echo "   Lancement de l'indexation..."
curl -X POST -s "http://localhost:8080/api/index/all" > /dev/null 2>&1 &
INDEXING_PID=$!

# Attendre un peu pour que l'indexation démarre
sleep 2

# Afficher les logs en temps réel
echo "   Logs de l'indexation:"
sudo docker compose logs -f --tail=20 api &
LOGS_PID=$!

# Attendre la fin de l'indexation (max 60 secondes)
WAIT_COUNT=0
while [ $WAIT_COUNT -lt 60 ]; do
    # Vérifier si l'indexation est terminée
    STATUS=$(curl -s "http://localhost:8080/api/index/status" 2>/dev/null | grep -o '"totalChunks":[0-9]*' | cut -d':' -f2 || echo "0")
    if [ "$STATUS" != "0" ] && [ "$STATUS" != "" ]; then
        sleep 2
        kill $LOGS_PID 2>/dev/null || true
        echo ""
        echo -e "${GREEN}✅ Indexation terminée !${NC}"
        break
    fi
    WAIT_COUNT=$((WAIT_COUNT + 1))
    sleep 1
done

if [ $WAIT_COUNT -eq 60 ]; then
    kill $LOGS_PID 2>/dev/null || true
    echo ""
    echo -e "${YELLOW}⚠️  L'indexation prend plus de temps que prévu${NC}"
    echo "   Vous pouvez vérifier le statut avec: curl http://localhost:8080/api/index/status"
fi

echo ""
echo -e "${GREEN}🎉 Basculement vers TF-IDF terminé !${NC}"
echo ""
echo "📊 Vérification du statut:"
curl -s "http://localhost:8080/api/index/status" | jq '.' 2>/dev/null || curl -s "http://localhost:8080/api/index/status"
echo ""
echo "🧪 Test de recherche:"
echo "   curl \"http://localhost:8080/api/search?q=graphics&maxResults=3\""
echo ""
echo -e "${BLUE}💡 Temps de recherche attendu: ~50-100ms${NC}"

