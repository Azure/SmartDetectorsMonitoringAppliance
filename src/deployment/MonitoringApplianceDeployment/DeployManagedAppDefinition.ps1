$subscriptionId = 'b4b7d4c1-8c25-4da3-bf1c-e50f647a8130'
$resourceGroup = 'SmartAlerts-Preview'
$storageAccountName = 'smartalertspreview'
$containerName = 'managedapp'
$storageAccount = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroup -Name $storageAccountName

# Generate a zip file containing the managed app resources template
# Service catalog expects the package name to be app.zip
$packageName = "app.zip"
$templateFilePath = [System.IO.Path]::Combine($PSScriptRoot, "mainTemplate.json")
$uiDefinitionFilePath = [System.IO.Path]::Combine($PSScriptRoot, "createUiDefinition.json")
$packagePath = [System.IO.Path]::Combine($PSScriptRoot, $packageName)
Compress-Archive -Path $templateFilePath, $uiDefinitionFilePath -DestinationPath $packagePath -Force

# Upload the package 
$blob = Set-AzureStorageBlobContent -File $packagePath -Container $containerName -Blob $packageName -Context $storageAccount.Context -Force

# Create the managed app definition
$azureMonitoringSmartAlertsGroupId = "45744670-bac4-444c-a8ae-267de77fbf07"
$ownerID = (Get-AzureRmRoleDefinition -Name Owner).Id

#New-AzureRmManagedApplicationDefinition `
#  -Name "SmartDetectorsMonitoringAppliance" `
#  -ResourceGroupName $resourceGroup `
#  -DisplayName "Smart Detectors Monitoring Appliance" `
#  -Location "eastus" `
#  -LockLevel ReadOnly `
#  -Description "This application will execute and manage the Smart Detectors" `
#  -Authorization "${nettaDirectGroupId}:$ownerID" `
#  -PackageFileUri $blob.ICloudBlob.StorageUri.PrimaryUri.AbsoluteUri

Set-AzureRmManagedApplicationDefinition `
  -Id "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.Solutions/applicationDefinitions/SmartDetectorsMonitoringAppliance" `
  -DisplayName "Smart Detectors Monitoring Appliance" `
  -Description "This application will execute and manage the Smart Detectors" `
  -Authorization "${azureMonitoringSmartAlertsGroupId}:$ownerID" `
  -PackageFileUri $blob.ICloudBlob.StorageUri.PrimaryUri.AbsoluteUri

# Delete the package
Remove-Item -Path $packagePath -Force