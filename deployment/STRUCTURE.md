# 📁 Structure du Répertoire Deployment

Ce document décrit l'organisation du répertoire `deployment/` et les conventions utilisées.

## 🎯 Objectif

Organiser tous les fichiers de déploiement par environnement pour :
- **Clarté** : Chaque environnement a son propre répertoire
- **Isolation** : Les configurations ne se mélangent pas
- **Documentation** : README dédié pour chaque environnement
- **Scalabilité** : Facile d'ajouter de nouveaux environnements

## 📂 Structure

```
deployment/
├── README.md              # Guide général (point d'entrée)
├── STRUCTURE.md           # Ce fichier
│
├── pi-nas/                # 🍓 Raspberry Pi NAS (ARM64)
│   ├── README.md          # Documentation spécifique Pi NAS
│   ├── docker-compose.yml # Configuration Docker Compose
│   ├── Dockerfile.arm64   # Image Docker ARM64 (production)
│   ├── Dockerfile.arm64.tests  # Image Docker ARM64 (tests)
│   └── deploy.sh          # Script de déploiement
│
├── azure/                 # ☁️ Azure Container Instances
│   ├── README.md          # Documentation spécifique Azure
│   ├── docker-compose.prod.yml  # Compose pour production
│   ├── Dockerfile         # Image Docker x64
│   ├── deploy.ps1         # Script PowerShell de déploiement
│   └── container-group.json     # Définition Azure Container Group
│
├── local-dev/             # 💻 Développement Local
│   ├── README.md          # Documentation développement
│   ├── docker-compose.yml # Compose pour dev local
│   └── Dockerfile         # Image Docker pour dev
│
└── portainer/             # 🐳 Portainer (gestion Docker)
    ├── README.md          # Documentation Portainer
    └── docker-compose.yml # Configuration Portainer
```

## 🔗 Chemins Relatifs

Tous les fichiers de configuration utilisent des chemins relatifs depuis leur répertoire :

### Depuis `deployment/pi-nas/`, `deployment/azure/`, `deployment/local-dev/` :

| Ressource | Chemin Relatif |
|-----------|----------------|
| Code source | `../../src/` |
| Base de connaissances | `../../knowledge/` |
| Scripts SQL | `../../scripts/` |
| Documentation | `../../docs/` |
| Tests | `../../tests/` |

### Exemple dans docker-compose.yml :

```yaml
services:
  api:
    build:
      context: ../..              # Racine du projet
      dockerfile: deployment/pi-nas/Dockerfile.arm64
    volumes:
      - ../../knowledge:/app/knowledge:ro
      - ../../scripts/init-db.sql:/docker-entrypoint-initdb.d/init-db.sql:ro
```

## 📋 Conventions

### Nommage des Fichiers

| Type | Convention | Exemple |
|------|-----------|---------|
| Docker Compose | `docker-compose.yml` ou `docker-compose.<env>.yml` | `docker-compose.prod.yml` |
| Dockerfile | `Dockerfile` ou `Dockerfile.<arch>[.purpose]` | `Dockerfile.arm64`, `Dockerfile.arm64.tests` |
| Scripts | `deploy.<ext>` | `deploy.sh`, `deploy.ps1` |
| Documentation | `README.md` | Toujours `README.md` |

### Structure d'un README

Chaque README d'environnement doit contenir :

1. **Titre et description** de l'environnement
2. **Prérequis** (matériel, logiciels)
3. **Installation** (étapes détaillées)
4. **Configuration** (variables d'environnement)
5. **Déploiement** (commandes)
6. **Vérification** (tests de santé)
7. **Monitoring** (logs, métriques)
8. **Maintenance** (mise à jour, sauvegarde)
9. **Dépannage** (problèmes courants)
10. **Documentation** (liens vers autres docs)

## 🚀 Ajout d'un Nouvel Environnement

Pour ajouter un nouvel environnement (ex: Kubernetes) :

1. **Créer le répertoire** :
   ```bash
   mkdir deployment/kubernetes
   ```

2. **Créer les fichiers de configuration** :
   ```bash
   touch deployment/kubernetes/README.md
   touch deployment/kubernetes/deployment.yaml
   touch deployment/kubernetes/service.yaml
   ```

3. **Utiliser les chemins relatifs** :
   - Code source : `../../src/`
   - Knowledge : `../../knowledge/`
   - Scripts : `../../scripts/`

4. **Documenter dans le README principal** :
   Ajouter une section dans `deployment/README.md`

5. **Mettre à jour ce fichier** :
   Ajouter la nouvelle structure dans ce document

## 🔐 Sécurité

### Fichiers Sensibles

Les fichiers suivants ne doivent **JAMAIS** être commités :

- `.env` (variables d'environnement avec secrets)
- `*.key` (clés privées)
- `*.pem` (certificats)
- `*.pfx` (certificats)
- Fichiers de configuration avec mots de passe

### .gitignore

Assurez-vous que `.gitignore` contient :

```gitignore
# Secrets
.env
*.key
*.pem
*.pfx

# Données locales
**/postgres-data/
**/volumes/
```

## 📊 Comparaison des Environnements

| Environnement | Architecture | Usage | Complexité |
|---------------|--------------|-------|------------|
| **pi-nas** | ARM64 | Production NAS | Moyenne |
| **azure** | x64 | Production Cloud | Élevée |
| **local-dev** | x64/ARM64 | Développement | Faible |
| **portainer** | x64/ARM64 | Gestion | Faible |

## 🛠️ Outils Recommandés

### Validation

```bash
# Valider un docker-compose.yml
cd deployment/pi-nas
docker compose config

# Vérifier les chemins relatifs
cd deployment/pi-nas
ls -la ../../knowledge/
ls -la ../../scripts/
```

### Tests

```bash
# Tester le build (sans démarrer)
cd deployment/pi-nas
docker compose build

# Tester le démarrage (mode détaché)
docker compose up -d

# Vérifier les logs
docker compose logs -f
```

## 📚 Ressources

- [Guide de déploiement général](README.md)
- [Guide de migration](../DEPLOYMENT-MIGRATION.md)
- [Documentation Docker Compose](https://docs.docker.com/compose/)
- [Best Practices Docker](https://docs.docker.com/develop/dev-best-practices/)

