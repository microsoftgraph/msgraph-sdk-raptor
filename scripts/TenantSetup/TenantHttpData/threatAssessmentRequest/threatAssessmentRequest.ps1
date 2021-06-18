# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath


if($null -eq $threatAssessmentRequest) {
    $threatAssessmentRequestContent = Get-RequestData -ChildEntity "threatAssessmentRequest"
    $threatAssessmentRequest = Request-DelegatedResource -Uri "informationProtection/threatAssessmentRequests" -Method "POST" -Body $threatAssessmentRequestContent
    $threatAssessmentRequest.id
}
$identifiers.threatAssessmentRequest._value = $threatAssessmentRequest.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
