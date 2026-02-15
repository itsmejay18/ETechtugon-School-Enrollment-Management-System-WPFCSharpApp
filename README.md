# School Management System (WinForms, .NET Framework 4.7.2)

## Setup
1. Create the MySQL database by running `DatabaseScripts/SchoolManagementSystem.sql` in MySQL Workbench, or run `DatabaseScripts/Init-MySQL.ps1`.
2. Verify the connection string in `School-Management-System/App.config` points to your MySQL server.
3. Build and run the solution `School-Management-System.sln`.

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
