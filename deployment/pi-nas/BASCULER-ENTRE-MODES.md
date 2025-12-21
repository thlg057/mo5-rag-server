# 🔄 Basculer entre TF-IDF et Modèle Neuronal

## 📋 Les deux configurations disponibles

Vous avez **2 fichiers Docker Compose** dans le répertoire `deployment/pi-nas/` :

1. **`docker-compose.yml`** → TF-IDF (rapide, ~5ms par recherche)
2. **`docker-compose.neuronal.yml`** → Sentence Transformers (lent, ~40s par recherche, meilleure qualité)

---

## ⚡ Passer au mode TF-IDF (rapide)

### Étape 1 : Arrêter le mode neuronal

```bash
cd /srv/dev-disk-by-uuid-8a747308-0fd1-4223-b1be-09ccfdf4bad1/sources/mo5-rag-server/deployment/pi-nas
sudo docker compose -f docker-compose.neuronal.yml down
```

### Étape 2 : Démarrer le mode TF-IDF

```bash
sudo docker compose up -d
```

### Étape 3 : Attendre que l'API démarre

```bash
sleep 10
curl http://localhost:8080/health
```

### Étape 4 : Ré-indexer les documents

**Important** : Les embeddings TF-IDF sont différents des embeddings neuronaux, donc il faut ré-indexer !

```bash
# Supprimer les anciens chunks
sudo docker exec -it mo5-rag-postgres psql -U mo5_user -d mo5_rag -c 'DELETE FROM "DocumentChunks"; DELETE FROM "DocumentTags"; DELETE FROM "Documents";'

# Ré-indexer (prend ~10 secondes)
curl -X POST "http://localhost:8080/api/index/all"
```

### Étape 5 : Tester

```bash
curl -s "http://localhost:8080/api/search?q=graphics&maxResults=3" | jq .
```

**Temps d'exécution attendu** : ~50-100ms (au lieu de 40s !)

---

## 🧠 Passer au mode Neuronal (meilleure qualité)

### Étape 1 : Arrêter le mode TF-IDF

```bash
cd /srv/dev-disk-by-uuid-8a747308-0fd1-4223-b1be-09ccfdf4bad1/sources/mo5-rag-server/deployment/pi-nas
sudo docker compose down
```

### Étape 2 : Démarrer le mode neuronal

```bash
sudo docker compose -f docker-compose.neuronal.yml up -d
```

### Étape 3 : Attendre que l'API démarre

```bash
sleep 10
curl http://localhost:8080/health
```

### Étape 4 : Ré-indexer les documents

```bash
# Supprimer les anciens chunks
sudo docker exec -it mo5-rag-postgres psql -U mo5_user -d mo5_rag -c 'DELETE FROM "DocumentChunks"; DELETE FROM "DocumentTags"; DELETE FROM "Documents";'

# Ré-indexer (prend ~6 minutes)
curl -X POST "http://localhost:8080/api/index/all"
```

### Étape 5 : Tester

```bash
curl -s "http://localhost:8080/api/search?q=graphics&maxResults=3&minScore=0.3" | jq .
```

**Temps d'exécution attendu** : ~40s (mais meilleure qualité sémantique !)

---

## 📊 Comparaison rapide

| Critère | TF-IDF | Neuronal |
|---------|--------|----------|
| **Vitesse indexation** | ⚡ ~1s/doc | 🐌 ~40s/doc |
| **Vitesse recherche** | ⚡ ~50ms | 🐌 ~40s |
| **Qualité** | Mots-clés uniquement | Sémantique + synonymes |
| **Mémoire** | ~100 MB | ~500 MB |
| **Multilingue** | ❌ Non | ✅ Oui (FR + EN) |
| **Fichier** | `docker-compose.yml` | `docker-compose.neuronal.yml` |
| **Dockerfile** | `Dockerfile.arm64` | `Dockerfile.arm64.neuronal` |

---

## 🎯 Recommandation

**Pour votre usage** (Raspberry Pi NAS) :

- **TF-IDF** si vous voulez des réponses instantanées
- **Neuronal** si vous préférez la qualité et que 40s d'attente ne vous dérange pas

**Astuce** : Vous pouvez garder TF-IDF en production et utiliser le neuronal occasionnellement pour des recherches complexes !

---

## 🔍 Différences de recherche

### Exemple avec TF-IDF
```bash
curl "http://localhost:8080/api/search?q=graphics&maxResults=3"
```
→ Trouve uniquement les documents contenant le mot "graphics"

### Exemple avec Neuronal
```bash
curl "http://localhost:8080/api/search?q=graphics&maxResults=3&minScore=0.3"
```
→ Trouve les documents sur les graphiques, images, affichage visuel, etc. (comprend le sens)

---

## ⚠️ Points importants

1. **Toujours ré-indexer** après avoir changé de mode (les embeddings sont incompatibles)
2. **Le paramètre GET** pour le score minimum s'appelle `minScore` (pas `minSimilarityScore`)
3. **Les données PostgreSQL** sont conservées entre les deux modes (seuls les chunks changent)
4. **Le cache HuggingFace** (~420 MB) reste en place même en mode TF-IDF

---

## 📝 Scripts utiles

### Vérifier quel mode est actif

```bash
sudo docker ps --format "table {{.Names}}\t{{.Image}}"
```

Si vous voyez `mo5-rag-api-neuronal:latest` → Mode neuronal
Si vous voyez `mo5-rag-api:latest` → Mode TF-IDF

### Voir les logs en temps réel

```bash
# Mode TF-IDF
sudo docker compose logs -f api

# Mode neuronal
sudo docker compose -f docker-compose.neuronal.yml logs -f api
```

### Vérifier le statut de l'index

```bash
curl -s "http://localhost:8080/api/index/status" | jq .
```

---

## 🚀 Déploiement rapide

### Script pour TF-IDF
```bash
#!/bin/bash
cd /srv/dev-disk-by-uuid-8a747308-0fd1-4223-b1be-09ccfdf4bad1/sources/mo5-rag-server/deployment/pi-nas
sudo docker compose -f docker-compose.neuronal.yml down
sudo docker compose up -d
sleep 10
sudo docker exec -it mo5-rag-postgres psql -U mo5_user -d mo5_rag -c 'DELETE FROM "DocumentChunks"; DELETE FROM "DocumentTags"; DELETE FROM "Documents";'
curl -X POST "http://localhost:8080/api/index/all"
```

### Script pour Neuronal
```bash
#!/bin/bash
cd /srv/dev-disk-by-uuid-8a747308-0fd1-4223-b1be-09ccfdf4bad1/sources/mo5-rag-server/deployment/pi-nas
sudo docker compose down
sudo docker compose -f docker-compose.neuronal.yml up -d
sleep 10
sudo docker exec -it mo5-rag-postgres psql -U mo5_user -d mo5_rag -c 'DELETE FROM "DocumentChunks"; DELETE FROM "DocumentTags"; DELETE FROM "Documents";'
curl -X POST "http://localhost:8080/api/index/all"
```

