param(
    [string]$Path = "."
)

function Get-VisibleItems {
    param(
        [string]$CurrentPath
    )

    Get-ChildItem -Path $CurrentPath |
        Where-Object {
            $_.Name -notlike "*.meta" -and
            $_.Name -ne "Library" -and
            $_.Name -ne "Temp" -and
            $_.Name -ne "Obj" -and
            $_.Name -ne "Build" -and
            $_.Name -ne "Logs"
        }
}

function Show-Tree {
    param(
        [string]$CurrentPath,
        [string]$Prefix = "",
        [int]$Depth = 0
    )

    $items = Get-VisibleItems -CurrentPath $CurrentPath

    $files = $items |
        Where-Object { -not $_.PSIsContainer } |
        Sort-Object Name

    $folders = $items |
        Where-Object { $_.PSIsContainer } |
        Sort-Object Name

    # 1. Print files first.
    for ($index = 0; $index -lt $files.Count; $index++) {
        $file = $files[$index]

        $isLastFile = $index -eq ($files.Count - 1)
        $hasFoldersAfter = $folders.Count -gt 0

        if ($isLastFile -and -not $hasFoldersAfter) {
            $connector = "\--- "
        }
        else {
            $connector = "+--- "
        }

        Write-Output "$Prefix$connector$($file.Name)"
    }

    # 2. Add one separator between files and subfolders.
    if ($files.Count -gt 0 -and $folders.Count -gt 0) {
        Write-Output "$Prefix|"
    }

    # 3. Print folders after files.
    for ($index = 0; $index -lt $folders.Count; $index++) {
        $folder = $folders[$index]

        $isLastFolder = $index -eq ($folders.Count - 1)

        if ($isLastFolder) {
            $connector = "\--- "
            $childPrefix = "$Prefix     "
        }
        else {
            $connector = "+--- "
            $childPrefix = "$Prefix|    "
        }

        Write-Output "$Prefix$connector$($folder.Name)"

        Show-Tree -CurrentPath $folder.FullName -Prefix $childPrefix -Depth ($Depth + 1)

        # Add a separator between sibling folders.
        if (-not $isLastFolder) {
            Write-Output "$Prefix|"
        }
    }
}

$resolvedPath = Resolve-Path $Path

Write-Output "# Folder Architecture"
Write-Output ""
Write-Output '```text'
Write-Output $resolvedPath.Path
Show-Tree -CurrentPath $resolvedPath.Path
Write-Output '```'