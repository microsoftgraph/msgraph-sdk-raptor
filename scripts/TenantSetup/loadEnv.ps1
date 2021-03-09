# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

$envFile = "env.json"
$env = @{}
if(Test-Path -Path (Join-Path $PSScriptRoot $envFile)){
    $env = Get-Content (Join-Path $PSScriptRoot $envFile) | ConvertFrom-Json
} else {
    $env.MsGraphTenantIdentifier = ${env:MSGRAPHTENANTIDENTIFIER}
    $env.MsGraphClientIdentifier = ${env:MSGRAPHCLIENTIDENTIFIER}
    $env.MsGraphCertificateThumbprint = ${env:MSGRAPHCERTIFICATETHUMBPRINT}

    $env.AzureTenantIdentifier = ${env:AZURETENANTIDENTIFIER}
    $env.AzureClientIdentifier = ${env:AZURECLIENTIDENTIFIER}
    $env.AzureCertificateThumbprint = ${env:AZURECERTIFICATETHUMBPRINT}

    #DefaultUserIdentifier
    $env.DefaultUserIdentifier = ${env:DEFAULTUSERIDENTIFIER}
}

$PSDefaultParameterValues=@{
"Connect-MgGraph:TenantId"=$env.MsGraphTenantIdentifier;
"Connect-MgGraph:ClientId"=$env.MsGraphClientIdentifier;
"Connect-MgGraph:CertificateThumbprint"=$env.MsGraphCertificateThumbprint;
"Connect-AzAccount:TenantId"=$env.AzureTenantIdentifier;
"Connect-AzAccount:ApplicationId"=$env.AzureClientIdentifier;
"Connect-AzAccount:CertificateThumbprint"=$env.AzureCertificateThumbprint;
"*:UserId"=$env.DefaultUserIdentifier
}