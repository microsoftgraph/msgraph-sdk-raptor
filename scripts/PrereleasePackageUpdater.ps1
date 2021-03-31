# updates prerelease dependencies for packages listed below as $packages
# expected to be run where working directory is the root of the repo

$packages = "Microsoft.Graph.Beta","Microsoft.Graph.Auth"

$projectFile = "msgraph-sdk-raptor-compiler-lib/msgraph-sdk-raptor-compiler-lib.csproj"

foreach ($package in $packages)
{
    dotnet remove $projectFile package $package

    if ($package -eq "Microsoft.Graph.Beta")
    {
        $lowerCasePackage = $package.ToLower()
        $nugetUrl = "https://api.nuget.org/v3/registration5-gz-semver2/$lowerCasePackage/index.json"
        $res = Invoke-RestMethod $nugetUrl
        $skipVersion = [version]"4.0.0"
        $allVersionsLessThan4 = $res.items.items.catalogEntry.version | ForEach-Object {
            if ($_.Contains("preview"))
            {
                $ver = [version]($_.Replace("-preview", ""))
                if ($ver -lt $skipVersion)
                {
                    return $ver
                }
            }
        }

        $latestVersion = ($allVersionsLessThan4 | Sort-Object -Descending)[0].ToString()

        dotnet add $projectFile package $package --version "$latestVersion-preview"
    }
    else
    {
        dotnet add $projectFile package $package --prerelease
    }
}
