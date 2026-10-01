param(
    [Parameter(Mandatory = $true)][string]$ServerExecutable,
    [Parameter(Mandatory = $true)][string]$ClientExecutable,
    [Parameter(Mandatory = $true)][string]$TerrainExecutable
)

$ErrorActionPreference = 'Stop'

function Get-FreeTcpPort {
    $listener = [System.Net.Sockets.TcpListener]::new(
        [System.Net.IPAddress]::Loopback,
        0)
    try {
        $listener.Start()
        return [int]$listener.LocalEndpoint.Port
    }
    finally {
        $listener.Stop()
    }
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $json = $Value | ConvertTo-Json -Depth 16
    $utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $json, $utf8WithoutBom)
}

function Wait-TcpEndpoint {
    param(
        [Parameter(Mandatory = $true)][int]$Port,
        [Parameter(Mandatory = $true)][System.Diagnostics.Process]$Process,
        [Parameter(Mandatory = $true)][string]$StandardOutputPath,
        [Parameter(Mandatory = $true)][string]$StandardErrorPath,
        [int]$TimeoutMilliseconds = 5000
    )

    $deadline = [DateTime]::UtcNow.AddMilliseconds($TimeoutMilliseconds)
    while ([DateTime]::UtcNow -lt $deadline) {
        if ($Process.HasExited) {
            $stdout = if (Test-Path -LiteralPath $StandardOutputPath) {
                Get-Content -LiteralPath $StandardOutputPath -Raw
            }
            else {
                ''
            }
            $stderr = if (Test-Path -LiteralPath $StandardErrorPath) {
                Get-Content -LiteralPath $StandardErrorPath -Raw
            }
            else {
                ''
            }

            throw "Server exited before opening port $Port. Exit code: $($Process.ExitCode).`nstdout:`n$stdout`nstderr:`n$stderr"
        }

        $client = [System.Net.Sockets.TcpClient]::new()
        try {
            $client.Connect('127.0.0.1', $Port)
            if ($client.Connected) {
                return
            }
        }
        catch {
        }
        finally {
            $client.Dispose()
        }

        Start-Sleep -Milliseconds 20
    }

    throw "Server endpoint did not become reachable on port $Port before the deadline."
}

function Wait-LogText {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][System.Diagnostics.Process]$Process,
        [int]$TimeoutMilliseconds = 5000
    )

    $deadline = [DateTime]::UtcNow.AddMilliseconds($TimeoutMilliseconds)
    while ([DateTime]::UtcNow -lt $deadline) {
        if (Test-Path -LiteralPath $Path) {
            [string]$content = Get-Content -LiteralPath $Path -Raw
            if (
                [string]::IsNullOrEmpty($content) -eq $false -and
                $content.Contains($Text)
            ) {
                return
            }
        }

        if ($Process.HasExited) {
            throw "Client exited before reporting a successful connection. Exit code: $($Process.ExitCode)"
        }

        Start-Sleep -Milliseconds 20
    }

    throw "Client did not report '$Text' before the deadline."
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("erelia-client-server-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

$serverProcess = $null
$clientProcess = $null
$terrainProcess = $null

try {
    $port = Get-FreeTcpPort
    do {
        $terrainPort = Get-FreeTcpPort
    } while ($terrainPort -eq $port)

    $terrainConfigPath = Join-Path $tempRoot 'terrain.json'
    Write-JsonFile -Path $terrainConfigPath -Value ([ordered]@{
        'server config' = [ordered]@{ port = $terrainPort }
    })
    $terrainOut = Join-Path $tempRoot 'terrain.out.log'
    $terrainErr = Join-Path $tempRoot 'terrain.err.log'
    $terrainProcess = Start-Process -FilePath $TerrainExecutable -WorkingDirectory (Split-Path -Parent $TerrainExecutable) -ArgumentList @("--config=$terrainConfigPath") -RedirectStandardOutput $terrainOut -RedirectStandardError $terrainErr -PassThru
    Wait-TcpEndpoint -Port $terrainPort -Process $terrainProcess -StandardOutputPath $terrainOut -StandardErrorPath $terrainErr

    $serverConfigPath = Join-Path $tempRoot 'server.json'
    Write-JsonFile -Path $serverConfigPath -Value ([ordered]@{
        'server config' = [ordered]@{
            port = $port
            nodeReconnectDelayMs = 25
        }
        nodes = @(
            [ordered]@{
                name = 'terrain'
                address = '127.0.0.1'
                port = $terrainPort
            }
        )
    })

    $clientConfigPath = Join-Path $tempRoot 'client.json'
    Write-JsonFile -Path $clientConfigPath -Value ([ordered]@{
        'server config' = [ordered]@{
            address = '127.0.0.1'
            port = $port
            retryDelayMs = 25
        }
        'terrain config' = [ordered]@{ viewRange = 2; unloadRange = 3 }
    })

    $serverOut = Join-Path $tempRoot 'server.out.log'
    $serverErr = Join-Path $tempRoot 'server.err.log'
    $clientOut = Join-Path $tempRoot 'client.out.log'
    $clientErr = Join-Path $tempRoot 'client.err.log'

    $serverProcess = Start-Process -FilePath $ServerExecutable -WorkingDirectory (Split-Path -Parent $ServerExecutable) -ArgumentList @("--config=$serverConfigPath") -RedirectStandardOutput $serverOut -RedirectStandardError $serverErr -PassThru

    Wait-TcpEndpoint -Port $port -Process $serverProcess -StandardOutputPath $serverOut -StandardErrorPath $serverErr

    $clientProcess = Start-Process -FilePath $ClientExecutable -WorkingDirectory (Split-Path -Parent $ClientExecutable) -ArgumentList @("--config=$clientConfigPath") -RedirectStandardOutput $clientOut -RedirectStandardError $clientErr -PassThru

    Wait-LogText -Path $clientErr -Text 'Connected to dedicated Server' -Process $clientProcess
    Wait-LogText -Path $clientErr -Text 'Chunk acquired' -Process $clientProcess

    if ($clientProcess.HasExited) {
        throw "Client did not remain alive after connecting. Exit code: $($clientProcess.ExitCode)"
    }

    Stop-Process -Id $serverProcess.Id -Force
    $serverProcess.WaitForExit(5000) | Out-Null

    if ($clientProcess.WaitForExit(5000) -eq $false) {
        throw 'Client did not terminate after the Server connection was lost.'
    }

    if ($clientProcess.ExitCode -eq 0) {
        throw 'Unexpected Server loss must terminate EreliaClient with a failure exit code.'
    }
}
finally {
    if ($clientProcess -ne $null -and $clientProcess.HasExited -eq $false) {
        Stop-Process -Id $clientProcess.Id -Force -ErrorAction SilentlyContinue
    }
    if ($serverProcess -ne $null -and $serverProcess.HasExited -eq $false) {
        Stop-Process -Id $serverProcess.Id -Force -ErrorAction SilentlyContinue
    }

    if ($terrainProcess -ne $null -and $terrainProcess.HasExited -eq $false) {
        Stop-Process -Id $terrainProcess.Id -Force -ErrorAction SilentlyContinue
    }

    Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
