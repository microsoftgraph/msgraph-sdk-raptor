function Get-AppSettings (
    [string] $AppSettingsPath = (Join-Path $MyInvocation.PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/appsettings.json")
) {
    $appSettings = Get-Content $AppSettingsPath -Raw | ConvertFrom-Json
    if (    !$appSettings.CertificateThumbprint `
            -or !$appSettings.ClientID `
            -or !$appSettings.Username `
            -or !$appSettings.Password `
            -or !$appSettings.TenantID) {
        Write-Error -ErrorAction Stop -Message "please provide CertificateThumbprint, ClientID, Username, Password and TenantID in appsettings.json"
    }
    return $appSettings
}

function Get-CurrentIdentifiers (
    [string] $IdentifiersPath = (Join-Path $MyInvocation.PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json")
) {
    $identifiers = Get-Content $IdentifiersPath -Raw | ConvertFrom-Json
    return $identifiers
}

function Get-CurrentDomain (
    [PSObject]$AppSettings
) {
    $domain = $appSettings.Username.Split("@")[1]
    return $domain
}


<#
   Assumes that data is Stored in the following format:-
   Entity
        - ChildEntity.json
        - ChildEntity.json
    Such as:-
    Team
        - OpenShift.json
        - Schedule.json
    Based on the Tree Structure in Identifiers.json
#>
function Get-RequestData (
    [string] $ChildEntity
    ) {
    $entityPath = Join-Path $MyInvocation.PSScriptRoot "./$($ChildEntity).json"
    $data = Get-Content -Path $entityPath -Raw | ConvertFrom-Json
    return $data
}


<#
    Helpers handles:-
        1. GraphVersion,
        2. MS-APP-ACTS-AS Headers
        3. Content-Type header
        4. HttpMethod

    Basic Validation of Parameters
#>
function Invoke-RequestHelper (
     [string] $Uri,
    [parameter(Mandatory = $False)][ValidateSet("v1.0", "beta")][string] $GraphVersion = "v1.0",
    [Parameter(Mandatory = $False)][ValidateSet("GET", "POST", "PUT", "PATCH", "DELETE")][string] $Method = "GET",
    $Headers = @{ },
    $Body,
    $User
) {
    #Append Content-Type to headers collection
    #Append "MS-APP-ACTS-AS" to headers collection
    $headers += @{ "Content-Type" = "application/json" }
    if ($null -ne $User) {
        $headers += @{"MS-APP-ACTS-AS" = $User }
    }
    #Convert Body to Json
    $jsonData = $body | ConvertTo-Json -Depth 3

    $response = Invoke-MgGraphRequest -Headers $headers -Method $Method -Uri "https://graph.microsoft.com/$GraphVersion/$Uri" -Body $jsonData -OutputType PSObject

    return $response.value ?? $response
}

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
            elseif ($method -eq "GET") {
                $joinedScopeString = $scopes.value |
                Where-Object { $_.Contains("Read") -and !$_.Contains("Write") } | # same selection as the read-only permissions for the app
                Join-String -Separator " "
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
    $response = Invoke-MgGraphRequest -Method $method -Headers $Headers -Uri "https://graph.microsoft.com/$version/$url" -Body $jsonBody -OutputType PSObject
    return $response.value ?? $response
}
