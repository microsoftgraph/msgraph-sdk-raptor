# msgraph-sdk-raptor

[![Dependabot Status](https://api.dependabot.com/badges/status?host=github&repo=microsoftgraph/msgraph-sdk-raptor)](https://dependabot.com)

This repository consists of test projects which are broadly categorized into 2.

1. compilation tests
2. execution tests

The compilation tests, test the successful compilation of the language-specific snippets from Microsoft Graph documentation. For each snippet, there is an NUnit test case that outputs compilation result i.e whether a test compiled successfully or not. 

The execution tests, other than testing compilation of the snippets, use the compiled binary to make a request to the demo tenant and reports error if there's a service exception i.e 4XX or 5xx response. Otherwise reports success.

A test result for both compilation and execution tests includes:
- Root cause, if it is a known failure
- Documentation page where the snippet appears
- Piece of code that is to be compiled with line numbers
- Compiler error message

There are 7 C# test projects in total as noted below. The first 4 C# tests below are compilation tests, the next 2 are execution tests and finally an arbitraryDllTest.

1. CsharpBetaTests
2. CsharpBetaKnownFailureTests
3. CsharpV1Tests
4. CsharpV1KnownFailureTests

5. CsharpBetaExecutionTests
6. CsharpV1ExecutionTests

7. CSharpArbitraryDllTests

 The arbitraryDllTest is useful in running customized tests for an unpublished dll, which can consist of a proposed metadata or generator changes.

There are also 4 Java test projects, as listed below. These are all compilation tests

1. JavaBetaTests
2. JavaBetaKnownFailureTests
3. JavaV1Tests
4. JavaV1KnownFailureTests


## How to run locally
1. Clone this repository
2. Clone microsoft-graph-docs repository:
   - `git clone https://github.com/microsoftgraph/microsoft-graph-docs`
3. Open msgraph-sdk-raptor.sln in Visual Studio
4. Make sure that the settings are correct for local run in `msgraph-sdk-raptor\msgraph-sdk-raptor-compiler-lib\appsettings.json`
   1. `"IsLocalRun"=true`
   2. `"DocsRepoCheckoutDirectory"= <parent directory for microsoft-graph-docs>`
5. Build and run tests
   - e.g `dotnet test CsharpV1ExecutionTests`
   - **Test count will show as 1 for each project initially** because test cases are generated on the fly from a single meta test description.
6. Local test runs also generate `.linq` files so that they can be analyzed using LinqPad.
   - If you want this option to be turned on, make sure that you have this in the settings: `"GenerateLinqPadOutputInLocalRun": true`
   - Default drop location for these files is:
     - `~\Documents\LINQPad Queries\RaptorResults`
   - They will automatically appear in LinqPad if the default setting for the location of queries are not changed.
   - The prerequisite on the LinqPad side is to have NuGet references to following packages:
     - `Microsoft.Graph`
     - `Microsoft.Graph.Beta`
   - Adding references to individual queries are not necessary, as the `.linq` file that Raptor generates includes correct NuGet package referenced.


## Pipelines
The repository also contains a couple of CI pipelines. The CI pipelines run the tests outlined above and the output of these tests are then used to report success or failure rates in a graphical and analytical approach.
The pipelines are running in a private Azure DevOps instance [here](https://microsoftgraph.visualstudio.com/Graph%20Developer%20Experiences/_build?view=folders&treeState=XFJhcHRvcg%3D%3D)
There exist pipelines that run when a PR is created on `msgraph-sdk-raptor` repo and others that run on a schedule. Pipelines that are triggered by PR creation are broadly categorized into 
- those that run **excluding** known issues 
- those that run on known issues (these tests are appended the suffix "Known Issues")

The pipelines with tests that run excluding known issues, can be used to report whether any new issues were introduced. Pipelines containing tests that run on known issues, can report whether any known issues have been fixed. There exists a list of known issues, within the `TestsCommon/TestDataGenerator.cs` file, which is useful in identifying known issues. 

The pipelines are:
- Beta C# Snippets  (runs c# compilation tests)
- V1 C# Snippets  (runs c# compilation tests)

- Beta C# Snippet Execution Tests
- V1 C# Snippet Execution Tests

- V1 C# Snippets - Known Issues
- Beta C# Snippets - Known Issues

And the equivalent pipelines for running java tests are
- V1 Java Snippet Compilation Tests
- Beta Java Snippet Compilation Tests

- Beta Java Snippet Compilation Tests - Known Issues
- V1 Java Snippet Compilation Tests - Known Issues

The scheduled pipelines are categorized into daily and weekly schedules. A single scheduled pipeline can contain a mix of categories of tests, for example the `PROD Beta Compilation - Daily`, runs "Beta C# Snippets" excluding known issues and after runs "Beta C# Snippets - Known Issues".
