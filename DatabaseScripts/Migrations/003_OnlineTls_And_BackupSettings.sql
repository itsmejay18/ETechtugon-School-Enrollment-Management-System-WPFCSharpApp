INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES
('DbHost.Online', 'localhost'),
('DbPort.Online', '3306'),
('DbName.Online', 'schoolmanagementsystem'),
('DbUser.Online', ''),
('DbPassword.Online', ''),
('DbSslMode.Online', 'Required'),
('DbSslCaPath.Online', ''),
('Backup.Directory', ''),
('Backup.PreferredType', 'Full')
ON DUPLICATE KEY UPDATE
`SettingValue` = IFNULL(`SettingValue`, VALUES(`SettingValue`));
