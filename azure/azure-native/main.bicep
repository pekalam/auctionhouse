targetScope = 'subscription'

@secure()
param dbPassword string

var projectName = 'auctionhouse'
var privateContainerRegistrySub = '0ebcf8c7-bd19-44e1-9a1e-77cfc61def57'
var privateContainerRegistry = 'mpfree.azurecr.io'
var privateContainerRegistryRg = 'acr'

resource rg1 'Microsoft.Resources/resourceGroups@2024-03-01'=  {
  name: 'auctionhouse-primary'
  location: 'westus'
}

resource dbRg 'Microsoft.Resources/resourceGroups@2024-03-01'=  {
  name: 'auctionhouse-data'
  location: 'westus'
}

resource infraRg 'Microsoft.Resources/resourceGroups@2024-03-01'=  {
  name: 'auctionhouse-infra'
  location: 'westus'
}

resource acrIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: 'acr-r'
  scope: resourceGroup(privateContainerRegistrySub, privateContainerRegistryRg)
}


module deployment 'deploy.bicep' = {
  name: 'deployment'
  scope: rg1
  params: {
    projectName: projectName
    acrIdentityId: acrIdentity.id
    privateContainerRegistry: privateContainerRegistry
    appDatabaseIdentityId: dataDeployment.outputs.appDatabaseIdentityId
    kvSecretReaderIdentityId: dataDeployment.outputs.kvSecretReaderIdentityId
    kvSecretReaderIdentityClientId: dataDeployment.outputs.kvSecretReaderIdentityClientId
    logAnalyticsWorkspaceName: dataDeployment.outputs.logAnalyticsWorkspaceName
    logAnalyticsWorkspaceRg: dbRg.name
    appConfigurationConnectionString: dataDeployment.outputs.appConfigurationConnectionString
  }
}

module dataDeployment 'deploy_data.bicep' = {
  name: 'dataDeployment'
  scope: dbRg
  params: {
    projectName: projectName
    dbPassword: dbPassword
  }
}

module infraDeployment 'deploy_infra.bicep' = {
  name: 'infraDeployment'
  scope: infraRg
  params: {
    projectName: projectName
  }
}
