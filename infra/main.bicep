targetScope = 'subscription'

param rgName string
param location string

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: rgName
  location: location
}

module webapp './webapp.bicep' = {
  name: 'webapp'
  scope: resourceGroup(rg.name)
  params: {
    location: location
  }
}

output appName string = webapp.outputs.appName