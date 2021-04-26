# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
$envFile = "env.json"
$envData = @{}
if (Test-Path -Path (Join-Path $PSScriptRoot $envFile)) {
    $envData = Get-Content (Join-Path $PSScriptRoot $envFile) | ConvertFrom-Json
}
else {
    $envData.MsGraphTenantIdentifier = ${env:MSGRAPH_TENANT_IDENTIFIER}
    $envData.MsGraphClientIdentifier = ${env:MSGRAPH_CLIENT_IDENTIFIER}
    $envData.MsGraphCertificateThumbprint = ${env:MSGRAPH_CERTIFICATE_THUMBPRINT}

    $envData.AzureTenantIdentifier = ${env:AZURE_TENANT_IDENTIFIER}
    $envData.AzureClientIdentifier = ${env:AZURE_CLIENT_IDENTIFIER}
    $envData.AzureCertificateThumbprint = ${env:AZURE_CERTIFICATE_THUMBPRINT}

    #DefaultUserIdentifier
    $envData.DefaultUserIdentifier = ${env:DEFAULT_USER_IDENTIFIER}
}

$PSDefaultParameterValues = @{
    "Connect-MgGraph:TenantId"                = $envData.MsGraphTenantIdentifier;
    "Connect-MgGraph:ClientId"                = $envData.MsGraphClientIdentifier;
    "Connect-MgGraph:CertificateThumbprint"   = $envData.MsGraphCertificateThumbprint;
    "Connect-AzAccount:TenantId"              = $envData.AzureTenantIdentifier;
    "Connect-AzAccount:ApplicationId"         = $envData.AzureClientIdentifier;
    "Connect-AzAccount:CertificateThumbprint" = $envData.AzureCertificateThumbprint;
    "*:UserId"                                = $envData.DefaultUserIdentifier
}
function Setup-Environment() {
    return $envData
}