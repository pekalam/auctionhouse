USE AuctionhouseDatabase
GO
IF OBJECT_ID(N'[SagaEventsConfirmations]') IS NULL
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


CREATE TABLE [SagaEventsConfirmations] (
    [Id] bigint NOT NULL IDENTITY,
    [CommandId] nvarchar(max) NOT NULL,
    [CorrelationId] nvarchar(max) NOT NULL,
    [Completed] bit NOT NULL,
    [Failed] bit NOT NULL,
    CONSTRAINT [PK_SagaEventsConfirmations] PRIMARY KEY ([Id])
);


CREATE TABLE [SagaEventsToConfirm] (
    [Id] bigint NOT NULL IDENTITY,
    [CorrelationId] nvarchar(max) NOT NULL,
    [EventName] nvarchar(max) NOT NULL,
    [Processed] bit NOT NULL,
    CONSTRAINT [PK_SagaEventsToConfirm] PRIMARY KEY ([Id])
);


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220112170220_m01', N'6.0.1');


COMMIT;



END
