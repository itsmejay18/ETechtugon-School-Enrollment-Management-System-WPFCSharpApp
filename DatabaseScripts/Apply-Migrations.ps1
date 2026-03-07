param(
    [string]$HostName = 'localhost',
    [int]$Port = 3306,
    [string]$Username = 'root',
    [string]$Password = '',
    [string]$Database = 'schoolmanagementsystem'
)

$ErrorActionPreference = 'Stop'

$mysqlExe = 'C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe'
if (!(Test-Path $mysqlExe)) {
    throw "mysql.exe not found at: $mysqlExe"
}

if ([string]::IsNullOrWhiteSpace($Password)) {
    throw "MySQL password is required. Pass -Password <password>."
}

$migrationDir = Join-Path $PSScriptRoot 'Migrations'
if (!(Test-Path $migrationDir)) {
    throw "Migration directory not found: $migrationDir"
}

$files = Get-ChildItem -Path $migrationDir -Filter '*.sql' | Sort-Object Name
if ($files.Count -eq 0) {
    Write-Host "No migration scripts found."
    exit 0
}

Write-Host ("Applying {0} migration script(s) to {1}:{2}/{3}..." -f $files.Count, $HostName, $Port, $Database)

foreach ($file in $files) {
    $resolved = (Resolve-Path $file.FullName).Path.Replace('\', '/')
    Write-Host (" - " + $file.Name)
    & $mysqlExe --host=$HostName --port=$Port --user=$Username --password=$Password --database=$Database --default-character-set=utf8mb4 --execute="source $resolved"

    if ($LASTEXITCODE -ne 0) {
        throw "Migration failed: $($file.Name). Exit code: $LASTEXITCODE"
    }
}

Write-Host "Migrations completed successfully."
