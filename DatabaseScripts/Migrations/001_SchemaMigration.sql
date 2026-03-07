CREATE TABLE IF NOT EXISTS `schemamigration` (
  `MigrationId` varchar(50) NOT NULL,
  `Description` varchar(255) NOT NULL,
  `AppliedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
