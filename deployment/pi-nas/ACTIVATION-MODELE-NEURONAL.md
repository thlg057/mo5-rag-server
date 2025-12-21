# 🧠 Activation du Modèle Neuronal (Sentence Transformers)

> Guide pour activer le modèle neuronal sur votre Raspberry Pi NAS

## 📋 Ce qu'on va faire

Actuellement, le RAG Server utilise **TF-IDF** (formule mathématique simple).

On va passer à **Sentence Transformers** (modèle neuronal) pour une meilleure qualité de recherche sémantique.

**Avantages** :
- ✅ Meilleure compréhension du sens des textes
- ✅ Détection des synonymes et paraphrases
- ✅ Recherche sémantique plus précise

**Inconvénients** :
- ❌ Nécessite Python 3.8+
- ❌ ~500 MB d'espace disque pour le modèle
- ❌ Plus lent que TF-IDF (mais toujours rapide)

---

## 🔧 Étape 1 : Installer Python sur le Raspberry Pi

### Vérifier si Python est déjà installé

Connectez-vous en SSH à votre Raspberry Pi :

```bash
ssh votre-user@votre-raspberry-pi
```

Vérifiez la version de Python :

```bash
python3 --version
```

**Si vous voyez** : `Python 3.x.x` (avec x >= 8) → **Parfait, passez à l'étape 2 !**

**Si Python n'est pas installé ou version < 3.8** → Installez-le :

```bash
# Mettre à jour les packages
sudo apt update

# Installer Python 3 et pip
sudo apt install -y python3 python3-pip python3-venv

# Vérifier l'installation
python3 --version
pip3 --version
```

---

## 🐳 Étape 2 : Modifier le Dockerfile

Le Dockerfile actuel n'inclut pas Python. On va l'ajouter.

**Fichier** : `deployment/pi-nas/Dockerfile.arm64`

Vous avez deux options :

### Option A : Modifier le Dockerfile existant (recommandé)

Je vais créer un nouveau Dockerfile avec Python inclus.

### Option B : Utiliser Python sur l'hôte (plus simple mais moins isolé)

Monter Python de l'hôte dans le conteneur (voir étape 3).

---

## 📝 Étape 3 : Modifier docker-compose.yml

**Fichier** : `deployment/pi-nas/docker-compose.yml`

### Changements à faire

1. **Changer le provider** : `TfIdf` → `Local`
2. **Ajouter la configuration Python**
3. **Monter Python dans le conteneur** (si option B)

### Configuration finale

```yaml
api:
  build:
    context: ../..
    dockerfile: deployment/pi-nas/Dockerfile.arm64
    target: runtime
  image: mo5-rag-api:latest
  container_name: mo5-rag-api
  ports:
    - "8080:8080"
  environment:
    - ASPNETCORE_ENVIRONMENT=Production
    - ASPNETCORE_URLS=http://+:8080
    - ConnectionStrings__DefaultConnection=Host=postgres;Database=mo5_rag;Username=mo5_user;Password=mo5_password_change_me;Include Error Detail=false
    # 🔥 CHANGEMENT ICI : TfIdf → Local
    - EmbeddingService__Provider=Local
    - EmbeddingService__VectorDimensions=384
    # 🔥 NOUVEAU : Configuration Python
    - LocalEmbedding__ModelName=paraphrase-multilingual-MiniLM-L12-v2
    - LocalEmbedding__PythonPath=python3
    - RagSettings__KnowledgeBasePath=/app/knowledge
    - RagSettings__ChunkSize=1000
    - RagSettings__ChunkOverlap=200
    - RagSettings__MaxResults=10
    - RagSettings__MinSimilarityScore=0.7
  depends_on:
    postgres:
      condition: service_healthy
  volumes:
    - ../../knowledge:/app/knowledge:ro
  restart: unless-stopped
  networks:
    - mo5-rag-network
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
    interval: 30s
    timeout: 10s
    retries: 3
    start_period: 40s
```

---

## 🚀 Étape 4 : Créer le nouveau Dockerfile avec Python

Je vais créer un nouveau Dockerfile qui inclut Python et les dépendances nécessaires.

**Fichier** : `deployment/pi-nas/Dockerfile.arm64.neuronal`

---

## 📦 Étape 5 : Déployer

### 1. Arrêter les conteneurs actuels

```bash
cd /srv/dev-disk-by-uuid-8a747308-0fd1-4223-b1be-09ccfdf4bad1/sources/mo5-rag-server/deployment/pi-nas
sudo docker-compose down
```

### 2. Reconstruire l'image avec Python

```bash
sudo docker-compose build --no-cache
```

**⚠️ Attention** : La première construction sera **longue** (~10-20 minutes) car :
- Installation de Python dans le conteneur
- Téléchargement du modèle (~420 MB)
- Installation des dépendances Python

### 3. Démarrer les conteneurs

```bash
sudo docker-compose up -d
```

### 4. Vérifier les logs

```bash
# Voir les logs en temps réel
sudo docker-compose logs -f api

# Vous devriez voir :
# [INFO] Initializing Local Embedding Service with model: paraphrase-multilingual-MiniLM-L12-v2
# [INFO] Python is available
# [INFO] Installing Python package: sentence-transformers
# [INFO] Installing Python package: torch
# [INFO] Installing Python package: numpy
# [INFO] Local Embedding Service initialized successfully
```

---

## ✅ Étape 6 : Tester

### Test 1 : Vérifier le health check

```bash
curl http://localhost:8080/health
```

**Résultat attendu** : `{"status":"Healthy"}`

### Test 2 : Faire une recherche

```bash
curl "http://localhost:8080/api/search?q=registre%20accumulateur&maxResults=3"
```

**Résultat attendu** : JSON avec des résultats de recherche

### Test 3 : Comparer avec TF-IDF

Pour voir la différence, vous pouvez comparer les résultats :

**Avec TF-IDF** (ancien) :
- Recherche par mots-clés exacts
- "registre A" trouve uniquement les docs avec "registre" ET "A"

**Avec Sentence Transformers** (nouveau) :
- Recherche sémantique
- "registre A" trouve aussi "accumulateur", "registre principal", etc.

---

## 🔄 Retour à TF-IDF (si besoin)

Si vous voulez revenir à TF-IDF (plus rapide, moins de ressources) :

### 1. Modifier docker-compose.yml

```yaml
environment:
  - EmbeddingService__Provider=TfIdf  # Changer Local → TfIdf
```

### 2. Redémarrer

```bash
sudo docker-compose down
sudo docker-compose up -d
```

**Pas besoin de reconstruire l'image !**

---

## 📊 Comparaison des performances

### TF-IDF (actuel)

```
Temps de génération d'embedding : ~5 ms
Mémoire utilisée : ~50 MB
Espace disque : 0 MB (aucune dépendance)
Qualité : ⭐⭐⭐ (bonne pour termes techniques)
```

### Sentence Transformers (nouveau)

```
Temps de génération d'embedding : ~50-100 ms
Mémoire utilisée : ~200-300 MB
Espace disque : ~500 MB (modèle + dépendances)
Qualité : ⭐⭐⭐⭐⭐ (excellente pour recherche sémantique)
```

**Sur Raspberry Pi 4 (4 GB RAM)** : Aucun problème, largement suffisant ! 🚀

---

## 🐛 Dépannage

### Problème : "Python is not available"

**Cause** : Python n'est pas installé dans le conteneur.

**Solution** : Utilisez le nouveau Dockerfile avec Python (voir étape 4).

### Problème : "Failed to install sentence-transformers"

**Cause** : Pas assez d'espace disque ou problème réseau.

**Solution** :

```bash
# Vérifier l'espace disque
df -h

# Nettoyer les images Docker inutilisées
sudo docker system prune -a
```

### Problème : "Model download timeout"

**Cause** : Le téléchargement du modèle (~420 MB) prend du temps.

**Solution** : Augmenter le timeout dans docker-compose.yml :

```yaml
healthcheck:
  start_period: 120s  # Au lieu de 40s
```

### Problème : Conteneur redémarre en boucle

**Cause** : Erreur au démarrage (Python, modèle, etc.).

**Solution** : Voir les logs :

```bash
sudo docker-compose logs api
```

### Problème : Recherche très lente

**Cause** : Le Raspberry Pi génère les embeddings à la demande.

**Solution** : C'est normal pour la première recherche (téléchargement du modèle). Les suivantes sont plus rapides (modèle en cache).

---

## 💡 Optimisations possibles

### 1. Pré-télécharger le modèle

Au lieu de télécharger le modèle au premier démarrage, vous pouvez le pré-télécharger :

```bash
# Sur le Raspberry Pi
python3 -c "from sentence_transformers import SentenceTransformer; SentenceTransformer('paraphrase-multilingual-MiniLM-L12-v2')"
```

### 2. Utiliser un volume pour le cache du modèle

Ajouter dans docker-compose.yml :

```yaml
volumes:
  - ../../knowledge:/app/knowledge:ro
  - ~/.cache/huggingface:/root/.cache/huggingface  # Cache du modèle
```

**Avantage** : Le modèle n'est téléchargé qu'une seule fois, même si vous reconstruisez l'image.

### 3. Ajuster le MinSimilarityScore

Avec Sentence Transformers, vous pouvez baisser le seuil :

```yaml
environment:
  - RagSettings__MinSimilarityScore=0.5  # Au lieu de 0.7
```

**Raison** : Le modèle neuronal donne des scores plus nuancés.

---

## 📝 Résumé des étapes

1. ✅ **Installer Python 3.8+** sur le Raspberry Pi
2. ✅ **Créer un nouveau Dockerfile** avec Python (je vais le faire)
3. ✅ **Modifier docker-compose.yml** : `Provider=Local`
4. ✅ **Reconstruire et déployer** : `docker-compose build && docker-compose up -d`
5. ✅ **Vérifier les logs** : `docker-compose logs -f api`
6. ✅ **Tester** : `curl http://localhost:8080/api/search?q=test`

---

## 🎯 Prochaines étapes

Voulez-vous que je :

1. **Crée le nouveau Dockerfile** avec Python inclus ?
2. **Modifie docker-compose.yml** pour activer le modèle neuronal ?
3. **Crée un script de déploiement** automatique ?

Dites-moi ce que vous préférez ! 😊
- Téléchargement du modèle (~420 MB)
- Installation des dépendances Python

### 3. Démarrer les conteneurs

```bash
sudo docker-compose up -d
```

### 4. Vérifier les logs

```bash
# Voir les logs en temps réel
sudo docker-compose logs -f api

# Vous devriez voir :
# [INFO] Initializing Local Embedding Service with model: paraphrase-multilingual-MiniLM-L12-v2
# [INFO] Python is available
# [INFO] Installing Python package: sentence-transformers
# [INFO] Installing Python package: torch
# [INFO] Installing Python package: numpy
# [INFO] Local Embedding Service initialized successfully
```

---

## ✅ Étape 6 : Tester

### Test 1 : Vérifier le health check

```bash
curl http://localhost:8080/health
```

**Résultat attendu** : `{"status":"Healthy"}`

### Test 2 : Faire une recherche

```bash
curl "http://localhost:8080/api/search?q=registre%20accumulateur&maxResults=3"
```

**Résultat attendu** : JSON avec des résultats de recherche


