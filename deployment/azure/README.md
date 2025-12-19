# ☁️ Déploiement Azure

Déploiement cloud sur Azure Container Instances avec haute disponibilité.

## 📋 Prérequis

### Logiciels
- Azure CLI 2.50+
- PowerShell 7+
- Docker (pour build local)

### Azure
- Compte Azure avec souscription active
- Permissions pour créer :
  - Container Instances
  - Container Registry
  - Database for PostgreSQL
  - Resource Groups

## 🚀 Installation

### 1. Installer Azure CLI

**Windows** :
```powershell
winget install Microsoft.AzureCLI
```

**Linux/macOS** :
```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

### 2. Se connecter à Azure

```powershell
az login
az account set --subscription "<votre-subscription-id>"
```

## 🔧 Configuration

### Variables d'environnement

Éditez le fichier `deploy.ps1` et configurez :

```powershell
$resourceGroup = "mo5-rag-rg"
$location = "westeurope"
$acrName = "mo5ragacr"
$containerGroupName = "mo5-rag-server"
$postgresServer = "mo5-rag-postgres"
```

### Base de données PostgreSQL

Deux options :

#### Option 1 : Azure Database for PostgreSQL (recommandé)

```powershell
# Créer le serveur PostgreSQL
az postgres flexible-server create `
  --resource-group mo5-rag-rg `
  --name mo5-rag-postgres `
  --location westeurope `
  --admin-user raguser `
  --admin-password <VotreMotDePasse> `
  --sku-name Standard_B1ms `
  --tier Burstable `
  --version 16

# Installer l'extension pgvector
az postgres flexible-server parameter set `
  --resource-group mo5-rag-rg `
  --server-name mo5-rag-postgres `
  --name azure.extensions `
  --value vector
```

#### Option 2 : PostgreSQL dans Container Instance

Utilisez `docker-compose.prod.yml` qui inclut PostgreSQL.

## 🚀 Déploiement

### Méthode 1 : Script PowerShell (recommandé)

```powershell
.\deploy.ps1
```

Le script va :
1. ✅ Créer le Resource Group
2. ✅ Créer le Container Registry
3. ✅ Construire et pousser l'image
4. ✅ Déployer le Container Group
5. ✅ Afficher l'URL publique

### Méthode 2 : Azure CLI manuel

```powershell
# 1. Créer le Resource Group
az group create --name mo5-rag-rg --location westeurope

# 2. Créer le Container Registry
az acr create --resource-group mo5-rag-rg --name mo5ragacr --sku Basic

# 3. Build et push l'image
az acr build --registry mo5ragacr --image mo5-rag-api:latest -f Dockerfile ../../

# 4. Déployer le Container Group
az container create --resource-group mo5-rag-rg --file container-group.json
```

### Méthode 3 : Docker Compose (développement)

```bash
docker-compose -f docker-compose.prod.yml up -d
```

## 📊 Vérification

### Obtenir l'URL publique

```powershell
az container show `
  --resource-group mo5-rag-rg `
  --name mo5-rag-server `
  --query ipAddress.fqdn `
  --output tsv
```

### Test de santé

```powershell
$fqdn = az container show --resource-group mo5-rag-rg --name mo5-rag-server --query ipAddress.fqdn -o tsv
curl "http://${fqdn}:8080/health"
```

### Test de recherche

```powershell
$fqdn = az container show --resource-group mo5-rag-rg --name mo5-rag-server --query ipAddress.fqdn -o tsv
curl -X POST "http://${fqdn}:8080/api/search" `
  -H "Content-Type: application/json" `
  -d '{"query": "graphics mode", "maxResults": 3}'
```

## 🔍 Monitoring

### Logs en temps réel

```powershell
az container logs `
  --resource-group mo5-rag-rg `
  --name mo5-rag-server `
  --follow
```

### Métriques

```powershell
az monitor metrics list `
  --resource "/subscriptions/<sub-id>/resourceGroups/mo5-rag-rg/providers/Microsoft.ContainerInstance/containerGroups/mo5-rag-server" `
  --metric CPUUsage,MemoryUsage
```

### Application Insights (optionnel)

Ajoutez Application Insights pour un monitoring avancé :

```powershell
az monitor app-insights component create `
  --app mo5-rag-insights `
  --location westeurope `
  --resource-group mo5-rag-rg
```

## 🛠️ Maintenance

### Redémarrer le conteneur

```powershell
az container restart `
  --resource-group mo5-rag-rg `
  --name mo5-rag-server
```

### Mettre à jour l'application

```powershell
# 1. Rebuild et push l'image
az acr build --registry mo5ragacr --image mo5-rag-api:latest -f Dockerfile ../../

# 2. Redéployer
az container delete --resource-group mo5-rag-rg --name mo5-rag-server --yes
az container create --resource-group mo5-rag-rg --file container-group.json
```

### Mise à l'échelle

Éditez `container-group.json` et modifiez :

```json
{
  "resources": {
    "requests": {
      "cpu": 2.0,
      "memoryInGB": 4.0
    }
  }
}
```

## 💰 Coûts

### Estimation mensuelle (Europe Ouest)

| Ressource | Configuration | Coût/mois |
|-----------|--------------|-----------|
| Container Instance | 1 vCPU, 2GB RAM | ~30€ |
| PostgreSQL Flexible | Standard_B1ms | ~25€ |
| Container Registry | Basic | ~5€ |
| Stockage | 10GB | ~1€ |
| **TOTAL** | | **~61€** |

### Optimisation des coûts

1. **Arrêter les instances non utilisées** :
   ```powershell
   az container stop --resource-group mo5-rag-rg --name mo5-rag-server
   ```

2. **Utiliser des instances Spot** (si disponible)

3. **Réduire la taille de la base de données** pour le développement

## 🔐 Sécurité

### Recommandations

1. **Utilisez Azure Key Vault** pour les secrets
2. **Activez HTTPS** avec un certificat SSL
3. **Configurez un WAF** (Web Application Firewall)
4. **Limitez l'accès réseau** avec des NSG (Network Security Groups)
5. **Activez les logs d'audit**

### Configurer HTTPS

```powershell
# Créer un certificat SSL (Let's Encrypt recommandé)
# Configurer Application Gateway avec SSL
```

## 📁 Structure des fichiers

```
azure/
├── docker-compose.prod.yml  # Compose pour production
├── Dockerfile               # Image Docker x64
├── container-group.json     # Définition Container Instance
├── deploy.ps1              # Script de déploiement
└── README.md               # Ce fichier
```

## 🆘 Dépannage

### Le conteneur ne démarre pas

```powershell
az container show --resource-group mo5-rag-rg --name mo5-rag-server
az container logs --resource-group mo5-rag-rg --name mo5-rag-server
```

### Erreur de connexion PostgreSQL

Vérifiez les règles de firewall :

```powershell
az postgres flexible-server firewall-rule create `
  --resource-group mo5-rag-rg `
  --name mo5-rag-postgres `
  --rule-name AllowAzureServices `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0
```

## 📚 Documentation

- [Guide de déploiement général](../README.md)
- [Documentation Azure Container Instances](https://docs.microsoft.com/azure/container-instances/)
- [Documentation API](../../docs/API-REFERENCE.md)

