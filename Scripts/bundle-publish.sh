#!/bin/bash

prefix="$1"
output_dir="../publish"
artifact_dir="../artifacts"

echo "Removing and recreating artifact directory: $artifact_dir"
rm -rf "$artifact_dir"
mkdir -p "$artifact_dir"

touch "$artifact_dir/SHA256SUMS.txt"

for config_dir in "$output_dir"/*; do
    config=$(basename "$config_dir")
    for platform_dir in "$config_dir"/*; do
        platform=$(basename "$platform_dir")
        target_dir="${platform}"

        if [ -n "$prefix" ]; then
            archive_base="${prefix}${target_dir}"
        else
            archive_base="${target_dir}"
        fi

        echo "Packing $target_dir..."

        if [[ "$platform" == win-* ]]; then
			archive_file="$artifact_dir/$archive_base.zip"
			tmp_zip="$archive_base.zip"
			(cd "$platform_dir" && zip -r "$tmp_zip" .)
			mv "$platform_dir/$tmp_zip" "$archive_file"
        else
            archive_file="$artifact_dir/$archive_base.tar.gz"
            tar -czf "$archive_file" -C "$platform_dir" .
        fi

        (cd "$artifact_dir" && sha256sum "$(basename "$archive_file")" >> SHA256SUMS.txt)

        echo "Packed $archive_file and updated SHA256SUMS.txt"
    done
done
