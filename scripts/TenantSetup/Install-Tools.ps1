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

function Install-MicrosoftGraph(){
    if(-not (Get-Module Microsoft.Graph.Authentication -ListAvailable)){
        Install-Module Microsoft.Graph.Authentication -Force
    }
}

function Install-Az(){
    if(-not (Get-Module Az -ListAvailable)){
        Install-Module Az -Force
    }
}

function Install-ApiDocHttpParser(){
    $apiDocPath = Join-Path $PSScriptRoot -ChildPath "apiDoctor"
    $pkgfolder = Get-ChildItem -LiteralPath $apidocPath -Directory | Where-Object {$_.name -match "ApiDoctor"}
    $apiDocHttpValidation = [System.IO.Path]::Combine($apidocPath, $pkgfolder.Name, "tools\ApiDoctor.Validation.dll")
    if(-not (Test-Path $apiDocHttpValidation)){
    Install-Package -Name ApiDoctor -ProviderName Nuget -Scope CurrentUser -Destination $apiDocPath -Force
    [System.Reflection.Assembly]::LoadFrom($apiDocHttpValidation)
    }
}