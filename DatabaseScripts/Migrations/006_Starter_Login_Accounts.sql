CREATE TABLE IF NOT EXISTS `schemamigration` (
  `MigrationId` varchar(50) NOT NULL,
  `Description` varchar(255) NOT NULL,
  `AppliedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TEMPORARY TABLE IF NOT EXISTS `starter_login_account` (
  `PreferredUserId` int NOT NULL,
  `Username` varchar(50) NOT NULL,
  `PasswordHashHex` varchar(64) NOT NULL,
  `PasswordSaltHex` varchar(32) NOT NULL,
  `RoleName` varchar(30) NOT NULL,
  `DisplayName` varchar(100) NOT NULL,
  PRIMARY KEY (`PreferredUserId`),
  UNIQUE KEY `UX_StarterLogin_Username` (`Username`)
) ENGINE=MEMORY;

TRUNCATE TABLE `starter_login_account`;

INSERT INTO `starter_login_account`
(`PreferredUserId`, `Username`, `PasswordHashHex`, `PasswordSaltHex`, `RoleName`, `DisplayName`)
VALUES
(1, 'admin', '491B306728DB9AB65DC7504B3EF8DA1293693F2E55A958405C4BB7BB6672A934', '2C8FA907A42255A3FB0F3C02B49C7473', 'Admin', 'System Administrator'),
(2, 'registrar', '828F4E3EE279C5D766D8C58C257E87C0CB44F4843EDC051D1A41219B72738523', '474715AC6A02F482A85FA09479BE5C30', 'Registrar', 'Registrar Office'),
(3, 'faculty1', 'DD9B92F18FDBD92BE8126B29ECF7362CA8DD9ACAFEFDCA7803CC62EB46B945AF', '42E2DBAFA5230A27DA3A31C49A8203F0', 'Faculty', 'Default Faculty');

UPDATE `users` u
INNER JOIN `starter_login_account` a
  ON a.`PreferredUserId` = u.`UserId`
LEFT JOIN `users` existing
  ON existing.`Username` = a.`Username`
 AND existing.`UserId` <> u.`UserId`
SET
  u.`Username` = a.`Username`,
  u.`PasswordHash` = UNHEX(a.`PasswordHashHex`),
  u.`PasswordSalt` = UNHEX(a.`PasswordSaltHex`),
  u.`Role` = a.`RoleName`,
  u.`DisplayName` = a.`DisplayName`,
  u.`IsActive` = 1,
  u.`UpdatedAt` = UTC_TIMESTAMP()
WHERE existing.`UserId` IS NULL;

INSERT INTO `users`
(
  `Username`,
  `PasswordHash`,
  `PasswordSalt`,
  `Role`,
  `DisplayName`,
  `IsActive`,
  `CreatedAt`,
  `UpdatedAt`
)
SELECT
  a.`Username`,
  UNHEX(a.`PasswordHashHex`),
  UNHEX(a.`PasswordSaltHex`),
  a.`RoleName`,
  a.`DisplayName`,
  1,
  UTC_TIMESTAMP(),
  UTC_TIMESTAMP()
FROM `starter_login_account` a
ON DUPLICATE KEY UPDATE
  `PasswordHash` = VALUES(`PasswordHash`),
  `PasswordSalt` = VALUES(`PasswordSalt`),
  `Role` = VALUES(`Role`),
  `DisplayName` = VALUES(`DisplayName`),
  `IsActive` = 1,
  `UpdatedAt` = UTC_TIMESTAMP();

INSERT INTO `schemamigration` (`MigrationId`, `Description`, `AppliedAt`)
VALUES ('2026051901', 'Ensure starter login accounts match documented credentials.', UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE
  `Description` = VALUES(`Description`),
  `AppliedAt` = `AppliedAt`;

DROP TEMPORARY TABLE IF EXISTS `starter_login_account`;
