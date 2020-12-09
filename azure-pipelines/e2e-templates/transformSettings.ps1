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

.Parameter Language
    Required. Language to run the tests against

.Parameter JavaCoreVersion
    Optional. Version number for the core library to use

.Parameter JavaLibVersion
    Optional. Version number for the service library to use

.Parameter JavaPreviewLibPath
    Optional. Path containing the core and service library repositories. Using this setting will ignore Java Core and Service lib versions.
#>
[CmdletBinding(DefaultParameterSetName='CSharp')]
Param(
    [Parameter(Mandatory = $true, ParameterSetName="CSharp")]
    [Parameter(Mandatory = $false, ParameterSetName="Java")]
    [string]$Version,
    [Parameter(Mandatory = $true, ParameterSetName="CSharp")]
    [Parameter(Mandatory = $false, ParameterSetName="Java")]
    [string]$KnownFailuresRequested,
    [Parameter(Mandatory = $true, ParameterSetName="CSharp")][string]$DllPath,
    [Parameter(Mandatory = $true, ParameterSetName="CSharp")]
    [Parameter(Mandatory = $false, ParameterSetName="Java")]
    [string]$Language,
    [Parameter(Mandatory = $true, ParameterSetName="CSharp")]
    [Parameter(Mandatory = $true, ParameterSetName="Java")]
    [string]$RunSettingsPath,
    [Parameter(Mandatory = $false, ParameterSetName="Java")][string]$JavaCoreVersion="",
    [Parameter(Mandatory = $false, ParameterSetName="Java")][string]$JavaLibVersion="",
    [Parameter(Mandatory = $false, ParameterSetName="Java")][string]$JavaPreviewLibPath=""
)

$mapping = @{}

$mapping.Add("Version", $Version)
$mapping.Add("KnownFailuresRequested", $KnownFailuresRequested)
$mapping.Add("DllPath", $DllPath)
$mapping.Add("Language", $Language)
$mapping.Add("JavaCoreVersion", $JavaCoreVersion)
$mapping.Add("JavaLibVersion", $JavaLibVersion)
$mapping.Add("JavaPreviewLibPath", $JavaPreviewLibPath)

[xml]$settings = Get-Content $RunSettingsPath
$settings.RunSettings.TestRunParameters.Parameter | % {
    Write-Host "Setting $($_.name) to $($mapping[$_.name])"
    $_.value = $mapping[$_.name];
}

$settings.Save((Resolve-Path $RunSettingsPath))