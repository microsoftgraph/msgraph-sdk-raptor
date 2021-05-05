$scriptsPath = $PWD.Path

$appSettingsPath = Join-Path $scriptsPath "../msgraph-sdk-raptor-compiler-lib/appsettings.json"
$appSettings = Get-Content $appSettingsPath | ConvertFrom-Json

if ( !$appSettings.CertificateThumbprint -or !$appSettings.ClientID -or !$appSettings.Username -or !$appSettings.TenantID)
{
    Write-Error -ErrorAction Stop -Message "please provide CertificateThumbprint, ClientID, Username and TenantID in appsettings.json"
}

$identifiersPath = Join-Path $scriptsPath "../msgraph-sdk-raptor-compiler-lib/identifiers.json"
$identifiers = Get-Content $identifiersPath | ConvertFrom-Json

$admin = "MOD Administrator"
$domain = $appSettings.Username.Split("@")[1]
$identifiers.domain._value = $domain

function req
{
    param(
        [string]$version = "v1.0",
        [PSCustomObject]$headers = @{},
        [string]$url
    )

    $response = Invoke-MgGraphRequest -Headers $headers -Method GET -Uri "https://graph.microsoft.com/$version/$url" -OutputType PSObject
    $response.value
}

Connect-MgGraph -CertificateThumbprint $appSettings.CertificateThumbprint -ClientId $appSettings.ClientID -TenantId $appSettings.TenantID

$user = req -url "users"  |
    Where-Object { $_.displayName -eq $admin}
    Select-Object -First 1
$user.id
$identifiers.user._value = $user.id

$calendarPermission = req -url "users/$($user.id)/calendar/calendarPermissions" |
    Select-Object -First 1
$calendarPermission.id
$identifiers.user.calendarPermission._value = $calendarPermission.id

$userScopeTeamsAppInstallation = req -url "users/$($user.id)/teamwork/installedApps?`$expand=teamsAppDefinition" |
    Where-Object { $_.teamsAppDefinition.displayName -eq "Teams" }
    Select-Object -First 1
$userScopeTeamsAppInstallation.id
$identifiers.user.userScopeTeamsAppInstallation._value = $userScopeTeamsAppInstallation.id

$team = req -url "groups" |
    Where-Object { $_.resourceProvisioningOptions -eq "Team" -and $_.displayName -eq "U.S. Sales"} |
    Select-Object -First 1
$team.id
$identifiers.team._value = $team.id
$identifiers.group._value = $team.id

$channel = req -url "teams/$($team.id)/channels" |
    Where-Object { $_.displayName -eq "Sales West"}
    Select-Object -First 1
$channel.id
$identifiers.team.channel._value = $channel.id

$member = req -url "teams/$($team.id)/channels/$($channel.id)/members" |
    Where-Object { $_.displayName -eq "MOD Administrator"}
    Select-Object -First 1
$member.id
$identifiers.team.channel.conversationMember._value = $member.id
$identifiers.team.conversationMember._value = $member.id

$installedApp = req -url "teams/$($team.id)/installedApps" |
    Select-Object -First 1
$installedApp.id
$identifiers.team.teamsAppInstallation._value = $installedApp.id

$drive = req -url "drives" |
    Where-Object { $_.createdBy.user.displayName -eq $admin } |
    Select-Object -First 1
$drive.id
$identifiers.drive._value = $drive.id

$driveItem = req -url "drives/$($drive.id)/root/children" |
    Where-Object { $_.name -eq "Blog Post preview.docx" } |
    Select-Object -First 1
$driveItem.id
$identifiers.drive.driveItem._value = $driveItem.id

$thumbnailSet = req -url "drives/$($drive.id)/items/$($driveItem.id)/thumbnails" |
    Select-Object -First 1
$thumbnailSet.id
$identifiers.driveItem.thumbnailSet._value = $thumbnailSet.id

$permission = req -url "drives/$($drive.id)/items/$($driveItem.id)/permissions" |
    Select-Object -First 1
$permission.id
$identifiers.driveItem.permission._value = $permission.id

$application = req -url "applications" |
    Where-Object { $_.displayName -eq "Salesforce" } |
    Select-Object -First 1
$application.id
$identifiers.application._value = $application.id

$applicationTemplate = req -url "applicationTemplates?`$filter=(displayName eq 'Microsoft Teams')" |
    Select-Object -First 1
$applicationTemplate.id
$identifiers.applicationTemplate._value = $applicationTemplate.id

$directoryAudit = req -url "auditLogs/directoryAudits?`$top=1" |
    Select-Object -First 1
$directoryAudit.id
$identifiers.directoryAudit._value = $directoryAudit.id

$signIn = req -url "auditLogs/signIns?`$top=1" |
    Select-Object -First 1
$signIn.id
$identifiers.signIn._value = $signIn.id

$contact = req -url "contacts" |
    Where-Object { $_.displayName -eq "Bob Kelly (TAILSPIN)" } |
    Select-Object -First 1
$contact.id
$identifiers.contact._value = $contact.id

$directoryRole = req -url "directoryRoles" |
    Where-Object { $_.displayName -eq "Global Administrator" }
    Select-Object -First 1
$directoryRole.id
$identifiers.directoryRole._value = $directoryRole.id

$directoryRoleTemplate = req -url "directoryRoleTemplates" |
    Where-Object { $_.displayName -eq "Global Administrator" }
    Select-Object -First 1
$directoryRoleTemplate.id
$identifiers.directoryRole._value = $directoryRoleTemplate.id

$educationUser = req -url "education/users" |
    Where-Object { $_.displayName -eq $admin }
$educationUser.id
$identifiers.educationUser._value = $educationUser.id

$conversation = req -url "groups/$($team.id)/conversations" |
    Where-Object { $_.topic -eq "The new U.S. Sales group is ready" }
    Select-Object -First 1
$conversation.id
$identifiers.group.conversation._value = $conversation.id

$conversationThread = req -url "groups/$($team.id)/conversations/$($conversation.id)/threads"
    Select-Object -First 1
$conversationThread.id
$identifiers.group.conversationThread._value = $conversationThread.id

$post = req -url "groups/$($team.id)/conversations/$($conversation.id)/threads/$($conversationThread.id)/posts" |
    Where-Object { $_.from.emailAddress.name -eq "U.S. Sales" } |
    Select-Object -First 1
$post.id
$identifiers.group.conversationThread.post._value = $post.id

$conditionalAccessPolicy = req -url "identity/conditionalAccess/policies" |
    Where-Object { $_.displayName -eq "Office 365 App Control" }
    Select-Object -First 1
$conditionalAccessPolicy.id
$identifiers.conditionalAccessPolicy._value = $conditionalAccessPolicy.id

$oauth2PermissionGrant = req -url "oauth2PermissionGrants" |
    Where-Object { $_.scope.Trim() -eq "user_impersonation" }
    Select-Object -First 1
$oauth2PermissionGrant.id
$identifiers.oAuth2PermissionGrant._value = $oauth2PermissionGrant.id

$organization = req -url "organization" |
    Where-Object { $_.displayName -eq "Contoso" } |
    Select-Object -First 1
$organization.id
$identifiers.organization._value = $organization.id

# id is not dynamic per tenant
$identifiers.permissionGrantPolicy._value = "microsoft-all-application-permissions"
$identifiers.secureScoreControlProfile._value = "OneAdmin"

$schemaExtension = req -url "schemaExtensions?`$filter=description eq 'Global settings'" |
    Select-Object -First 1
$schemaExtension.id
$identifiers.schemaExtension._value = $schemaExtension.id

$secureScore = req -url "security/secureScores?`$top=1" |
    Select-Object -First 1
$secureScore.id
$identifiers.secureScore._value = $secureScore.id

$subscribedSku = req -url "subscribedSkus" |
    Where-Object { $_.skuPartNumber -eq 'ENTERPRISEPACK'}
    Select-Object -First 1
$subscribedSku.id
$identifiers.subscribedSku._value = $subscribedSku.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath

# $test = req -url "planner/tasks"

<#
$msAppActsAsHeader = @{ "MS-APP-ACTS-AS" = $user.id }

$offerShiftRequests = req -headers $msAppActsAsHeader -url "teams/$($team.id)/schedule/offerShiftRequests"
$offerShiftRequests

$openShiftChangeRequests = req -headers $msAppActsAsHeader -url "teams/$($team.id)/schedule/openShiftChangeRequests"
$openShiftChangeRequests

$openShifts = req -headers $msAppActsAsHeader -url "teams/$($team.id)/schedule/openShifts"
$openShifts

$schedulingGroups = req -headers $msAppActsAsHeader -url "teams/$($team.id)/schedule/schedulingGroups"
$schedulingGroups

$shifts = req -headers $msAppActsAsHeader -url "teams/$($team.id)/schedule/shifts"
$shifts

$swapShiftsChangeRequests = req -headers $msAppActsAsHeader -url "teams/$($team.id)/schedule/swapShiftsChangeRequests"
$swapShiftsChangeRequests

$timeOffReasons = req -headers $msAppActsAsHeader -url "teams/$($team.id)/schedule/timeOffReasons"
$timeOffReasons

$timeOffRequests = req -headers $msAppActsAsHeader -url "teams/$($team.id)/schedule/timeOffRequests"
$timeOffRequests

$timesOff = req -headers $msAppActsAsHeader -url "teams/$($team.id)/schedule/timesOff"
$timesOff

$workforceIntegrations = req -headers $msAppActsAsHeader -url "teamwork/workforceIntegrations"
$workforceIntegrations

$places = req -headers $msAppActsAsHeader -url "places"
$places
#>

# data missing
# $test = req -url "contracts"

# permission missing
# $test = req -url "dataPolicyOperations"

# data missing - my teams login in one device caused a data entry
# but the actual data is missing
# $test = req -url "devices"

# data missing
# $test = req -url "directory/administrativeUnits"

# can't query
# $test = req -url "directory/deletedItems"

# can't query
# $test = req -url "directoryObjects"

# $educationClasses = req -url "education/classes"
# $educationClasses

# $educationSchools = req -url "education/schools"
# $educationSchools

# access denied
# $test = req -url "groups/$($team.id)/events"

# request not supported
# $extension = req -url "groups/$($team.id)/conversations/$($conversation.id)/threads/$($conversationThread.id)/posts/$($post.id)/extensions" |
#     Select-Object -First 1
# $extension.id
# $identifiers.group.conversationThread.post.extension._value = $extension.id

# resource not found for the segment
# $groupSetting = req -url "groupSettings" |
#     Select-Object -First 1

# resource not found for the segment
# $groupSettingTemplate = req -url "groupSettingTemplates" |
#     Select-Object -First 1

# no data
# $namedLocation = req -url "identity/conditionalAccess/namedLocations" |
#     Select-Object -First 1

# no data
# $appContentRequest = req -url "identityGovernance/appConsent/appConsentRequests" |
#     Select-Object -First 1

# 500
# $agreement = req -url "identityGovernance/termsOfUse/agreements" |
#     Select-Object -First 1

# no data
# $identityProvider = req -url "identityProviders" |
#     Select-Object -First 1

# no data
# $threatAssessmentRequest = req -url "informationProtection/threatAssessmentRequests" |
#     Select-Object -First 1

# 401 Unauthorized
# $test = req -url "planner/buckets"
# $test = req -url "planner/plans"
# $test = req -url "planner/tasks"

# no data
# $test = req -url "policies/activityBasedTimeoutPolicies"

# 404 No HTTP resource was found that matches the request URI 'https://mface.windowsazure.com/odata/authenticationMethodsPolicy/authenticationMethodConfigurations
# $test = req -url "policies/authenticationMethodsPolicy/authenticationMethodConfigurations"

# no data
# $test = req -url "policies/claimsMappingPolicies"

# 403
# $test = req -url "policies/featureRolloutPolicies"

# no data
# $test = req -url "policies/homeRealmDiscoveryPolicies"

# no data
# $test = req -url "policies/tokenIssuancePolicies"

# no data
# $test = req -url "policies/tokenLifetimePolicies"

# 403
# $test = req -url "print/connectors"

# no data
# $test = req -url "print/printers"

# 403
# $test = req -url "print/services"

# 403
# $test = req -url "print/shares"

# 403
# $test = req -url "print/taskDefinitions"

# 403
# $test = req -url "reports/dailyPrintUsageByPrinter"

# 403
# $test = req -url "reports/dailyPrintUsageByUser"

# no data
# $test = req -url "security/alerts"

# 400 bad request
# $test = req -url "shares/id"

# no data
# $test = req -url "subscriptions"

# no data
# req -url "users/$($user.id)/authentication/microsoftAuthenticatorMethods"

# no data
# req -url "users/$($user.id)/authentication/windowsHelloForBusinessMethods"

# $test = req -url "users/$($user.id)/calendar/calendarPermissions"

# $test = req -url "subscriptions"


