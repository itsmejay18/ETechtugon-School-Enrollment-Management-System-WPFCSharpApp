School Management System - Installation Guide

Package contents:
- SchoolManagementSystemSetup.exe
- README_INSTALL.txt

System requirements:
- Windows 10 or Windows 11
- .NET Framework 4.7.2 or newer
- MySQL Server reachable from the client machine

Install steps:
1. Run SchoolManagementSystemSetup.exe as Administrator.
2. Complete the setup wizard.
3. Launch School Management System from Desktop or Start Menu.

Database connection setup:
1. Open Settings > Database in the app.
2. Select the correct mode/profile (Local, Wired, or Wireless).
3. Enter Host/IP, Port, Database, Username, and Password.
4. Save profile, then test connection.

If app cannot connect to database:
- Confirm MySQL service is running.
- Confirm host IP and port (default 3306).
- Confirm firewall allows inbound TCP 3306 on server.
- Confirm client and server are on same network when using LAN IP.
- Confirm database user has remote access privileges.

Notes:
- If moving to a different network, update Host/IP in database settings.
- Keep backup of database before applying major updates.
