$scriptsPath = $PSScriptRoot

$appSettingsPath = Join-Path $scriptsPath "../msgraph-sdk-raptor-compiler-lib/appsettings.json"
$appSettings = Get-Content $appSettingsPath | ConvertFrom-Json

if (    !$appSettings.CertificateThumbprint `
    -or !$appSettings.ClientID `
    -or !$appSettings.Username `
    -or !$appSettings.Password `
    -or !$appSettings.TenantID)
{
    Write-Error -ErrorAction Stop -Message "please provide CertificateThumbprint, ClientID, Username, Password and TenantID in appsettings.json"
}

$identifiersPath = Join-Path $scriptsPath "../msgraph-sdk-raptor-compiler-lib/identifiers.json"
$identifiers = Get-Content $identifiersPath | ConvertFrom-Json

$domain = $appSettings.Username.Split("@")[1]

function getToken
{
    param(
        [string]$path,
        [string]$scopeOverride,
        [Parameter(Mandatory = $False)]
        [ValidateSet("GET", "POST", "PUT", "PATCH", "DELETE")]
        [string] $method = "GET"
    )

    $tokenEndpoint = "https://login.microsoftonline.com/$domain/oauth2/v2.0/token"
    $grantType = "password"

    if ($scopeOverride)
    {
        $joinedScopeString = $scopeOverride
    }
    else
    {
        try {
            $scopes = Invoke-RestMethod -Method Get -Uri "https://graphexplorerapi.azurewebsites.net/permissions?requesturl=$path&method=$method"
            if ($scopes.Count -eq 1 -and $scopes[0].value -eq "Not supported.") {
                $joinedScopeString = ".default"
            }
            else {
                $joinedScopeString = $scopes.value |
                Join-String -Separator " "
            }
        }
        catch {
            # try with empty scopes if we can't get permissions from the DevX API
            $joinedScopeString = ".default"
        }
    }

    $body = "grant_type=$grantType&username=$($appSettings.Username)&password=$($appSettings.Password)&client_id=$($appSettings.ClientID)&scope=$($joinedScopeString)"
    $token = Invoke-RestMethod -Method Post -Uri $tokenEndpoint -Body $body -ContentType 'application/x-www-form-urlencoded'

    Write-Debug "== got token with the following scopes"
    foreach ($scope in $token.scope.Split())
    {
        Write-Debug "    $scope"
    }

    $token.access_token
}

function reqDelegated
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$url,
        [Parameter(Mandatory = $False)]
        $body,
        [ValidateSet("GET", "POST", "PUT", "PATCH", "DELETE")]
        [string] $method="GET",
        $headers = @{ },
        [string]$scopeOverride,
        [string]$version = "v1.0"
    )

    $headers += @{ "Content-Type" = "application/json" }
    Write-Debug "== getting token for $url for method $method"

    $token = getToken -path "/$url" -scopeOverride $scopeOverride -method $method
    Connect-MgGraph -AccessToken $token | Out-Null
    
    $jsonBody = $body | ConvertTo-Json -Depth 3
    $response = Invoke-MgGraphRequest -Method $method -Headers $Headers -Uri "https://graph.microsoft.com/$version/$url" -Body $jsonBody -OutputType PSObject -Debug:$debug
    return $response.value ?? $response
}