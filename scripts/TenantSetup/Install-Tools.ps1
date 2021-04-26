# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
function Install-MicrosoftGraph() {
    if (-not (Get-Module Microsoft.Graph.Authentication -ListAvailable)) {
        Install-Module Microsoft.Graph.Authentication -Force -Scope CurrentUser
    }
}

function Install-Az() {
    if (-not (Get-Module Az -ListAvailable)) {
        Install-Module Az -Force -Scope CurrentUser -Scope CurrentUser
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
}