# 🚀 Déploiement Mo5 RAG Server

Ce répertoire contient tous les fichiers de déploiement pour différents environnements.

## 📁 Structure

```
deployment/
├── pi-nas/         # Déploiement sur Raspberry Pi NAS
├── azure/          # Déploiement sur Azure Container Instances
├── local-dev/      # Développement local avec Docker
├── portainer/      # Interface de gestion Portainer
└── README.md       # Ce fichier
```

## 🎯 Environnements Disponibles

### 1. Raspberry Pi NAS (`pi-nas/`)

Déploiement optimisé pour Raspberry Pi avec architecture ARM64.

**Caractéristiques** :
- Architecture ARM64 (aarch64)
- PostgreSQL 16 avec pgvector
- API ASP.NET Core directe (port 8080)
- Embeddings TF-IDF locaux (pas d'API externe)
- Optimisé pour faible consommation

**Documentation** : [pi-nas/README.md](pi-nas/README.md)

**Démarrage rapide** :
```bash
cd deployment/pi-nas
./deploy.sh
```

### 2. Azure Container Instances (`azure/`)

Déploiement cloud sur Azure avec mise à l'échelle automatique.

**Caractéristiques** :
- Architecture x64
- Azure Container Instances
- Azure Database for PostgreSQL
- Scalabilité automatique
- Haute disponibilité

**Documentation** : [azure/README.md](azure/README.md)

**Démarrage rapide** :
```powershell
cd deployment/azure
.\deploy.ps1
```

### 3. Développement Local (`local-dev/`)

Environnement de développement local avec Docker Compose.

**Caractéristiques** :
- PostgreSQL local
- Hot reload
- Debugging activé
- Volumes montés pour développement

**Documentation** : [local-dev/README.md](local-dev/README.md)

**Démarrage rapide** :
```bash
cd deployment/local-dev
docker-compose up
```

### 4. Portainer (`portainer/`)

Interface web pour gérer les conteneurs Docker.

**Documentation** : [portainer/README.md](portainer/README.md)

**Démarrage rapide** :
```bash
cd deployment/portainer
docker-compose up -d
```

Accès : http://localhost:9000

## 🔧 Prérequis

### Tous les environnements
- Docker 20.10+
- Docker Compose 2.0+

### Pi NAS
- Raspberry Pi 4/5 (4GB+ RAM recommandé)
- Raspberry Pi OS Lite 64-bit
- 10GB+ espace disque

### Azure
- Azure CLI
- Compte Azure avec souscription active
- PowerShell 7+

### Local Dev
- .NET 8 SDK (pour développement)
- 4GB+ RAM disponible

## 📊 Comparaison des Environnements

| Caractéristique | Pi NAS | Azure | Local Dev |
|----------------|--------|-------|-----------|
| Architecture | ARM64 | x64 | x64/ARM64 |
| RAM requise | 2GB+ | 4GB+ | 4GB+ |
| Coût | Gratuit* | Payant | Gratuit |
| Scalabilité | Limitée | Élevée | Limitée |
| Disponibilité | 24/7** | 99.9% | Variable |
| Performance | Moyenne | Élevée | Élevée |

*Coût électricité uniquement  
**Dépend de votre infrastructure

## 🔐 Configuration

Chaque environnement nécessite sa propre configuration. Consultez le README de chaque environnement pour les détails.

### Variables d'environnement communes

```bash
# Base de données
POSTGRES_USER=raguser
POSTGRES_PASSWORD=<votre-mot-de-passe>
POSTGRES_DB=ragdb

# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

## 📚 Documentation Complémentaire

- [Guide de déploiement complet](../docs/DEPLOYMENT-GUIDE.md)
- [Référence API](../docs/API-REFERENCE.md)
- [Migration vers embeddings locaux](../docs/MIGRATION-TO-LOCAL-EMBEDDINGS.md)
- [Corrections recherche sémantique](../docs/SEMANTIC-SEARCH-FIX-SUMMARY.md)

## 🆘 Support

Pour toute question ou problème :
1. Consultez le README de l'environnement spécifique
2. Vérifiez les logs : `docker logs <container-name>`
3. Consultez la documentation dans `/docs`

## 📝 Notes

- Les fichiers `Dockerfile` et `docker-compose.yml` sont spécifiques à chaque environnement
- Les chemins dans les fichiers de configuration sont relatifs au répertoire de déploiement
- Assurez-vous d'être dans le bon répertoire avant d'exécuter les commandes

