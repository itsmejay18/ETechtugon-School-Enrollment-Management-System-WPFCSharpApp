# School Management System (WinForms, .NET Framework 4.7.2)

## User manual
End-user documentation is available in [USER_MANUAL.TXT](USER_MANUAL.TXT).

## What is implemented now
- Database profiles: `Local`, `Wired`, `Wireless`, `Online (Hostinger)`
- Full, Incremental, and Differential backups (`.smsbak`)
- Restore flow with automatic safety Full backup before destructive restore
- Backup files are now encrypted + integrity-signed (new format), with legacy backup compatibility
- User activity logs (filter + CSV export)
- Runtime schema migration runner on login
- Database passwords in `SystemSetting` are stored protected (DPAPI)
- XML + JSON student/enrollment integration service with NUnit coverage for well-formedness and merge validation

## Quick setup
1. Create database schema:
   - Run `DatabaseScripts/SchoolManagementSystem.sql`
2. Apply incremental migrations (recommended for existing DBs):
   - `powershell -ExecutionPolicy Bypass -File .\DatabaseScripts\Apply-Migrations.ps1 -HostName <host> -Port 3306 -Username <user> -Password <password> -Database schoolmanagementsystem`
3. Configure connection values using environment variables (preferred):
   - `SMS_DB_HOST`, `SMS_DB_PORT`, `SMS_DB_NAME`, `SMS_DB_USER`, `SMS_DB_PASSWORD`
   - Optional: `SMS_DB_MODE=Local|Wired|Wireless|Online`
4. For Online/Hostinger TLS:
   - Set profile values in `Settings > Database > Database Connection Profiles`
   - Use `DbSslMode.Online` (`Required` recommended)
   - Set `DbSslCaPath.Online` if your provider requires CA bundle validation
5. Build and run `School-Management-System.sln`

## Backup and restore behavior
- Backup types:
  - `Full`: complete snapshot
  - `Incremental`: delta from latest backup
  - `Differential`: delta from latest Full backup
- Restore:
  - Select target backup file
  - System automatically creates a safety Full backup first
  - Then applies restore chain (Full + dependent deltas)
- Cancellation:
  - Backup/Restore UI has a `Cancel` button
  - Operations stop at safe checkpoints

## Backup encryption key configuration
- Optional override key for portable encrypted backups:
  - Env var: `SMS_BACKUP_KEY`
  - App.config key: `BackupEncryptionKey`
- If not set, app generates a local protected secret in `%LOCALAPPDATA%\SchoolManagementSystem\backup-state\backup.secret`

## Remote MySQL (Windows host) - least privilege app user
Use this script on the MySQL host machine:

```powershell
powershell -ExecutionPolicy Bypass -File .\DatabaseScripts\Allow-RemoteRoot.ps1 `
  -HostName localhost `
  -Port 3306 `
  -AdminUser root `
  -AdminPassword <ADMIN_PASSWORD> `
  -ClientHost % `
  -AppUser sms_app `
  -AppPassword <APP_PASSWORD> `
  -Database schoolmanagementsystem
```

All-in-one setup (bind address + firewall + grants):

```powershell
powershell -ExecutionPolicy Bypass -File .\DatabaseScripts\Enable-RemoteMySQL-Windows.ps1 `
  -MySqlHostName localhost `
  -MySqlPort 3306 `
  -AdminUser root `
  -AdminPassword <ADMIN_PASSWORD> `
  -ClientHost % `
  -AppUser sms_app `
  -AppPassword <APP_PASSWORD> `
  -Database schoolmanagementsystem `
  -MySqlServiceName MySQL80
```

## Patch remote schema (existing DB)

```powershell
powershell -ExecutionPolicy Bypass -File .\DatabaseScripts\Patch-RemoteMySQL.ps1 `
  -HostName <DB_HOST> `
  -Port 3306 `
  -Username <DB_USER> `
  -Password <DB_PASSWORD> `
  -Database schoolmanagementsystem
```

## Tests
A baseline NUnit test project is included:
- `School-Management-System.Tests`

Run tests (after NuGet restore):

```powershell
dotnet test .\School-Management-System.Tests\School-Management-System.Tests.csproj
```

## Security notes
- Do not commit real DB credentials
- Rotate DB credentials if previously exposed
- Change seeded default users/passwords immediately in production
- Database credentials are expected from environment variables, installer-time configuration, or the in-app database profile settings. The committed `App.config` files intentionally contain no production password.
- Demo quick-login presets are disabled by default. For a temporary classroom demo only, set `EnableDemoQuickLogin=true` and provide `DemoQuickLoginAccounts` as `Label|username|password;Label|username|password`, then disable it before release.

## Seeded users
The seed script may create starter accounts for local setup. Reset all starter passwords immediately before any production or defense deployment, and do not enable demo quick-login on deployed machines.

## Architecture
- `School-Management-System/Presentation`: WinForms UI
- `School-Management-System/BusinessLayer`: business logic/services
- `School-Management-System/DataLayer`: MySQL data access
- `School-Management-System/Models`: domain models
- `DatabaseScripts/`: bootstrap, patch, and migration scripts

Technical integration note: [DATA_INTEGRATION_NOTES.md](DATA_INTEGRATION_NOTES.md)
