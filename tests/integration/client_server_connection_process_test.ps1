param(
    [Parameter(Mandatory = $true)][string]$ServerExecutable,
    [Parameter(Mandatory = $true)][string]$ClientExecutable
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
        [int]$TimeoutMilliseconds = 5000
    )

    $deadline = [DateTime]::UtcNow.AddMilliseconds($TimeoutMilliseconds)
    while ([DateTime]::UtcNow -lt $deadline) {
        $client = [System.Net.Sockets.TcpClient]::new()
        try {
            $connection = $client.ConnectAsync('127.0.0.1', $Port)
            if ($connection.Wait(100) -and $client.Connected) {
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

    throw "Server endpoint did not become reachable on port $Port."
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
            $content = Get-Content -LiteralPath $Path -Raw
            if ($content.Contains($Text)) {
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

try {
    $port = Get-FreeTcpPort

    $serverConfigPath = Join-Path $tempRoot 'server.json'
    Write-JsonFile -Path $serverConfigPath -Value ([ordered]@{
        'server config' = [ordered]@{
            port = $port
            nodeReconnectDelayMs = 25
        }
        nodes = @()
    })

    $clientConfigPath = Join-Path $tempRoot 'client.json'
    Write-JsonFile -Path $clientConfigPath -Value ([ordered]@{
        'server config' = [ordered]@{
            address = '127.0.0.1'
            port = $port
        }
    })

    $serverOut = Join-Path $tempRoot 'server.out.log'
    $serverErr = Join-Path $tempRoot 'server.err.log'
    $clientOut = Join-Path $tempRoot 'client.out.log'
    $clientErr = Join-Path $tempRoot 'client.err.log'

    $serverProcess = Start-Process -FilePath $ServerExecutable -ArgumentList @("--config=$serverConfigPath") -RedirectStandardOutput $serverOut -RedirectStandardError $serverErr -PassThru

    Wait-TcpEndpoint -Port $port

    $clientProcess = Start-Process -FilePath $ClientExecutable -ArgumentList @("--config=$clientConfigPath") -RedirectStandardOutput $clientOut -RedirectStandardError $clientErr -PassThru

    Wait-LogText -Path $clientOut -Text 'Connected to dedicated Server' -Process $clientProcess

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

    Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
