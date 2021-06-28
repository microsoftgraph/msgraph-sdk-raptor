# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath


$intgData = Get-RequestData -ChildEntity "workforceIntegration"
$intg = Request-DelegatedResource -Uri teamwork/workforceintegrations -Method "POST" -Body $intgData
$intg.id
$identifiers.workforceIntegration._value = $intg.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
