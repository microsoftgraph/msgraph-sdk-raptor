param(
    [bool]$IsLocalRun = $false,
    [bool]$GenerateLinqPadOutputInLocalRun = $false,
    [string]$DocsRepoCheckoutDirectory = "C:/github",
    [string]$TenantID,
    [string]$ClientID,
    [string]$ClientSecret
)

$json = @{
    TenantID = $TenantID;
    ClientID = $ClientID;
    ClientSecret = $ClientSecret;
    IsLocalRun = $IsLocalRun;
    GenerateLinqPadOutputInLocalRun = $GenerateLinqPadOutputInLocalRun;
    DocsRepoCheckoutDirectory = $DocsRepoCheckoutDirectory;
}

$repoRoot = (Get-Item $MyInvocation.MyCommand.Source).Directory.Parent.FullName
$libDir = Join-Path $repoRoot "msgraph-sdk-raptor-compiler-lib"
$appSettings = Join-Path $libDir "appsettings.json"

($json | ConvertTo-Json) > $appSettings