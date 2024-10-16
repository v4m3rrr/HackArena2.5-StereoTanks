$baseDirs = @(
    "..\GameClient\bin", 
    "..\GameServer\bin"
)

$zipDestinationBase = "..\ZippedBuilds"
$zipDestinationBase = Resolve-Path -Path $zipDestinationBase

if (Test-Path $zipDestinationBase) {
    Write-Host "Clearing destination folder: $zipDestinationBase"
    Remove-Item -Path "$zipDestinationBase\*" -Recurse -Force
} else {
    Write-Host "Creating destination folder: $zipDestinationBase"
    New-Item -Path $zipDestinationBase -ItemType Directory
}

foreach ($baseDir in $baseDirs) {
    $baseFullDir = $baseDir | Resolve-Path 
    $netDirs = Get-ChildItem -Path $baseFullDir -Recurse -Directory | Where-Object { $_.Name -like "net*" }

    foreach ($dir in $netDirs) {

        if ($dir.FullName -like "*\runtimes\*") {
            Write-Host "Skipping folder: $($dir.FullName)"
            continue
        }

        $relativePath = $dir.FullName.Substring($baseFullDir.ToString().Length).TrimStart("\")
        $zipName = "$baseDir\$relativePath" -replace "\\", "-" -replace "\.\.-", "" -replace "bin-", "" -replace "Game", ""
        $zipFile = "$zipDestinationBase\$zipName.zip"

        Compress-Archive -Path "$($dir.FullName)\*" -DestinationPath $zipFile
    }
}

Write-Host "All zips created in: $zipDestinationBase"
