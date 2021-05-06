$scriptsPath = $PWD.Path

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
        [string]$scopeOverride
    )

    $tokenEndpoint = "https://login.microsoftonline.com/$domain/oauth2/v2.0/token"
    $grantType = "password"
    $method = "GET"

    if ($scopeOverride)
    {
        $joinedScopeString = $scopeOverride
    }
    else
    {
        $scopes = Invoke-RestMethod -Method Get -Uri "https://graphexplorerapi.azurewebsites.net/permissions?requesturl=$path&method=$method"
        $joinedScopeString = $scopes.value |
            Where-Object { $_.Contains("Read") -and !$_.Contains("Write") } | # same selection as the read-only permissions for the app
            Join-String -Separator " "
    }

    $body = "grant_type=$grantType&username=$($appSettings.Username)&password=$($appSettings.Password)&client_id=$($appSettings.ClientID)&scope=$($joinedScopeString)"
    $token = Invoke-RestMethod -Method Post -Uri $tokenEndpoint -Body $body -ContentType 'application/x-www-form-urlencoded'

    $token.access_token
}

function reqDelegated
{
    param(
        [string]$version = "v1.0",
        [string]$url,
        [string]$scopeOverride
    )

    $token = getToken -path "/$url" -scopeOverride $scopeOverride
    Connect-MgGraph -AccessToken $token | Out-Null

    $response = Invoke-MgGraphRequest -Method GET -Uri "https://graph.microsoft.com/$version/$url" -OutputType PSObject
    $response.value
}

$calendarGroup = reqDelegated -url "me/calendarGroups" |
    Where-Object { $_.name -eq "My Calendars" } |
    Select-Object -First 1
$calendarGroup.id
$identifiers.calendarGroup._value = $calendarGroup.id

$todoTaskList = reqDelegated -url "me/todo/lists" -scopeOverride "Tasks.Read" |
    Select-Object -First 1
$todoTaskList.id
$identifiers.todoTaskList._value = $todoTaskList.id

#Get Group with plans
$groupWithPlan = reqDelegated -url "groups" |
    Where-Object {$_.displayName -eq "Mark 8 Project Team"}
    Select-Object -First 1
$groupWithPlan.id
#Get Planner via Group
$plannerPlan = reqDelegated -url "groups/$($groupWithPlan.id)/planner/plans" |
    Where-Object {$_.title -eq "Mark8 project tracking"}
    Select-Object -First 1

$plannerPlan.id
$identifiers.plannerPlan._value=$plannerPlan.Id

$plannerTask = reqDelegated -url "planner/plans/$($plannerPlan.id)/tasks" |
    Where-Object {$_.title -eq "Organize Catering"}
    Select-Object -First 1

$plannerTask.id
$identifiers.plannerTask._value=$plannerTask.Id

$plannerBucket = reqDelegated -url "planner/plans/$($plannerPlan.id)/buckets" |
    Where-Object {$_.name -eq "After party"}
    Select-Object -First 1

$plannerBucket.id
$identifiers.plannerBucket._value=$plannerBucket.Id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
