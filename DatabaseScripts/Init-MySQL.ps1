param(
    [string]$HostName = 'localhost',
    [int]$Port = 3306,
    [string]$Username = 'root',
    [string]$Password = '',
    [string]$Database = 'schoolmanagementsystem'
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

Write-Host ("Ensuring database '{0}' exists on MySQL {1}:{2} ..." -f $Database, $HostName, $Port)
& $mysqlExe --host=$HostName --port=$Port --user=$Username --password=$Password --default-character-set=utf8mb4 --execute="CREATE DATABASE IF NOT EXISTS $Database CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;"

if ($LASTEXITCODE -ne 0) {
    throw "Database creation failed with exit code $LASTEXITCODE."
}

Write-Host ("Initializing SchoolManagementSystem schema into {0} ..." -f $Database)
Get-Content -Raw $scriptPath | & $mysqlExe --host=$HostName --port=$Port --user=$Username --password=$Password --database=$Database --default-character-set=utf8mb4

if ($LASTEXITCODE -ne 0) {
    throw "Database initialization failed with exit code $LASTEXITCODE."
}

Write-Host 'Database initialized successfully.'
