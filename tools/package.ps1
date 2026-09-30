# KeeperSpawner yayın paketi
#
#   powershell -ExecutionPolicy Bypass -File tools\package.ps1 [-PublishedFileId 123] [-ChangeNote "..."] [-SkipBuild]
#
# Çıktı: artifacts\<sürüm>\
#   content\                         Atölye içerik klasörü (oyunun yükleyicisi ya da SteamCMD için)
#     BepInEx\plugins\KeeperSpawner\KeeperSpawner.dll
#     INSTALL.txt
#     Thumbnail.png                  (workshop\preview.png varsa; oyunun yükleyicisi önizleme olarak kullanır)
#   KeeperSpawner-<sürüm>.zip        elle kurulum / Nexus (Thumbnail olmadan)
#   workshop_item.vdf                SteamCMD workshop_build_item için
param(
    [string]$PublishedFileId = "0",
    [string]$ChangeNote = "",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src\KeeperSpawner\KeeperSpawner.csproj"

# Sürüm csproj'dan
[xml]$csproj = Get-Content -Raw -Encoding UTF8 $project
$version = ($csproj.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1).Version
if (-not $version) { throw "csproj içinde <Version> bulunamadı." }
if (-not $ChangeNote) { $ChangeNote = "v$version" }
Write-Host "KeeperSpawner $version paketleniyor"

if (-not $SkipBuild) {
    & dotnet build $project -c Release -nologo -v quiet
    if ($LASTEXITCODE -ne 0) { throw "Derleme başarısız." }
}
$dll = Join-Path $root "src\KeeperSpawner\bin\Release\KeeperSpawner.dll"
if (-not (Test-Path $dll)) { throw "DLL bulunamadı: $dll" }

# Workshop Loader'ın oyuncuya uyarı gösterdiği API'ler (docs: FOR_MODDERS.txt, madde 5)
$flagged = "System.Net", "UnityWebRequest", "WebClient", "HttpClient", "Socket", "System.Diagnostics.Process",
    "Assembly.Load", "LoadFrom", "LoadFile", "System.Reflection.Emit", "ILGenerator", "TypeBuilder", "DynamicMethod",
    "Microsoft.Win32", "Registry", "File.Delete", "Directory.Delete", "GetFolderPath", "DllImport"
$text = [Text.Encoding]::ASCII.GetString([IO.File]::ReadAllBytes($dll))
$hits = @($flagged | Where-Object { $text.Contains($_) })
if ($hits.Count -gt 0) { throw "DLL güvenlik uyarısı verecek API içeriyor: $($hits -join ', ')" }
Write-Host "Güvenlik taraması temiz"

$out = Join-Path $root "artifacts\$version"
$content = Join-Path $out "content"
$pluginDir = Join-Path $content "BepInEx\plugins\KeeperSpawner"
New-Item -ItemType Directory -Force $pluginDir | Out-Null
Copy-Item $dll $pluginDir -Force
Copy-Item (Join-Path $root "workshop\INSTALL.txt") $content -Force

# Önizleme: Atölye 1 MB üstünü kabul etmiyor
$preview = Join-Path $root "workshop\preview.png"
$previewForVdf = ""
if (Test-Path $preview) {
    $size = (Get-Item $preview).Length
    if ($size -ge 1MB) { throw "workshop\preview.png 1 MB'dan büyük ($size bayt)." }
    Copy-Item $preview (Join-Path $content "Thumbnail.png") -Force
    $previewForVdf = $preview
} else {
    Write-Warning "workshop\preview.png yok; Atölye itemı önizlemesiz kalır."
}

# Elle kurulum / Nexus zip'i (önizleme olmadan). PowerShell 5.1'in Compress-Archive'ı ayırıcı olarak "\"
# yazıyor; zip standardı "/" istiyor, bazı açıcılar ve sunucular aksi halde klasörleri tanımıyor.
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$zip = Join-Path $out "KeeperSpawner-$version.zip"
$stream = [IO.File]::Open($zip, [IO.FileMode]::Create)
try {
    $archive = New-Object IO.Compression.ZipArchive($stream, [IO.Compression.ZipArchiveMode]::Create)
    try {
        Get-ChildItem $content -Recurse -File | Where-Object { $_.Name -ne "Thumbnail.png" } | ForEach-Object {
            $entryName = $_.FullName.Substring($content.Length + 1).Replace('\', '/')
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $_.FullName, $entryName, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
        }
    } finally {
        $archive.Dispose()
    }
} finally {
    $stream.Dispose()
}

# SteamCMD vdf
$vdf = Get-Content -Raw -Encoding UTF8 (Join-Path $root "workshop\keeperspawner.vdf.template")
$vdf = $vdf.Replace("{PUBLISHED_FILE_ID}", $PublishedFileId).Replace("{CONTENT_FOLDER}", $content).Replace("{PREVIEW_FILE}", $previewForVdf).Replace("{CHANGE_NOTE}", $ChangeNote.Replace('"', "'"))
[IO.File]::WriteAllText((Join-Path $out "workshop_item.vdf"), $vdf, (New-Object Text.UTF8Encoding($false)))

Write-Host ""
Write-Host "Hazır: $out"
Get-ChildItem $out -Recurse -File | ForEach-Object { "  {0,9:N0} B  {1}" -f $_.Length, $_.FullName.Substring($out.Length + 1) }
