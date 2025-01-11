param projectName string
@description('Dockerhub registry password')
@secure()
param registryPassword string = ''

@secure()
param dbPassword string

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${projectName}-law'
  location: resourceGroup().location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}


resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-09-01-preview' = {
  name: '${projectName}-appcfg'
  location: resourceGroup().location
  sku: {
    name: 'Free'
  }
}


resource kv 'Microsoft.KeyVault/vaults@2024-04-01-preview' = {
  name: '${projectName}-kv'
  location: resourceGroup().location
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    tenantId: tenant().tenantId
    enableRbacAuthorization: true
    enablePurgeProtection: true
  }
}


resource kvSecretReaderIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${projectName}-kv-secret-r'
  location: resourceGroup().location
}


resource kvSecretReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(kv.id, kvSecretReaderIdentity.id, 'Key Vault Secrets User')
  scope: kv
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
    principalId: kvSecretReaderIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource registryPassSecret 'Microsoft.KeyVault/vaults/secrets@2024-04-01-preview' = {
  parent: kv
  name: 'dockerhubRegistryPassword'  
  properties: {
    attributes: {
      enabled: true
    }
    value: registryPassword
  }
}

resource sqlServer 'Microsoft.Sql/servers@2014-04-01' ={
  name: '${projectName}-sql'
  location: resourceGroup().location
  properties: {
    administratorLogin: 'azureadm'
    administratorLoginPassword: dbPassword
  }
}

resource sqlServerDatabaseNetworkConfig 'Microsoft.Sql/servers/firewallRules@2024-05-01-preview' = {
  name: 'AllowAzureServices'
  parent: sqlServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}


resource sqlServerDatabase 'Microsoft.Sql/servers/databases@2014-04-01' = {
  parent: sqlServer
  name: projectName
  location: resourceGroup().location
  properties: {
    edition: 'Basic'
    requestedServiceObjectiveName: 'Basic'
  }
}



resource pipelineDatabaseIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${projectName}-pipeline-db-rw'
  location: resourceGroup().location
}


resource appDatabaseIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${projectName}-db-rw'
  location: resourceGroup().location
}


output kvSecretReaderIdentityId string = kvSecretReaderIdentity.id
output kvSecretReaderIdentityClientId string =  kvSecretReaderIdentity.properties.clientId
output appDatabaseIdentityId string = appDatabaseIdentity.id  
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name
output appConfigurationConnectionString string = appConfiguration.listKeys().value[0].connectionString
