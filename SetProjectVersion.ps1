param (
    [string] $newVersion
)

$USAGE = "[i] Usage: ./SetProjectVersion.ps1 <version number>"
$VERSION_FORMAT = "[i] Acceptable format: \d+\.\d+\.\d+"

if ([string]::IsNullOrEmpty($newVersion)) {
    Write-Host "[!] A version number is needed for this script to work."
    Write-Host $USAGE

    exit 1
}

# Verify version number format
if ($newVersion -notmatch '^\d+\.\d+\.\d+$') {
    Write-Host "[!] Invalid version number format."
    Write-Host $VERSION_FORMAT

    exit 1
}

# NuGet only supports digits
$versionPattern = "<Version>0\.0\.0</Version>"
$newPattern = "<Version>$newVersion</Version>"

# Get matching project configuration files
$projectFiles = Get-ChildItem -Recurse -Filter "osu.*.csproj"

$updatedCount = 0

foreach ($file in $projectFiles) {
    $content = Get-Content $file.FullName -Raw
    
    if ($content -match $versionPattern) {
        Write-Host "[i] Updating file: $($file.FullName)"
        $newContent = $content -replace $versionPattern, $newPattern

        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        
        $updatedCount++
    }
}

if ($updatedCount -eq 0) {
    Write-Host "[i] No project file needs updating."
} else {
    Write-Host "[i] Updated $updatedCount file(s)."
}
