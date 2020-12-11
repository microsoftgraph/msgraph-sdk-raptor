# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

# set output and typewriter executable path
$outputPath = Join-Path $env:BUILD_SOURCESDIRECTORY "output"
Write-Host "Path to typewriter output $outputPath"

$typewriterDirectory = Join-Path $env:BUILD_SOURCESDIRECTORY "MSGraph-SDK-Code-Generator" "src" "Typewriter" "bin" $env:BuildConfiguration "net5.0"
$typewriterPath = Join-Path $typewriterDirectory "Typewriter"
Write-Host "Path to typewriter tool: $typewriterPath"

# download metadata
$metadataDir = Join-Path $env:BUILD_SOURCESDIRECTORY "metadata"
mkdir $metadataDir
$metadataFile = Join-Path $metadataDir "metadata.xml"
Invoke-WebRequest -Uri $env:MetadataURL -OutFile $metadataFile
Write-Host "Downloaded $env:MetadataURL to $metadataFile" -ForegroundColor Green

# transform metadata
$xslTranformScriptPath = "$env:BUILD_SOURCESDIRECTORY/msgraph-metadata/transforms/csdl/transform.ps1"
$xslPath = "$env:BUILD_SOURCESDIRECTORY/msgraph-metadata/transforms/csdl/preprocess_csdl.xsl"
$cleanMetadataFile = Join-Path $metadataDir "clean_metadata.xml"

# transform script uses relative path to working directory
& $xslTranformScriptPath -xslPath $xslPath -inputPath $metadataFile -outputPath $cleanMetadataFile
Write-Host "Transformed $metadataFile to $cleanMetadataFile" -ForegroundColor Green

# run Typewriter
Write-Host "Running Typewriter..."
Push-Location -Path $typewriterDirectory
& $typewriterPath -v Info -m $cleanMetadataFile -o $outputPath -g Files
Pop-Location

# set repo models directory
$repoModelsDir = "$env:BUILD_SOURCESDIRECTORY/msgraph-sdk-dotnet/src/Microsoft.Graph/Generated/"
Write-Host "Path to repo models directory: $repoModelsDir"

# clean old models
Remove-Item -Recurse $repoModelsDir | Write-Host
Write-Host "Removed the existing generated files in the repo." -ForegroundColor Green

# copy new models
$modelsDirectory = Join-Path $outputPath "/com/microsoft/graph/"
Move-Item $modelsDirectory $repoModelsDir
Write-Host "Moved the models from $modelsDirectory into the local repo." -ForegroundColor Green