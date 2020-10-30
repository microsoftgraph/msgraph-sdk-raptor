# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
.Synopsis
    assigns runsettings values for Raptor tests

.Description
    assigns runsettings values for Raptor tests

.Example
    .\CSharpArbitraryDllTests\transformSettings.ps1 -Version v1.0 -KnownFailuresRequested true -DllPath C:\github\msgraph-sdk-dotnet\tests\Microsoft.Graph.DotnetCore.Test\bin\Debug\netcoreapp3.1\Microsoft.Graph.dll -Language CSharp -RunSettingsPath .\CSharpArbitraryDllTests\Test.runsettings

.Parameter Version
    Required. Either v1.0 or beta

.Parameter KnownFailuresRequested
    Required. Determines whether known issue tests should be run

.Parameter DllPath
    Required. Full path to Microsoft.Graph.dll

.Parameter RunSettingsPath
    Required. Full or relative path to .runsettings file to be modified

#>
Param(
    [Parameter(Mandatory = $true)][string]$Version,
    [Parameter(Mandatory = $true)][string]$KnownFailuresRequested,
    [Parameter(Mandatory = $true)][string]$DllPath,
    [Parameter(Mandatory = $true)][string]$Language,
    [Parameter(Mandatory = $true)][string]$RunSettingsPath
)

$mapping = @{}

$mapping.Add("Version", $Version)
$mapping.Add("KnownFailuresRequested", $KnownFailuresRequested)
$mapping.Add("DllPath", $DllPath)
$mapping.Add("Language", $Language)

[xml]$settings = Get-Content $RunSettingsPath
$settings.RunSettings.TestRunParameters.Parameter | % {
    $_.value = $mapping[$_.name];
}

$settings.Save((Resolve-Path $RunSettingsPath))