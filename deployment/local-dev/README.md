# 💻 Développement Local

Environnement de développement local avec Docker Compose.

## 📋 Prérequis

### Logiciels
- Docker 20.10+
- Docker Compose 2.0+
- .NET 8 SDK (pour développement)
- Git

### Ressources
- 4GB+ RAM disponible
- 10GB+ espace disque

## 🚀 Installation

### 1. Cloner le projet

```bash
git clone <votre-repo> mo5-rag-server
cd mo5-rag-server/deployment/local-dev
```

### 2. Installer .NET 8 SDK (optionnel)

**Windows** :
```powershell
winget install Microsoft.DotNet.SDK.8
```

**Linux/macOS** :
```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

## 🔧 Configuration

### Variables d'environnement

Créez un fichier `.env` :

```bash
# Base de données PostgreSQL
POSTGRES_USER=raguser
POSTGRES_PASSWORD=DevPassword123!
POSTGRES_DB=ragdb

# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080

# Chemins (relatifs au répertoire deployment/local-dev)
KNOWLEDGE_PATH=../../knowledge
```

## 🚀 Démarrage

### Méthode 1 : Docker Compose (recommandé)

```bash
# Démarrer tous les services
docker-compose up

# Ou en arrière-plan
docker-compose up -d

# Voir les logs
docker-compose logs -f
```

### Méthode 2 : Développement avec .NET CLI

```bash
# Démarrer uniquement PostgreSQL
docker-compose up -d postgres

# Exécuter l'API en mode développement
cd ../../src/Mo5.RagServer.Api
dotnet run
```

### Méthode 3 : Visual Studio / VS Code

1. Ouvrir `Mo5.RagServer.sln`
2. Démarrer PostgreSQL : `docker-compose up -d postgres`
3. Appuyer sur F5 pour déboguer

## 📊 Accès aux services

| Service | URL | Credentials |
|---------|-----|-------------|
| API | http://localhost:8080 | - |
| Swagger UI | http://localhost:8080/swagger | - |
| PostgreSQL | localhost:5432 | raguser / DevPassword123! |
| Adminer (DB UI) | http://localhost:8081 | raguser / DevPassword123! |

## 🔍 Vérification

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

### Swagger UI

Ouvrez http://localhost:8080/swagger dans votre navigateur pour tester l'API interactivement.

## 🧪 Tests

### Exécuter les tests unitaires

```bash
cd ../../
dotnet test --filter "Category!=RequiresPostgreSQL"
```

### Exécuter tous les tests (avec PostgreSQL)

```bash
cd ../../
docker-compose -f deployment/local-dev/docker-compose.yml up -d postgres
dotnet test
```

### Tests avec couverture

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🛠️ Développement

### Hot Reload

En mode développement avec `dotnet run`, les modifications de code sont automatiquement rechargées.

### Debugging

1. **Visual Studio** : F5 pour démarrer le débogage
2. **VS Code** : Utilisez la configuration de lancement `.vscode/launch.json`
3. **Rider** : Configuration de débogage intégrée

### Migrations de base de données

```bash
cd ../../src/Mo5.RagServer.Infrastructure

# Créer une nouvelle migration
dotnet ef migrations add NomDeLaMigration

# Appliquer les migrations
dotnet ef database update
```

### Ajouter des documents de test

```bash
# Copier des fichiers Markdown dans knowledge/
cp mon-document.md ../../knowledge/

# L'API va automatiquement les indexer au démarrage
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

### Base de données

Accédez à Adminer : http://localhost:8081

- **Système** : PostgreSQL
- **Serveur** : postgres
- **Utilisateur** : raguser
- **Mot de passe** : DevPassword123!
- **Base de données** : ragdb

## 🛠️ Maintenance

### Redémarrer les services

```bash
docker-compose restart
```

### Arrêter les services

```bash
docker-compose down
```

### Nettoyer complètement

```bash
# Arrêter et supprimer les volumes
docker-compose down -v

# Supprimer les images
docker-compose down --rmi all
```

### Reconstruire les images

```bash
docker-compose build --no-cache
docker-compose up
```

## 📁 Structure des fichiers

```
local-dev/
├── docker-compose.yml    # Configuration Docker Compose
├── Dockerfile           # Image Docker pour développement
└── README.md           # Ce fichier
```

## 🔧 Configuration avancée

### Modifier le port de l'API

Éditez `docker-compose.yml` :

```yaml
services:
  api:
    ports:
      - "8080:8080"  # Changez le premier port
```

### Ajouter Adminer (interface DB)

Déjà inclus dans `docker-compose.yml` ! Accédez à http://localhost:8081

### Activer le mode verbose

Éditez `.env` :

```bash
Logging__LogLevel__Default=Debug
```

## 🆘 Dépannage

### L'API ne démarre pas

```bash
# Vérifier les logs
docker-compose logs api

# Vérifier que PostgreSQL est prêt
docker-compose logs postgres | grep "ready to accept connections"
```

### Port déjà utilisé

```bash
# Trouver le processus utilisant le port 8080
lsof -i :8080  # macOS/Linux
netstat -ano | findstr :8080  # Windows

# Changer le port dans docker-compose.yml
```

### Erreur de connexion à PostgreSQL

```bash
# Vérifier que PostgreSQL est en cours d'exécution
docker-compose ps postgres

# Tester la connexion
docker exec -it mo5-rag-postgres psql -U raguser -d ragdb -c "SELECT 1;"
```

### Problèmes de volumes

```bash
# Supprimer et recréer les volumes
docker-compose down -v
docker-compose up
```

## 📚 Documentation

- [Guide de déploiement général](../README.md)
- [Documentation API](../../docs/API-REFERENCE.md)
- [Tests](../../tests/README.md)
- [Architecture](../../docs/DEPLOYMENT-GUIDE.md)

## 💡 Conseils

1. **Utilisez Swagger UI** pour tester l'API rapidement
2. **Activez le hot reload** pour un développement plus rapide
3. **Utilisez Adminer** pour inspecter la base de données
4. **Exécutez les tests** avant de commiter
5. **Consultez les logs** en cas de problème

