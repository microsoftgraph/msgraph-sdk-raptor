# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $AppSettingsPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/appsettings.json" -Resolve),
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$appSettings = Get-AppSettings -AppSettingsPath $AppSettingsPath
$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath
$domain = Get-CurrentDomain -AppSettings $appSettings


$userId = $identifiers.user._value
$userId
if($null -eq $chat) {
    $content = Get-RequestData -ChildEntity "chat"
    $content.members[0].'user@odata.bind' += $userId
    $user2 = reqDelegated -url "users"  |Select-Object -First 2
    $content.members[1].'user@odata.bind' += $user2[0].id
    $content.members[2].'user@odata.bind' += $user2[1].id
    $chat = reqDelegated -url "chats" -method "POST" -body $content
    $chat.id
}
$identifiers.chat._value = $chat.id

if($null -eq $conversationMember) {
    $conversationMember = reqDelegated -url "chats/$($identifiers.chat._value)/members" | Select-Object -Last 1
    $conversationMember.id
}
$identifiers.chat.conversationMember._value = $conversationMember.id

if($null -eq $teamsAppInstallation) {
    $teamsAppInstallation = reqDelegated -url "chats/$($identifiers.chat._value)/installedApps?`$expand=teamsApp&`$filter=teamsApp/displayName eq 'Word'" | Select-Object -First 1
    $teamsAppInstallation.id
}
$identifiers.chat.teamsAppInstallation._value = $teamsAppInstallation.id

if($null -eq $wordTeamsApp) {
    $wordTeamsApp = reqDelegated -url "appCatalogs/teamsApps?`$filter=displayName eq 'Word'"
    $wordTeamsApp.id
}

if($null -eq $teamsTabContent) {
    $teamsTabContent = Get-RequestData -ChildEntity "teamsTab"
    $teamsTabContent.'teamsApp@odata.bind' += $wordTeamsApp.id
    $teamsTab = reqDelegated -url "chats/$($identifiers.chat._value)/tabs" -method "POST" -body $teamsTabContent
    $teamsTab.id
}
$identifiers.chat.teamsTab._value = $teamsTab.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
