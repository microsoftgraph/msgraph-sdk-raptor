# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
BeforeAll {
    function Get-RaptorIdentifiers() {
        if (Test-Path "RaptorIdentifiers.json") {
            $currentRaptorData = Get-Content "RaptorIdentifiers.json" -Raw |  ConvertFrom-Json -AsHashtable
            return $currentRaptorData ??= Get-IDTree
        }
        else {
            return Get-IDTree
        }
    }
    function Get-MethodAndPrimaryKey($testTag) {
        $tags = $testTag -split ","
        
        return @{
            "PrimaryKey" = $tags[0]
            "Method"     = $tags[1]
        }
    }
    $loadEnvironment = Join-Path $PSScriptRoot 'loadEnv.ps1'
    $installTools = Join-Path $PSScriptRoot 'Install-Tools.ps1'
    $treeUtil = Join-Path $PSScriptRoot 'treeUtils.ps1'
    . $loadEnvironment
    . $installTools
    . $treeUtil

    $environment = Setup-Environment
    $raptorIdentifiers = Get-RaptorIdentifiers

    Install-MicrosoftGraph

    Connect-MgGraph

    Install-ApiDocHttpParser
}

Describe 'Users' -Tag "User" {
    BeforeEach {
        $GETHttpRequest = Get-Content .\TenantHttpData\Users\GET\user.http -Raw
        $parsedGETHttpRequest = [ApiDoctor.Validation.Http.HttpParser]::ParseHttpRequest($GETHttpRequest)
        $POSTHttpRequest = Get-Content .\TenantHttpData\Users\POST\user.http -Raw
        $parsedPOSTHttpRequest = [ApiDoctor.Validation.Http.HttpParser]::ParseHttpRequest($POSTHttpRequest)
    }   
    It "CREATE User" -Tag "userPrincipalName,POST" {
        $method = $parsedGETHttpRequest.Method
        $uri = $parsedGETHttpRequest.Url
        $entityBodyData = $parsedPOSTHttpRequest.Body | ConvertFrom-Json -AsHashtable

        #Execute GET ALL Request
        $entities = Invoke-MgGraphRequest -Method $method -Uri $uri -OutputType PSObject

        $methodAndPrimaryKey = Get-MethodAndPrimaryKey($____Pester.CurrentTest.Tag)
        $primaryKey = $methodAndPrimaryKey["PrimaryKey"]
        #Check if Entity Exists
        $entity = $entities.value | Where-Object -Property $primaryKey -eq $entityBodyData[$primaryKey]

        if ($null -eq $entity) {
            $createMethod = $parsedPOSTHttpRequest.Method
            $createUri = $parsedPOSTHttpRequest.Url
            $createBody = $parsedPOSTHttpRequest.Body | ConvertFrom-Json -AsHashtable
            $createContentType = $parsedPOSTHttpRequest.ContentType
        
            $createdEntity = Invoke-MgGraphRequest -Method $createMethod -Uri $createUri -Body $createBody -ContentType $createContentType -OutputType PSObject
            $createdEntity | Should -Not -BeNullOrEmpty

            $raptorIdentifiers.user._value = $createdEntity.Id
        }
        else {
            $raptorIdentifiers.user._value = $entity.Id
            $message = "Entity Exists: TestName:{0} EntityId: {1}" -f $____Pester.CurrentTest.Name, $raptorIdentifiers.user._value
            Set-ItResult -Skipped -Because $message
        }
    } 
}
Describe 'Applications' -Tag "Application" {
    BeforeEach {
        $GETHttpRequest = Get-Content .\TenantHttpData\Applications\GET\application.http -Raw
        $parsedGETHttpRequest = [ApiDoctor.Validation.Http.HttpParser]::ParseHttpRequest($GETHttpRequest)
        $POSTHttpRequest = Get-Content .\TenantHttpData\Applications\POST\application.http -Raw
        $parsedPOSTHttpRequest = [ApiDoctor.Validation.Http.HttpParser]::ParseHttpRequest($POSTHttpRequest)
    }   
    It "CREATE Application" -Tag "displayName,POST" {
        $method = $parsedGETHttpRequest.Method
        $uri = $parsedGETHttpRequest.Url
        $entityBodyData = $parsedPOSTHttpRequest.Body | ConvertFrom-Json -AsHashtable

        #Execute GET ALL Request
        $entities = Invoke-MgGraphRequest -Method $method -Uri $uri -OutputType PSObject

        $methodAndPrimaryKey = Get-MethodAndPrimaryKey($____Pester.CurrentTest.Tag)
        $primaryKey = $methodAndPrimaryKey["PrimaryKey"]

        #Check if Entity Exists
        $entity = $entities.value | Where-Object -Property $primaryKey -eq $entityBodyData[$primaryKey]

        if ($null -eq $entity) {
            $createMethod = $parsedPOSTHttpRequest.Method
            $createUri = $parsedPOSTHttpRequest.Url
            $createBody = $parsedPOSTHttpRequest.Body | ConvertFrom-Json -AsHashtable
            $createContentType = $parsedPOSTHttpRequest.ContentType
        
            $createdEntity = Invoke-MgGraphRequest -Method $createMethod -Uri $createUri -Body $createBody -ContentType $createContentType -OutputType PSObject
            $createdEntity | Should -Not -BeNullOrEmpty

            $raptorIdentifiers.application._value = $createdEntity.Id
        }
        else {
            $raptorIdentifiers.application._value = $entity.Id
            $message = "Entity Exists: TestName:{0} EntityId: {1}" -f $____Pester.CurrentTest.Name, $raptorIdentifiers.application._value
            Set-ItResult -Skipped -Because $message
        }
    } 
}
AfterAll {
    $raptorIdentifiers | ConvertTo-Json | Out-File "RaptorIdentifiers.json"
}
