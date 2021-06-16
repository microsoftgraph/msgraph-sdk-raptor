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
    if ($null -eq $User) {
        $headers += @{"MS-APP-ACTS-AS" = $User }
    }
    #Convert Body to Json
    $jsonData = $body | ConvertTo-Json -Depth 3

    $response = Invoke-MgGraphRequest -Headers $headers -Method $Method -Uri "https://graph.microsoft.com/$GraphVersion/$Uri" -Body $jsonData -OutputType PSObject

    return $response.value ?? $response
}

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