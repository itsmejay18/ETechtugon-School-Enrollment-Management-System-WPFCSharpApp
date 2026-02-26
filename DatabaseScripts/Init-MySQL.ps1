param(
    [string]$HostName = 'localhost',
    [int]$Port = 3306,
    [string]$Username = 'root',
    [string]$Password = ''
)

$scriptPath = Join-Path $PSScriptRoot 'SchoolManagementSystem.sql'
$mysqlExe = 'C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe'

if (!(Test-Path $mysqlExe)) {
    throw "mysql.exe not found at: $mysqlExe"
}

if (!(Test-Path $scriptPath)) {
    throw "SQL script not found: $scriptPath"
}

if ([string]::IsNullOrWhiteSpace($Password)) {
    throw "MySQL password is required. Pass -Password <password>."
}

Write-Host ("Initializing SchoolManagementSystem on MySQL {0}:{1} ..." -f $HostName, $Port)
Get-Content -Raw $scriptPath | & $mysqlExe --host=$HostName --port=$Port --user=$Username --password=$Password --default-character-set=utf8mb4

if ($LASTEXITCODE -ne 0) {
    throw "Database initialization failed with exit code $LASTEXITCODE."
}

Write-Host 'Database initialized successfully.'
