param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidatePattern('^[A-Za-z][A-Za-z0-9]*$')]
    [string]$Name
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$templateRoot = Join-Path $repoRoot 'tools/templates/server-node'
$nodesRoot = Join-Path $repoRoot 'server/nodes'

$nodeName = $Name.Substring(0, 1).ToUpperInvariant() + $Name.Substring(1)
$nodeSnake = [regex]::Replace($nodeName, '(?<!^)([A-Z])', '_$1').ToLowerInvariant()
$nodeSlug = $nodeSnake.Replace('_', '-')
$destination = Join-Path $nodesRoot $nodeSlug

if (-not (Test-Path -LiteralPath $templateRoot)) {
    throw "Server-node template was not found at '$templateRoot'."
}

if (Test-Path -LiteralPath $destination) {
    throw "Server node '$nodeSlug' already exists at '$destination'."
}

New-Item -ItemType Directory -Path $destination -Force | Out-Null
Copy-Item -Path (Join-Path $templateRoot '*') -Destination $destination -Recurse -Force

$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
Get-ChildItem -LiteralPath $destination -Recurse -File | ForEach-Object {
    $content = [System.IO.File]::ReadAllText($_.FullName)
    $content = $content.Replace('__NODE_NAME__', $nodeName)
    $content = $content.Replace('__NODE_SNAKE__', $nodeSnake)
    $content = $content.Replace('__NODE_SLUG__', $nodeSlug)
    [System.IO.File]::WriteAllText($_.FullName, $content, $utf8WithoutBom)
}

Rename-Item -LiteralPath (Join-Path $destination 'include/node.hpp') -NewName ($nodeSnake + '_node.hpp')
Rename-Item -LiteralPath (Join-Path $destination 'include/node_application.hpp') -NewName ($nodeSnake + '_node_application.hpp')
Rename-Item -LiteralPath (Join-Path $destination 'src/node.cpp') -NewName ($nodeSnake + '_node.cpp')
Rename-Item -LiteralPath (Join-Path $destination 'src/node_application.cpp') -NewName ($nodeSnake + '_node_application.cpp')
Rename-Item -LiteralPath (Join-Path $destination 'tests/node_test.cpp') -NewName ($nodeSnake + '_node_test.cpp')

Write-Host "[Erelia] Created Server node '$nodeName' at server/nodes/$nodeSlug."
Write-Host '[Erelia] CMake, tests and run-client-server discovery are automatic.'
Write-Host "[Erelia] Extend server/nodes/$nodeSlug/resources/config.json with node-specific settings as needed."
