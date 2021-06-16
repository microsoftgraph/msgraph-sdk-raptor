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

#Connect To Microsoft Graph Using ClientId, TenantId and Certificate
Connect-MgGraph -CertificateThumbprint $appSettings.CertificateThumbprint -ClientId $appSettings.ClientID -TenantId $appSettings.TenantID

#Create activityBasedTimeoutPolicy https://docs.microsoft.com/en-us/graph/api/activitybasedtimeoutpolicy-post-activitybasedtimeoutpolicies?view=graph-rest-1.0&tabs=http
$activityBasedTimeoutPolicyData = Get-RequestData -ChildEntity "ActivityBasedTimeoutPolicy"
$currentActivityBasedTimeoutPolicyPolicy = Invoke-RequestHelper -Uri "policies/activityBasedTimeoutPolicies" -Method GET |
        Where-Object { $_.displayName -eq $activityBasedTimeoutPolicyData.displayName } |
        Select-Object -First 1

if($null -eq $currentActivityBasedTimeoutPolicyPolicy){
    $currentActivityBasedTimeoutPolicyPolicy = Invoke-RequestHelper -Uri "policies/activityBasedTimeoutPolicies" -Method POST -Body $activityBasedTimeoutPolicyData
    $currentActivityBasedTimeoutPolicyPolicy.id
}

$claimsMappingPolicyData = Get-RequestData -ChildEntity "ClaimsMappingPolicy"
$currentClaimsMappingPolicy = Invoke-RequestHelper -Uri "policies/claimsMappingPolicies" -Method GET |
        Where-Object { $_.displayName -eq $claimsMappingPolicyData.displayName } |
        Select-Object -First 1

if($null -eq $currentClaimsMappingPolicy){
    $currentClaimsMappingPolicy = Invoke-RequestHelper -Uri "policies/claimsMappingPolicies" -Method POST -Body $claimsMappingPolicyData
    $currentClaimsMappingPolicy.id
}

$homeRealmDiscoveryPolicyData = Get-RequestData -ChildEntity "HomeRealmDiscoveryPolicy"
$currentHomeRealmDiscoveryPolicyData = Invoke-RequestHelper -Uri "policies/homeRealmDiscoveryPolicies" -Method GET |
        Where-Object { $_.displayName -eq $homeRealmDiscoveryPolicyData.displayName } |
        Select-Object -First 1

if($null -eq $currentHomeRealmDiscoveryPolicyData){
    $currentHomeRealmDiscoveryPolicyData = Invoke-RequestHelper -Uri "policies/homeRealmDiscoveryPolicies" -Method POST -Body $homeRealmDiscoveryPolicyData
    $currentHomeRealmDiscoveryPolicyData.id
}

$tokenIssuancePolicyData = Get-RequestData -ChildEntity "TokenIssuancePolicy"
$currentTokenIssuancePolicy = Invoke-RequestHelper -Uri "policies/tokenIssuancePolicies" -Method GET |
        Where-Object { $_.displayName -eq $tokenIssuancePolicyData.displayName } |
        Select-Object -First 1

if($null -eq $currentTokenIssuancePolicy){
    $currentTokenIssuancePolicy = Invoke-RequestHelper -Uri "policies/tokenIssuancePolicies" -Method POST -Body $tokenIssuancePolicyData
    $currentTokenIssuancePolicy.id
}

$identifiers.activityBasedTimeoutPolicy._value = $currentActivityBasedTimeoutPolicyPolicy.id
$identifiers.claimsMappingPolicy._value = $currentClaimsMappingPolicy.id
$identifiers.homeRealmDiscoveryPolicy._value = $currentHomeRealmDiscoveryPolicyData.id
$identifiers.tokenIssuancePolicy._value = $currentHomeRealmDiscoveryPolicyData.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath