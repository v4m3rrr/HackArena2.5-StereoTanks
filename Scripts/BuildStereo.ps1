$platforms = @("x64", "arm64")
$configurations = @("StereoDebug", "StereoRelease")
$systems = @("Windows", "Linux", "macOS")

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$env:Path += ";C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\"

$solutionDir = "../"
$solutionPrefix = "HackArenaGame_"

$failedBuilds = @()
$buildResults = @()
$totalDuration = [TimeSpan]::Zero

foreach ($system in $systems) {
    $solutionPath = Join-Path $solutionDir "$solutionPrefix$system.sln"
    
    Write-Host "Restoring $solutionPath..."
    $restoreOutput = dotnet restore $solutionPath

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to restore packages for system: $system" -ForegroundColor Red
        Write-Host "Error: $restoreOutput" -ForegroundColor Red
	Write-Host $restoreOutput -ForegroundColor Red
    } else {
        Write-Host "Success: restore $solutionPath" -ForegroundColor Green
        
        foreach ($platform in $platforms) {
            foreach ($config in $configurations) {
                Write-Host "Building $system for $platform in $config..."

                $startTime = Get-Date
                $buildResult = msbuild $solutionPath /p:RestorePackages=false /p:Configuration=$config /p:Platform=$platform /m
                $endTime = Get-Date

                $buildResult = $buildResult -join "`n"

                $duration = $endTime - $startTime
                $totalDuration += $duration

                if ($LASTEXITCODE -ne 0) {
                    $failedBuilds += "Error building $system for $platform in ${config}"
                    Write-Host "Error: $($failedBuilds[-1])`n$buildResult" -ForegroundColor Red
                    $buildResults += [PSCustomObject]@{ System = $system; Platform = $platform; Configuration = $config; Status = "Failed"; Duration = $duration }
                } else {
                    Write-Host "Success: build $system for $platform in $config" -ForegroundColor Green
                    $buildResults += [PSCustomObject]@{ System = $system; Platform = $platform; Configuration = $config; Status = "Success"; Duration = $duration }
                }
            }
        }
    }
}

Write-Host "Build Summary:" -ForegroundColor Yellow

foreach ($result in $buildResults) {
    $formattedDuration = "{0:00}:{1:00}:{2:00}.{3:000}" -f $result.Duration.Hours, $result.Duration.Minutes, $result.Duration.Seconds, $result.Duration.Milliseconds

    if ($result.Status -eq "Failed") {
        Write-Host "$($result.System) - $($result.Configuration) / $($result.Platform): $($result.Status) (Duration: $formattedDuration)" -ForegroundColor Red
    } else {
        Write-Host "$($result.System) - $($result.Configuration) / $($result.Platform): $($result.Status) (Duration: $formattedDuration)" -ForegroundColor Green
    }
}

$formattedTotalDuration = "{0:00}:{1:00}:{2:00}.{3:000}" -f $totalDuration.Hours, $totalDuration.Minutes, $totalDuration.Seconds, $totalDuration.Milliseconds
Write-Host "Total time: $formattedTotalDuration" -ForegroundColor Cyan

if ($failedBuilds.Count -eq 0) {
    Write-Host "All builds completed successfully!" -ForegroundColor Green
}
