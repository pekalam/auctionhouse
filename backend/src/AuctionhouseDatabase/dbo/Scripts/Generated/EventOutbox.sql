USE AuctionhouseDatabase
GO
IF OBJECT_ID(N'[OutboxItems]') IS NULL
BEGIN
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;


BEGIN TRANSACTION;


CREATE TABLE [OutboxItems] (
    [Id] bigint NOT NULL IDENTITY,
    [Event] nvarchar(max) NOT NULL,
    [CommandContext_CommandId_Id] nvarchar(max) NULL,
    [CommandContext_CorrelationId_Value] nvarchar(max) NOT NULL,
    [CommandContext_User] uniqueidentifier NULL,
    [CommandContext_HttpQueued] bit NOT NULL,
    [CommandContext_WSQueued] bit NOT NULL,
    [CommandContext_Name] nvarchar(max) NOT NULL,
    [ReadModelNotifications] int NOT NULL,
    [Timestamp] bigint NOT NULL,
    [Processed] bit NOT NULL,
    CONSTRAINT [PK_OutboxItems] PRIMARY KEY ([Id])
);


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220113231236_Initial', N'6.0.1');


COMMIT;



END
