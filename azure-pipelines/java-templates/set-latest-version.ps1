Param(
    [String]$url="https://repo1.maven.org/maven2/com/microsoft/graph/microsoft-graph/maven-metadata.xml",
    [String]$versionName="V1")
$webResponse = Invoke-WebRequest $url -UseBasicParsing
$version = Select-Xml -Content $webResponse.Content -XPath '/metadata/versioning/versions/version[last()]'
$serviceLibraryVersion = $version.Node.InnerText

Write-Host "Java $versionName Service Library Version: $serviceLibraryVersion"
Write-Host "##vso[task.setvariable variable=serviceLibraryVersion]$serviceLibraryVersion"