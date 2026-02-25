# =============================================================================
# PadTime — Script de déploiement Azure (PowerShell)
# À lancer UNE SEULE FOIS pour provisionner toute l'infrastructure
# Ensuite GitHub Actions prend le relais automatiquement
# =============================================================================

# -----------------------------------------------------------------------------
# CONFIGURATION — vérifie ces valeurs avant de lancer
# -----------------------------------------------------------------------------

$RESOURCE_GROUP = "padtime-rg"
$LOCATION       = "westeurope"
$PROJECT_NAME   = "padtime"

$DOMAIN_WEB  = "padtime.nikoden.io"
$DOMAIN_API  = "padtime-api.nikoden.io"
$DOMAIN_AUTH = "padtime-auth.nikoden.io"

# -----------------------------------------------------------------------------
# ÉTAPE 0 — Connexion Azure
# -----------------------------------------------------------------------------

Write-Host "`n🔐 Connexion Azure..." -ForegroundColor Cyan
az login

Write-Host "`n📋 Abonnements disponibles :" -ForegroundColor Cyan
az account list --output table

Write-Host "`n✅ Abonnement actif :" -ForegroundColor Green
az account show --output table

# Si tu as plusieurs subscriptions, décommente et adapte :
# az account set --subscription "TON_SUBSCRIPTION_ID"

# -----------------------------------------------------------------------------
# ÉTAPE 1 — Resource Group
# -----------------------------------------------------------------------------

Write-Host "`n📁 Création du Resource Group '$RESOURCE_GROUP'..." -ForegroundColor Cyan
az group create --name $RESOURCE_GROUP --location $LOCATION
Write-Host "✅ Resource Group créé" -ForegroundColor Green

# -----------------------------------------------------------------------------
# ÉTAPE 2 — Déploiement Bicep
# Le mot de passe PostgreSQL sera demandé de manière sécurisée
# Règles mot de passe Azure : min 8 chars, 1 majuscule, 1 chiffre, 1 spécial
# -----------------------------------------------------------------------------

Write-Host "`n🚀 Déploiement Bicep..." -ForegroundColor Cyan
Write-Host "⚠️  Le mot de passe PostgreSQL va être demandé" -ForegroundColor Yellow
Write-Host "    Règles : min 8 chars, 1 majuscule, 1 chiffre, 1 caractère spécial`n" -ForegroundColor Yellow

az deployment group create `
  --resource-group $RESOURCE_GROUP `
  --template-file ./main.bicep `
  --parameters `
    projectName=$PROJECT_NAME `
    location=$LOCATION `
    customDomainWeb=$DOMAIN_WEB `
    customDomainApi=$DOMAIN_API `
    customDomainAuth=$DOMAIN_AUTH `
  --query "properties.outputs" `
  --output json

Write-Host "`n✅ Infrastructure déployée !" -ForegroundColor Green

# -----------------------------------------------------------------------------
# ÉTAPE 3 — Récupération des infos pour GitHub Actions
# -----------------------------------------------------------------------------

Write-Host "`n📋 Récupération des credentials ACR..." -ForegroundColor Cyan

$ACR_NAME     = az acr list --resource-group $RESOURCE_GROUP --query "[0].name" --output tsv
$ACR_SERVER   = az acr show --name $ACR_NAME --query "loginServer" --output tsv
$ACR_USERNAME = az acr credential show --name $ACR_NAME --query "username" --output tsv
$ACR_PASSWORD = az acr credential show --name $ACR_NAME --query "passwords[0].value" --output tsv

Write-Host "`n============================================================" -ForegroundColor Yellow
Write-Host "  SECRETS À CONFIGURER DANS GITHUB ACTIONS" -ForegroundColor Yellow
Write-Host "  Settings → Secrets and variables → Actions → New secret" -ForegroundColor Yellow
Write-Host "============================================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "AZURE_RESOURCE_GROUP  = $RESOURCE_GROUP"
Write-Host "AZURE_ACR_SERVER      = $ACR_SERVER"
Write-Host "AZURE_ACR_USERNAME    = $ACR_USERNAME"
Write-Host "AZURE_ACR_PASSWORD    = $ACR_PASSWORD"
Write-Host ""
Write-Host "AZURE_CREDENTIALS     = (voir étape 4 ci-dessous)"
Write-Host "============================================================" -ForegroundColor Yellow

# -----------------------------------------------------------------------------
# ÉTAPE 4 — Service Principal pour GitHub Actions
# Un "compte technique" avec droits limités au Resource Group PadTime
# GitHub Actions l'utilisera pour déployer sans avoir ton mot de passe perso
# -----------------------------------------------------------------------------

Write-Host "`n🔑 Création du Service Principal GitHub Actions..." -ForegroundColor Cyan

$SUBSCRIPTION_ID = az account show --query "id" --output tsv

$SP_OUTPUT = az ad sp create-for-rbac `
  --name "padtime-github-actions" `
  --role "Contributor" `
  --scopes "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP" `
  --json-auth

Write-Host "`n============================================================" -ForegroundColor Yellow
Write-Host "  SECRET AZURE_CREDENTIALS (JSON complet à copier)" -ForegroundColor Yellow
Write-Host "============================================================" -ForegroundColor Yellow
Write-Host $SP_OUTPUT
Write-Host "============================================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "⚠️  Copie ce JSON dans GitHub Secret 'AZURE_CREDENTIALS'" -ForegroundColor Red
Write-Host "   Ne le stocke nulle part ailleurs." -ForegroundColor Red

# -----------------------------------------------------------------------------
# ÉTAPE 5 — Push d'images placeholder dans l'ACR
# Container Apps a besoin d'une image existante pour démarrer
# GitHub Actions les remplacera par les vraies images au premier push
# -----------------------------------------------------------------------------

Write-Host "`n🐳 Push d'images placeholder..." -ForegroundColor Cyan

az acr login --name $ACR_NAME

docker pull nginx:alpine
docker tag nginx:alpine "${ACR_SERVER}/padtime/web:latest"
docker tag nginx:alpine "${ACR_SERVER}/padtime/identity-server:latest"
docker tag nginx:alpine "${ACR_SERVER}/padtime/backend-api:latest"

docker push "${ACR_SERVER}/padtime/web:latest"
docker push "${ACR_SERVER}/padtime/identity-server:latest"
docker push "${ACR_SERVER}/padtime/backend-api:latest"

Write-Host "`n✅ Images placeholder pushées" -ForegroundColor Green

# -----------------------------------------------------------------------------
# RÉSUMÉ FINAL
# -----------------------------------------------------------------------------

Write-Host "`n============================================================" -ForegroundColor Green
Write-Host "  ✅ DÉPLOIEMENT INITIAL TERMINÉ" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Prochaines étapes :"
Write-Host "  1. Configure les 5 secrets dans GitHub (valeurs affichées ci-dessus)"
Write-Host "  2. Configure le DNS pour tes domaines nikoden.io"
Write-Host "  3. Push sur 'main' → GitHub Actions déploie automatiquement"
Write-Host ""
Write-Host "Commandes utiles :" -ForegroundColor Cyan
Write-Host "  Stop DB  : az postgres flexible-server stop  --name padtime-db --resource-group $RESOURCE_GROUP"
Write-Host "  Start DB : az postgres flexible-server start --name padtime-db --resource-group $RESOURCE_GROUP"
Write-Host "  Logs API : az containerapp logs show --name backend-api --resource-group $RESOURCE_GROUP --follow"
Write-Host "  Logs IDS : az containerapp logs show --name identity-server --resource-group $RESOURCE_GROUP --follow"
