School Enrollment Management System - Installation Guide
=========================================================

PACKAGE CONTENTS
- SchoolManagementSystemSetup.exe   (the installer)
- README_INSTALL.txt                (this file)

SYSTEM REQUIREMENTS
- Windows 10 (1809+) or Windows 11
- .NET Framework 4.7.2 or newer    (already shipped with Windows 10/11)
- MySQL Server 8.x reachable from this machine
  - Local install, LAN IP, or hosted (e.g. Hostinger)

INSTALL STEPS
1. Right-click SchoolManagementSystemSetup.exe and choose "Run as administrator".
2. Step through the wizard. The installer creates:
     %ProgramFiles%\School Enrollment Management System\
       (executable, DLLs, assets)
       \DatabaseScripts\   (DB setup helpers, see below)
       \docs\              (User Manual, README)
3. Let the installer launch the app at the end, or open it from the Start menu.

DATABASE DEPLOYMENT
The application needs a populated MySQL database before first login.
Run these on the machine that hosts MySQL (or anywhere with the MySQL client):

  1. Create the schema and base tables (fresh server):
     powershell -ExecutionPolicy Bypass -File "C:\Program Files\School Enrollment Management System\DatabaseScripts\Init-MySQL.ps1" `
       -HostName <DB_HOST> -Port 3306 `
       -Username <ADMIN_USER> -Password <ADMIN_PASSWORD> `
       -Database schoolmanagementsystem

  2. Apply incremental migrations (idempotent, safe to re-run):
     powershell -ExecutionPolicy Bypass -File "C:\Program Files\School Enrollment Management System\DatabaseScripts\Apply-Migrations.ps1" `
       -HostName <DB_HOST> -Port 3306 `
       -Username <ADMIN_USER> -Password <ADMIN_PASSWORD> `
       -Database schoolmanagementsystem

  3. (Optional) Create a least-privilege app user with remote access:
     powershell -ExecutionPolicy Bypass -File "C:\Program Files\School Enrollment Management System\DatabaseScripts\Enable-RemoteMySQL-Windows.ps1" `
       -MySqlHostName localhost -MySqlPort 3306 `
       -AdminUser root -AdminPassword <ADMIN_PASSWORD> `
       -AppUser sms_app -AppPassword <APP_PASSWORD> `
       -Database schoolmanagementsystem -MySqlServiceName MySQL80

CONNECTION CONFIGURATION
The shipped App.config has empty credentials by design. Configure connection
values in any of these three ways (highest priority first):

  A. Per-machine environment variables (recommended for production)
       SMS_DB_MODE       = Local | Wired | Wireless | Online
       SMS_DB_HOST       = e.g. 192.168.1.10  or  myhost.example.com
       SMS_DB_PORT       = 3306
       SMS_DB_NAME       = schoolmanagementsystem
       SMS_DB_USER       = sms_app
       SMS_DB_PASSWORD   = <strong password>
     Set them via System Properties > Environment Variables and restart the app.

  B. App.config in the install folder
       Edit School-Management-System.Wpf.exe.config under {app} as Administrator
       and fill the DbHost/DbPort/DbName/DbUser/DbPassword keys (Local profile)
       or DbHostOnline/DbPortOnline/... (Online profile). Update DbMode to
       match the active profile.

  C. In-app Settings > Database
       Sign in once with bootstrap credentials (A or B), then open
       Settings > Database, fill all four profiles, click Save Profile,
       click Test Connection, click Apply Runtime.

FIRST-RUN CHECKLIST
1. Launch the app. The login screen shows the active profile summary.
2. Sign in as the seeded administrator (ask the deployment lead).
3. CHANGE EVERY SEEDED PASSWORD IMMEDIATELY in Settings > User Management.
4. Open Settings > System and set:
     - Current School Year
     - Current Semester
     - Tuition Per Unit, Misc Fee, Registration Fee, Laboratory Fee
5. Open Settings > Database and click Test Connection. Should be green.
6. Create a Full backup. Verify by restoring it into a sandbox database.

BACKUP / RESTORE
- Backups go to the folder configured in Settings > Database > Backup.
- Backups are encrypted with a key derived per-machine from
  %LOCALAPPDATA%\SchoolManagementSystem\backup-state\backup.secret
- For portable backups across machines, set BackupEncryptionKey in
  School-Management-System.Wpf.exe.config (or the SMS_BACKUP_KEY env var).

TROUBLESHOOTING
- App fails to start
    Look at:  %LOCALAPPDATA%\SchoolManagementSystem\logs\app.log
    Check:    Windows Event Viewer > Windows Logs > Application
- Cannot connect to database
    Confirm MySQL service is running.
    Confirm host/IP and port (default 3306).
    Confirm Windows Firewall allows inbound TCP 3306 on the server.
    Confirm the DB user has remote access privileges (use Allow-RemoteRoot.ps1).
    From the client PC try:  telnet <host> 3306
- COR will not print
    Verify a default printer is configured. Print COR uses the system default.
- Blurry text on high-DPI displays
    Right-click the .exe > Properties > Compatibility >
      Change high DPI settings > High DPI scaling override:
      "System (Enhanced)". The shipped manifest already declares
      PerMonitorV2 awareness; this should not be needed.
- Quick-login dropdown appears unexpectedly
    Quick-login is disabled by default. To re-disable, set
    EnableDemoQuickLogin=false in App.config and restart.

SECURITY
- Production database password is NOT embedded in the installer.
- Demo quick-login presets are off by default. Keep them off in production.
- All database passwords stored inside the schema are DPAPI-protected.
- Rotate any seeded passwords before going live.

UNINSTALL
Use Settings > Apps > Installed Apps > "School Enrollment Management System".
Per-user data under %LOCALAPPDATA%\SchoolManagementSystem is preserved on
uninstall so backups, photos, and audit logs survive a reinstall. Delete that
folder manually if a full clean removal is required.
