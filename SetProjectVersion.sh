#!/bin/bash

# Usage message
USAGE="[i] Usage: ./SetProjectVersion.sh <version number>"
VERSION_FORMAT="[i] Acceptable format: ^[0-9]+\.[0-9]+\.[0-9]+$"

# Check if version number is provided
if [ -z "$1" ]; then
    echo "[!] A version number is needed for this script to work."
    echo "$USAGE"
    exit 1
fi

# Assign the provided version number to a variable
newVersion="$1"

# Verify version number format
if ! [[ $newVersion =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
    echo "[!] Invalid version number format."
    echo "$VERSION_FORMAT"
    exit 1
fi

# Define the version pattern and new pattern
versionPattern="<Version>0\.0\.0</Version>"
newPattern="<Version>$newVersion</Version>"

# Find matching project configuration files
projectFiles=$(find . -name "osu.*.csproj")

updatedCount=0

# Loop through each project file
for file in $projectFiles; do
    # Check if the file contains the version pattern
    if grep -q "$versionPattern" "$file"; then
        echo "[i] Updating file: $file"
        # Use sed to replace the version pattern with the new pattern
        sed -i "s/$versionPattern/$newPattern/g" "$file"
        updatedCount=$((updatedCount+1))
    fi
done

# Output the result
if [ $updatedCount -eq 0 ]; then
    echo "[i] No project file needs updating."
else
    echo "[i] Updated $updatedCount file(s)."
fi
