param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration,

    [ValidateSet('Full', 'Core')]
    [string]$Variant = 'Full'
)

$ErrorActionPreference = 'Stop'

$ereliaRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$sparkleRoot = if ($env:SPARKLE_SOURCE_DIR) {
    $env:SPARKLE_SOURCE_DIR
} else {
    Join-Path (Split-Path $ereliaRoot -Parent) 'Sparkle'
}
$sparkleCMake = Join-Path $sparkleRoot 'CMakeLists.txt'
if (-not (Test-Path -LiteralPath $sparkleCMake)) {
    throw "Sparkle source checkout was not found at $sparkleRoot"
}
if (-not $env:VCPKG_ROOT) {
    throw 'VCPKG_ROOT must point to a vcpkg installation.'
}

$name = $Variant.ToLowerInvariant()
$buildDir = Join-Path $ereliaRoot "build/dependencies/sparkle-$name-$Configuration-build"
$installDir = Join-Path $ereliaRoot "build/dependencies/sparkle-$name-$Configuration-install"
$graphics = if ($Variant -eq 'Full') { 'ON' } else { 'OFF' }
$noDefaultFeatures = if ($Variant -eq 'Core') { 'ON' } else { 'OFF' }
$toolchain = Join-Path $env:VCPKG_ROOT 'scripts/buildsystems/vcpkg.cmake'

& cmake -S $sparkleRoot -B $buildDir -G Ninja `
    '-DCMAKE_CXX_COMPILER=clang++' `
    "-DCMAKE_BUILD_TYPE=$Configuration" `
    "-DCMAKE_TOOLCHAIN_FILE=$toolchain" `
    '-DVCPKG_TARGET_TRIPLET=x64-windows' `
    "-DVCPKG_MANIFEST_NO_DEFAULT_FEATURES=$noDefaultFeatures" `
    "-DCMAKE_INSTALL_PREFIX=$installDir" `
    "-DSPARKLE_BUILD_GRAPHICS=$graphics" `
    '-DSPARKLE_BUILD_TESTS=OFF' `
    "-DSPARKLE_BUILD_TEST_LIBRARY=$graphics"
if ($LASTEXITCODE -ne 0) { throw 'Sparkle configuration failed.' }

& cmake --build $buildDir --parallel 4
if ($LASTEXITCODE -ne 0) { throw 'Sparkle build failed.' }

& cmake --install $buildDir
if ($LASTEXITCODE -ne 0) { throw 'Sparkle installation failed.' }
