# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
function Install-MicrosoftGraph() {
    if (-not (Get-Module -FullyQualifiedName @{ModuleName="Microsoft.Graph.Authentication"; ModuleVersion="1.5.0"} -ListAvailable)) {
        Install-Module Microsoft.Graph.Authentication -Force -AllowClobber -Scope CurrentUser -RequiredVersion 1.5.0
        Import-Module Microsoft.Graph.Authentication -Force
    }
}

function Install-Az() {
    if (-not (Get-Module Az -ListAvailable)) {
        Install-Module Az -Force -AllowClobber -Scope CurrentUser -Scope CurrentUser
    }
}

function Install-ApiDocHttpParser() {
    $apiDocPath = Join-Path $PSScriptRoot -ChildPath "apiDoctor"
    if (-not (Test-Path $apiDocPath)) {
        Install-Package -Name ApiDoctor -ProviderName Nuget -Scope CurrentUser -Destination $apiDocPath -Force
        $pkgfolder = Get-ChildItem -LiteralPath $apidocPath -Directory | Where-Object { $_.name -match "ApiDoctor" }
        $apiDocHttpValidation = [System.IO.Path]::Combine($apidocPath, $pkgfolder.Name, "tools\ApiDoctor.Validation.dll")
        [System.Reflection.Assembly]::LoadFrom($apiDocHttpValidation)
    }
    else {
        $pkgfolder = Get-ChildItem -LiteralPath $apidocPath -Directory | Where-Object { $_.name -match "ApiDoctor" }
        $apiDocHttpValidation = [System.IO.Path]::Combine($apidocPath, $pkgfolder.Name, "tools\ApiDoctor.Validation.dll")
        [System.Reflection.Assembly]::LoadFrom($apiDocHttpValidation)   
    }
    
}
