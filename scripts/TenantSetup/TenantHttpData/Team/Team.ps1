# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $AppSettingsPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/appsettings.json"),
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json")
)

$appSettings = Get-Content $AppSettingsPath | ConvertFrom-Json
$identifiers = Get-Content $IdentifiersPath | ConvertFrom-Json
$domain = $appSettings.Username.Split("@")[1]

if (    !$appSettings.CertificateThumbprint `
        -or !$appSettings.ClientID `
        -or !$appSettings.Username `
        -or !$appSettings.Password `
        -or !$appSettings.TenantID) {
    Write-Error -ErrorAction Stop -Message "please provide CertificateThumbprint, ClientID, Username, Password and TenantID in appsettings.json"
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
#>
function Get-RequestData {
    param (
        [string] $ChildEntity
    )
    $entityPath = Join-Path $PSScriptRoot "./$($childEntity).json" -Resolve
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
function Invoke-RequestHelper {
    param(
        [string]
        $Uri,
        [Parameter(Mandatory = $False)]
        [ValidateSet("v1.0", "beta")]
        [string] $GraphVersion = "v1.0",
        [Parameter(Mandatory = $False)]
        [ValidateSet("GET", "POST", "PUT", "PATCH", "DELETE")]
        [string] $Method = "GET",
        $Headers = @{ },
        $Body,
        $User
    )
    #Append Content-Type to headers collection
    #Append "MS-APP-ACTS-AS" to headers collection
    $headers += @{ "Content-Type" = "application/json" }
    $headers += @{"MS-APP-ACTS-AS" = $User }
    #Convert Body to Json
    $jsonData = $body | ConvertTo-Json -Depth 3

    $response = Invoke-MgGraphRequest -Headers $headers -Method $Method -Uri "https://graph.microsoft.com/$GraphVersion/$Uri" -Body $jsonData -OutputType PSObject

    return $response.value ?? $response
}

#Connect To Microsoft Graph Using ClientId, TenantId and Certificate
Connect-MgGraph -CertificateThumbprint $appSettings.CertificateThumbprint -ClientId $appSettings.ClientID -TenantId $appSettings.TenantID

#Use Team and User already in identifiers.json
$team = $identifiers.team._value
$team
$user = $identifiers.user._value
$user
#Set User as a default parameter to avoid setting it with every call
$PSDefaultParameterValues = @{"Invoke-RequestHelper:User" = $user }

#Create or Replace Schedule https://docs.microsoft.com/en-us/graph/api/team-put-schedule?view=graph-rest-1.0
$scheduleData = Get-RequestData -ChildEntity "Schedule"
$schedule = Invoke-RequestHelper -Uri "teams/$($team)/schedule" -Body $scheduleData -Method PUT
$schedule.id

# Create Scheduling Group https://docs.microsoft.com/en-us/graph/api/schedule-post-schedulinggroups?view=graph-rest-1.0&tabs=http
<#
    Create a Scheduling Group with all the Team members as members. 
#>
$schedulingGroupData = Get-RequestData -ChildEntity "SchedulingGroup"
# Get Other Group Members and Add them to the SchedulingGroup
$groupMembers = Invoke-RequestHelper -Uri "teams/$($team)/members" | 
Select-Object -First 2
$schedulingGroupData.userIds = $groupMembers.userId
# Check and Get Scheduling Group if it exists
$currentSchedulingGroup = Invoke-RequestHelper -Uri "teams/$($team)/schedule/schedulingGroups" -Method GET |
Where-Object { $_.displayName -eq $schedulingGroupData.displayName } | 
Select-Object -First 1

if ($null -eq $currentSchedulingGroup) {
    $currentSchedulingGroup = Invoke-RequestHelper -Uri "teams/$($team)/schedule/schedulingGroups" -Method POST -Body $schedulingGroupData
    $currentSchedulingGroup.id
}

# Create Shift https://docs.microsoft.com/en-us/graph/api/schedule-post-shifts?view=graph-rest-1.0&tabs=http
$shiftData = Get-RequestData -ChildEntity "Shift"
$shiftData.userId = $user
$shiftData.schedulingGroupId = $currentSchedulingGroup.id
$createdShift = Invoke-RequestHelper -Uri "teams/$($team)/schedule/shifts" -Body $shiftData -Method POST

#Create OpenShift https://docs.microsoft.com/en-us/graph/api/openshift-post?view=graph-rest-1.0
$openShiftData = Get-RequestData -ChildEntity "OpenShift"
$openShiftData.schedulingGroupId = $currentSchedulingGroup.id
$createdOpenShift = Invoke-RequestHelper -Uri "teams/$($team)/schedule/openshifts" -Body $openShiftData -Method POST

#Create OfferShiftRequests https://docs.microsoft.com/en-us/graph/api/offershiftrequest-post?view=graph-rest-1.0&tabs=http
$offerShiftData = Get-RequestData -ChildEntity "OfferShiftRequest"
$offerShiftData.senderShiftId = $createdShift.id
$offerShiftData.recipientUserId = $groupMembers[0].userId
$createdOfferShift = Invoke-RequestHelper -Uri "teams/$($team)/schedule/offershiftrequests" -Body $offerShiftData -Method POST

#Create OpenShiftChangeRequest https://docs.microsoft.com/en-us/graph/api/openshiftchangerequest-post?view=graph-rest-1.0
$openShiftChangeRequestData = Get-RequestData -ChildEntity "OpenShiftChangeRequest"
$openShiftChangeRequestData.openShiftId = $createdOpenShift.id
$createOpenShiftChangeRequest = Invoke-RequestHelper -Uri "teams/$($team)/schedule/openShiftChangeRequests" -Body $openShiftChangeRequestData -Method POST

#Create SwapShiftsChangeRequest https://docs.microsoft.com/en-us/graph/api/swapshiftschangerequest-post?view=graph-rest-1.0
<#
    Swap Shift Change Request, requires a Shift on the destination user.
#>

#Create a Shift for the Destination User
$recipientShiftData = Get-RequestData -ChildEntity "Shift"
$recipientShiftData.userId = $groupMembers[1].userId
$recipientShiftData.schedulingGroupId = $currentSchedulingGroup.id
$createdRecipientShift = Invoke-RequestHelper -Uri "teams/$($team)/schedule/shifts" -Body $recipientShiftData -Method POST

$swapShiftsChangeRequestData = Get-RequestData -ChildEntity "SwapShiftsChangeRequest"
$swapShiftsChangeRequestData.senderShiftId = $createdOpenShift.id
$swapShiftsChangeRequestData.recipientUserId = $groupMembers[1].userId
$swapShiftsChangeRequestData.senderShiftId = $createdShift.id
$swapShiftsChangeRequestData.recipientShiftId = $createdRecipientShift.id
$createdSwapShiftsChangeRequest = Invoke-RequestHelper -Uri "teams/$($team)/schedule/swapShiftsChangeRequests" -Body $swapShiftsChangeRequestData -Method POST

#Get Or Create TimeOffReason if does not exist https://docs.microsoft.com/en-us/graph/api/schedule-post-timeoffreasons?view=graph-rest-1.0&tabs=http
$timeOffReasonData = Get-RequestData -ChildEntity "TimeOffReason"
$currentTimeOffReason = Invoke-RequestHelper -Uri "teams/$($team)/schedule/timeOffReasons" -Method GET |
Where-Object { $_.displayName -eq $timeOffReasonData.displayName } | 
Select-Object -First 1
if ($null -eq $timeOffReasonData) {
    $currentTimeOffReason = Invoke-RequestHelper -Uri "teams/$($team)/schedule/timeOffReasons" -Method POST -Body $timeOffReasonData
}

$timeoffData = Get-RequestData -ChildEntity "TimesOff"
$timeoffData.userId = $user
$timeoffData.draftTimeOff.timeOffReasonId = $currentTimeOffReason.id
$createdTimesOffData = Invoke-RequestHelper -Uri "teams/$($team)/schedule/timesOff" -Body $timeoffData -Method POST

$timeOffRequest = Invoke-RequestHelper -Uri "teams/$($team)/schedule/timeOffRequests/$($createOpenShiftChangeRequest.id)" -Method GET |
Select-Object -First 1

$identifiers.team.shift._value = $createdShift.id
$identifiers.team.schedulingGroup._value = $currentSchedulingGroup.id
$identifiers.team.openShift._value = $createdOpenShift.id
$identifiers.team.offerShiftRequest._value = $createdOfferShift.id
$identifiers.team.openShiftChangeRequest._value = $createOpenShiftChangeRequest.id
$identifiers.team.swapShiftsChangeRequest._value = $createdSwapShiftsChangeRequest.id
$identifiers.team.timeOffReason._value = $currentTimeOffReason.id
$identifiers.team.timeOff._value = $createdTimesOffData.id
$identifiers.team.timeOffRequest._value = $timeOffRequest.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath