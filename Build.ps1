<#
.SYNOPSIS
    Build PowerMenu and produce a user-space MSIX installer.

.DESCRIPTION
    1. Builds a self-contained x64 publish of the WPF app.
    2. Generates placeholder MSIX asset images via System.Drawing.
    3. Copies the appxmanifest + assets into the layout folder.
    4. Creates a self-signed code-signing certificate (once).
    5. Packs the MSIX with MakeAppx.exe.
    6. Signs it with SignTool.exe.
    7. Installs the signing cert into CurrentUser\TrustedPeople so
       the package can be side-loaded without Developer Mode.

.NOTES
    Requires the Windows 10/11 SDK (for MakeAppx.exe / SignTool.exe).
    Run once with -InstallCert to trust the self-signed cert (needs
    elevation only for that step; the rest runs in user space).

.EXAMPLE
    .\Build.ps1
    .\Build.ps1 -InstallCert   # also installs the cert to trusted store
#>
param(
    [switch]$InstallCert,
    [string]$SdkVersion = ""          # auto-detected when empty
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── Paths ────────────────────────────────────────────────────────────────────
$root      = $PSScriptRoot
$src       = Join-Path $root "PowerMenu.csproj"
$outDir    = Join-Path $root "dist"
$publishDir= Join-Path $outDir "publish"
$layoutDir = Join-Path $outDir "msix-layout"
$assetsDir = Join-Path $layoutDir "Assets"
$pkgAssets = Join-Path $root "Package\Assets"
$manifest  = Join-Path $root "Package\Package.appxmanifest"
$msixPath  = Join-Path $outDir "PowerMenu.msix"
$pfxPath   = Join-Path $outDir "PowerMenu.pfx"
$certPass  = "PowerMenu-dev"

# ── Find Windows SDK ─────────────────────────────────────────────────────────
function Find-SdkTool([string]$name) {
    $sdkRoot = "C:\Program Files (x86)\Windows Kits\10\bin"
    if (!(Test-Path $sdkRoot)) {
        throw "Windows SDK not found at $sdkRoot. Install it from https://developer.microsoft.com/windows/downloads/windows-sdk/"
    }
    $versions = Get-ChildItem $sdkRoot -Directory |
        Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
        Sort-Object { [version]$_.Name } -Descending

    if ($SdkVersion) {
        $versions = $versions | Where-Object { $_.Name -eq $SdkVersion }
    }

    foreach ($ver in $versions) {
        $path = Join-Path $ver.FullName "x64\$name"
        if (Test-Path $path) { return $path }
    }
    throw "$name not found in any Windows SDK version under $sdkRoot"
}

$makeAppx  = Find-SdkTool "MakeAppx.exe"
$signTool  = Find-SdkTool "SignTool.exe"
Write-Host "SDK tools:`n  MakeAppx : $makeAppx`n  SignTool  : $signTool`n" -ForegroundColor Cyan

# ── 1. Build ──────────────────────────────────────────────────────────────────
Write-Host "Building PowerMenu..." -ForegroundColor Cyan
& dotnet publish $src `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $publishDir `
    /p:PublishSingleFile=true `
    /p:EnableCompressionInSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

# ── 2. Generate placeholder MSIX asset images ─────────────────────────────────
Add-Type -AssemblyName System.Drawing

function New-PlaceholderImage([string]$path, [int]$w, [int]$h) {
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

    # Catppuccin Mocha Base background
    $bgColor = [System.Drawing.Color]::FromArgb(255,30,30,46)
    $g.Clear($bgColor)

    # Draw a simple power symbol scaled to image size
    $margin  = [int]($w * 0.15)
    $cx      = $w / 2
    $cy      = $h / 2
    $radius  = ($w - 2*$margin) / 2

    $arcPen = New-Object System.Drawing.Pen(
        [System.Drawing.Color]::FromArgb(255,203,166,247),
        [float]($w * 0.08))
    $arcPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $arcPen.EndCap   = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawArc($arcPen, [float]($cx - $radius), [float]($cy - $radius),
               [float]($radius*2), [float]($radius*2), -60, 300)
    $g.DrawLine($arcPen, [float]$cx, [float]($cy-$radius), [float]$cx, [float]$cy)

    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $arcPen.Dispose(); $bmp.Dispose()
    Write-Host "  Generated: $path" -ForegroundColor DarkGray
}

# Create package assets directory
New-Item -ItemType Directory -Path $pkgAssets -Force | Out-Null

$assetSizes = @{
    "Square44x44Logo.png"    = @(44,44)
    "Square150x150Logo.png"  = @(150,150)
    "Wide310x150Logo.png"    = @(310,150)
    "SmallTile.png"          = @(71,71)
    "LargeTile.png"          = @(310,310)
    "StoreLogo.png"          = @(50,50)
    "SplashScreen.png"       = @(620,300)
}

Write-Host "Generating MSIX asset images..." -ForegroundColor Cyan
foreach ($entry in $assetSizes.GetEnumerator()) {
    $imgPath = Join-Path $pkgAssets $entry.Key
    if (!(Test-Path $imgPath)) {
        New-PlaceholderImage $imgPath $entry.Value[0] $entry.Value[1]
    }
}

# ── 3. Assemble MSIX layout ───────────────────────────────────────────────────
Write-Host "Assembling MSIX layout..." -ForegroundColor Cyan
Remove-Item -Path $layoutDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null

Copy-Item -Path "$publishDir\*" -Destination $layoutDir -Recurse
Copy-Item -Path $manifest       -Destination (Join-Path $layoutDir "AppxManifest.xml")
Copy-Item -Path "$pkgAssets\*"  -Destination $assetsDir -Recurse

# ── 4. Create / reuse self-signed cert ───────────────────────────────────────
$secPwd = ConvertTo-SecureString -String $certPass -Force -AsPlainText
if (!(Test-Path $pfxPath)) {
    Write-Host "Creating self-signed code-signing certificate..." -ForegroundColor Cyan
    $cert = New-SelfSignedCertificate `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -Subject "CN=PowerMenu" `
        -KeyUsage DigitalSignature `
        -Type CodeSigningCert `
        -NotAfter (Get-Date).AddYears(5)

    Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $secPwd | Out-Null
    Write-Host "  Certificate saved to: $pfxPath" -ForegroundColor DarkGray
} else {
    Write-Host "Reusing existing certificate: $pfxPath" -ForegroundColor DarkGray
}

if ($InstallCert) {
    Write-Host "Installing cert to Root + TrustedPeople stores (requires elevation)..." -ForegroundColor Yellow
    Import-PfxCertificate -FilePath $pfxPath -Password $secPwd `
        -CertStoreLocation "Cert:\LocalMachine\Root" | Out-Null
    Import-PfxCertificate -FilePath $pfxPath -Password $secPwd `
        -CertStoreLocation "Cert:\LocalMachine\TrustedPeople" | Out-Null
    Write-Host "  Certificate installed." -ForegroundColor DarkGray
}

# ── 5. Pack ───────────────────────────────────────────────────────────────────
Write-Host "Packing MSIX..." -ForegroundColor Cyan
Remove-Item -Path $msixPath -Force -ErrorAction SilentlyContinue
& $makeAppx pack /d $layoutDir /p $msixPath /overwrite /nv
if ($LASTEXITCODE -ne 0) { throw "MakeAppx failed" }

# ── 6. Sign ───────────────────────────────────────────────────────────────────
Write-Host "Signing MSIX..." -ForegroundColor Cyan
& $signTool sign /fd SHA256 /a /f $pfxPath /p $certPass $msixPath
if ($LASTEXITCODE -ne 0) { throw "SignTool failed" }

# ── Done ──────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "Build complete!" -ForegroundColor Green
Write-Host "  MSIX : $msixPath"
Write-Host ""
Write-Host "To install:" -ForegroundColor Yellow
Write-Host "  1. First run (once):  .\Build.ps1 -InstallCert"
Write-Host "     (or enable Developer Mode in Windows Settings)"
Write-Host "  2. Double-click  $msixPath  and click Install."
Write-Host ""
Write-Host "The app starts in the system tray. Press Ctrl+Alt+P to open the menu." -ForegroundColor Cyan
