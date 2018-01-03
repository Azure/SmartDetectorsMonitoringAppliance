Param(
    [string] [Parameter(Mandatory=$true)] $PackageFilePath
)

function Get-Manifest
{
    param(
        [Parameter(Mandatory=$true)]
        [string]$PackageFilePath
    )
    
    if (![System.IO.File]::Exists($PackageFilePath))
    {
        Throw New-Object System.ArgumentException -ArgumentList "Package file does not exists"
    }
    
    # Extract manifest file
    Add-Type -Assembly "System.IO.Compression.FileSystem"
    $archive = [System.IO.Compression.ZipFile]::OpenRead($PackageFilePath)
    try 
    {
        $manifestEntry = $archive.GetEntry("manifest.json")
        if (!$manifestEntry) 
        {
            Throw New-Object System.ArgumentException -ArgumentList "Package file does not contain manifest.json file"
        }
        
        $manifestStream = $manifestEntry.Open()
        $manifesStreamtReader = New-Object System.IO.StreamReader -ArgumentList $manifestStream
        try 
        {
            $manifestHashtable = @{}
            $manifestText = $manifesStreamtReader.ReadToEnd()
            (ConvertFrom-Json $manifestText).PSObject.Properties | foreach { if ($_.Value.GetType() -eq [System.String]) { $manifestHashtable[$_.Name] = $_.Value } else { $manifestHashtable[$_.Name] = (ConvertTo-Json $_.Value) -replace "`t|`n|`r|` ","" } }
            return $manifestHashtable
        }
        finally 
        {
            $manifesStreamtReader.Dispose()
        }
    }
    finally 
    {
        $archive.Dispose()
    }
}

function Validate-CorrectPackageVersion
{
    param(
        [Parameter(Mandatory=$true)]
        [string]$SignalId,
        
        [Parameter(Mandatory=$true)]
        [System.Version]$CurrentPackageVersion,
    
        [Parameter(Mandatory=$true)]
        [string]$ContainerName,
        
        [Parameter(Mandatory=$true)]
        [Microsoft.Azure.Commands.Management.Storage.Models.PSStorageAccount]$StorageAccount
    )
    
    $blobs = Get-AzureStorageBlob -Container $ContainerName -Context $StorageAccount.Context -Prefix "$SignalId/"
    $blobsWithVersion = [System.Linq.Enumerable]::Where($blobs, [Func[object,bool]]{ param($blob) $blob.ICloudBlob.Metadata.ContainsKey("version") })
    $blobsOrederedByVersion = [System.Linq.Enumerable]::OrderByDescending($blobsWithVersion, [Func[object,System.Version]]{ param($blob) [System.Version]::Parse($blob.ICloudBlob.Metadata["version"]) })
    $latestBlob = [System.Linq.Enumerable]::FirstOrDefault($blobsOrederedByVersion)
    if ($latestBlob)
    {
        # Verifiying that current package is a later version of the latest existing signal version
        $latestVersion = [System.Version]::Parse($latestBlob.ICloudBlob.Metadata["version"])
        if ($latestVersion -gt $CurrentPackageVersion)
        {
            Throw New-Object System.ArgumentException -ArgumentList "current package version should be greater than existing deployed version"
        }
    } 
}

#########################
# 		 MAIN           #
#########################

# Get the manifest and signal ID
$manifestHashtable = Get-Manifest -PackageFilePath $PackageFilePath
$signalId = $manifestHashtable['id']
if (!$signalId)
{
    Throw New-Object System.ArgumentException -ArgumentList "manifest file does not contain signal ID"
}

$currentPackageVersion = [System.Version]::Parse($manifestHashtable['version'])
if (!$currentPackageVersion)
{
    Throw New-Object System.ArgumentException -ArgumentList "manifest file does not contain a valid version"
}

# Login and set Azure subscription context
Import-Module AzureRm
Login-AzureRmAccount -SubscriptionId b4b7d4c1-8c25-4da3-bf1c-e50f647a8130

# Get signals storage account
$resourceGroup = 'SmartSignalsDev'
$storageAccountName = 'globalsmartsignals'
$containerName = 'signals'
$storageAccount = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroup -Name $storageAccountName

# Verify current package has a greater version
Validate-CorrectPackageVersion -SignalId $signalId -CurrentPackageVersion $currentPackageVersion -ContainerName $containerName -StorageAccount $storageAccount

# Upload blob
$fileName = [System.IO.Path]::GetFileName($PackageFilePath)
$blobName = "$signalId/$fileName.v$currentPackageVersion"
Set-AzureStorageBlobContent -Container $containerName -Context $storageAccount.Context -Blob $blobName -BlobType Block -Metadata $manifestHashtable -File $PackageFilePath
