# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

<#
.SYNOPSIS
  This script updates an AAD application manifest file to have all read-only permissions to Microsoft.Graph
.DESCRIPTION
  - Fetches all Microsoft.Graph permissions
  - Selects read-only permissions
  - Updates application manifest file to have read-only permissions
.EXAMPLE
  In-place manifest update
  PS C:\> applyReadOnlyPermissionsToManifest.ps1 -manifestPath <path_to_manifest>

  You can also specify optional outputPath if you don't want in-place update
  PS C:\> applyReadOnlyPermissionsToManifest.ps1 -manifestPath <path_to_manifest> -outputPath <path_to_output>
.NOTES
  Manifest file can be downloaded from Azure Portal -> Application -> Manifest
  On the same Manifest page, you can upload the manifest file modified with this script
  When you switch back to API Permissions page, you will see that all read-only
  Microsoft.Graph permissions will be given to your application
  For admin consent, you will still need to click on "Grant admin consent for Contoso" button.
#>

Param(
  [Parameter(Mandatory)]
  [string]$manifestPath,
  [string]$outputPath
)

if (-not (Test-Path $manifestPath))
{
  Write-Error "Manifest file not found in $manifestPath"
  return;
}

$manifest = Get-Content $manifestPath |
  ConvertFrom-Json

if (!$manifest.requiredResourceAccess.resourceAppId)
{
  Write-Error "Given file is not a manifest file that we can modify. It needs to have requiredResourcesAccess.resourceAppId field already defined!"
  return;
}

# fetch permissions from microsoft-graph-devx-content
$permissionsFile = "https://raw.githubusercontent.com/microsoftgraph/microsoft-graph-devx-content/dev/permissions/permissions-descriptions.json"
$permissions = (Invoke-WebRequest -Uri $permissionsFile -UseBasicParsing).Content | ConvertFrom-Json

# filter read only permissions
$readOnlyDelegatedPermissions = $permissions.delegatedScopesList |
  Where-Object { $_.value.Contains("Read") -and !$_.value.Contains("Write") }

$readOnlyApplicationPermissions = $permissions.applicationScopesList |
  Where-Object { $_.value.Contains("Read") -and !$_.value.Contains("Write") }

# these IDs are not known to the Microsoft.Graph permissions list. It errors in Azure Portal
$unknownIds = "d8e4ec18-f6c0-4620-8122-c8b1f2bf400e","2f3e6f8c-093b-4c57-a58b-ba5ce494a169" # AgreementAcceptance.Read.All Agreement.Read.All

# create the ArrayList containing permissions in the structure of Azure Portal application manifest file
# e.g.
# "requiredResourceAccess": [
#    {
#      "resourceAppId":  "00000003-0000-0000-c000-000000000000",
#      "resourceAccess": [
#        {
#            "id":  "99201db3-7652-4d5a-809a-bdb94f85fe3c",
#            "type":  "Scope"
#        },
#        ...
#      ]
#    }
#  ]
$resourceAccess = New-Object System.Collections.ArrayList
$countDelegated = $readOnlyDelegatedPermissions |
  ForEach-Object {
    $accessObject = @{
      id = $_.id;
      type = "Scope"
    }
    $resourceAccess.Add($accessObject)
  } |
  Measure-Object |
  Select-Object -ExpandProperty Count

$countApplication = $readOnlyApplicationPermissions |
  Where-Object { $unknownIds -notcontains $_.id } |
  ForEach-Object {
    $accessObject = @{
      id = $_.id;
      type = "Role"
    }
    $resourceAccess.Add($accessObject)
  } |
  Measure-Object |
  Select-Object -ExpandProperty Count

$manifest.requiredResourceAccess[0].resourceAccess = $resourceAccess

if (!$outputPath)
{
  $outputPath = $manifestPath
}

$manifest |
  ConvertTo-Json -Depth 10 |
  Out-File -FilePath $outputPath

Write-Host "Updated manifest file with $countDelegated delegated and $countApplication application permissions!" -ForegroundColor Green