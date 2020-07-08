# msgraph-sdk-raptor

[![Dependabot Status](https://api.dependabot.com/badges/status?host=github&repo=microsoftgraph/msgraph-sdk-raptor)](https://dependabot.com)

This repository consists of 4 test projects.

1. CsharpBetaKnownFailureTests
2. CsharpBetaTests
3. CsharpV1KnownFailureTests
4. CsharpV1Tests

Tests compile C# snippets, which based on HTTP snippets in Microsoft Graph documentation, and report the results. For each snippet, there is an NUnit test case that outputs compilation result. Compilation result includes:
- Root cause, if it is a known failure
- Documentation page where the snippet appears
- Piece of code that is to be compiled with line numbers
- Compiler error message

## How to run locally
1. Clone this repository
2. Clone microsoft-graph-docs repository:
   - `git clone https://github.com/microsoftgraph/microsoft-graph-docs`
3. Open msgraph-sdk-raptor.sln in Visual Studio
4. Make sure that the settings are correct for local run in `msgraph-sdk-raptor\msgraph-sdk-raptor-compiler-lib\appsettings.json`
   1. `"IsLocalRun"=true`
   2. `"LocalRootGitDirectory"= <where microsoft-graph-docs repo is located>`
5. Build and run tests
   - Test count will show as 1 for each project initially because test cases are generated on the fly from a single meta test description.
6. Local test runs also generate `.linq` files so that they can be analyzed using LinqPad.
   - If you want this option to be turned on, make sure that you have this in the settings: `"GenerateLinqPadOutputInLocalRun": true`
   - Default drop location for these files is:
     - `~\Documents\LINQPad Queries\RaptorResults`
   - They will automatically appear in LinqPad if the default setting for the location of queries are not changed.
   - The prerequisite on the LinqPad side is to have NuGet references to following packages:
     - `Microsoft.Graph`
     - `Microsoft.Graph.Beta`
   - Adding references to individual queries are not necessary, as the `.linq` file that Raptor generates includes correct NuGet package referenced.
