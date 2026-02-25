#!/bin/bash
# =============================================================================
# PadTime — Script de déploiement Azure
# À lancer UNE SEULE FOIS pour provisionner toute l'infrastructure
# Ensuite GitHub Actions prend le relais pour les mises à jour
# =============================================================================

# -----------------------------------------------------------------------------
# CONFIGURATION — modifie ces valeurs avant de lancer
# -----------------------------------------------------------------------------

RESOURCE_GROUP="padtime-rg"
LOCATION="westeurope"
PROJECT_NAME="padtime"

# Domaines custom — laisser vide si pas encore configurés
DOMAIN_WEB="padtime.nikoden.io"
DOMAIN_API="padtime-api.nikoden.io"
DOMAIN_AUTH="padtime-auth.nikoden.io"

# -----------------------------------------------------------------------------
# ÉTAPE 0 — Vérifications préalables
# -----------------------------------------------------------------------------

echo "🔍 Vérification Azure CLI..."
az --version > /dev/null 2>&1 || { echo "❌ Azure CLI non installé. https://docs.microsoft.com/cli/azure/install-azure-cli"; exit 1; }

echo "🔐 Connexion Azure (ouvre le browser)..."
az login

echo "📋 Abonnements disponibles :"
az account list --output table

# Si tu as plusieurs subscriptions, décommente et adapte :
# az account set --subscription "TON_SUBSCRIPTION_ID"

echo "✅ Subscription active :"
az account show --output table

# -----------------------------------------------------------------------------
# ÉTAPE 1 — Resource Group
# Le "dossier" qui contient toutes les ressources PadTime
# -----------------------------------------------------------------------------

echo ""
echo "📁 Création du Resource Group '$RESOURCE_GROUP'..."
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION

echo "✅ Resource Group créé"

# -----------------------------------------------------------------------------
# ÉTAPE 2 — Déploiement Bicep
# Azure lit le fichier, crée toutes les ressources dans l'ordre
# Le mot de passe PostgreSQL sera demandé de manière sécurisée
# -----------------------------------------------------------------------------

echo ""
echo "🚀 Déploiement de l'infrastructure via Bicep..."
echo "⚠️  Le mot de passe PostgreSQL va être demandé (min 8 chars, majuscule + chiffre + spécial)"
echo ""

az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file ./main.bicep \
  --parameters \
    projectName=$PROJECT_NAME \
    location=$LOCATION \
    customDomainWeb="$DOMAIN_WEB" \
    customDomainApi="$DOMAIN_API" \
    customDomainAuth="$DOMAIN_AUTH" \
  --query "properties.outputs" \
  --output json

# --query "properties.outputs" = affiche uniquement les outputs définis dans le Bicep
# --output json = format lisible

echo ""
echo "✅ Infrastructure déployée !"

# -----------------------------------------------------------------------------
# ÉTAPE 3 — Récupère les infos pour GitHub Actions
# -----------------------------------------------------------------------------

echo ""
echo "📋 Récupération des informations pour GitHub Actions..."

ACR_NAME=$(az acr list --resource-group $RESOURCE_GROUP --query "[0].name" --output tsv)
ACR_SERVER=$(az acr show --name $ACR_NAME --query "loginServer" --output tsv)
ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query "username" --output tsv)
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query "passwords[0].value" --output tsv)

echo ""
echo "============================================================"
echo "  SECRETS À CONFIGURER DANS GITHUB ACTIONS"
echo "  (Settings → Secrets and variables → Actions → New secret)"
echo "============================================================"
echo ""
echo "AZURE_RESOURCE_GROUP    = $RESOURCE_GROUP"
echo "AZURE_ACR_SERVER        = $ACR_SERVER"
echo "AZURE_ACR_USERNAME      = $ACR_USERNAME"
echo "AZURE_ACR_PASSWORD      = $ACR_PASSWORD"
echo ""
echo "AZURE_CLIENT_ID         = (voir étape 4)"
echo "AZURE_TENANT_ID         = (voir étape 4)"
echo "AZURE_SUBSCRIPTION_ID   = (voir étape 4)"
echo "============================================================"

# -----------------------------------------------------------------------------
# ÉTAPE 4 — Service Principal pour GitHub Actions
# GitHub Actions a besoin d'une identité Azure pour déployer
# Un Service Principal = un "compte technique" avec des droits limités
# -----------------------------------------------------------------------------

echo ""
echo "🔑 Création du Service Principal pour GitHub Actions..."

SUBSCRIPTION_ID=$(az account show --query "id" --output tsv)
TENANT_ID=$(az account show --query "tenantId" --output tsv)

# Crée le SP avec droits limités au Resource Group PadTime uniquement
SP_OUTPUT=$(az ad sp create-for-rbac \
  --name "padtime-github-actions" \
  --role "Contributor" \
  --scopes "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP" \
  --json-auth)

echo ""
echo "============================================================"
echo "  SECRET SUPPLÉMENTAIRE GITHUB ACTIONS"
echo "============================================================"
echo ""
echo "AZURE_CREDENTIALS (contenu JSON complet ci-dessous) :"
echo ""
echo $SP_OUTPUT | python3 -m json.tool
echo ""
echo "AZURE_SUBSCRIPTION_ID   = $SUBSCRIPTION_ID"
echo "AZURE_TENANT_ID         = $TENANT_ID"
echo "============================================================"
echo ""
echo "⚠️  IMPORTANT : Ces credentials donnent accès à Azure."
echo "    Copie-les dans GitHub Secrets MAINTENANT et ne les stocke nulle part ailleurs."

# -----------------------------------------------------------------------------
# ÉTAPE 5 — Premier push d'images placeholder
# Container Apps a besoin d'une image pour démarrer
# On push des images nginx/hello-world temporaires
# GitHub Actions les remplacera par les vraies images au premier déploiement
# -----------------------------------------------------------------------------

echo ""
echo "🐳 Login ACR et push d'images placeholder..."

az acr login --name $ACR_NAME

# Pull images publiques et re-tag pour l'ACR
docker pull nginx:alpine
docker tag nginx:alpine $ACR_SERVER/padtime/web:latest
docker tag nginx:alpine $ACR_SERVER/padtime/identity-server:latest
docker tag nginx:alpine $ACR_SERVER/padtime/backend-api:latest

# Push vers l'ACR
docker push $ACR_SERVER/padtime/web:latest
docker push $ACR_SERVER/padtime/identity-server:latest
docker push $ACR_SERVER/padtime/backend-api:latest

echo ""
echo "✅ Images placeholder pushées"
echo ""
echo "============================================================"
echo "  DÉPLOIEMENT INITIAL TERMINÉ"
echo "============================================================"
echo ""
echo "Prochaines étapes :"
echo "1. Configure les secrets dans GitHub (voir valeurs ci-dessus)"
echo "2. Configure le DNS pour tes domaines custom (si applicable)"
echo "3. Push sur 'main' → GitHub Actions déploie automatiquement"
echo ""
echo "Commandes utiles :"
echo "  Stop DB  : az postgres flexible-server stop  --name padtime-db --resource-group $RESOURCE_GROUP"
echo "  Start DB : az postgres flexible-server start --name padtime-db --resource-group $RESOURCE_GROUP"
echo "  Logs API : az containerapp logs show --name backend-api --resource-group $RESOURCE_GROUP --follow"
