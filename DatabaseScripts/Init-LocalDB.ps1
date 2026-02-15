param(
    [string]$Instance = "(localdb)\MSSQLLocalDB",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

function Get-BatchesFromSqlFile([string]$path) {
    $lines = Get-Content -LiteralPath $path
    $batches = New-Object System.Collections.Generic.List[string]
    $current = New-Object System.Text.StringBuilder

    foreach ($line in $lines) {
        if ($line -match "^[\\s]*GO[\\s]*$") {
            $text = $current.ToString().Trim()
            if ($text.Length -gt 0) {
                $batches.Add($text)
            }
            $null = $current.Clear()
            continue
        }

        $null = $current.AppendLine($line)
    }

    $tail = $current.ToString().Trim()
    if ($tail.Length -gt 0) {
        $batches.Add($tail)
    }

    return $batches
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sqlPath = Join-Path $scriptDir "SchoolManagementSystem.sql"

if (!(Test-Path -LiteralPath $sqlPath)) {
    throw "SQL script not found: $sqlPath"
}

$masterCs = "Data Source=$Instance;Initial Catalog=master;Integrated Security=True;MultipleActiveResultSets=True"

Write-Host "Connecting to: $Instance"

$conn = New-Object System.Data.SqlClient.SqlConnection($masterCs)
$conn.Open()

try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(1) FROM sys.databases WHERE name = 'SchoolManagementSystem';"
    $exists = [int]$cmd.ExecuteScalar()

    if ($exists -gt 0 -and -not $Force) {
        throw "Database 'SchoolManagementSystem' already exists. Re-running will DROP tables and data. Re-run with -Force to continue."
    }

    $batches = Get-BatchesFromSqlFile $sqlPath
    Write-Host ("Executing {0} batch(es)..." -f $batches.Count)

    for ($i = 0; $i -lt $batches.Count; $i++) {
        $cmd = $conn.CreateCommand()
        $cmd.CommandTimeout = 120
        $cmd.CommandText = $batches[$i]
        $null = $cmd.ExecuteNonQuery()
    }

    Write-Host "Done. Database initialized."
}
finally {
    $conn.Close()
}
