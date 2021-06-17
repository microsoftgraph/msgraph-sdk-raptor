# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $AppSettingsPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/appsettings.json" -Resolve),
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$appSettings = Get-AppSettings -AppSettingsPath $AppSettingsPath
$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath
$domain = Get-CurrentDomain -AppSettings $appSettings


$userId = $identifiers.user._value
$userId

if($null -eq $dataPolicyOperation) {
    $dataPolicyOperationContent = Get-RequestData -ChildEntity "dataPolicyOperation"
    $dataPolicyOperationContent.storageLocation = $appSettings.SASUrl
    $dataPolicyOperation = Request-DelegatedResource -Uri "users/$($userId)/exportPersonalData" -Method "Post" -Body $dataPolicyOperationContent
    $dataPolicyOperation.Location
    $dataPolicyOperation_id = $dataPolicyOperation.Location -split "/" | Select-Object -last 1
}
$identifiers.dataPolicyOperation._value = $dataPolicyOperation_id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
