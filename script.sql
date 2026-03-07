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
CREATE TABLE [Categories] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);

CREATE TABLE [Locations] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Address] nvarchar(255) NULL,
    [ImageUrl] nvarchar(max) NULL,
    [MapUrl] nvarchar(500) NULL,
    [Capacity] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Locations] PRIMARY KEY ([Id])
);

CREATE TABLE [Roles] (
    [Id] uniqueidentifier NOT NULL,
    [RoleName] nvarchar(50) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [Username] nvarchar(50) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [PasswordHash] nvarchar(500) NULL,
    [AvatarUrl] nvarchar(500) NULL,
    [PhoneNumber] nvarchar(30) NULL,
    [UniversityId] nvarchar(255) NULL,
    [SchoolName] nvarchar(255) NULL,
    [ClassName] nvarchar(max) NULL,
    [Gender] int NULL,
    [DOB] datetime2 NULL,
    [Address] nvarchar(255) NULL,
    [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit),
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeletedAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Events] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(250) NOT NULL,
    [Description] nvarchar(4000) NULL,
    [ThumbnailUrl] nvarchar(500) NULL,
    [StartTime] datetimeoffset NOT NULL,
    [EndTime] datetimeoffset NOT NULL,
    [MaxParticipants] int NOT NULL,
    [Status] int NOT NULL,
    [IsPrivate] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CategoryId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NOT NULL,
    [OrganizerId] uniqueidentifier NOT NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeletedAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Events] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Events_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Events_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Events_Users_OrganizerId] FOREIGN KEY ([OrganizerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [UserRoles] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [EventCollaborators] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Role] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_EventCollaborators] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EventCollaborators_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EventCollaborators_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [EventImages] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [ImageUrl] nvarchar(500) NOT NULL,
    [Caption] nvarchar(200) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_EventImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EventImages_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [EventReviews] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [ParentId] uniqueidentifier NULL,
    [Content] nvarchar(1000) NOT NULL,
    [Rating] int NOT NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DeletedAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_EventReviews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EventReviews_EventReviews_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [EventReviews] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EventReviews_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EventReviews_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [TicketTypes] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Price] decimal(18,2) NOT NULL DEFAULT 0.0,
    [Quantity] int NOT NULL DEFAULT 0,
    [SoldCount] int NOT NULL DEFAULT 0,
    [SaleStartDate] datetimeoffset NULL,
    [SaleEndDate] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_TicketTypes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TicketTypes_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [EventRegistrations] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TicketTypeId] uniqueidentifier NOT NULL,
    [TicketCode] nvarchar(max) NOT NULL,
    [QrCodeUrl] nvarchar(max) NULL,
    [CheckInTime] datetimeoffset NULL,
    [PaymentExpiryTime] datetimeoffset NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_EventRegistrations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EventRegistrations_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EventRegistrations_TicketTypes_TicketTypeId] FOREIGN KEY ([TicketTypeId]) REFERENCES [TicketTypes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EventRegistrations_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Transactions] (
    [Id] uniqueidentifier NOT NULL,
    [EventRegistrationId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [TransactionNo] nvarchar(50) NULL,
    [BankCode] nvarchar(10) NULL,
    [OrderInfo] nvarchar(255) NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transactions_EventRegistrations_EventRegistrationId] FOREIGN KEY ([EventRegistrationId]) REFERENCES [EventRegistrations] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);

CREATE INDEX [IX_EventCollaborators_EventId] ON [EventCollaborators] ([EventId]);

CREATE UNIQUE INDEX [IX_EventCollaborators_EventId_UserId] ON [EventCollaborators] ([EventId], [UserId]);

CREATE INDEX [IX_EventCollaborators_Role] ON [EventCollaborators] ([Role]);

CREATE INDEX [IX_EventCollaborators_UserId] ON [EventCollaborators] ([UserId]);

CREATE INDEX [IX_EventImages_EventId] ON [EventImages] ([EventId]);

CREATE INDEX [IX_EventRegistrations_EventId] ON [EventRegistrations] ([EventId]);

CREATE UNIQUE INDEX [IX_EventRegistrations_EventId_UserId] ON [EventRegistrations] ([EventId], [UserId]);

CREATE INDEX [IX_EventRegistrations_Status] ON [EventRegistrations] ([Status]);

CREATE INDEX [IX_EventRegistrations_TicketTypeId] ON [EventRegistrations] ([TicketTypeId]);

CREATE INDEX [IX_EventRegistrations_UserId] ON [EventRegistrations] ([UserId]);

CREATE INDEX [IX_EventReviews_EventId] ON [EventReviews] ([EventId]);

CREATE INDEX [IX_EventReviews_IsDeleted] ON [EventReviews] ([IsDeleted]);

CREATE INDEX [IX_EventReviews_ParentId] ON [EventReviews] ([ParentId]);

CREATE INDEX [IX_EventReviews_Rating] ON [EventReviews] ([Rating]);

CREATE INDEX [IX_EventReviews_UserId] ON [EventReviews] ([UserId]);

CREATE INDEX [IX_Events_CategoryId] ON [Events] ([CategoryId]);

CREATE INDEX [IX_Events_IsDeleted] ON [Events] ([IsDeleted]);

CREATE INDEX [IX_Events_LocationId] ON [Events] ([LocationId]);

CREATE INDEX [IX_Events_OrganizerId] ON [Events] ([OrganizerId]);

CREATE INDEX [IX_Events_StartTime] ON [Events] ([StartTime]);

CREATE INDEX [IX_Events_Status] ON [Events] ([Status]);

CREATE INDEX [IX_Locations_Name] ON [Locations] ([Name]);

CREATE UNIQUE INDEX [IX_Roles_RoleName] ON [Roles] ([RoleName]);

CREATE INDEX [IX_TicketTypes_EventId] ON [TicketTypes] ([EventId]);

CREATE UNIQUE INDEX [IX_TicketTypes_EventId_Name] ON [TicketTypes] ([EventId], [Name]);

CREATE INDEX [IX_Transactions_CreatedAt] ON [Transactions] ([CreatedAt]);

CREATE INDEX [IX_Transactions_EventRegistrationId] ON [Transactions] ([EventRegistrationId]);

CREATE INDEX [IX_Transactions_Status] ON [Transactions] ([Status]);

CREATE UNIQUE INDEX [IX_Transactions_TransactionNo] ON [Transactions] ([TransactionNo]) WHERE TransactionNo IS NOT NULL;

CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);

CREATE INDEX [IX_UserRoles_UserId] ON [UserRoles] ([UserId]);

CREATE UNIQUE INDEX [IX_UserRoles_UserId_RoleId] ON [UserRoles] ([UserId], [RoleId]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

CREATE INDEX [IX_Users_IsDeleted] ON [Users] ([IsDeleted]);

CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260116015237_InitSchema', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'ClassName');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Users] DROP COLUMN [ClassName];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'UniversityId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Users] DROP COLUMN [UniversityId];

DROP INDEX [IX_Users_Username] ON [Users];
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Username');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Users] ALTER COLUMN [Username] varchar(50) NOT NULL;
CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'SchoolName');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [Users] ALTER COLUMN [SchoolName] nvarchar(200) NULL;

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'PhoneNumber');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [Users] ALTER COLUMN [PhoneNumber] varchar(20) NULL;

DROP INDEX [IX_Users_Email] ON [Users];
DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Email');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [Users] ALTER COLUMN [Email] varchar(255) NOT NULL;
CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'AvatarUrl');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [Users] ALTER COLUMN [AvatarUrl] varchar(500) NULL;

ALTER TABLE [Users] ADD [RefreshToken] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Users] ADD [RefreshTokenExpiryTime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Users] ADD [StudentId] varchar(50) NULL;

CREATE INDEX [IX_Users_StudentId] ON [Users] ([StudentId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260117135926_AddJWTTokenToUserproperties', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Events] ADD [ClubId] uniqueidentifier NULL;

CREATE TABLE [Clubs] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Code] varchar(50) NULL,
    [Description] nvarchar(2000) NULL,
    [LogoUrl] nvarchar(500) NULL,
    [SocialLink] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Clubs] PRIMARY KEY ([Id])
);

CREATE TABLE [ClubMembers] (
    [Id] uniqueidentifier NOT NULL,
    [ClubId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Role] int NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ClubMembers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClubMembers_Clubs_ClubId] FOREIGN KEY ([ClubId]) REFERENCES [Clubs] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ClubMembers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Events_ClubId] ON [Events] ([ClubId]);

CREATE UNIQUE INDEX [IX_ClubMembers_ClubId_UserId] ON [ClubMembers] ([ClubId], [UserId]);

CREATE INDEX [IX_ClubMembers_UserId] ON [ClubMembers] ([UserId]);

CREATE UNIQUE INDEX [IX_Clubs_Code] ON [Clubs] ([Code]) WHERE [Code] IS NOT NULL;

CREATE INDEX [IX_Clubs_Name] ON [Clubs] ([Name]);

ALTER TABLE [Events] ADD CONSTRAINT [FK_Events_Clubs_ClubId] FOREIGN KEY ([ClubId]) REFERENCES [Clubs] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260206045239_AddClubsEntity', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [UserRoles] DROP CONSTRAINT [FK_UserRoles_Roles_RoleId];

ALTER TABLE [UserRoles] DROP CONSTRAINT [FK_UserRoles_Users_UserId];

ALTER TABLE [UserRoles] ADD CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE;

ALTER TABLE [UserRoles] ADD CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260209011257_UpdateUserRoleConfiguration', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [EventRegistrations] ADD [CancellationReason] nvarchar(500) NULL;

ALTER TABLE [EventRegistrations] ADD [CancelledAt] datetimeoffset NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260217104814_UpdateEventRegistration', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Users] ADD [Major] nvarchar(200) NULL;

CREATE TABLE [SocialLinks] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Platform] int NOT NULL,
    [Url] nvarchar(500) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ModifiedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SocialLinks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SocialLinks_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_SocialLinks_UserId] ON [SocialLinks] ([UserId]);

CREATE INDEX [IX_SocialLinks_UserId_Platform] ON [SocialLinks] ([UserId], [Platform]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260226151401_AddUserMajorAndSocialLinks', N'10.0.3');

COMMIT;
GO

