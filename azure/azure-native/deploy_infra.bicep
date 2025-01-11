
param projectName string


resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' = {
  name: '${projectName}.pekalam.store'
  location: 'global'
  properties: {
    zoneType: 'Public'
  }
}


resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: '${projectName}-law'
  scope: resourceGroup('${projectName}-data')
}



resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${projectName}-ais'
  location: resourceGroup().location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    Flow_Type: 'Bluefield'
  }
}
