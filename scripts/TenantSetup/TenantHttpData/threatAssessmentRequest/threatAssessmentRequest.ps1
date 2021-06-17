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


if($null -eq $threatAssessmentRequest) {
    $threatAssessmentRequestContent = Get-RequestData -ChildEntity "threatAssessmentRequest"
    $threatAssessmentRequest = reqDelegated -url "informationProtection/threatAssessmentRequests" -method "POST" -body $threatAssessmentRequestContent
    $threatAssessmentRequest.id
}
$identifiers.threatAssessmentRequest._value = $threatAssessmentRequest.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath