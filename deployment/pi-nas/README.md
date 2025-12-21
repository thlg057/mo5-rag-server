# 🍓 Déploiement Raspberry Pi NAS

Déploiement optimisé pour Raspberry Pi avec architecture ARM64.

## 🧠 Deux modes disponibles

### Mode TF-IDF (par défaut)
- ✅ Rapide et léger
- ✅ Aucune dépendance externe
- ✅ Parfait pour termes techniques
- 📄 Fichiers : `docker-compose.yml`, `Dockerfile.arm64`

### Mode Neuronal (Sentence Transformers)
- ✅ Meilleure qualité sémantique
- ✅ Comprend les synonymes
- ⚠️ Nécessite Python 3.8+
- 📄 Fichiers : `docker-compose.neuronal.yml`, `Dockerfile.arm64.neuronal`
- 📖 Guide : [ACTIVATION-MODELE-NEURONAL.md](ACTIVATION-MODELE-NEURONAL.md)

## 📋 Prérequis

### Matériel
- Raspberry Pi 4 ou 5 (4GB+ RAM recommandé)
- Carte SD 32GB+ ou SSD
- Connexion réseau stable

### Logiciel
- Raspberry Pi OS Lite 64-bit
- Docker 20.10+
- Docker Compose 2.0+

## 🚀 Installation

### 1. Installer Docker (si nécessaire)

```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER
```

### 2. Installer Docker Compose (si nécessaire)

```bash
sudo apt-get update
sudo apt-get install -y docker-compose-plugin
```

### 3. Cloner le projet

```bash
cd /srv
git clone <votre-repo> mo5-rag-server
cd mo5-rag-server/deployment/pi-nas
```

## 🔧 Configuration

### Variables d'environnement

Créez un fichier `.env` :

```bash
# Base de données PostgreSQL
POSTGRES_USER=raguser
POSTGRES_PASSWORD=VotreMotDePasseSecurise123!
POSTGRES_DB=ragdb

# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Chemins (relatifs au répertoire deployment/pi-nas)
KNOWLEDGE_PATH=../../knowledge
```

## 🚀 Déploiement

### Mode TF-IDF (par défaut)

#### Méthode 1 : Script automatique (recommandé)

```bash
./deploy.sh
```

Le script va :
1. ✅ Vérifier les prérequis
2. ✅ Construire l'image ARM64
3. ✅ Démarrer les services
4. ✅ Vérifier la santé de l'application
5. ✅ Afficher les logs

#### Méthode 2 : Manuel

```bash
# Construire l'image
docker build -f Dockerfile.arm64 -t mo5-rag-api:arm64 ../../

# Démarrer les services
docker-compose up -d

# Vérifier les logs
docker-compose logs -f
```

### Mode Neuronal (Sentence Transformers)

**📖 Guide complet** : [ACTIVATION-MODELE-NEURONAL.md](ACTIVATION-MODELE-NEURONAL.md)

#### Déploiement rapide

```bash
# 1. Installer Python (si nécessaire)
sudo apt update && sudo apt install -y python3 python3-pip python3-venv

# 2. Déployer avec le script automatique
./deploy-neuronal.sh
```

Le script va :
1. ✅ Vérifier Python et l'espace disque
2. ✅ Créer le cache pour le modèle
3. ✅ Construire l'image avec Python
4. ✅ Télécharger le modèle (~420 MB)
5. ✅ Démarrer les services
6. ✅ Tester l'API

**⚠️ Attention** : Le premier déploiement prend 10-20 minutes (téléchargement du modèle).

## 📊 Vérification

### Santé de l'application

```bash
curl http://localhost:8080/health
# Devrait retourner: Healthy
```

### Test de recherche

```bash
curl -X POST http://localhost:8080/api/search \
  -H "Content-Type: application/json" \
  -d '{"query": "graphics mode", "maxResults": 3}'
```

### Vérifier les conteneurs

```bash
docker-compose ps
```

## 🔍 Monitoring

### Logs en temps réel

```bash
# Tous les services
docker-compose logs -f

# API uniquement
docker-compose logs -f api

# PostgreSQL uniquement
docker-compose logs -f postgres
```

### Statistiques des conteneurs

```bash
docker stats
```

## 🛠️ Maintenance

### Redémarrer les services

```bash
docker-compose restart
```

### Arrêter les services

```bash
docker-compose down
```

### Mettre à jour l'application

```bash
# Récupérer les dernières modifications
git pull

# Reconstruire et redémarrer
./deploy.sh
```

### Sauvegarder la base de données

```bash
docker exec mo5-rag-postgres pg_dump -U raguser ragdb > backup_$(date +%Y%m%d).sql
```

### Restaurer la base de données

```bash
docker exec -i mo5-rag-postgres psql -U raguser ragdb < backup_20231219.sql
```

### Exécuter les tests ARM64

```bash
# Build de l'image de tests
docker build -f Dockerfile.arm64.tests -t mo5-rag-tests:arm64 ../..

# Exécution des tests
docker run --rm mo5-rag-tests:arm64
```

## 📁 Structure des fichiers

```
pi-nas/
├── docker-compose.yml              # Configuration Docker Compose (TF-IDF)
├── docker-compose.neuronal.yml     # Configuration Docker Compose (Neuronal)
├── Dockerfile.arm64                # Image Docker ARM64 (TF-IDF)
├── Dockerfile.arm64.neuronal       # Image Docker ARM64 (Neuronal + Python)
├── Dockerfile.arm64.tests          # Image Docker ARM64 (tests)
├── deploy.sh                       # Script de déploiement (TF-IDF)
├── deploy-neuronal.sh              # Script de déploiement (Neuronal)
├── ACTIVATION-MODELE-NEURONAL.md   # Guide d'activation du modèle neuronal
└── README.md                       # Ce fichier
```

## 🔐 Sécurité

### Recommandations

1. **Changez le mot de passe PostgreSQL** dans `.env`
2. **Utilisez HTTPS** en production (configurez un certificat SSL)
3. **Limitez l'accès réseau** avec un firewall
4. **Mettez à jour régulièrement** les images Docker

### Firewall (optionnel)

```bash
sudo ufw allow 8080/tcp  # API
sudo ufw enable
```

## ⚡ Performance

### Optimisations appliquées

- ✅ Image multi-stage pour réduire la taille
- ✅ Embeddings locaux (TF-IDF ou Sentence Transformers)
- ✅ PostgreSQL avec pgvector optimisé
- ✅ Cache du modèle HuggingFace (mode neuronal)
- ✅ API ASP.NET Core optimisée

### Ressources utilisées

#### Mode TF-IDF
- **RAM** : ~1.5GB (API + PostgreSQL)
- **CPU** : ~10-20% au repos, ~50-80% pendant l'indexation
- **Disque** : ~2GB (images + données)

#### Mode Neuronal
- **RAM** : ~2GB (API + PostgreSQL + modèle)
- **CPU** : ~20-30% au repos, ~80-100% pendant l'indexation
- **Disque** : ~2.5GB (images + données + modèle ~500MB)

## 🆘 Dépannage

### L'API ne démarre pas

```bash
# Vérifier les logs
docker-compose logs api

# Vérifier que PostgreSQL est prêt
docker-compose logs postgres | grep "ready to accept connections"
```

### Erreur de connexion à PostgreSQL

```bash
# Vérifier que le conteneur PostgreSQL est en cours d'exécution
docker-compose ps postgres

# Tester la connexion
docker exec mo5-rag-postgres psql -U raguser -d ragdb -c "SELECT 1;"
```

### Recherche retourne 0 résultats

Consultez [SEMANTIC-SEARCH-FIX-SUMMARY.md](../../docs/SEMANTIC-SEARCH-FIX-SUMMARY.md)

## 📚 Documentation

- [Guide de déploiement général](../README.md)
- [Documentation API](../../docs/API-REFERENCE.md)
- [Tests](../../tests/README.md)

