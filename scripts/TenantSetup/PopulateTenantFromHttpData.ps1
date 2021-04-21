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

    
    . $loadEnvironment
    . $installTools

    $treeUtil = Join-Path $PSScriptRoot 'treeUtils.ps1'
    . $treeUtil
        
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
        $PATCHHttpRequest = Get-Content .\TenantHttpData\Users\PATCH\user.http -Raw
        $parsedPATCHHttpRequest = [ApiDoctor.Validation.Http.HttpParser]::ParseHttpRequest($PATCHHttpRequest)
        $deleteHttpRequest = Get-Content .\TenantHttpData\Users\DELETE\user.http -Raw
        $parsedDELETEHttpRequest = [ApiDoctor.Validation.Http.HttpParser]::ParseHttpRequest($deleteHttpRequest)
    }   
    It "GET User" -Tag "userPrincipalName,GET" {
        
        $method = $parsedGETHttpRequest.Method
        $uri = $parsedGETHttpRequest.Url
        $body = $parsedPOSTHttpRequest.Body | ConvertFrom-Json -AsHashtable

        #Execute Request
        $entities = Invoke-MgGraphRequest -Method $method -Uri $uri -OutputType PSObject
        $entities | Should -Not -BeNullOrEmpty

        #Get Method,PrimaryKey Field name from Tag
        $methodAndPrimaryKey = Get-MethodAndPrimaryKey($____Pester.CurrentTest.Tag)
        $method = $methodAndPrimaryKey["Method"]
        $primaryKey = $methodAndPrimaryKey["PrimaryKey"]

        #Check if Entity Exists
        $entity = $entities.value | Where-Object -Property $primaryKey -eq $body[$primaryKey]
        if ($null -eq $entity) {
            $message = "Entity Does Not Exist: TestName:{0}:{1} EntityId: {2} Needs Creation" -f $method, $____Pester.CurrentTest.Name, $body[$primaryKey]
            Set-ItResult -Skipped -Because $message
        }
        else {
            $raptorIdentifiers.user._value = $entity.Id
        }
    }
    It "CREATE User" -Tag "POST" {
        if (($null -eq $raptorIdentifiers.user._value) -or ($raptorIdentifiers.user._value -eq "<user>")) {
            $method = $parsedPOSTHttpRequest.Method
            $uri = $parsedPOSTHttpRequest.Url
            $body = $parsedPOSTHttpRequest.Body | ConvertFrom-Json -AsHashtable
            $contentType = $parsedPOSTHttpRequest.ContentType
        
            $entity = Invoke-MgGraphRequest -Method $method -Uri $uri -Body $body -ContentType $contentType
            $entity | Should -Not -BeNullOrEmpty

            $raptorIdentifiers.user._value = $entity.Id
        }
        else {
            $message = "Not Entity To Create: TestName:{0} EntityId: {1}" -f $____Pester.CurrentTest.Name, $raptorIdentifiers.user._value
            Set-ItResult -Skipped -Because "Entity Exists"
        }
    } 
   
    It "UPDATE User" -Tag "PATCH" {
        if (($null -eq $raptorIdentifiers.user._value) -or ($raptorIdentifiers.user._value -eq "<user>")) {
            $method = $parsedPATCHHttpRequest.Method
            $uri = "{0}/{1}" -f $parsedGETHttpRequest.Url, $raptorIdentifiers.user._value
            $body = $parsedPATCHHttpRequest.Body
            $contentType = $parsedPATCHHttpRequest.ContentType
    
            $entity = Invoke-MgGraphRequest -Method $method -Uri $uri -Body $body -ContentType $contentType
            $entity | Should -BeNullOrEmpty
        }
        else {
            $message = "Not Entity To Update: TestName:{0} EntityId: {1}" -f $____Pester.CurrentTest.Name, $raptorIdentifiers.user._value
            Set-ItResult -Skipped -Because $message
        }
    }
    It "DELETE User"  -Tag "DELETE" {
        if (($null -ne $raptorIdentifiers.user._value) -or ($raptorIdentifiers.user._value -ne "<user>")) {
            $method = $parsedDELETEHttpRequest.Method
            $uri = "{0}/{1}" -f $parsedGETHttpRequest.Url, $raptorIdentifiers.user._value
            $body = $parsedDELETEHttpRequest.Body
            $contentType = $parsedDELETEHttpRequest.ContentType
    
            $entity = Invoke-MgGraphRequest -Method $method -Uri $uri -Body $body -ContentType $contentType
            $entity | Should -BeNullOrEmpty

            $raptorIdentifiers.user._value = "<user>"
        }
        else {
            $message = "Not Entity To Delete: TestName:{0} EntityId: {1}" -f $____Pester.CurrentTest.Name, $raptorIdentifiers.user._value
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
        $PATCHHttpRequest = Get-Content .\TenantHttpData\Applications\PATCH\application.http -Raw
        $parsedPATCHHttpRequest = [ApiDoctor.Validation.Http.HttpParser]::ParseHttpRequest($PATCHHttpRequest)
        $deleteHttpRequest = Get-Content .\TenantHttpData\Applications\DELETE\application.http -Raw
        $parsedDELETEHttpRequest = [ApiDoctor.Validation.Http.HttpParser]::ParseHttpRequest($deleteHttpRequest)
    }   
    It "GET Application" -Tag "displayName,GET" {
        
        $method = $parsedGETHttpRequest.Method
        $uri = $parsedGETHttpRequest.Url
        $body = $parsedPOSTHttpRequest.Body | ConvertFrom-Json -AsHashtable

        #Execute Request
        $entities = Invoke-MgGraphRequest -Method $method -Uri $uri -OutputType PSObject
        $entities | Should -Not -BeNullOrEmpty

        #Get Method,PrimaryKey Field name from Tag
        $methodAndPrimaryKey = Get-MethodAndPrimaryKey($____Pester.CurrentTest.Tag)
        $method = $methodAndPrimaryKey["Method"]
        $primaryKey = $methodAndPrimaryKey["PrimaryKey"]
        #Check if Entity Exists
        $entity = $entities.value | Where-Object -Property $primaryKey -eq $body[$primaryKey]
        if ($null -eq $entity) {
            $message = "Entity Does Not Exist. TestName:[{0}:{1}] EntityId: [{2}] Needs Creation" -f $method, $____Pester.CurrentTest.Name, $body[$primaryKey]
            Set-ItResult -Skipped -Because $message
        }
        else {
            $raptorIdentifiers.application._value = $entity.Id
        }
    }
    It "CREATE Application" -Tag "POST" {
        if (($null -eq $raptorIdentifiers.application._value) -or ($raptorIdentifiers.application._value -eq "<application>")) {
            $method = $parsedPOSTHttpRequest.Method
            $uri = $parsedPOSTHttpRequest.Url
            $body = $parsedPOSTHttpRequest.Body | ConvertFrom-Json -AsHashtable
            $contentType = $parsedPOSTHttpRequest.ContentType
        
            $entity = Invoke-MgGraphRequest -Method $method -Uri $uri -Body $body -ContentType $contentType
            $entity | Should -Not -BeNullOrEmpty

            $raptorIdentifiers.application._value = $entity.Id
        }
        else {
            $message = "No Entity To Create TestName:[{0}] EntityId: [{1}] Already Exists in Identifier File" -f $____Pester.CurrentTest.Name, $raptorIdentifiers.application._value
            Set-ItResult -Skipped -Because $message
        }
    } 
    
    It "UPDATE Application" -Tag "PATCH" {
        if (($null -eq $raptorIdentifiers.application._value) -or ($raptorIdentifiers.application._value -eq "<application>")) {
            $method = $parsedPATCHHttpRequest.Method
            $uri = "{0}/{1}" -f $parsedGETHttpRequest.Url, $raptorIdentifiers.application._value
            $body = $parsedPATCHHttpRequest.Body
            $contentType = $parsedPATCHHttpRequest.ContentType
    
            $entity = Invoke-MgGraphRequest -Method $method -Uri $uri -Body $body -ContentType $contentType
            $entity | Should -BeNullOrEmpty
        }
        else {
            $message = "Not Entity To Update: TestName:{0} EntityId: [{1}]" -f $____Pester.CurrentTest.Name, $raptorIdentifiers.application._value
            Set-ItResult -Skipped -Because $message
        }
    }
    It "DELETE Application"  -Tag "DELETE" {
        if (($null -ne $raptorIdentifiers.application._value) -or ($raptorIdentifiers.application._value -ne "<application>")) {
            $method = $parsedDELETEHttpRequest.Method
            $uri = "{0}/{1}" -f $parsedGETHttpRequest.Url, $raptorIdentifiers.application._value
            $body = $parsedDELETEHttpRequest.Body
            $contentType = $parsedDELETEHttpRequest.ContentType
    
            $entity = Invoke-MgGraphRequest -Method $method -Uri $uri -Body $body -ContentType $contentType
            $entity | Should -BeNullOrEmpty

            $raptorIdentifiers.application._value = "<application>"
        }
        else {
            $message = "Not Entity To Delete: TestName:{0} EntityId: [{1}][" -f $____Pester.CurrentTest.Name, $raptorIdentifiers.application._value
            Set-ItResult -Skipped -Because $message
        }
    }

}
AfterAll {
    $raptorIdentifiers | ConvertTo-Json | Out-File "RaptorIdentifiers.json"
}
