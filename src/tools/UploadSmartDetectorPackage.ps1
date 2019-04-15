Param(
    [string] [Parameter(Mandatory=$true)] $PackageFilePath,
    [string] [Parameter(Mandatory=$true)] [ValidateSet('PROD', 'DOGFOOD', 'FAIRFAX')] $Environment,
    [switch] $Force = $false
)

function Get-Manifest
{
    param(
        [Parameter(Mandatory=$true)]
        [string]$PackageContentPath
    )
    
	$manifestFile = [System.IO.Path]::Combine($PackageContentPath, "Manifest.json");
	if (![System.IO.File]::Exists($manifestFile)) 
	{
		$manifestFile = [System.IO.Path]::Combine($PackageContentPath, "manifest.json");
		if (![System.IO.File]::Exists($manifestFile)) 
		{
			Throw New-Object System.ArgumentException -ArgumentList "Package file does not contain Manifest.json file"
		}
	}

	$manifestText = [System.IO.File]::ReadAllText($manifestFile)
	$manifestHashtable = @{}
    (ConvertFrom-Json $manifestText).PSObject.Properties | foreach { if ($_.Value.GetType() -eq [System.String]) { $manifestHashtable[$_.Name] = $_.Value } else { $manifestHashtable[$_.Name] = (ConvertTo-Json $_.Value) -replace "`t|`n|`r|` ","" } }
    return $manifestHashtable
}

function Validate-CorrectPackageVersion
{
    param(
        [Parameter(Mandatory=$true)]
        [string]$SmartDetectorId,
        
        [Parameter(Mandatory=$true)]
        [System.Version]$CurrentPackageVersion,
    
        [Parameter(Mandatory=$true)]
        [string]$ContainerName,
        
        [Parameter(Mandatory=$true)]
        [Microsoft.Azure.Commands.Management.Storage.Models.PSStorageAccount]$StorageAccount
    )
    
    $blobs = Get-AzureStorageBlob -Container $ContainerName -Context $StorageAccount.Context -Prefix "$SmartDetectorId/"
	if (!$blobs) {
		# No older versions found
		return
	}

    $blobsWithVersion = $blobs | Where-Object { $_.ICloudBlob.Metadata.ContainsKey("version") }
	if (!$blobsWithVersion) {
		# No older versions found
		return
	}

    $blobsOrederedByVersion = $blobsWithVersion | Sort-Object -Property @{Expression = {$_.ICloudBlob.Metadata["version"]}; Descending = $True}
    $latestBlob = $blobsOrederedByVersion | Select-Object -First 1
    if ($latestBlob)
    {
        # Verifiying that current package version is not smaller than the version of the latest existing Smart Detector
        $latestVersion = [System.Version]::Parse($latestBlob.ICloudBlob.Metadata["version"])
        if ($latestVersion -gt $CurrentPackageVersion)
        {
            Throw New-Object System.ArgumentException -ArgumentList "Current package version should be greater than the existing deployed version"
        }
    }
}

#########################
# 		 MAIN           #
#########################

# Verify package exists
if (![System.IO.File]::Exists($PackageFilePath))
{
    Throw New-Object System.ArgumentException -ArgumentList "Package file does not exists"
}

# get storage context based on environment
if ($Environment -eq "PROD")
{
    $subscriptionId = '2266ad8e-1dfd-40a7-bd22-350ba2f0080a'
    $resourceGroup = 'AMS-SmartAlerts-EUS'
    $storageAccountName = 'globalsmartdetectors'
}
elseif ($Environment -eq "DOGFOOD")
{
    $subscriptionId = '07f7ce71-7ee8-4291-af73-7b68a64e0b41'
    $resourceGroup = 'SmartDetectorAlertsRuleEngineINT'
    $storageAccountName = 'globalsmartdetectorsint'
}
elseif ($Environment -eq "FAIRFAX")
{
    $subscriptionId = '7f9906c0-20c2-437a-8f76-7402230c9c98'
    $resourceGroup = 'SmartAlerts-Fairfax-Global'
    $storageAccountName = 'globalsmartdetectorsff'
}
else
{
    Throw New-Object System.ArgumentException -ArgumentList "Environment $Environment is not supported"
}

# Extract package content
Add-Type -AssemblyName System.IO.Compression.FileSystem
$directoryPath = [System.IO.Path]::GetDirectoryName($PackageFilePath)
$tempDirectoryName = [System.Guid]::NewGuid().ToString()
$tempDirectoryPath = [System.IO.Path]::Combine($directoryPath, $tempDirectoryName);
[System.IO.Compression.ZipFile]::ExtractToDirectory($PackageFilePath, $tempDirectoryPath)

try 
{
	# Get the manifest and Smart Detector ID
	$manifestHashtable = Get-Manifest -PackageContentPath $tempDirectoryPath
	$smartDetectorId = $manifestHashtable['id']
	if (!$smartDetectorId)
	{
		Throw New-Object System.ArgumentException -ArgumentList "manifest file does not contain Smart Detector ID"
	}

	$currentPackageVersion = [System.Version]::Parse($manifestHashtable['version'])
	if (!$currentPackageVersion)
	{
	    Throw New-Object System.ArgumentException -ArgumentList "manifest file does not contain a valid version"
	}
	
	# Login and set Azure subscription context
	if ([string]::IsNullOrEmpty($(Get-AzureRmContext).Account))
	{
	    Login-AzureRmAccount -SubscriptionId $subscriptionId
	}
	else 
	{
	    Set-AzureRmContext -SubscriptionId $subscriptionId
	}
	
	# Get Smart Detectors storage account
	$containerName = 'detectors'
	$storageAccount = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroup -Name $storageAccountName
	
	# Verify current package has a greater version
	Validate-CorrectPackageVersion -SmartDetectorId $smartDetectorId -CurrentPackageVersion $currentPackageVersion -ContainerName $containerName -StorageAccount $storageAccount
	
	# Upload images to the blob storage
	$blobDirectory = "$smartDetectorId/v$currentPackageVersion"
	$imagePathsString = $manifestHashtable['imagePaths']
	if ($imagePathsString)
	{
		$imagesToUpload = @()
		$imagePaths = [Newtonsoft.Json.Linq.JArray]::Parse($imagePathsString)
		ForEach ($jTokenImage in $imagePaths)
		{
			$relativeImagePath = $jTokenImage.ToString().Replace("/", "\\")
			$fullImageFilePath = [System.IO.Path]::Combine($tempDirectoryPath, $relativeImagePath);
			if (![System.IO.File]::Exists($fullImageFilePath))
			{
				Throw New-Object System.ArgumentException -ArgumentList "image file does not exists"
			}

			$imagesToUpload += $relativeImagePath
		}
		
		ForEach ($relativeImagePath in $imagesToUpload)
		{
			$blobName = "$blobDirectory/$relativeImagePath".Replace("\\", "/")
			$fullImageFilePath = [System.IO.Path]::Combine($tempDirectoryPath, $relativeImagePath);
			Set-AzureStorageBlobContent -Container $containerName -Context $storageAccount.Context -Blob $blobName -BlobType Block -File $fullImageFilePath -Force:$Force
		}
	}
	
	# Upload package blob
	$fileName = [System.IO.Path]::GetFileName($PackageFilePath)
	$blobName = "$blobDirectory/$fileName"
	Set-AzureStorageBlobContent -Container $containerName -Context $storageAccount.Context -Blob $blobName -BlobType Block -Metadata $manifestHashtable -File $PackageFilePath -Force:$Force
}
finally
{
	#Delete temp folder
	Remove-Item –path $tempDirectoryPath -Recurse -Force
}
