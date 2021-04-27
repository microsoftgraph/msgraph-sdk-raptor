# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
$envFile = "env.json"
$envData = @{}

if (Test-Path -Path (Join-Path $PSScriptRoot $envFile)) {
    $envData = Get-Content (Join-Path $PSScriptRoot $envFile) | ConvertFrom-Json
}
else {
    $envData.MsGraphClientIdentifier = ${env:MSGRAPH_CLIENT_IDENTIFIER}
    $envData.TokenEndpoint = ${env:MSGRAPH_TOKEN_ENDPOINT}
    $envData.GrantType = ${env:AUTH_GRANT_TYPE}
    $envData.Username = ${env:AUTH_USERNAME}
    $envData.Password = ${env:AUTH_PASSWORD}
}

function Request-MgDelegatedAccess($path, $method) {
    $scope = Invoke-RestMethod -Method Get -Uri "https://graphexplorerapi.azurewebsites.net/permissions?requesturl=$($path)&method=$($method)"
    $joinedScopeString = $scope | ForEach-Object {$_.value} | Join-String -Separator ","
    $body = "grant_type=$($envData.GrantType)&username=$($envData.Username)&password=$($envData.Password)&client_id=$($envData.MsGraphClientIdentifier)&scope=$($joinedScopeString)"
    $token = Invoke-RestMethod -Method Post -Uri $envData.TokenEndpoint -Body $body -ContentType 'application/x-www-form-urlencoded'

    return Connect-MgGraph -AccessToken $token.access_token
}
