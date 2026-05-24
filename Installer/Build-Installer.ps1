param(
    [string]$Configuration = "Release",
    [switch]$InstallInnoSetup
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repoRoot "School-Management-System.sln"
$projectOutDir = Join-Path $repoRoot "School-Management-System.Wpf\bin\$Configuration\net472"
$exePath = Join-Path $projectOutDir "School-Management-System.Wpf.exe"
$issPath = Join-Path $repoRoot "Installer\SchoolManagementSystem.iss"
$distDir = Join-Path $repoRoot "dist\installer"
$installReadmePath = Join-Path $distDir "README_INSTALL.txt"
$bundlePath = Join-Path $distDir "SchoolManagementSystemInstallerBundle.zip"

function Get-AppSettingMap {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ConfigPath
    )

    [xml]$xml = Get-Content -LiteralPath $ConfigPath
    $settings = @{}
    foreach ($node in $xml.configuration.appSettings.add) {
        $settings[[string]$node.key] = [string]$node.value
    }

    return $settings
}

function Test-OnlineProfileComplete {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Settings
    )

    return -not [string]::IsNullOrWhiteSpace($Settings["DbHostOnline"]) -and
           -not [string]::IsNullOrWhiteSpace($Settings["DbNameOnline"]) -and
           -not [string]::IsNullOrWhiteSpace($Settings["DbUserOnline"]) -and
           -not [string]::IsNullOrWhiteSpace($Settings["DbPasswordOnline"])
}

function Find-InitializedConfig {
    $candidates = @()

    if (-not [string]::IsNullOrWhiteSpace($env:SMS_INSTALLER_PROFILE_CONFIG)) {
        $candidates += $env:SMS_INSTALLER_PROFILE_CONFIG
    }

    $candidates += @(
        (Join-Path $repoRoot "School-Management-System.Wpf\bin\Debug\net472\SMSApp.exe.config"),
        (Join-Path $repoRoot "School-Management-System.Wpf\bin\Release\net472\SMSApp.exe.config")
    )

    foreach ($candidate in $candidates) {
        if (-not (Test-Path $candidate)) {
            continue
        }

        try {
            $settings = Get-AppSettingMap -ConfigPath $candidate
            if (Test-OnlineProfileComplete -Settings $settings) {
                return (Resolve-Path -LiteralPath $candidate).Path
            }
        }
        catch {
            Write-Warning "Skipping unreadable config profile: $candidate"
        }
    }

    return $null
}

function Set-AppSetting {
    param(
        [Parameter(Mandatory = $true)]
        [xml]$Xml,
        [Parameter(Mandatory = $true)]
        [string]$Key,
        [AllowNull()]
        [string]$Value
    )

    $appSettings = $Xml.SelectSingleNode("/configuration/appSettings")
    if ($null -eq $appSettings) {
        $appSettings = $Xml.CreateElement("appSettings")
        $configuration = $Xml.SelectSingleNode("/configuration")
        [void]$configuration.AppendChild($appSettings)
    }

    $node = $Xml.SelectSingleNode("/configuration/appSettings/add[@key='$Key']")
    if ($null -eq $node) {
        $node = $Xml.CreateElement("add")
        [void]$node.SetAttribute("key", $Key)
        [void]$appSettings.AppendChild($node)
    }

    $settingValue = ""
    if ($null -ne $Value) {
        $settingValue = $Value
    }

    [void]$node.SetAttribute("value", $settingValue)
}

function Copy-InstallerProfileSettings {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceConfig,
        [Parameter(Mandatory = $true)]
        [string]$OutputDirectory
    )

    $sourceSettings = Get-AppSettingMap -ConfigPath $SourceConfig
    $keysToCopy = @(
        "DbHost",
        "DbPort",
        "DbName",
        "DbUser",
        "DbPassword",
        "DbMode",
        "DbHostOnline",
        "DbPortOnline",
        "DbNameOnline",
        "DbUserOnline",
        "DbPasswordOnline",
        "DbSslModeOnline",
        "DbSslCaPathOnline",
        "BackupEncryptionKey"
    )

    $targetConfigs = @(
        (Join-Path $OutputDirectory "School-Management-System.Wpf.exe.config")
    )

    foreach ($targetConfig in $targetConfigs) {
        if (-not (Test-Path $targetConfig)) {
            Copy-Item -LiteralPath (Join-Path $OutputDirectory "School-Management-System.Wpf.exe.config") -Destination $targetConfig -Force
        }

        [xml]$targetXml = Get-Content -LiteralPath $targetConfig
        foreach ($key in $keysToCopy) {
            if ($sourceSettings.ContainsKey($key)) {
                Set-AppSetting -Xml $targetXml -Key $key -Value $sourceSettings[$key]
            }
        }

        $targetXml.Save($targetConfig)
    }

    Write-Host "Installer database profile copied from initialized local config."
}

function Find-MsBuild {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $found = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
        if ($found -and (Test-Path $found)) {
            return $found
        }
    }

    $candidates = @(
        "C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) { return $candidate }
    }

    throw "MSBuild.exe not found. Install Visual Studio Build Tools with .NET Framework support."
}

function Find-Iscc {
    $candidates = @(
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe",
        (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe")
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) { return $candidate }
    }

    $fromPath = Get-Command iscc -ErrorAction SilentlyContinue
    if ($fromPath) { return $fromPath.Source }

    return $null
}

$msbuild = Find-MsBuild
Write-Host "Building solution using: $msbuild"
& $msbuild $solutionPath /t:Build /p:Configuration=$Configuration /p:Platform="Any CPU" /v:m

if (-not (Test-Path $exePath)) {
    throw "Build completed but executable not found at: $exePath"
}

$initializedConfig = Find-InitializedConfig
if ($initializedConfig) {
    Copy-InstallerProfileSettings -SourceConfig $initializedConfig -OutputDirectory $projectOutDir
}
else {
    Write-Warning "No initialized online database config was found. The installer will keep the built app config values."
}

$iscc = Find-Iscc
if (-not $iscc -and $InstallInnoSetup) {
    Write-Host "Inno Setup not found. Installing via winget..."
    winget install --id JRSoftware.InnoSetup -e --silent --accept-package-agreements --accept-source-agreements
    $iscc = Find-Iscc
}

if (-not $iscc) {
    throw "Inno Setup compiler (ISCC.exe) not found. Install Inno Setup 6, then rerun this script."
}

if (-not (Test-Path $distDir)) {
    New-Item -ItemType Directory -Path $distDir -Force | Out-Null
}

Write-Host "Building installer using: $iscc"
& $iscc $issPath

$setup = Get-ChildItem -Path $distDir -Filter "*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $setup) {
    throw "Installer build did not produce an .exe in $distDir"
}

Write-Host "Installer created: $($setup.FullName)"

if (Test-Path $installReadmePath) {
    if (Test-Path $bundlePath) {
        Remove-Item -LiteralPath $bundlePath -Force
    }

    Compress-Archive -LiteralPath @($setup.FullName, $installReadmePath) -DestinationPath $bundlePath -Force
    Write-Host "Installer bundle created: $bundlePath"
}
