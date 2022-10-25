param location string

resource hostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: toLower('hp${uniqueString(resourceGroup().id)}')
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: 'B1'
  }
  kind: 'linux'
}

resource website 'Microsoft.Web/sites@2021-03-01' = {
  name: toLower('as${uniqueString(resourceGroup().id)}')
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|6.0'
    }
  }
}

output appName string = website.name