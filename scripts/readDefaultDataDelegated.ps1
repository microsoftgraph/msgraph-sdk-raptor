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
        try {
            $scopes = Invoke-RestMethod -Method Get -Uri "https://graphexplorerapi.azurewebsites.net/permissions?requesturl=$path&method=$method"
            if ($scopes.Count -eq 1 -and $scopes[0].value -eq "Not supported.") {
                $joinedScopeString = ".default"
            }
            else {
                $joinedScopeString = $scopes.value |
                Where-Object { $_.Contains("Read") -and !$_.Contains("Write") } | # same selection as the read-only permissions for the app
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
        [string]$version = "v1.0",
        [string]$url,
        [string]$scopeOverride
    )

    Write-Debug "== getting token for $url"

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

# no data
# $todoTask = reqDelegated -url "me/todo/lists/$($todoTaskList.id)/tasks" -scopeOverride "Tasks.Read"

# no data
# $linkedResource = reqDelegated -url "me/todo/lists/$($todoTaskList.id)/tasks/$($todoTask.id)/linkedResources" -scopeOverride "Tasks.Read"

# no data
# $contactFolder = reqDelegated -url "me/contactFolders" # already in readDefaultData under user/contactFolders. Remove this??

# no data
# $contact = reqDelegated -url "me/contacts?`$top=1"

$mailFolder = reqDelegated -url "me/mailFolders/inbox"
$mailFolder.id
$identifiers.mailFolder._value = $mailFolder.id

# no data
# $messageRule = reqDelegated -url "me/mailFolders/inbox/messageRules"

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

$driveItem = reqDelegated -url "me/drive/root/children?`$filter=name eq 'Contoso Purchasing Data - Q1.xlsx'" |
    Select-Object -First 1
$driveItem.id
$identifiers.driveItem._value = $driveItem.id

$driveItemPermission = reqDelegated -url "me/drive/items/$($driveItem.id)/permissions" |
    Select-Object -First 1
$driveItemPermission.id
$identifiers.driveItem.permission._value = $driveItemPermission.id

$driveItemThumbnail = reqDelegated -url "me/drive/items/$($driveItem.id)/thumbnails" |
    Select-Object -First 1
$driveItemThumbnail.id
$identifiers.driveItem.thumbnailSet._value = $driveItemThumbnail.id

$driveItemVersion = reqDelegated -url "me/drive/items/$($driveItem.id)/versions" |
    Select-Object -First 1
$driveItemVersion.id
$identifiers.driveItem.driveItemVersion._value = $driveItemVersion.id

$driveItemWorkbookTable = reqDelegated -url "me/drive/items/$($driveItem.id)/workbook/tables" |
    Where-Object { $_.name -eq "Table1" } |
    Select-Object -First 1
$driveItemWorkbookTable.id
$identifiers.driveItem.workbookTable._value = $driveItemWorkbookTable.id

$tableColumn = reqDelegated -url "me/drive/items/$($driveItem.id)/workbook/tables/$($driveItemWorkbookTable.id)/columns" |
    Select-Object -First 1
$tableColumn.id
$identifiers.driveItem.workbookTable.workbookTableColumn._value = $tableColumn.id

$tableRow = reqDelegated -url "me/drive/items/$($driveItem.id)/workbook/tables/$($driveItemWorkbookTable.id)/rows" |
    Select-Object -First 1
$tableRow.'@odata.id'
$identifiers.driveItem.workbookTable.workbookTableRow._value = $tableRow.'@odata.id'

$worksheet = reqDelegated -url "me/drive/items/$($driveItem.id)/workbook/worksheets" |
    Where-Object { $_.name -eq "Sheet1" } |
    Select-Object -First 1
$worksheet.id
$identifiers.driveItem.workbookWorksheet._value = $worksheet.id

# tenant agnostic data
$printEndpointId = "mpsdiscovery"
$printEndpointId
$identifiers.printService.printServiceEndpoint._value = $printEndpointId

$printService = reqDelegated -url "print/services" |
    Where-Object { $_.endpoints[0].id -eq $printEndpointId } |
    Select-Object -First 1

$printService.id
$identifiers.printService._value = $printService.id

$groupSettingTemplate = reqDelegated -url "groupSettingTemplates" |
    Where-Object { $_.displayName -eq "Prohibited Names Settings" } |
    Select-Object -First 1

$groupSettingTemplate.id
$identifiers.groupSettingTemplate._value = $groupSettingTemplate.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath

# no data
# $driveItemWorkbookNames = reqDelegated -url "me/drive/items/$($driveItem.id)/workbook/names"

# bad request
# $driveItemWorkbookOperations = reqDelegated -url "me/drive/items/$($driveItem.id)/workbook/operations"

# no data
# reqDelegated -url "me/drive/items/$($driveItem.id)/workbook/worksheets/$($worksheet.id)/charts"

# no data
# $connector = reqDelegated -url "print/connectors" -scopeOverride "PrintConnector.Read.All"

# no data
# $printShares = reqDelegated -url "print/shares"

# no data
# $taskDefition = reqDelegated -url "print/taskDefinitions"

# 500
# reqDelegated -url "reports/dailyPrintUsageByPrinter"

# 500
# reqDelegated -url "reports/dailyPrintUsageByUser"

# no data
# reqDelegated -url "identityGovernance/termsOfUse/agreements"
# reqDelegated -url "identityGovernance/appConsent/appConsentRequests"
# reqDelegated -url "identityProviders"

# no data
# reqDelegated -url "groupSettings"