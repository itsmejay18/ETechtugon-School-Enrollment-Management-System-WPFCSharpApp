-- Migration 004: Store photo binary data directly in the database.
-- Adds PhotoData LONGBLOB column to student, faculty, and users tables.
-- Existing PhotoPath column is kept for backward compatibility.

ALTER TABLE `student`
    ADD COLUMN IF NOT EXISTS `PhotoData` LONGBLOB NULL AFTER `PhotoPath`;

ALTER TABLE `faculty`
    ADD COLUMN IF NOT EXISTS `PhotoData` LONGBLOB NULL AFTER `PhotoPath`;

ALTER TABLE `users`
    ADD COLUMN IF NOT EXISTS `PhotoData` LONGBLOB NULL AFTER `PhotoPath`;
