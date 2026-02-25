// =============================================================================
// PadTime — Infrastructure Azure (v2 — corrigé)
// Corrections vs v1 :
//   - Containers en HTTP pur (TLS terminé par Container Apps en amont)
//   - Signing keys IdentityServer générées au runtime, pas depuis le repo
//   - Pas de montage de certificats .pfx (inutile en prod Azure)
// =============================================================================

// -----------------------------------------------------------------------------
// PARAMETERS
// -----------------------------------------------------------------------------

@description('Région Azure.')
param location string = 'westeurope'

@description('Préfixe pour nommer toutes les ressources.')
param projectName string = 'padtime'

@description('Mot de passe admin PostgreSQL. Demandé interactivement, jamais stocké ici.')
@secure()
param postgresAdminPassword string

@description('Domaine custom frontend. Vide = URL Azure générée.')
param customDomainWeb string = ''

@description('Domaine custom API. Vide = URL Azure générée.')
param customDomainApi string = ''

@description('Domaine custom IdentityServer. Vide = URL Azure générée.')
param customDomainAuth string = ''

// -----------------------------------------------------------------------------
// VARIABLES
// -----------------------------------------------------------------------------

var acrName = '${projectName}acr'
var postgresServerName = '${projectName}-db'
var keyVaultName = '${projectName}-kv'
var containerAppsEnvName = '${projectName}-env'
var logAnalyticsName = '${projectName}-logs'
var postgresAdminLogin = 'padtimeadmin'

var identityDbConnectionString = 'Host=${postgresServerName}.postgres.database.azure.com;Port=5432;Database=identityserver;Username=${postgresAdminLogin};Password=${postgresAdminPassword};SslMode=Require'
var apiDbConnectionString = 'Host=${postgresServerName}.postgres.database.azure.com;Port=5432;Database=padtime;Username=${postgresAdminLogin};Password=${postgresAdminPassword};SslMode=Require'

// -----------------------------------------------------------------------------
// BLOC 1 — Azure Container Registry
// -----------------------------------------------------------------------------

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

// -----------------------------------------------------------------------------
// BLOC 2 — PostgreSQL Flexible Server
// -----------------------------------------------------------------------------

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: postgresServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: postgresAdminLogin
    administratorLoginPassword: postgresAdminPassword
    version: '16'
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

resource identityDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  parent: postgresServer
  name: 'identityserver'
}

resource apiDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  parent: postgresServer
  name: 'padtime'
}

resource postgresFirewall 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-03-01-preview' = {
  parent: postgresServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// -----------------------------------------------------------------------------
// BLOC 3 — Key Vault
// -----------------------------------------------------------------------------

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    softDeleteRetentionInDays: 7
    enableSoftDelete: true
  }
}

resource postgresPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'postgres-admin-password'
  properties: {
    value: postgresAdminPassword
  }
}

// -----------------------------------------------------------------------------
// BLOC 4 — Log Analytics + Container Apps Environment
// -----------------------------------------------------------------------------

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppsEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// -----------------------------------------------------------------------------
// BLOC 5 — Container App : IdentityServer
//
// CORRECTION v2 :
// - ASPNETCORE_URLS = http://+:80 uniquement
//   Container Apps termine TLS en amont → le container ne voit que du HTTP
//   C'est le pattern standard "TLS offloading" — identique à un load balancer
// - IssuerUri reste HTTPS — c'est l'URL publique que les clients voient
//   Le container peut tourner en HTTP en interne, ses tokens référencent HTTPS
// - Signing keys : générées automatiquement par IdentityServer au démarrage
//   Stockées dans le filesystem éphémère du container (ok pour projet scolaire)
// -----------------------------------------------------------------------------

resource identityServerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: 'identity-server'
  location: location
  properties: {
    environmentId: containerAppsEnvironment.id

    configuration: {
      ingress: {
        external: true
        targetPort: 80
        transport: 'auto'
        customDomains: customDomainAuth != '' ? [
          {
            name: customDomainAuth
            bindingType: 'SniEnabled'
          }
        ] : []
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.listCredentials().username
          passwordSecretRef: 'acr-password'
        }
      ]
      secrets: [
        {
          name: 'acr-password'
          value: containerRegistry.listCredentials().passwords[0].value
        }
        {
          name: 'db-connection'
          value: identityDbConnectionString
        }
      ]
    }

    template: {
      containers: [
        {
          name: 'identity-server'
          image: '${containerRegistry.properties.loginServer}/padtime/identity-server:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:80'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'db-connection'
            }
            {
              name: 'IdentityServer__IssuerUri'
              value: customDomainAuth != '' ? 'https://${customDomainAuth}' : ''
            }
            {
              name: 'Clients__WebRedirectUri'
              value: customDomainWeb != '' ? 'https://${customDomainWeb}/callback' : 'https://${webApp.properties.configuration.ingress.fqdn}/callback'
            }
            {
              name: 'Clients__WebPostLogoutUri'
              value: customDomainWeb != '' ? 'https://${customDomainWeb}' : 'https://${webApp.properties.configuration.ingress.fqdn}'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// -----------------------------------------------------------------------------
// BLOC 6 — Container App : Backend API
// -----------------------------------------------------------------------------

resource backendApiApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: 'backend-api'
  location: location
  properties: {
    environmentId: containerAppsEnvironment.id

    configuration: {
      ingress: {
        external: true
        targetPort: 80
        transport: 'auto'
        customDomains: customDomainApi != '' ? [
          {
            name: customDomainApi
            bindingType: 'SniEnabled'
          }
        ] : []
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.listCredentials().username
          passwordSecretRef: 'acr-password'
        }
      ]
      secrets: [
        {
          name: 'acr-password'
          value: containerRegistry.listCredentials().passwords[0].value
        }
        {
          name: 'db-connection'
          value: apiDbConnectionString
        }
      ]
    }

    template: {
      containers: [
        {
          name: 'backend-api'
          image: '${containerRegistry.properties.loginServer}/padtime/backend-api:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:80'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'db-connection'
            }
            {
              name: 'Authentication__Authority'
              value: customDomainAuth != '' ? 'https://${customDomainAuth}' : 'https://${identityServerApp.properties.configuration.ingress.fqdn}'
            }
            {
              name: 'Authentication__Audience'
              value: 'padtime-api'
            }
            {
              name: 'Authentication__RequireHttpsMetadata'
              value: 'true'
            }
            {
              name: 'Cors__AllowedOrigins__0'
              value: customDomainWeb != '' ? 'https://${customDomainWeb}' : 'https://${webApp.properties.configuration.ingress.fqdn}'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// -----------------------------------------------------------------------------
// BLOC 7 — Container App : Frontend Angular (nginx)
// Les URLs sont compilées dans environment.production.ts au ng build
// Aucune variable d'env nécessaire ici
// -----------------------------------------------------------------------------

resource webApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: 'web'
  location: location
  properties: {
    environmentId: containerAppsEnvironment.id

    configuration: {
      ingress: {
        external: true
        targetPort: 80
        transport: 'auto'
        customDomains: customDomainWeb != '' ? [
          {
            name: customDomainWeb
            bindingType: 'SniEnabled'
          }
        ] : []
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.listCredentials().username
          passwordSecretRef: 'acr-password'
        }
      ]
      secrets: [
        {
          name: 'acr-password'
          value: containerRegistry.listCredentials().passwords[0].value
        }
      ]
    }

    template: {
      containers: [
        {
          name: 'web'
          image: '${containerRegistry.properties.loginServer}/padtime/web:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// -----------------------------------------------------------------------------
// OUTPUTS
// -----------------------------------------------------------------------------

output acrLoginServer string = containerRegistry.properties.loginServer
output acrName string = containerRegistry.name

output identityServerUrl string = customDomainAuth != ''
  ? 'https://${customDomainAuth}'
  : 'https://${identityServerApp.properties.configuration.ingress.fqdn}'

output backendApiUrl string = customDomainApi != ''
  ? 'https://${customDomainApi}'
  : 'https://${backendApiApp.properties.configuration.ingress.fqdn}'

output webUrl string = customDomainWeb != ''
  ? 'https://${customDomainWeb}'
  : 'https://${webApp.properties.configuration.ingress.fqdn}'

output postgresHost string = '${postgresServerName}.postgres.database.azure.com'
output keyVaultName string = keyVault.name
