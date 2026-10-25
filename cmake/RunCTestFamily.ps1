param(
    [Parameter(Mandatory = $true)][string]$BuildDirectory,
    [Parameter(Mandatory = $true)][string]$Family,
    [Parameter(Mandatory = $true)][string]$ReportDirectory,
    [switch]$Optional
)

$ErrorActionPreference = 'Stop'
# CTest resolves output-junit paths from its build directory, so pass an
# absolute destination rather than a path relative to the caller.
$ReportDirectory = [System.IO.Path]::GetFullPath($ReportDirectory)
New-Item -ItemType Directory -Path $ReportDirectory -Force | Out-Null

# CTest succeeds even when -L matches zero tests; distinguish that from a
# meaningful pass so a renamed or missing mandatory family cannot go unnoticed.
$catalog = & ctest --test-dir $BuildDirectory --show-only=json-v1 -L "^$Family$"
if ($LASTEXITCODE -ne 0) {
    throw "CTest discovery failed for $Family"
}
$count = @((($catalog -join "`n" | ConvertFrom-Json).tests)).Count
if ($count -eq 0) {
    if ($Optional) {
        Write-Host "SKIPPED: $Family has no tests yet."
        exit 0
    }
    throw "Mandatory CTest family '$Family' has no discovered tests."
}

Write-Host "Running $count test(s) in $Family"
& ctest --test-dir $BuildDirectory --output-on-failure -L "^$Family$" --output-junit (Join-Path $ReportDirectory "$Family.xml")
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
