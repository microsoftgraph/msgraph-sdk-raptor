# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

BeforeAll {
    $loadEnvironment = Join-Path $PSScriptRoot 'loadEnv.ps1'
    $installTools = Join-Path $PSScriptRoot 'Install-Tools.ps1'
    . $loadEnvironment
    . $installTools

    Install-MicrosoftGraph
    Install-Az

    Connect-MgGraph
    #Connect-AzAccount
}

$raptorIdentifiers = @{}
<#
    Users
#>
Describe "Users" {
    It "Get User" {
        #Get User
        $user = Get-MgUser
        $user | Should -Not -BeNullOrEmpty
        $raptorIdentifiers.UserId = $user.Id
    }
}

<#
    Applications
#>
Describe "Applications" {
    It "Get-MgApplication" {
        #Get Application
        $application = Get-MgApplication -Filter "appId eq 'd8681ced-0794-496d-93bb-2e7c1e62a2fc'"
        $application | Should -Not -BeNullOrEmpty
        $raptorIdentifiers.ApplicationId = $application.Id
    }
    It "Application Owners Should Not Be Null" -Tag "PowerShell Issue" {
        #Owners
        #ApplicationId is misnamed, should be Id instead
        $applicationOwners = Get-MgApplicationOwner -ApplicationId $raptorIdentifiers.ApplicationId -All
        $applicationOwners | Should -Not -BeNullOrEmpty
    }
}

<#
    Contacts
#>
Describe "Contacts" {
    It "Create Contact Folder If Not Exists" {
        #Contacts
        #Create If Not Exists a Contact Folder
        $contactFolder = Get-MgUserContactFolder
        if ($null -eq $contactFolder) {
            $contactFolder = New-MgUserContactFolder -DisplayName "RaptorTesting"
        }
        $contactFolderChildFolder = Get-MgUserContactFolderChildFolder -ContactFolderId $contactFolder.Id
        if ($null -eq $contactFolderChildFolder) {
            $contactFolderChildFolder = New-MgUserContactFolderChildFolder -DisplayName "RaptorTestingChildFolder" -ContactFolderId $contactFolder.Id
        }

        $contactFolder | Should -Not -BeNullOrEmpty
        $contactFolderChildFolder | Should -Not -BeNullOrEmpty

        $raptorIdentifiers.ContactFolderId = $contactFolder.Id
        $raptorIdentifiers.ContactFolderChildFolderId = $contactFolderChildFolder.Id
    }
    It "Get Contact" {
        $contact = Get-MgContact
        #Returns a6fe29ca-d86e-40f5-9526-d45c4a21ae76
        $contactMemberOf = Get-MgContactMember -OrgContactId $contact.Id

        $contact | Should -Not -BeNullOrEmpty
        $contactMemberOf | Should -Not -BeNullOrEmpty

        $raptorIdentifiers.ContactId = $contact.Id
        $raptorIdentifiers.OrgContactId = $contactMemberOf.Id
    }
}

<#
    Drivers
#>
Describe "Drives" {
    It "Get Drive" {
        #OneDrive
        $drive = Get-MgUserDrive
        $drive | Should -Not -BeNullOrEmpty
        $raptorIdentifiers.DriveId = $drive.Id
    }
}

<#
#Groups
get-acceptedsenders-csharp-snippets

Raptor Permissions Issue, Works in GE.
https://graph.microsoft.com/v1.0/groups/f80870bd-60f4-4ec6-8549-3f51ac58d8b2/acceptedSenders
GroupId: f80870bd-60f4-4ec6-8549-3f51ac58d8b2
#>
Describe "Groups" {
    It "Get Groups" -Tag "PermissionIssues" {

        $group = Get-MgGroup -Filter "displayName eq 'Contoso'"
        $groupAcceptedSender = Get-MgGroupAcceptedSender -GroupId $group.Id

        $group | Should -Not -BeNullOrEmpty
        $groupAcceptedSender | Should -Not -BeNullOrEmpty

        $raptorIdentifiers.GroupId = $group.Id
        $raptorIdentifiers.GroupId = $groupAcceptedSender.Id
    }

}

<# 
    Calendar
#>
Describe "Calendar" {
<#
event-delta-csharp-snippets
DevX Issue: Returns Http 404 for
https://graphexplorerapi.azurewebsites.net/permissions?requesturl=/me/calendarView/microsoft.graph.delta()&method=GET'
#>
    It "Events Delta" {

    }
    It "Get Calendar Group"{
        $calendarGroup = Get-MgUserCalendarGroup
        $calendarGroup | Should -Not -BeNullOrEmpty

        $raptorIdentifiers.CalendarGroupId = $calendarGroup.Id
    }
    It "Get Calendar Group Calendars"{
        $calendarGroupCalendar = Get-MgUserCalendarGroupCalendar -CalendarGroupId $raptorIdentifiers.CalendarGroupId -All
        $calendarGroupCalendar | Should -Not -BeNullOrEmpty

        $raptorIdentifiers.CalendarGroupCalendarId = $calendarGroupCalendar.Id
    }
    It "Get User Calendar Permission"{
        $userCalendarPermission = Get-MgUserCalendarPermission
        $userCalendarPermission | Should -Not -BeNullOrEmpty

        $raptorIdentifiers.UserCalendarPermission = $userCalendarPermission.Id
    }

<#
event-get-attachments-v1-csharp-snippets
UserId: admin@M365x393677.OnMicrosoft.com
userEventId= AQMkAGZmATZjZTk0LWM4ZGUtNDk0Zi04OWU1LWViMzllNThkYjM4ZABGAAADBY8UEA2lr0WUPcOE8RSa1gcAEGdtFOVyj0SgBJ01ZOLEOgAAAgENAAAAEGdtFOVyj0SgBJ01ZOLEOgAAAlA1AAAA
#>

    It "Get User Event" {

        $userEvent = Get-MgUserEvent -Filter "Subject eq 'Company All Hands'"
        $userEvent | Should -Not -BeNullOrEmpty
        $raptorIdentifiers.UserEventId = $userEvent.Id
    }

<#
eventmessage-get-attachments-v1-csharp-snippets
UserId: admin@M365x393677.OnMicrosoft.com
UserMessageId=AAMkAGZmZjZjZTk0LWM4ZGUtNDk0Zi04OWU1LWViMzllNThkYjM4ZABGAAAAAAAFjxQQDaWvRZQ9w4TxFJrWBwAQZ20U5XKPRKAEnTVk4sQ6AAAAAAEMAAAQZ20U5XKPRKAEnTVk4sQ6AAA6tadmAAA= 
#>
    It "User Event Message " {

        $attachment = @{
            Name          = "attachment.txt";
            ContentBytes  = "UEsDBBQABgAIAAAAIQ4AAAAA";
            "@odata.type" = "#microsoft.graph.fileAttachment"
        }
        $userMessage = New-MgUserMessage -Attachments $attachment -Body @{ Content = "Raptor Testing Message" } -Subject "Raptor Testing Subject"
        $userMessageAttachment = Get-MgUserMessageAttachment -MessageId $userMessage.Id

        $userMessage | Should -Not -BeNullOrEmpty
        $userMessageAttachment | Should -Not -BeNullOrEmpty

        $raptorIdentifiers.UserMessageId = $userMessage.Id
        $raptorIdentifiers.UserMessageAttachmentId = $userMessageAttachment.Id
    }
}

<# 
    Sites
#>
Describe "Sites" {
<#
enum-lists-csharp-snippets 
PlaceHolder: {site-id}
SiteId="m365x393677.sharepoint.com,9792fe9e-4cc3-415a-9a82-91a75f6e2ea7,1aa3fc1c-489f-4ebd-b6cf-c634809e0327"

Snippet Fails due to DevX API Issue
https://graphexplorerapi.azurewebsites.net/permissions?requesturl=/sites/{site-id}/lists&method=GET%27
Returns Http 404
#>
    It "Sites" {

        $sites = Get-MgSite
        $siteList = Get-MgSiteList -SiteId $sites[$sites.Count - 1].Id

        $sites | Should -Not -BeNullOrEmpty
        $siteList | Should -Not -BeNullOrEmpty

        $raptorIdentifiers.SiteId = $sites[$sites.Count - 1].Id
        $raptorIdentifiers.SiteListId = $siteList.Id
    }
}

<# 
    Policies
#>
Describe "Policies" {
        It "TokenIssuancePolicies" {

            $tokenIssuancePolicy = Get-MgPolicyTokenIssuancePolicy
            if($null -eq $tokenIssuancePolicy){
                $definition = Get-Content .\TokenIssuance.txt
                $tokenIssuancePolicy = New-MgPolicyTokenIssuancePolicy -Definition $definition -DisplayName "Raptor Token Issuance" -Debug
            }

            $tokenIssuancePolicy | Should -Not -BeNullOrEmpty
            $raptorIdentifiers.TokenIssuancePolicyId = $tokenIssuancePolicy.Id

        }
    }


AfterAll {
    $raptorIdentifiers | ConvertTo-Json -Depth 1 | Out-File "RaptorIdentifiers.json"
}

