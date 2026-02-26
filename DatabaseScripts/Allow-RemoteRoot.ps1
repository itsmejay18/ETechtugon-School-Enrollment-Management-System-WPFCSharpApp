param(
    [string]$HostName = 'localhost',
    [int]$Port = 3306,
    [string]$AdminUser = 'root',
    [string]$AdminPassword = 'root',
    [string]$ClientHost = '%',
    [string]$RootPasswordForClient = 'root',
    [string]$Database = 'schoolmanagementsystem'
)

$mysqlExe = 'C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe'
if (!(Test-Path $mysqlExe)) {
    throw "mysql.exe not found at: $mysqlExe"
}

function Escape-Sql([string]$value) {
    if ($null -eq $value) { return '' }
    return $value.Replace("'", "''")
}

$safeClientHost = Escape-Sql $ClientHost
$safeRootPasswordForClient = Escape-Sql $RootPasswordForClient
$safeDatabase = Escape-Sql $Database

$dbIdentifier = ([char]96 + $safeDatabase + [char]96 + ".*")
$sql = @"
CREATE USER IF NOT EXISTS 'root'@'$safeClientHost' IDENTIFIED WITH mysql_native_password BY '$safeRootPasswordForClient';
ALTER USER 'root'@'$safeClientHost' IDENTIFIED WITH mysql_native_password BY '$safeRootPasswordForClient';
GRANT ALL PRIVILEGES ON $dbIdentifier TO 'root'@'$safeClientHost';
FLUSH PRIVILEGES;
SHOW GRANTS FOR 'root'@'$safeClientHost';
"@

$tmpSql = Join-Path $env:TEMP ("sms-remote-root-" + [guid]::NewGuid().ToString("N") + ".sql")
try {
    Set-Content -Path $tmpSql -Value $sql -Encoding UTF8

    Write-Host ("Configuring remote MySQL access for root@{0} on {1}:{2}..." -f $ClientHost, $HostName, $Port)
    & $mysqlExe --host=$HostName --port=$Port --user=$AdminUser --password=$AdminPassword --default-character-set=utf8mb4 --execute="source $tmpSql"

    if ($LASTEXITCODE -ne 0) {
        throw "MySQL command failed with exit code $LASTEXITCODE."
    }

    Write-Host "Remote access configured successfully."
    Write-Host ("Client host allowed: {0}" -f $ClientHost)
    Write-Host ("Database granted: {0}" -f $Database)
}
finally {
    if (Test-Path $tmpSql) {
        Remove-Item -Path $tmpSql -Force -ErrorAction SilentlyContinue
    }
}
