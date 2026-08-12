BEGIN TRANSACTION;
GO

ALTER TABLE [DemoBookings] ADD [TeacherId] bigint NULL;
GO

CREATE TABLE [Teachers] (
    [TeacherId] bigint NOT NULL IDENTITY,
    [FullName] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Subjects] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Teachers] PRIMARY KEY ([TeacherId])
);
GO

CREATE INDEX [IX_DemoBookings_TeacherId] ON [DemoBookings] ([TeacherId]);
GO

ALTER TABLE [DemoBookings] ADD CONSTRAINT [FK_DemoBookings_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([TeacherId]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260812145038_AddTeacherAndAssignment', N'8.0.8');
GO

COMMIT;
GO

