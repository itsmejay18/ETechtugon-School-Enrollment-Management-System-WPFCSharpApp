ALTER TABLE `users` ADD COLUMN IF NOT EXISTS `PhotoPath` varchar(260) DEFAULT NULL AFTER `DisplayName`;
ALTER TABLE `faculty` ADD COLUMN IF NOT EXISTS `PhotoPath` varchar(260) DEFAULT NULL AFTER `Address`;
ALTER TABLE `student` ADD COLUMN IF NOT EXISTS `PhotoPath` varchar(260) DEFAULT NULL AFTER `Address`;

CREATE TABLE IF NOT EXISTS `activitylog` (
  `ActivityLogId` int NOT NULL AUTO_INCREMENT,
  `UserId` int DEFAULT NULL,
  `Action` varchar(50) NOT NULL,
  `Entity` varchar(50) DEFAULT NULL,
  `EntityId` int DEFAULT NULL,
  `Details` varchar(4000) DEFAULT NULL,
  `MachineName` varchar(100) DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`ActivityLogId`),
  KEY `IX_ActivityLog_CreatedAt` (`CreatedAt`),
  KEY `FK_ActivityLog_Users` (`UserId`),
  CONSTRAINT `FK_ActivityLog_Users` FOREIGN KEY (`UserId`) REFERENCES `users` (`UserId`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

ALTER TABLE `activitylog` ADD COLUMN IF NOT EXISTS `MachineName` varchar(100) DEFAULT NULL AFTER `Details`;
