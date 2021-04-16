$obj = [ordered]@{
    version = "2.0.0"
    tasks = @()
}

$obj.tasks += [ordered]@{
    label = "build"
    command = "dotnet"
    type = "shell"
    args = @(
        "build",
        "`${workspaceFolder}/msgraph-sdk-raptor.sln",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
    )
    group = "build"
    presentation = @{
        reveal = "silent"
    }
    problemMatcher = "`$msCompile"
}

$obj.tasks += [ordered]@{
    label = "checkout docs repo"
    type = "shell"
    command = "`${workspaceFolder}/scripts/tasks/checkout-docs-repo.ps1 '`${workspaceFolder}/..' -branchName '`${input:branchName}'"
    presentation = [ordered]@{
        echo = $true
        reveal = "always"
        focus = $false
        panel = "shared"
        showReuseMessage = $true
        clear = $false
    }
}

$testProjects = Get-ChildItem $PSScriptRoot/../*Tests | Select-Object -ExpandProperty Name

foreach ($testProject in $testProjects)
{
    $task = [ordered]@{
        label = $testProject
        type = "process"
        command = "dotnet"
        args = @(
            "test",
            "`${workspaceFolder}/$testProject/$testProject.csproj"
        )
        isTestCommand = $true
        problemMatcher = "`$msCompile"
    }

    $obj.tasks += $task

    # deep copy
    $taskFiltered = $task | ConvertTo-Json -Depth 3 | ConvertFrom-Json
    $taskFiltered.label += " filtered"
    $taskFiltered.args += "--filter `"`${input:testFilter}`""

    $obj.tasks += $taskFiltered
}

$obj.inputs = @(
    [ordered]@{
        id = "branchName"
        type = "promptString"
        description = "documentation repo branch name"
    },
    [ordered]@{
        id = "testFilter"
        type = "promptString"
        description = "test filter"
    }
)

$obj | ConvertTo-Json -Depth 10 > $PSScriptRoot/tasks.json