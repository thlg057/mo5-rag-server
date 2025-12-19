# 🐳 Portainer - Interface de Gestion Docker

Portainer est une interface web pour gérer vos conteneurs Docker, images, volumes et réseaux.

## 📋 Prérequis

- Docker 20.10+
- Docker Compose 2.0+

## 🚀 Installation

### Démarrage rapide

```bash
cd deployment/portainer
docker-compose up -d
```

### Accès

Ouvrez votre navigateur : **http://localhost:9000**

## 🔧 Configuration initiale

### 1. Première connexion

Lors de la première connexion, vous devrez :

1. **Créer un compte administrateur**
   - Nom d'utilisateur : admin
   - Mot de passe : (minimum 12 caractères)

2. **Choisir l'environnement**
   - Sélectionnez "Docker" (environnement local)
   - Cliquez sur "Connect"

### 2. Configuration de l'environnement

Portainer se connectera automatiquement au socket Docker local via le volume monté.

## 📊 Fonctionnalités

### Gestion des conteneurs

- ✅ Démarrer/Arrêter/Redémarrer des conteneurs
- ✅ Voir les logs en temps réel
- ✅ Accéder à la console d'un conteneur
- ✅ Inspecter les détails (CPU, RAM, réseau)
- ✅ Créer de nouveaux conteneurs

### Gestion des images

- ✅ Lister toutes les images
- ✅ Supprimer les images inutilisées
- ✅ Pull de nouvelles images depuis Docker Hub
- ✅ Build d'images depuis Dockerfile

### Gestion des volumes

- ✅ Créer/Supprimer des volumes
- ✅ Inspecter le contenu des volumes
- ✅ Sauvegarder/Restaurer des volumes

### Gestion des réseaux

- ✅ Créer des réseaux personnalisés
- ✅ Connecter/Déconnecter des conteneurs
- ✅ Inspecter la configuration réseau

### Stacks (Docker Compose)

- ✅ Déployer des stacks depuis des fichiers docker-compose.yml
- ✅ Gérer les stacks existantes
- ✅ Mettre à jour les stacks

## 🎯 Utilisation avec Mo5 RAG Server

### Déployer le stack Pi NAS

1. Allez dans **Stacks** → **Add stack**
2. Nommez le stack : `mo5-rag-server`
3. Choisissez **Upload** et sélectionnez `deployment/pi-nas/docker-compose.yml`
4. Cliquez sur **Deploy the stack**

### Surveiller l'application

1. Allez dans **Containers**
2. Cliquez sur `mo5-rag-api` pour voir :
   - Logs en temps réel
   - Statistiques (CPU, RAM, réseau)
   - Console interactive

### Gérer la base de données

1. Allez dans **Containers**
2. Cliquez sur `mo5-rag-postgres`
3. Utilisez **Console** pour accéder à psql :
   ```bash
   psql -U raguser -d ragdb
   ```

## 🔍 Monitoring

### Dashboard

Le dashboard principal affiche :
- Nombre de conteneurs (en cours, arrêtés)
- Nombre d'images
- Nombre de volumes
- Nombre de réseaux
- Utilisation des ressources

### Statistiques en temps réel

Pour chaque conteneur :
- CPU usage (%)
- Memory usage (MB)
- Network I/O
- Block I/O

## 🛠️ Maintenance

### Redémarrer Portainer

```bash
docker-compose restart
```

### Arrêter Portainer

```bash
docker-compose down
```

### Mettre à jour Portainer

```bash
docker-compose pull
docker-compose up -d
```

### Sauvegarder la configuration

```bash
# Les données sont dans le volume portainer_data
docker run --rm -v portainer_data:/data -v $(pwd):/backup alpine tar czf /backup/portainer-backup.tar.gz /data
```

### Restaurer la configuration

```bash
docker run --rm -v portainer_data:/data -v $(pwd):/backup alpine tar xzf /backup/portainer-backup.tar.gz -C /
```

## 🔐 Sécurité

### Recommandations

1. **Changez le mot de passe par défaut** immédiatement
2. **Activez HTTPS** en production
3. **Limitez l'accès réseau** (firewall)
4. **Créez des utilisateurs avec permissions limitées**
5. **Activez l'authentification à deux facteurs** (version Business)

### Activer HTTPS

Éditez `docker-compose.yml` :

```yaml
services:
  portainer:
    command: --ssl --sslcert /certs/cert.pem --sslkey /certs/key.pem
    volumes:
      - ./certs:/certs
```

### Limiter l'accès

```bash
# Firewall (exemple UFW)
sudo ufw allow from 192.168.1.0/24 to any port 9000
```

## 📁 Structure des fichiers

```
portainer/
├── docker-compose.yml    # Configuration Docker Compose
└── README.md            # Ce fichier
```

## 🆘 Dépannage

### Impossible d'accéder à Portainer

```bash
# Vérifier que le conteneur est en cours d'exécution
docker ps | grep portainer

# Vérifier les logs
docker logs portainer
```

### Erreur de connexion au socket Docker

```bash
# Vérifier les permissions du socket
ls -la /var/run/docker.sock

# Ajouter l'utilisateur au groupe docker
sudo usermod -aG docker $USER
```

### Port 9000 déjà utilisé

Éditez `docker-compose.yml` :

```yaml
ports:
  - "9001:9000"  # Changez le premier port
```

## 📚 Documentation

- [Documentation officielle Portainer](https://docs.portainer.io/)
- [Guide de déploiement général](../README.md)
- [Portainer Community Edition vs Business](https://www.portainer.io/pricing)

## 💡 Conseils

1. **Utilisez les templates** pour déployer rapidement des applications courantes
2. **Créez des équipes** pour gérer les permissions
3. **Utilisez les webhooks** pour automatiser les déploiements
4. **Activez les notifications** pour être alerté des problèmes
5. **Explorez les App Templates** pour découvrir de nouvelles applications

## 🎓 Ressources

- [Tutoriels vidéo](https://www.portainer.io/videos)
- [Forum communautaire](https://community.portainer.io/)
- [GitHub](https://github.com/portainer/portainer)

