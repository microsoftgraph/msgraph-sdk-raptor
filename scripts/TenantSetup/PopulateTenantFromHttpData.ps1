# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
BeforeAll {
    $loadEnvironment = Join-Path $PSScriptRoot 'loadEnv.ps1'
    $installTools = Join-Path $PSScriptRoot 'Install-Tools.ps1'
    $treeUtil = Join-Path $PSScriptRoot 'treeUtils.ps1'
    . $loadEnvironment
    . $installTools
    . $treeUtil

    Install-MicrosoftGraph

    Connect-MgGraph

    Install-ApiDocHttpParser

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
    function Get-HttpRequests($entity){
        Write-Debug $entity
        $GETHttpRequest = Get-Content ".\TenantHttpData\$entity\GET\$entity.http" -Raw
        $parsedGETHttpRequest = [ApiDoctor.Validation.Http.HttpParser]::ParseHttpRequest($GETHttpRequest)
        $POSTHttpRequest = Get-Content ".\TenantHttpData\$entity\POST\$entity.http" -Raw
        $parsedPOSTHttpRequest = [ApiDoctor.Validation.Http.HttpParser]::ParseHttpRequest($POSTHttpRequest)
        return @{
            "GET" = $parsedGETHttpRequest
            "POST" = $parsedPOSTHttpRequest
        }
    }

    $environment = Setup-Environment
    $raptorIdentifiers = Get-RaptorIdentifiers
}

Describe 'User' -Tag "User" {
    BeforeAll {
        $httpRequests = Get-HttpRequests("User")
    }   
    It "CREATE User" -Tag "userPrincipalName,POST" {
        $GETRequest = $httpRequests["GET"]
        $POSTRequest = $httpRequests["POST"]

        $method = $GETRequest.Method
        $uri = $GETRequest.Url
        $entityBodyData = $POSTRequest.Body | ConvertFrom-Json -AsHashtable

        #Execute GET ALL Request
        $entities = Invoke-MgGraphRequest -Method $method -Uri $uri -OutputType PSObject

        $methodAndPrimaryKey = Get-MethodAndPrimaryKey($____Pester.CurrentTest.Tag)
        $primaryKey = $methodAndPrimaryKey["PrimaryKey"]
        #Check if Entity Exists
        $entity = $entities.value | Where-Object -Property $primaryKey -eq $entityBodyData[$primaryKey]

        if ($null -eq $entity) {
            $createMethod = $POSTRequest.Method
            $createUri = $POSTRequest.Url
            $createBody = $POSTRequest.Body | ConvertFrom-Json -AsHashtable
            $createContentType = $POSTRequest.ContentType
        
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
Describe 'Application' -Tag "Application" {
    BeforeAll {
       $httpRequests = Get-HttpRequests("Application")
    }   
    It "CREATE Application" -Tag "displayName,POST" {
        $GETRequest = $httpRequests["GET"]
        $POSTRequest = $httpRequests["POST"]
        
        $method = $GETRequest.Method
        $uri = $GETRequest.Url
        $entityBodyData = $POSTRequest.Body | ConvertFrom-Json -AsHashtable

        #Execute GET ALL Request
        $entities = Invoke-MgGraphRequest -Method $method -Uri $uri -OutputType PSObject

        $methodAndPrimaryKey = Get-MethodAndPrimaryKey($____Pester.CurrentTest.Tag)
        $primaryKey = $methodAndPrimaryKey["PrimaryKey"]

        #Check if Entity Exists
        $entity = $entities.value | Where-Object -Property $primaryKey -eq $entityBodyData[$primaryKey]

        if ($null -eq $entity) {
            $createMethod = $POSTRequest.Method
            $createUri = $POSTRequest.Url
            $createBody = $POSTRequest.Body | ConvertFrom-Json -AsHashtable
            $createContentType = $POSTRequest.ContentType
        
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
