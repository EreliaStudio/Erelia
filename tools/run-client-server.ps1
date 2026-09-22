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

    Write-Host "[Erelia] Building server and client ($mode)..."
    & cmake --build --preset $preset --target EreliaServer EreliaClient
    if ($LASTEXITCODE -ne 0) {
        throw "CMake build failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

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
        [string]$AdditionalPath
    )

    $repoLiteral = ConvertTo-PowerShellLiteral $repoRoot
    $executableLiteral = ConvertTo-PowerShellLiteral $Executable
    $titleLiteral = ConvertTo-PowerShellLiteral "Erelia $Name ($mode)"
    $startMessageLiteral = ConvertTo-PowerShellLiteral "[Erelia] Starting $Name ($mode)..."
    $exitMessagePrefixLiteral = ConvertTo-PowerShellLiteral "[Erelia] $Name exited with code "

    $commands = @(
        "$Host.UI.RawUI.WindowTitle = $titleLiteral"
        "Set-Location -LiteralPath $repoLiteral"
    )

    if ($AdditionalPath) {
        $pathLiteral = ConvertTo-PowerShellLiteral $AdditionalPath
        $commands += "$env:PATH = $pathLiteral + ';' + $env:PATH"
    }

    $commands += @(
        "Write-Host $startMessageLiteral"
        "& $executableLiteral"
        "$processExitCode = $LASTEXITCODE"
        "Write-Host ($exitMessagePrefixLiteral + $processExitCode)"
    )

    $command = $commands -join [Environment]::NewLine
    $encodedCommand = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))

    $powerShellHost = (Get-Process -Id $PID).Path
    if ([string]::IsNullOrWhiteSpace($powerShellHost)) {
        $powerShellHost = 'powershell.exe'
    }

    Start-Process -FilePath $powerShellHost `
        -ArgumentList @('-NoExit', '-NoProfile', '-EncodedCommand', $encodedCommand) `
        -WorkingDirectory $repoRoot | Out-Null
}

Write-Host '[Erelia] Launching server and client in separate PowerShell consoles...'
Start-EreliaConsole -Name 'Server' -Executable $serverExecutable
Start-EreliaConsole -Name 'Client' -Executable $clientExecutable -AdditionalPath $clientRuntimePath
