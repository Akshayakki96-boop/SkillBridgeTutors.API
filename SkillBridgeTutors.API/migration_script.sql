IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808075138_InitialCreate'
)
BEGIN
    CREATE TABLE [DemoSlots] (
        [Id] int NOT NULL IDENTITY,
        [SlotDateTime] datetime2 NOT NULL,
        [IsBooked] bit NOT NULL,
        [TutorName] nvarchar(max) NULL,
        CONSTRAINT [PK_DemoSlots] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808075138_InitialCreate'
)
BEGIN
    CREATE TABLE [Leads] (
        [Id] int NOT NULL IDENTITY,
        [ParentName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Subject] nvarchar(max) NOT NULL,
        [Query] nvarchar(max) NOT NULL,
        [CallStatus] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Leads] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808075138_InitialCreate'
)
BEGIN
    CREATE TABLE [CallRecords] (
        [Id] int NOT NULL IDENTITY,
        [LeadId] int NOT NULL,
        [RetellCallId] nvarchar(max) NOT NULL,
        [Transcript] nvarchar(max) NULL,
        [RecordingUrl] nvarchar(max) NULL,
        [DurationSeconds] int NULL,
        [Summary] nvarchar(max) NULL,
        [CallStatus] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [EndedAt] datetime2 NULL,
        CONSTRAINT [PK_CallRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CallRecords_Leads_LeadId] FOREIGN KEY ([LeadId]) REFERENCES [Leads] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808075138_InitialCreate'
)
BEGIN
    CREATE TABLE [DemoBookings] (
        [Id] int NOT NULL IDENTITY,
        [LeadId] int NOT NULL,
        [DemoSlotId] int NOT NULL,
        [StudentName] nvarchar(max) NOT NULL,
        [Grade] nvarchar(max) NOT NULL,
        [Curriculum] nvarchar(max) NOT NULL,
        [Subject] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [MeetingLink] nvarchar(max) NULL,
        [BookedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_DemoBookings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DemoBookings_DemoSlots_DemoSlotId] FOREIGN KEY ([DemoSlotId]) REFERENCES [DemoSlots] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DemoBookings_Leads_LeadId] FOREIGN KEY ([LeadId]) REFERENCES [Leads] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808075138_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CallRecords_LeadId] ON [CallRecords] ([LeadId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808075138_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DemoBookings_DemoSlotId] ON [DemoBookings] ([DemoSlotId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808075138_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DemoBookings_LeadId] ON [DemoBookings] ([LeadId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808075138_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260808075138_InitialCreate', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808145150_AddAdminUser'
)
BEGIN
    CREATE TABLE [AdminUsers] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AdminUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808145150_AddAdminUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260808145150_AddAdminUser', N'8.0.8');
END;
GO

COMMIT;
GO

