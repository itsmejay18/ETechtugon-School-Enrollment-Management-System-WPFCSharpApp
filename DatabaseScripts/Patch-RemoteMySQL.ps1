param(
    [string]$HostName = '192.168.1.107',
    [int]$Port = 3306,
    [string]$Username = 'root',
    [string]$Password = '',
    [string]$Database = 'schoolmanagementsystem'
)

$scriptPath = Join-Path $PSScriptRoot 'Patch-RemoteSchema.sql'
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

Write-Host ("Patching remote MySQL schema on {0}:{1} ({2}) ..." -f $HostName, $Port, $Database)

$resolvedScriptPath = (Resolve-Path $scriptPath).Path.Replace('\', '/')
& $mysqlExe --host=$HostName --port=$Port --user=$Username --password=$Password --default-character-set=utf8mb4 --execute="source $resolvedScriptPath"

if ($LASTEXITCODE -ne 0) {
    throw "Remote schema patch failed with exit code $LASTEXITCODE."
}

Write-Host 'Remote schema patch completed successfully.'
