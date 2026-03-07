param(
    [string]$HostName = 'localhost',
    [int]$Port = 3306,
    [string]$AdminUser = 'root',
    [string]$AdminPassword = '',
    [string]$ClientHost = '%',
    [string]$AppUser = 'sms_app',
    [string]$AppPassword = '',
    [string]$Database = 'schoolmanagementsystem'
)

$mysqlExe = 'C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe'
if (!(Test-Path $mysqlExe)) {
    throw "mysql.exe not found at: $mysqlExe"
}

if ([string]::IsNullOrWhiteSpace($AdminPassword)) {
    throw "Admin password is required. Pass -AdminPassword <password>."
}

if ([string]::IsNullOrWhiteSpace($AppPassword)) {
    throw "Application password is required. Pass -AppPassword <password>."
}

function Escape-Sql([string]$value) {
    if ($null -eq $value) { return '' }
    return $value.Replace("'", "''")
}

$safeClientHost = Escape-Sql $ClientHost
$safeAppUser = Escape-Sql $AppUser
$safeAppPassword = Escape-Sql $AppPassword
$safeDatabase = Escape-Sql $Database
$dbIdentifier = ([char]96 + $safeDatabase + [char]96 + ".*")

$sql = @"
CREATE USER IF NOT EXISTS '$safeAppUser'@'$safeClientHost' IDENTIFIED BY '$safeAppPassword';
ALTER USER '$safeAppUser'@'$safeClientHost' IDENTIFIED BY '$safeAppPassword';
GRANT SELECT, INSERT, UPDATE, DELETE ON $dbIdentifier TO '$safeAppUser'@'$safeClientHost';
FLUSH PRIVILEGES;
SHOW GRANTS FOR '$safeAppUser'@'$safeClientHost';
"@

$tmpSql = Join-Path $env:TEMP ("sms-remote-app-user-" + [guid]::NewGuid().ToString("N") + ".sql")
try {
    Set-Content -Path $tmpSql -Value $sql -Encoding UTF8

    Write-Host ("Configuring remote MySQL app user '{0}'@'{1}' on {2}:{3}..." -f $AppUser, $ClientHost, $HostName, $Port)
    & $mysqlExe --host=$HostName --port=$Port --user=$AdminUser --password=$AdminPassword --default-character-set=utf8mb4 --execute="source $tmpSql"

    if ($LASTEXITCODE -ne 0) {
        throw "MySQL command failed with exit code $LASTEXITCODE."
    }

    Write-Host "Remote least-privilege user configured successfully."
    Write-Host ("Client host allowed: {0}" -f $ClientHost)
    Write-Host ("Database granted: {0}" -f $Database)
    Write-Host ("Application user: {0}" -f $AppUser)
}
finally {
    if (Test-Path $tmpSql) {
        Remove-Item -Path $tmpSql -Force -ErrorAction SilentlyContinue
    }
}
