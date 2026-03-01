param(
    [string]$MySqlHostName = 'localhost',
    [int]$MySqlPort = 3306,
    [string]$AdminUser = 'root',
    [string]$AdminPassword = '',
    [string]$ClientHost = '%',
    [string]$RootPasswordForClient = '',
    [string]$Database = 'schoolmanagementsystem',
    [string]$MySqlServiceName = 'MySQL80',
    [string]$BindAddress = '0.0.0.0'
)

$ErrorActionPreference = 'Stop'

function Require-Admin {
    $id = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($id)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Run PowerShell as Administrator."
    }
}

function Get-MyIniPath([string]$serviceName) {
    $service = Get-CimInstance Win32_Service -Filter "Name='$serviceName'" -ErrorAction SilentlyContinue
    if ($service -and $service.PathName) {
        $pathName = $service.PathName
        if ($pathName -match '--defaults-file=\"([^\"]+)\"') {
            return $matches[1]
        }
        if ($pathName -match '--defaults-file=([^ ]+)') {
            return $matches[1].Trim('"')
        }
    }

    $candidates = @(
        'C:\ProgramData\MySQL\MySQL Server 8.4\my.ini',
        'C:\ProgramData\MySQL\MySQL Server 8.3\my.ini',
        'C:\ProgramData\MySQL\MySQL Server 8.2\my.ini',
        'C:\ProgramData\MySQL\MySQL Server 8.1\my.ini',
        'C:\ProgramData\MySQL\MySQL Server 8.0\my.ini'
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    return $null
}

function Set-BindAddressInMyIni([string]$myIniPath, [string]$bindAddress) {
    if (-not (Test-Path $myIniPath)) {
        throw "my.ini not found: $myIniPath"
    }

    $lines = Get-Content -Path $myIniPath
    $inMysqld = $false
    $mysqldStart = -1
    $mysqldEnd = $lines.Count
    $bindLineIndex = -1

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match '^\s*\[mysqld\]\s*$') {
            $inMysqld = $true
            $mysqldStart = $i
            continue
        }

        if ($inMysqld -and $line -match '^\s*\[') {
            $mysqldEnd = $i
            break
        }

        if ($inMysqld -and $line -match '^\s*bind-address\s*=') {
            $bindLineIndex = $i
        }
    }

    if ($mysqldStart -lt 0) {
        $lines += ''
        $lines += '[mysqld]'
        $lines += "bind-address=$bindAddress"
    }
    elseif ($bindLineIndex -ge 0) {
        $lines[$bindLineIndex] = "bind-address=$bindAddress"
    }
    else {
        $before = @()
        $after = @()

        if ($mysqldEnd -gt 0) {
            $before = $lines[0..($mysqldEnd - 1)]
        }
        if ($mysqldEnd -lt $lines.Count) {
            $after = $lines[$mysqldEnd..($lines.Count - 1)]
        }

        $lines = @($before + "bind-address=$bindAddress" + $after)
    }

    $backupPath = "$myIniPath.bak"
    Copy-Item -Path $myIniPath -Destination $backupPath -Force
    Set-Content -Path $myIniPath -Value $lines -Encoding ASCII
    Write-Host "Updated bind-address in my.ini: $myIniPath"
    Write-Host "Backup created: $backupPath"
}

function Ensure-FirewallRule([int]$port) {
    $ruleName = "MySQL TCP $port (SchoolManagementSystem)"
    $existing = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
    if (-not $existing) {
        New-NetFirewallRule `
            -DisplayName $ruleName `
            -Direction Inbound `
            -Action Allow `
            -Protocol TCP `
            -LocalPort $port | Out-Null
        Write-Host "Created firewall rule: $ruleName"
    }
    else {
        Write-Host "Firewall rule already exists: $ruleName"
    }
}

Require-Admin

if ([string]::IsNullOrWhiteSpace($AdminPassword)) {
    throw "Admin password is required. Pass -AdminPassword <password>."
}

if ([string]::IsNullOrWhiteSpace($RootPasswordForClient)) {
    throw "Remote root password is required. Pass -RootPasswordForClient <password>."
}

$myIniPath = Get-MyIniPath -serviceName $MySqlServiceName
if (-not $myIniPath) {
    throw "Unable to locate my.ini for service '$MySqlServiceName'. Set the correct service name or update manually."
}

Set-BindAddressInMyIni -myIniPath $myIniPath -bindAddress $BindAddress
Ensure-FirewallRule -port $MySqlPort

Write-Host "Restarting MySQL service: $MySqlServiceName"
Restart-Service -Name $MySqlServiceName -Force
Start-Sleep -Seconds 2

$allowRemoteScript = Join-Path $PSScriptRoot 'Allow-RemoteRoot.ps1'
if (-not (Test-Path $allowRemoteScript)) {
    throw "Required script not found: $allowRemoteScript"
}

Write-Host "Applying remote root grants..."
& powershell -ExecutionPolicy Bypass -File $allowRemoteScript `
    -HostName $MySqlHostName `
    -Port $MySqlPort `
    -AdminUser $AdminUser `
    -AdminPassword $AdminPassword `
    -ClientHost $ClientHost `
    -RootPasswordForClient $RootPasswordForClient `
    -Database $Database

if ($LASTEXITCODE -ne 0) {
    throw "Allow-RemoteRoot.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host ""
Write-Host "Remote MySQL setup completed."
Write-Host "Use this host in app Wired/Wireless profile: <Windows10_IP>"
Write-Host "Port: $MySqlPort"
