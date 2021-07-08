# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath


$subData = Get-RequestData -ChildEntity "subscription"
$subData.expirationDateTime = (Get-Date).AddDays(1).ToString("yyyy-MM-ddThh:mm:ss.0000000Z")
$subscription = Request-DelegatedResource -Uri subscriptions -Method "POST" -Body $subData
$subscription.id
$identifiers.subscription._value = $subscription.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
