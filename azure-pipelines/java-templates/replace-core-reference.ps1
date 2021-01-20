$buildFilePath = "build.gradle";
$dependenciesFilePath = "gradle/dependencies.gradle";
$buildAndDependencyFilesDistinct = (Test-Path -Path $dependenciesFilePath);
if($buildAndDependencyFilesDistinct -eq $false) {
    $dependenciesFilePath = $buildFilePath
} 
$buildContent = Get-Content -Path $buildFilePath -Raw;
$dependenciesContent = Get-Content -Path $dependenciesFilePath -Raw;
$dependenciesContent = $dependenciesContent -replace "api 'com\.microsoft\.graph:microsoft-graph-core:\d\.\d\.\d(?:-SNAPSHOT)?'", "implementation name: 'msgraph-sdk-java-core'";
$flatDirRef = "mavenCentral()`r`n    flatDir {`r`n        dirs '$Env:CORE_PATH/build/libs'`r`n    }"
if($buildAndDependencyFilesDistinct -eq $false) {
    $buildContent = $dependenciesContent
}
$buildContent = $buildContent -replace "mavenCentral\(\)", $flatDirRef
Set-Content -Value $buildContent -Path $buildFilePath -Verbose;
if($buildAndDependencyFilesDistinct -eq $true) {
    Set-Content -Value $dependenciesContent -Path $dependenciesFilePath -Verbose;
}