# School Management System (WinForms, .NET Framework 4.7.2)

## Setup
1. Create the MySQL database by running `DatabaseScripts/SchoolManagementSystem.sql` in MySQL Workbench, or run `DatabaseScripts/Init-MySQL.ps1`.
2. Set database values using `SMS_DB_HOST`, `SMS_DB_PORT`, `SMS_DB_NAME`, `SMS_DB_USER`, `SMS_DB_PASSWORD` environment variables (recommended), or update `School-Management-System/App.config` locally.
3. Choose active connection mode using either:
   - command line: `School-Management-System.exe --db-mode=Local` or `School-Management-System.exe --db-mode=Network`
   - Settings module: `Settings > General > Database Connection Profiles`
4. Build and run the solution `School-Management-System.sln`.

## Navigation Layout
- Workflow menu: `Dashboard`, `Students`, `Faculty`, `Enrollment`, `Schedule`, `Calendar`, `Settings`
- Administrative setup is consolidated under `Settings` tabs:
  - `Departments`, `Courses`, `Year Levels`, `Sections`, `Subjects`, `Curriculum`, `User Management`

## Remote Client Access (Laptop -> Windows 10 MySQL Host)
If login fails with `Access denied for user ...` from a remote host, run this on the **Windows 10 machine that hosts MySQL**:

```powershell
powershell -ExecutionPolicy Bypass -File .\DatabaseScripts\Allow-RemoteRoot.ps1 `
  -HostName localhost `
  -Port 3306 `
  -AdminUser <ADMIN_USER> `
  -AdminPassword <ADMIN_PASSWORD> `
  -ClientHost <CLIENT_HOSTNAME_OR_%> `
  -RootPasswordForClient <REMOTE_ROOT_PASSWORD> `
  -Database schoolmanagementsystem
```

Then keep `School-Management-System/App.config` host set to the server IP (example: `192.168.1.107`) and retry login from the laptop.

If modules fail with errors like `Table '...systemsetting' doesn't exist`, run the remote schema/seed patch:

```powershell
powershell -ExecutionPolicy Bypass -File .\DatabaseScripts\Patch-RemoteMySQL.ps1 `
  -HostName <DB_HOST_IP> `
  -Port 3306 `
  -Username <DB_USER> `
  -Password <DB_PASSWORD> `
  -Database schoolmanagementsystem
```

## Security Note
- Do not commit real database hostnames, usernames, or passwords to Git.
- If credentials were already pushed, rotate them on MySQL and then push the sanitized config/docs.

## Default Login (Seeded By SQL Script)
- Username: `admin`
- Password: `admin123`
- Username: `registrar`
- Password: `registrar123`
- Username: `faculty1`
- Password: `faculty123`

## Architecture
- `School-Management-System/Presentation`: WinForms Forms + UserControls (UI only)
- `School-Management-System/BusinessLayer`: validation + business rules
- `School-Management-System/DataLayer`: ADO.NET (MySqlConnection/MySqlCommand/MySqlDataAdapter)
- `School-Management-System/Models`: OOP models (encapsulated properties)
