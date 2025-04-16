#!/bin/bash

self_contained=false
configurations=()
platforms=()

project_name="HackArenaGame"

declare -A solution_files
solution_files["win-x64"]="${project_name}_Windows.sln"
solution_files["win-arm64"]="${project_name}_Windows.sln"
solution_files["linux-x64"]="${project_name}_Linux.sln"
solution_files["linux-arm64"]="${project_name}_Linux.sln" 
solution_files["osx-x64"]="${project_name}_macOS.sln"
solution_files["osx-arm64"]="${project_name}_macOS.sln"

while [[ $# -gt 0 ]]; do
    case "$1" in
        self-contained)
            self_contained=true
            shift
            ;;
        Debug|Release|HackathonDebug|HackathonRelease|StereoDebug|StereoRelease)
            configurations+=("$1")
            shift
            ;;
        win-x64|win-arm64|linux-x64|linux-arm64|osx-x64|osx-arm64)
            platforms+=("$1")
            shift
            ;;
        *)
            echo "Unknown argument: $1"
            exit 1
            ;;
    esac
done

if [ ${#configurations[@]} -eq 0 ]; then
    configurations=("Debug" "Release" "HackathonDebug" "HackathonRelease", "StereoDebug", "StereoRelease")
fi

if [ ${#platforms[@]} -eq 0 ]; then
    platforms=("win-x64" "win-arm64" "linux-x64" "linux-arm64" "osx-x64" "osx-arm64")
fi

outputDirBase="../publish"

echo "Removing existing base output directory: $outputDirBase"
rm -rf "$outputDirBase"

for config in "${configurations[@]}"; do
    for platform in "${platforms[@]}"; do
        solution_file=${solution_files[$platform]}
        solution_file_path="../$solution_file"

        outputDir="$outputDirBase/$config/$platform"
        if [ ! -d "$outputDir" ]; then
            echo "Creating output directory: $outputDir"
            mkdir -p "$outputDir"
        fi

        platform_target="${platform##*-}"
        
        echo "Publishing for configuration '$config' and platform '$platform' using '$solution_file'"
        
        if $self_contained; then
            dotnet publish "$solution_file_path" -c "$config" -r "$platform" /p:PlatformTarget="$platform_target" /p:RestorePackages=false --output "$outputDir" --self-contained
        else
            dotnet publish "$solution_file_path" -c "$config" -r "$platform" /p:PlatformTarget="$platform_target" /p:RestorePackages=false --output "$outputDir"
        fi

        echo "Publishing done for configuration '$config' and platform '$platform'"
    done
done
