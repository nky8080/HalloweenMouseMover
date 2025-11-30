# Halloween Mouse Mover - Release Package Creator
# This script creates a distribution ZIP file for GitHub Releases

$ErrorActionPreference = "Stop"

# Configuration
$releaseDir = "HalloweenMouseMover\bin\Release\net6.0-windows"
$outputZip = "HalloweenMouseMover-Release.zip"
$tempDir = "ReleasePackage"

Write-Host "Creating Halloween Mouse Mover release package..." -ForegroundColor Cyan

# Clean up previous temp directory if exists
if (Test-Path $tempDir) {
    Remove-Item -Recurse -Force $tempDir
}

# Create temp directory
New-Item -ItemType Directory -Path $tempDir | Out-Null

# Files to include in the release
$filesToInclude = @(
    # Your application
    "HalloweenMouseMover.exe",
    "HalloweenMouseMover.dll",
    "HalloweenMouseMover.deps.json",
    "HalloweenMouseMover.runtimeconfig.json",
    
    # Your resources
    "horror_mouse.cur",
    "horror_mouse.ico",
    
    # Third-party libraries (allowed by their licenses)
    "FlaUI.Core.dll",
    "FlaUI.UIA3.dll",
    "Interop.UIAutomationClient.dll",
    "NAudio.dll",
    "NAudio.Asio.dll",
    "NAudio.Core.dll",
    "NAudio.Midi.dll",
    "NAudio.Wasapi.dll",
    "NAudio.WinForms.dll",
    "NAudio.WinMM.dll",
    "System.Management.dll",
    "System.Windows.Extensions.dll"
)

# Copy files from release directory
Write-Host "Copying application files..." -ForegroundColor Yellow
foreach ($file in $filesToInclude) {
    $sourcePath = Join-Path $releaseDir $file
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath -Destination $tempDir
        Write-Host "  ✓ $file" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ Warning: $file not found" -ForegroundColor Yellow
    }
}

# Copy runtimes directory if exists
$runtimesPath = Join-Path $releaseDir "runtimes"
if (Test-Path $runtimesPath) {
    Write-Host "Copying runtime dependencies..." -ForegroundColor Yellow
    Copy-Item -Recurse $runtimesPath -Destination $tempDir
    Write-Host "  ✓ runtimes directory" -ForegroundColor Green
}

# Copy documentation files
Write-Host "Copying documentation..." -ForegroundColor Yellow
Copy-Item "THIRD-PARTY-NOTICES.txt" -Destination $tempDir
Write-Host "  ✓ THIRD-PARTY-NOTICES.txt" -ForegroundColor Green

Copy-Item "RELEASE-README.txt" -Destination (Join-Path $tempDir "README.txt")
Write-Host "  ✓ README.txt" -ForegroundColor Green

Copy-Item "LICENSE" -Destination $tempDir
Write-Host "  ✓ LICENSE" -ForegroundColor Green

# Verify Scream03.mp3 is NOT included
$audioFile = Join-Path $tempDir "Scream03.mp3"
if (Test-Path $audioFile) {
    Write-Host "  ⚠ Removing Scream03.mp3 (copyright protected)" -ForegroundColor Yellow
    Remove-Item $audioFile
}

# Create ZIP file
Write-Host "`nCreating ZIP archive..." -ForegroundColor Cyan
if (Test-Path $outputZip) {
    Remove-Item $outputZip
}

Compress-Archive -Path "$tempDir\*" -DestinationPath $outputZip -CompressionLevel Optimal

# Clean up temp directory
Remove-Item -Recurse -Force $tempDir

# Display results
Write-Host "`n✓ Release package created successfully!" -ForegroundColor Green
Write-Host "  Output: $outputZip" -ForegroundColor Cyan
Write-Host "  Size: $([math]::Round((Get-Item $outputZip).Length / 1MB, 2)) MB" -ForegroundColor Cyan

Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "  1. Go to https://github.com/nky8080/HalloweenMouseMover/releases" -ForegroundColor White
Write-Host "  2. Click 'Create a new release'" -ForegroundColor White
Write-Host "  3. Upload $outputZip" -ForegroundColor White
Write-Host "  4. Add release notes mentioning the audio file requirement" -ForegroundColor White

Write-Host "`nIMPORTANT NOTES:" -ForegroundColor Red
Write-Host "  - Audio file (Scream03.mp3) is NOT included due to copyright" -ForegroundColor Yellow
Write-Host "  - Users must provide their own audio file" -ForegroundColor Yellow
Write-Host "  - All third-party libraries are included with proper licenses" -ForegroundColor Yellow
