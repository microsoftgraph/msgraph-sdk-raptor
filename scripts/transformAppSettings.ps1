param(
    [bool]$IsLocalRun = $false,
    [bool]$GenerateLinqPadOutputInLocalRun = $false,
    [string]$DocsRepoCheckoutDirectory = "C:/github",
    [Parameter(Mandatory=$true)][string]$TenantID,
    [Parameter(Mandatory=$true)][string]$ClientID,
    [Parameter(Mandatory=$true)][string]$ClientSecret,
    [Parameter(Mandatory=$true)][string]$Username,
    [Parameter(Mandatory=$true)][string]$Password,
    [Parameter(Mandatory=$true)][string]$Authority,
    [Parameter(Mandatory=$true)][string]$RaptorStorageConnectionString
)

$json = @{
    TenantID = $TenantID;
    ClientID = $ClientID;
    ClientSecret = $ClientSecret;
    IsLocalRun = $IsLocalRun;
    GenerateLinqPadOutputInLocalRun = $GenerateLinqPadOutputInLocalRun;
    DocsRepoCheckoutDirectory = $DocsRepoCheckoutDirectory;
    Username = $Username;
    Password = $Password;
    Authority = $Authority;
    RaptorStorageConnectionString = $RaptorStorageConnectionString;
}

$repoRoot = (Get-Item $MyInvocation.MyCommand.Source).Directory.Parent.FullName
$libDir = Join-Path $repoRoot "msgraph-sdk-raptor-compiler-lib"
$appSettings = Join-Path $libDir "appsettings.json"

($json | ConvertTo-Json) > $appSettings
