-- SQL Script to add missing columns to Students table
-- Run this script directly on your database if the application initialization doesn't work

-- Add GoogleId column if it doesn't exist
IF OBJECT_ID(N'[dbo].[Students]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'GoogleId'
    )
    BEGIN
        ALTER TABLE dbo.Students ADD GoogleId nvarchar(max) NULL;
        PRINT 'Column GoogleId added to Students table';
    END
    ELSE
    BEGIN
        PRINT 'Column GoogleId already exists in Students table';
    END
END
GO

-- Add AuthProvider column if it doesn't exist
IF OBJECT_ID(N'[dbo].[Students]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'AuthProvider'
    )
    BEGIN
        ALTER TABLE dbo.Students ADD AuthProvider nvarchar(max) NULL;
        PRINT 'Column AuthProvider added to Students table';
    END
    ELSE
    BEGIN
        PRINT 'Column AuthProvider already exists in Students table';
    END
END
GO

-- Verify the columns exist
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Students' 
  AND (COLUMN_NAME = 'GoogleId' OR COLUMN_NAME = 'AuthProvider')
ORDER BY COLUMN_NAME;
