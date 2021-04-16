param(
    [Parameter(Mandatory=$true)][string]$rootDirectory,
    [string]$branchName = "main"
)

$docsRepoName = "microsoft-graph-docs"

Set-Location $rootDirectory
$docsRepoExists = Test-Path $docsRepoName
if (!$docsRepoExists)
{
    git clone "https://github.com/microsoftgraph/$docsRepoName"
}

Set-Location $docsRepoName
git fetch
git reset --hard
git checkout $branchName
git pull -f

Write-Host "checkout docs repo script"