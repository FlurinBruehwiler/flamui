$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$svgDir = Join-Path $scriptDir "Icons/SVG"
$tvgDir = Join-Path $scriptDir "Icons/TVG"

# Ensure output directory exists
if (!(Test-Path $tvgDir)) {
    New-Item -ItemType Directory -Path $tvgDir | Out-Null
}

# Process each SVG file
Get-ChildItem -Path $svgDir -Filter "*.svg" | ForEach-Object {
    $svgFile = $_.FullName
    $baseName = $_.BaseName
    $tvgtFile = Join-Path $tvgDir "$baseName.tvgt"
    $tvgFile = Join-Path $tvgDir "$baseName.tvg"

    # Convert SVG to TVGT
    Write-Output "Converting $baseName"
    svg2tvgt $svgFile -o $tvgtFile

    # Convert TVGT to TVG
    tvg-text -O tvg $tvgtFile -o $tvgFile

    # Remove intermediate TVGT file
    Remove-Item $tvgtFile -Force
}