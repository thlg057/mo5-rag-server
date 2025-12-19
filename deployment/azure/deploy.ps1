# Script PowerShell pour déployer Mo5 RAG Server sur Azure
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$Location,
    
    [Parameter(Mandatory=$true)]
    [string]$ContainerRegistryName,
    
    [Parameter(Mandatory=$true)]
    [string]$OpenAIApiKey,
    
    [Parameter(Mandatory=$true)]
    [string]$PostgresPassword,
    
    [string]$DnsNameLabel = "mo5-rag-server-$(Get-Random -Minimum 1000 -Maximum 9999)",
    [string]$ContainerGroupName = "mo5-rag-server",
    [switch]$SkipBuild
)

Write-Host "🚀 Déploiement de Mo5 RAG Server sur Azure" -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Cyan
Write-Host "Location: $Location" -ForegroundColor Cyan
Write-Host "Container Registry: $ContainerRegistryName" -ForegroundColor Cyan
Write-Host "DNS Name: $DnsNameLabel" -ForegroundColor Cyan
Write-Host ""

# Vérifier que Azure CLI est installé
try {
    az --version | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI n'est pas installé"
    }
} catch {
    Write-Host "❌ Erreur: Azure CLI n'est pas installé" -ForegroundColor Red
    Write-Host "Installez Azure CLI depuis: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Vérifier la connexion Azure
Write-Host "🔐 Vérification de la connexion Azure..." -ForegroundColor Blue
try {
    $account = az account show --query "name" -o tsv
    Write-Host "✅ Connecté à Azure: $account" -ForegroundColor Green
} catch {
    Write-Host "❌ Non connecté à Azure" -ForegroundColor Red
    Write-Host "Connectez-vous avec: az login" -ForegroundColor Yellow
    exit 1
}

# Créer le groupe de ressources s'il n'existe pas
Write-Host "📦 Création du groupe de ressources..." -ForegroundColor Blue
az group create --name $ResourceGroupName --location $Location

# Créer le registre de conteneurs s'il n'existe pas
Write-Host "🐳 Création du registre de conteneurs..." -ForegroundColor Blue
$acrExists = az acr show --name $ContainerRegistryName --resource-group $ResourceGroupName --query "name" -o tsv 2>$null
if (-not $acrExists) {
    az acr create --resource-group $ResourceGroupName --name $ContainerRegistryName --sku Basic --admin-enabled true
    Write-Host "✅ Registre de conteneurs créé" -ForegroundColor Green
} else {
    Write-Host "✅ Registre de conteneurs existe déjà" -ForegroundColor Green
}

# Construire et pousser l'image Docker
if (-not $SkipBuild) {
    Write-Host "🔨 Construction et push de l'image Docker..." -ForegroundColor Blue
    
    # Se connecter au registre
    az acr login --name $ContainerRegistryName
    
    # Construire et pousser l'image
    $imageName = "$ContainerRegistryName.azurecr.io/mo5-rag-server:latest"
    docker build -t $imageName .
    docker push $imageName
    
    Write-Host "✅ Image Docker poussée vers le registre" -ForegroundColor Green
} else {
    Write-Host "⏭️  Construction Docker ignorée (--SkipBuild)" -ForegroundColor Yellow
}

# Obtenir les informations du registre
Write-Host "🔑 Récupération des informations du registre..." -ForegroundColor Blue
$acrServer = az acr show --name $ContainerRegistryName --resource-group $ResourceGroupName --query "loginServer" -o tsv
$acrUsername = az acr credential show --name $ContainerRegistryName --resource-group $ResourceGroupName --query "username" -o tsv
$acrPassword = az acr credential show --name $ContainerRegistryName --resource-group $ResourceGroupName --query "passwords[0].value" -o tsv

# Déployer le groupe de conteneurs
Write-Host "🚀 Déploiement du groupe de conteneurs..." -ForegroundColor Blue

$deploymentParams = @{
    "containerGroupName" = $ContainerGroupName
    "location" = $Location
    "openAIApiKey" = $OpenAIApiKey
    "postgresPassword" = $PostgresPassword
    "dnsNameLabel" = $DnsNameLabel
}

# Créer le fichier de paramètres temporaire
$paramsFile = "azure-deploy-params.json"
$deploymentParams | ConvertTo-Json | Set-Content $paramsFile

try {
    # Modifier le template pour utiliser notre registre
    $templateContent = Get-Content "azure/container-group.json" -Raw
    $templateContent = $templateContent -replace "your-registry.azurecr.io", $acrServer
    $templateContent | Set-Content "azure/container-group-temp.json"
    
    # Déployer avec Azure CLI
    $deployment = az deployment group create `
        --resource-group $ResourceGroupName `
        --template-file "azure/container-group-temp.json" `
        --parameters "@$paramsFile" `
        --query "properties.outputs" -o json | ConvertFrom-Json
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Déploiement réussi!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 Informations de déploiement:" -ForegroundColor Cyan
        Write-Host "  • IP publique: $($deployment.containerIPv4Address.value)" -ForegroundColor White
        Write-Host "  • FQDN: $($deployment.containerFQDN.value)" -ForegroundColor White
        Write-Host "  • API URL: http://$($deployment.containerFQDN.value):8080" -ForegroundColor White
        Write-Host "  • Swagger: http://$($deployment.containerFQDN.value):8080/swagger" -ForegroundColor White
        Write-Host ""
        Write-Host "🔧 Commandes utiles:" -ForegroundColor Cyan
        Write-Host "  • Voir les logs: az container logs --resource-group $ResourceGroupName --name $ContainerGroupName --container-name api" -ForegroundColor White
        Write-Host "  • Redémarrer: az container restart --resource-group $ResourceGroupName --name $ContainerGroupName" -ForegroundColor White
        Write-Host "  • Supprimer: az group delete --name $ResourceGroupName --yes --no-wait" -ForegroundColor White
    } else {
        Write-Host "❌ Échec du déploiement" -ForegroundColor Red
        exit 1
    }
} finally {
    # Nettoyer les fichiers temporaires
    if (Test-Path $paramsFile) { Remove-Item $paramsFile }
    if (Test-Path "azure/container-group-temp.json") { Remove-Item "azure/container-group-temp.json" }
}

Write-Host ""
Write-Host "🎉 Déploiement Azure terminé!" -ForegroundColor Green
