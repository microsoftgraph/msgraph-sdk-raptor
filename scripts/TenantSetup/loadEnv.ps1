# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
$envFile = "env.json"
$env = @{}
if (Test-Path -Path (Join-Path $PSScriptRoot $envFile)) {
    $env = Get-Content (Join-Path $PSScriptRoot $envFile) | ConvertFrom-Json
}
else {
    $env.MsGraphTenantIdentifier = ${env:MSGRAPHTENANTIDENTIFIER}
    $env.MsGraphClientIdentifier = ${env:MSGRAPHCLIENTIDENTIFIER}
    $env.MsGraphCertificateThumbprint = ${env:MSGRAPHCERTIFICATETHUMBPRINT}

    $env.AzureTenantIdentifier = ${env:AZURETENANTIDENTIFIER}
    $env.AzureClientIdentifier = ${env:AZURECLIENTIDENTIFIER}
    $env.AzureCertificateThumbprint = ${env:AZURECERTIFICATETHUMBPRINT}

    #DefaultUserIdentifier
    $env.DefaultUserIdentifier = ${env:DEFAULTUSERIDENTIFIER}
}
$PSDefaultParameterValues = @{
    "Connect-MgGraph:TenantId"                = $env.MsGraphTenantIdentifier;
    "Connect-MgGraph:ClientId"                = $env.MsGraphClientIdentifier;
    "Connect-MgGraph:CertificateThumbprint"   = $env.MsGraphCertificateThumbprint;
    "Connect-AzAccount:TenantId"              = $env.AzureTenantIdentifier;
    "Connect-AzAccount:ApplicationId"         = $env.AzureClientIdentifier;
    "Connect-AzAccount:CertificateThumbprint" = $env.AzureCertificateThumbprint;
    "*:UserId"                                = $env.DefaultUserIdentifier
}
function Setup-Environment() {
    return $env
}