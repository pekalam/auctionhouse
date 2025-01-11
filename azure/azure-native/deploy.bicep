param projectName string
param acrIdentityId string
param privateContainerRegistry string
param kvSecretReaderIdentityId string
param kvSecretReaderIdentityClientId string
param appDatabaseIdentityId string
param logAnalyticsWorkspaceName string
param logAnalyticsWorkspaceRg string
@secure()
param appConfigurationConnectionString string
var dockerhubPasswordSecretName = 'dockerhubpassword'


resource auctionhouseVnet 'Microsoft.Network/virtualNetworks@2019-11-01' = {
  name: '${projectName}-vnet'
  location: resourceGroup().location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.7.0.0/16'
      ]
    }
    subnets: [
      {
        name: '${projectName}-subnet-default'
        properties: {
          addressPrefix: '10.7.0.0/23'
        }
      }
    ]
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: logAnalyticsWorkspaceName
  scope: resourceGroup(logAnalyticsWorkspaceRg)
}
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: '${projectName}-env'
  location: resourceGroup().location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    vnetConfiguration: {
      infrastructureSubnetId: auctionhouseVnet.properties.subnets[0].id
      internal: false
    }
  }
}


var defaultRegistry = privateContainerRegistry != null ? '${privateContainerRegistry}/' : ''

resource rabbitmqContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'rabbitmq'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${acrIdentityId}': {}
    }
  }
  location: resourceGroup().location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: false
        transport: 'tcp'
        targetPort: 5672
        additionalPortMappings: [
          {
            external: false
            targetPort: 15672
          }
        ]
      }
      registries: [
        {
          server: privateContainerRegistry
          identity: acrIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'rabbitmq'
          image: '${defaultRegistry}rabbitmq:3-management'
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Readiness'
              initialDelaySeconds: 15
              tcpSocket: {port: 5672}
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

resource redisContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'redis'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${acrIdentityId}': {}
    }
  }
  location: resourceGroup().location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: false
        transport: 'tcp'
        targetPort: 6379
      }
      registries: [
        {
          server: privateContainerRegistry
          identity: acrIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'redis'
          image: '${defaultRegistry}redis'
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Readiness'
              initialDelaySeconds: 15
              tcpSocket: {port: 6379}
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}


resource quartzWebTaskservice 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'quartz-web-task-service'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${acrIdentityId}': {}
    }
  }
  location: resourceGroup().location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: false
        transport: 'http'
        targetPort: 80
      }
      registries: [
        {
          server: privateContainerRegistry
          identity: acrIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'quartz-web-task-service'
          image: '${defaultRegistry}marekbf3/quartz-web-task-service'
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Readiness'
              initialDelaySeconds: 15
              tcpSocket: {port: 80}
            }
          ]
          env: [
            {
              name: 'ClientKey'
              value: 'testk'
            }
            {
              name: 'ManagmentKey'
              value: 'testm'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}



resource otelCollecterSa 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: '${projectName}otelsa'
  location: resourceGroup().location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource otelCollectorSaFs 'Microsoft.Storage/storageAccounts/fileServices@2023-05-01' = {
  parent: otelCollecterSa
  name: 'default'
}

resource otelCollectorSaShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-05-01' = {
  parent: otelCollectorSaFs
  name: 'otelconfig'
}

resource otelconfigVol 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  parent: containerAppsEnvironment
  name: 'otelconfig'
  properties: {
    azureFile: {
      accountName: otelCollecterSa.name
      shareName: 'otelconfig'
      accessMode: 'ReadOnly'
      accountKey: otelCollecterSa.listKeys().keys[0].value
    }
  }
}

resource otelCollector 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'otel-collector'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${acrIdentityId}': {}
    }
  }
  location: resourceGroup().location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: false
        transport: 'tcp'
        targetPort: 4317
        additionalPortMappings: [
          {
            external: false
            targetPort: 4318
          }
        ]
      }
      registries: [
        {
          server: privateContainerRegistry
          identity: acrIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'otel-collector'
          image: '${defaultRegistry}otel/opentelemetry-collector-contrib:latest'
          args: [
            '--config=/otelconfig/otel-collector-config.yml'
          ]
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Readiness'
              initialDelaySeconds: 15
              tcpSocket: {port: 4317}
            }
          ]
          volumeMounts: [
            {
              volumeName: otelconfigVol.name
              mountPath: '/otelconfig'
            }
          ]
        }
      ]
      volumes: [
        {
          storageName: otelconfigVol.name
          name: otelconfigVol.name
          storageType: 'AzureFile'
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}



resource zipkinContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'zipkin'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${acrIdentityId}': {}
    }
  }
  location: resourceGroup().location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: false
        transport: 'tcp'
        targetPort: 9411
      }
      registries: [
        {
          server: privateContainerRegistry
          identity: acrIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'zipkin'
          image: '${defaultRegistry}openzipkin/zipkin'
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Readiness'
              initialDelaySeconds: 15
              tcpSocket: {port: 9411}
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}


resource seqContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'seq'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${acrIdentityId}': {}
    }
  }
  location: resourceGroup().location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: false
        transport: 'tcp'
        targetPort: 5341
      }
      registries: [
        {
          server: privateContainerRegistry
          identity: acrIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'seq'
          image: '${defaultRegistry}datalust/seq'
          env: [
            {
              name: 'ACCEPT_EULA'
              value: 'Y'
            }
          ]
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Readiness'
              initialDelaySeconds: 15
              tcpSocket: {port: 5341}
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

resource envoygatewayApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'envoygateway'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${acrIdentityId}': {}
    }
  }
  location: resourceGroup().location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: true
        transport: 'http'
        targetPort: 8080
        customDomains: [
          {
            name: 'auctionhouse.pekalam.store'
            bindingType: 'Disabled'
          }
        ]
        corsPolicy: {
          allowedOrigins: [
            'https://auctionhouse.pekalam.store'
          ]
        }
      }
      registries: [
        {
          server: privateContainerRegistry
          identity: acrIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'envoygateway'
          image: '${defaultRegistry}pekalam/auctionhouse-envoyaz'
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Readiness'
              initialDelaySeconds: 15
              tcpSocket: {port: 8080}
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}




resource appCommandApi 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'command'
  location: resourceGroup().location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${kvSecretReaderIdentityId}': {}
      '${appDatabaseIdentityId}': {}
      '${acrIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: false
        transport: 'http'
        targetPort: 80
      }
      registries: [
        // {
        //   server: 'index.docker.io'
        //   username: 'pekalam'
        //   passwordSecretRef: dockerhubPasswordSecretName
        // }
        {
          server: privateContainerRegistry
          identity: acrIdentityId
        }
      ]
      secrets: [
        // {
        //   identity: kvSecretReaderIdentityId
        //   keyVaultUrl: registryPassSecret.properties.secretUri
        //   name: dockerhubPasswordSecretName
        // }
      ]
    }
    template: {
      containers: [
        {
          name: 'auctionhousecommand'
          image: '${defaultRegistry}pekalam/auctionhousecommand'
          env: [
            {
              name: 'APP_ENV'
              value: 'command'
            }
            {
              name: 'KV_MI_CLIENT_ID'
              value: kvSecretReaderIdentityClientId
            }
            {
              name: 'ConnectionStrings__AppConfigurationProd'
              value: appConfigurationConnectionString
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:80'
            }
            {
              name: 'DemoMode__Enabled'
              value: 'false'
            }
          ]
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Readiness'
              initialDelaySeconds: 60
              tcpSocket: {port: 80}
              failureThreshold: 10
              timeoutSeconds: 240
              periodSeconds: 240
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}



resource appQueryApi 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'query'
  location: resourceGroup().location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${kvSecretReaderIdentityId}': {}
      '${appDatabaseIdentityId}': {}
      '${acrIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: false
        transport: 'http'
        targetPort: 80
      }
      registries: [
        // {
        //   server: 'index.docker.io'
        //   username: 'pekalam'
        //   passwordSecretRef: dockerhubPasswordSecretName
        // }
        {
          server: privateContainerRegistry
          identity: acrIdentityId
        }
      ]
      secrets: [
        // {
        //   identity: kvSecretReaderIdentityId
        //   keyVaultUrl: registryPassSecret.properties.secretUri
        //   name: dockerhubPasswordSecretName
        // }
      ]
    }
    template: {
      containers: [
        {
          name: 'auctionhousequery'
          image: '${defaultRegistry}pekalam/auctionhousequery'
          env: [
            {
              name: 'APP_ENV'
              value: 'query'
            }
            {
              name: 'KV_MI_CLIENT_ID'
              value: kvSecretReaderIdentityClientId
            }
            {
              name: 'ConnectionStrings__AppConfigurationProd'
              value: appConfigurationConnectionString
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:80'
            }
          ]
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Readiness'
              initialDelaySeconds: 60
              tcpSocket: {port: 80}
              failureThreshold: 10
              timeoutSeconds: 240
              periodSeconds: 240
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}



resource appCommandStatusApi 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'commandstatus'
  location: resourceGroup().location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${kvSecretReaderIdentityId}': {}
      '${appDatabaseIdentityId}': {}
      '${acrIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: false
        transport: 'http'
        targetPort: 80
      }
      registries: [
        // {
        //   server: 'index.docker.io'
        //   username: 'pekalam'
        //   passwordSecretRef: dockerhubPasswordSecretName
        // }
        {
          server: privateContainerRegistry
          identity: acrIdentityId
        }
      ]
      secrets: [
        // {
        //   identity: kvSecretReaderIdentityId
        //   keyVaultUrl: registryPassSecret.properties.secretUri
        //   name: dockerhubPasswordSecretName
        // }
      ]
    }
    template: {
      containers: [
        {
          name: 'auctionhousecommandstatus'
          image: '${defaultRegistry}pekalam/auctionhousecommandstatus'
          env: [
            {
              name: 'APP_ENV'
              value: 'commandstatus'
            }
            {
              name: 'ConnectionStrings__AppConfigurationProd'
              value: appConfigurationConnectionString
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:80'
            }
          ]
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Readiness'
              initialDelaySeconds: 60
              tcpSocket: {port: 80}
              failureThreshold: 10
              timeoutSeconds: 240
              periodSeconds: 240
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}





resource appFrontend 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'front'
  location: resourceGroup().location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${kvSecretReaderIdentityId}': {}
      '${appDatabaseIdentityId}': {}
      '${acrIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: false
        transport: 'http'
        targetPort: 80
        // customDomains: [
        //   {
        //     name: 'auctionhouse.pekalam.store'
        //     bindingType: 'Disabled'
        //   }
        // ]
      }
      registries: [
        // {
        //   server: 'index.docker.io'
        //   username: 'pekalam'
        //   passwordSecretRef: dockerhubPasswordSecretName
        // }
        {
          server: privateContainerRegistry
          identity: acrIdentityId
        }
      ]
      secrets: [
        // {
        //   identity: kvSecretReaderIdentityId
        //   keyVaultUrl: registryPassSecret.properties.secretUri
        //   name: dockerhubPasswordSecretName
        // }
      ]
    }
    template: {
      containers: [
        {
          name: 'auctionhousefront'
          image: '${defaultRegistry}pekalam/auctionhouse-front'
          resources: {
            cpu: '0.25'
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Readiness'
              initialDelaySeconds: 60
              tcpSocket: {port: 80}
              failureThreshold: 10
              timeoutSeconds: 240
              periodSeconds: 240
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}
