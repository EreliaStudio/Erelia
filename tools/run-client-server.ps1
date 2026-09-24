$ErrorActionPreference = 'Stop'

$mode = 'Debug'

for ($i = 0; $i -lt $args.Count; $i++) {
    $argument = [string]$args[$i]

    if ($argument -match '^--mode=(Debug|Release)$') {
        $mode = $Matches[1]
        continue
    }

    if ($argument -match '^-Mode=(Debug|Release)$') {
        $mode = $Matches[1]
        continue
    }

    if ($argument -in @('--mode', '-Mode')) {
        if ($i + 1 -ge $args.Count) {
            throw "Missing value for $argument. Expected Debug or Release."
        }

        $candidate = [string]$args[++$i]
        if ($candidate -notin @('Debug', 'Release')) {
            throw "Unsupported mode '$candidate'. Expected Debug or Release."
        }

        $mode = $candidate
        continue
    }

    throw "Unknown argument '$argument'. Usage: ./tools/run-client-server.ps1 [--mode=Debug|Release]"
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$preset = $mode.ToLowerInvariant()
$prepareSparkle = Join-Path $repoRoot 'tools/dev/prepare-sparkle.ps1'
$nodesSourceRoot = Join-Path $repoRoot 'server/nodes'
$runtimeConfigRoot = Join-Path $repoRoot "build/$preset/runtime-config"

if (-not (Get-Command cmake -ErrorAction SilentlyContinue)) {
    throw 'cmake was not found in PATH.'
}

Write-Host "[Erelia] Preparing Sparkle ($mode)..."
& $prepareSparkle -Configuration $mode -Variant Full

Push-Location $repoRoot
try {
    Write-Host "[Erelia] Configuring CMake preset '$preset'..."
    & cmake --preset $preset
    if ($LASTEXITCODE -ne 0) {
        throw "CMake configuration failed with exit code $LASTEXITCODE."
    }

    Write-Host "[Erelia] Building Server nodes, Server and Client ($mode)..."
    & cmake --build --preset $preset --target EreliaServerNodes EreliaServer EreliaClient
    if ($LASTEXITCODE -ne 0) {
        throw "CMake build failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

function Get-FreeTcpPort {
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
    try {
        $listener.Start()
        return [int]$listener.LocalEndpoint.Port
    }
    finally {
        $listener.Stop()
    }
}

$allocatedPorts = [System.Collections.Generic.HashSet[int]]::new()

function Get-UniqueFreeTcpPort {
    do {
        $port = Get-FreeTcpPort
    } while (-not $allocatedPorts.Add($port))
    return $port
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $parent = Split-Path -Parent $Path
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
    $json = $Value | ConvertTo-Json -Depth 32
    $utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $json, $utf8WithoutBom)
}

if (Test-Path -LiteralPath $runtimeConfigRoot) {
    Remove-Item -LiteralPath $runtimeConfigRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $runtimeConfigRoot -Force | Out-Null

$nodeDirectories = @(
    Get-ChildItem -LiteralPath $nodesSourceRoot -Directory |
        Where-Object {
            Test-Path -LiteralPath (Join-Path $_.FullName 'CMakeLists.txt')
        } |
        Sort-Object Name
)

$routerNodes = @()
$nodeRuntimeConfigs = @{}

foreach ($nodeDirectory in $nodeDirectories) {
    $sourceConfigPath = Join-Path $nodeDirectory.FullName 'resources/config.json'
    if (-not (Test-Path -LiteralPath $sourceConfigPath)) {
        throw "Node '$($nodeDirectory.Name)' is missing '$sourceConfigPath'."
    }

    $nodeConfig = Get-Content -LiteralPath $sourceConfigPath -Raw | ConvertFrom-Json
    if ($null -eq $nodeConfig.'server config') {
        throw "Node '$($nodeDirectory.Name)' config is missing 'server config'."
    }

    $port = Get-UniqueFreeTcpPort
    $nodeConfig.'server config'.port = $port

    $runtimePath = Join-Path $runtimeConfigRoot ("node-" + $nodeDirectory.Name + ".json")
    Write-JsonFile -Value $nodeConfig -Path $runtimePath
    $nodeRuntimeConfigs[$nodeDirectory.Name] = $runtimePath

    $routerNodes += [ordered]@{
        name = $nodeDirectory.Name
        address = '127.0.0.1'
        port = $port
    }
}

$routerTemplatePath = Join-Path $repoRoot 'server/resources/config.json'
$routerConfig = Get-Content -LiteralPath $routerTemplatePath -Raw | ConvertFrom-Json
$routerConfig.'server config'.port = Get-UniqueFreeTcpPort
$routerConfig.nodes = @($routerNodes)

$routerRuntimeConfig = Join-Path $runtimeConfigRoot 'router.json'
Write-JsonFile -Value $routerConfig -Path $routerRuntimeConfig

$serverExecutable = Join-Path $repoRoot "build/$preset/server/EreliaServer.exe"
$clientExecutable = Join-Path $repoRoot "build/$preset/client/EreliaClient.exe"

if (-not (Test-Path -LiteralPath $serverExecutable)) {
    throw "Server executable was not found at '$serverExecutable'."
}
if (-not (Test-Path -LiteralPath $clientExecutable)) {
    throw "Client executable was not found at '$clientExecutable'."
}

$clientRuntimePath = if ($mode -eq 'Debug') {
    Join-Path $repoRoot 'build/debug/vcpkg_installed/x64-windows/debug/bin'
}
else {
    Join-Path $repoRoot 'build/release/vcpkg_installed/x64-windows/bin'
}

function ConvertTo-PowerShellLiteral {
    param([Parameter(Mandatory = $true)][string]$Value)
    return "'" + $Value.Replace("'", "''") + "'"
}

function Start-EreliaConsole {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Executable,
        [string[]]$Arguments = @(),
        [string]$AdditionalPath
    )

    $repoLiteral = ConvertTo-PowerShellLiteral $repoRoot
    $executableLiteral = ConvertTo-PowerShellLiteral $Executable
    $titleLiteral = ConvertTo-PowerShellLiteral "Erelia $Name ($mode)"
    $startMessageLiteral = ConvertTo-PowerShellLiteral "[Erelia] Starting $Name ($mode)..."
    $exitMessagePrefixLiteral = ConvertTo-PowerShellLiteral "[Erelia] $Name exited with code "

    $argumentLiterals = @(
        $Arguments | ForEach-Object {
            ConvertTo-PowerShellLiteral ([string]$_)
        }
    )

    $commands = @(
        '$Host.UI.RawUI.WindowTitle = ' + $titleLiteral
        'Set-Location -LiteralPath ' + $repoLiteral
    )

    if ($AdditionalPath) {
        $pathLiteral = ConvertTo-PowerShellLiteral $AdditionalPath
        $commands += '$env:PATH = ' + $pathLiteral + " + ';' + " + '$env:PATH'
    }

    $commands += '$processArguments = @(' + ($argumentLiterals -join ', ') + ')'
    $commands += @(
        'Write-Host ' + $startMessageLiteral
        '& ' + $executableLiteral + ' @processArguments'
        '$processExitCode = $LASTEXITCODE'
        'Write-Host (' + $exitMessagePrefixLiteral + ' + $processExitCode)'
    )

    $command = $commands -join [Environment]::NewLine
    $encodedCommand = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))

    $powerShellHost = (Get-Process -Id $PID).Path
    if ([string]::IsNullOrWhiteSpace($powerShellHost)) {
        $powerShellHost = 'powershell.exe'
    }

    Start-Process -FilePath $powerShellHost -ArgumentList @('-NoExit', '-NoProfile', '-EncodedCommand', $encodedCommand) -WorkingDirectory $repoRoot | Out-Null
}

Write-Host '[Erelia] Launching Server nodes...'
foreach ($nodeDirectory in $nodeDirectories) {
    $nodeBuildDirectory = Join-Path $repoRoot "build/$preset/server/nodes/$($nodeDirectory.Name)"
    $executables = @(Get-ChildItem -LiteralPath $nodeBuildDirectory -Filter 'Erelia*Node.exe' -File)

    if ($executables.Count -ne 1) {
        throw "Expected exactly one node executable in '$nodeBuildDirectory', found $($executables.Count)."
    }

    Start-EreliaConsole -Name ("Node " + $nodeDirectory.Name) -Executable $executables[0].FullName -Arguments @("--config=$($nodeRuntimeConfigs[$nodeDirectory.Name])")
}

Write-Host '[Erelia] Launching main Server router...'
Start-EreliaConsole -Name 'Server' -Executable $serverExecutable -Arguments @("--config=$routerRuntimeConfig")

Write-Host '[Erelia] Launching Client...'
Start-EreliaConsole -Name 'Client' -Executable $clientExecutable -AdditionalPath $clientRuntimePath
