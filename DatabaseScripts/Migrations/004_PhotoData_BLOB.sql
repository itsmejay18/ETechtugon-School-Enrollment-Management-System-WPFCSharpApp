-- Migration 004: Store photo binary data directly in the database.
-- Adds PhotoData LONGBLOB column to student, faculty, and users tables.
-- Existing PhotoPath column is kept for backward compatibility.

SET @schema_name = DATABASE();

SET @sql = IF(
    EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = @schema_name
          AND table_name = 'student'
          AND column_name = 'PhotoData'
    ),
    'SET @migration_noop = 1',
    'ALTER TABLE `student` ADD COLUMN `PhotoData` LONGBLOB NULL AFTER `PhotoPath`'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = @schema_name
          AND table_name = 'faculty'
          AND column_name = 'PhotoData'
    ),
    'SET @migration_noop = 1',
    'ALTER TABLE `faculty` ADD COLUMN `PhotoData` LONGBLOB NULL AFTER `PhotoPath`'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = @schema_name
          AND table_name = 'users'
          AND column_name = 'PhotoData'
    ),
    'SET @migration_noop = 1',
    'ALTER TABLE `users` ADD COLUMN `PhotoData` LONGBLOB NULL AFTER `PhotoPath`'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
