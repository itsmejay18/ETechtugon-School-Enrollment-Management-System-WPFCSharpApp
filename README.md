# School Management System (WinForms, .NET Framework 4.7.2)

## Setup
1. Create the MySQL database by running `DatabaseScripts/SchoolManagementSystem.sql` in MySQL Workbench, or run `DatabaseScripts/Init-MySQL.ps1`.
2. Verify the connection string in `School-Management-System/App.config` points to your MySQL server.
3. Build and run the solution `School-Management-System.sln`.

## Remote Client Access (Laptop -> Windows 10 MySQL Host)
If login fails with `Access denied for user ...` from a remote host, run this on the **Windows 10 machine that hosts MySQL**:

```powershell
powershell -ExecutionPolicy Bypass -File .\DatabaseScripts\Allow-RemoteRoot.ps1 `
  -HostName localhost `
  -Port 3306 `
  -AdminUser root `
  -AdminPassword root `
  -ClientHost ZM `
  -RootPasswordForClient root `
  -Database schoolmanagementsystem
```

Then keep `School-Management-System/App.config` host set to the server IP (example: `192.168.1.107`) and retry login from the laptop.

If modules fail with errors like `Table '...systemsetting' doesn't exist`, run the remote schema/seed patch:

```powershell
powershell -ExecutionPolicy Bypass -File .\DatabaseScripts\Patch-RemoteMySQL.ps1 `
  -HostName 192.168.1.107 `
  -Port 3306 `
  -Username root `
  -Password root `
  -Database schoolmanagementsystem
```

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
