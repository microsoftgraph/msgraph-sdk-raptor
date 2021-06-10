# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $AppSettingsPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/appsettings.json"),
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json")
)

$appSettings = Get-Content $AppSettingsPath | ConvertFrom-Json
$identifiers = Get-Content $IdentifiersPath | ConvertFrom-Json
$domain = $appSettings.Username.Split("@")[1]

if (    !$appSettings.CertificateThumbprint `
        -or !$appSettings.ClientID `
        -or !$appSettings.Username `
        -or !$appSettings.Password `
        -or !$appSettings.TenantID) {
    Write-Error -ErrorAction Stop -Message "please provide CertificateThumbprint, ClientID, Username, Password and TenantID in appsettings.json"
}
<#
   Assumes that data is Stored in the following format:-
   Entity
        - ChildEntity.json
        - ChildEntity.json
    Such as:-
    Team
        - OpenShift.json
        - Schedule.json
    Based on the Tree Structure in Identifiers.json
#>
function Get-RequestData {
    param (
        [string] $ChildEntity
    )
    $entityPath = Join-Path $PSScriptRoot "./$($childEntity).json" -Resolve
    $data = Get-Content -Path $entityPath -Raw | ConvertFrom-Json
    return $data
}
<#
    Helpers handles:-
        1. GraphVersion,
        2. MS-APP-ACTS-AS Headers
        3. Content-Type header
        4. HttpMethod

    Basic Validation of Parameters
#>
function Invoke-RequestHelper {
    param(
        [string]
        $Uri,
        [Parameter(Mandatory = $False)]
        [ValidateSet("v1.0", "beta")]
        [string] $GraphVersion = "v1.0",
        [Parameter(Mandatory = $False)]
        [ValidateSet("GET", "POST", "PUT", "PATCH", "DELETE")]
        [string] $Method = "GET",
        $Headers = @{ },
        $Body,
        $User
    )
    #Append Content-Type to headers collection
    #Append "MS-APP-ACTS-AS" to headers collection
    $headers += @{ "Content-Type" = "application/json" }
    if($null -eq $User) {
        $headers += @{"MS-APP-ACTS-AS" = $User }
    }
    #Convert Body to Json
    $jsonData = $body | ConvertTo-Json -Depth 3

    $response = Invoke-MgGraphRequest -Headers $headers -Method $Method -Uri "https://graph.microsoft.com/$GraphVersion/$Uri" -Body $jsonData -OutputType PSObject

    return $response.value ?? $response
}

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

$tokenIssuancePolicyData = Get-RequestData -ChildEntity "TokenIssuancePolicy "
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