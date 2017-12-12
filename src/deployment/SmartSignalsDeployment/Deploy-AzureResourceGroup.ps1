#Requires -Version 3.0

Param(
    [string] $TemplateParametersFile = 'SmartSignals.dev.parameters.json',
    [string] $ArtifactStagingDirectory = '.',
    [switch] $UploadArtifact,
    [switch] $ValidateOnly
)

try {
    [Microsoft.Azure.Common.Authentication.AzureSession]::ClientFactory.AddUserAgent("VSAzureTools-$UI$($host.name)".replace(' ','_'), '3.0.0')
} catch { }

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3

$ScriptRoot = $PSScriptRoot

function Format-ValidationOutput {
    param ($ValidationOutput, [int] $Depth = 0)
    Set-StrictMode -Off
    return @($ValidationOutput | Where-Object { $_ -ne $null } | ForEach-Object { @('  ' * $Depth + ': ' + $_.Message) + @(Format-ValidationOutput @($_.Details) ($Depth + 1)) })
}

Write-Host "Got template param file $TemplateParametersFile"
$TemplateParametersFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($ScriptRoot, $TemplateParametersFile))
Write-Host "Full path of template param file $TemplateParametersFile"

# Parse the parameter file
$TemplateParameters = Get-Content $TemplateParametersFile -Raw | ConvertFrom-Json
$DeploymentMetadata = $TemplateParameters.ServiceMetadata

$TemplateFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($ScriptRoot, $DeploymentMetadata.ArmTemplatePath.value))
Write-Host "Got template file $TemplateFile"

$SubscriptionId = $DeploymentMetadata.AzureSubscriptionId.value
Select-AzureRmSubscription -SubscriptionId $SubscriptionId

$OptionalParameters = New-Object -TypeName Hashtable
if ($UploadArtifact) {
    
	# Create deployment resource group if doesn't exist
    $DeploymentResourceGroupName = "SmartSignals-Deployment"
    $DeploymentResourceGroupLocation = "southcentralus"
    New-AzureRmResourceGroup -Name $DeploymentResourceGroupName -Location $DeploymentResourceGroupLocation -Verbose -Force
    
    # Create the deployment storage account and container if it doesn't already exist
    $Env = $DeploymentMetadata.Environment.Value.ToLower()
    $DeploymentStorageAccountName = "smartsignalsdep$Env"
    $DeploymentStorageContainerName = 'deploymentartifacts'
        
    $StorageAccount = (Get-AzureRmStorageAccount | Where-Object{$_.StorageAccountName -eq $DeploymentStorageAccountName})
    if ($StorageAccount -eq $null) {
        $StorageAccount = New-AzureRmStorageAccount -StorageAccountName $DeploymentStorageAccountName -Type 'Standard_LRS' -ResourceGroupName $DeploymentResourceGroupName -Location $DeploymentResourceGroupLocation
    }
    
    New-AzureStorageContainer -Name $DeploymentStorageContainerName -Context $StorageAccount.Context -ErrorAction SilentlyContinue *>&1
    
    # Copy files from the local storage staging location to the storage account container
    $PacakgeName = $TemplateParameters.parameters.PackageLink.value
    $ArtifactStagingDirectory = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($ScriptRoot, $ArtifactStagingDirectory))
    $SourcePath = [System.IO.Path]::Combine($ArtifactStagingDirectory, $PacakgeName)
    
    Write-Host "Uploading from source path $SourcePath"
    Set-AzureStorageBlobContent -File $SourcePath -Blob $PacakgeName -Container $DeploymentStorageContainerName -Context $StorageAccount.Context -Force

	# generate package link
    $SasToken = New-AzureStorageContainerSASToken -Container $DeploymentStorageContainerName -Context $StorageAccount.Context -Permission r -ExpiryTime (Get-Date).AddHours(4)    
    $PackageLink = $StorageAccount.Context.BlobEndPoint + $DeploymentStorageContainerName + '/' + $PacakgeName + $SasToken
    
    $OptionalParameters['PackageLink'] = ConvertTo-SecureString -AsPlainText -Force $PackageLink
}

$ResourceGroupName = $DeploymentMetadata.AzureResourceGroupName.value

if ($ValidateOnly) {
    $ErrorMessages = Format-ValidationOutput (Test-AzureRmResourceGroupDeployment -ResourceGroupName $ResourceGroupName `
                                                                                  -TemplateFile $TemplateFile `
                                                                                  -TemplateParameterFile $TemplateParametersFile `
                                                                                  @OptionalParameters)
    if ($ErrorMessages) {
        Write-Output '', 'Validation returned the following errors:', @($ErrorMessages), '', 'Template is invalid.'
    }
    else {
        Write-Output '', 'Template is valid.'
    }
}
else {
    New-AzureRmResourceGroupDeployment -Name ((Get-ChildItem $TemplateFile).BaseName + '-' + ((Get-Date).ToUniversalTime()).ToString('yyyyMMddHHmmss')) `
                                       -ResourceGroupName $ResourceGroupName `
                                       -TemplateFile $TemplateFile `
                                       -TemplateParameterFile $TemplateParametersFile `
                                       @OptionalParameters `
                                       -Force -Verbose `
                                       -ErrorVariable ErrorMessages
    if ($ErrorMessages) {
        Write-Output '', 'Template deployment returned the following errors:', @(@($ErrorMessages) | ForEach-Object { $_.Exception.Message.TrimEnd("`r`n") })
    }
}