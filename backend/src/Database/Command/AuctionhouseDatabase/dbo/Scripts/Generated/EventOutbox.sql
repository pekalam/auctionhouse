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


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220113231236_Initial')
BEGIN
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
END;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220113231236_Initial')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220113231236_Initial', N'6.0.1');
END;


COMMIT;


BEGIN TRANSACTION;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220811232430_01_Add_ExtraData')
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxItems]') AND [c].[name] = N'CommandContext_CommandId_Id');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [OutboxItems] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [OutboxItems] DROP COLUMN [CommandContext_CommandId_Id];
END;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220811232430_01_Add_ExtraData')
BEGIN
    EXEC sp_rename N'[OutboxItems].[CommandContext_CorrelationId_Value]', N'CommandContext_ExtraData', N'COLUMN';
END;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220811232430_01_Add_ExtraData')
BEGIN
    ALTER TABLE [OutboxItems] ADD [CommandContext_CommandId] nvarchar(max) NOT NULL DEFAULT N'';
END;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220811232430_01_Add_ExtraData')
BEGIN
    ALTER TABLE [OutboxItems] ADD [CommandContext_CorrelationId] nvarchar(max) NOT NULL DEFAULT N'';
END;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220811232430_01_Add_ExtraData')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220811232430_01_Add_ExtraData', N'6.0.1');
END;


COMMIT;


BEGIN TRANSACTION;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220822035544_02_Null_ExtraData')
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxItems]') AND [c].[name] = N'CommandContext_ExtraData');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [OutboxItems] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [OutboxItems] ALTER COLUMN [CommandContext_ExtraData] nvarchar(max) NULL;
END;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220822035544_02_Null_ExtraData')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220822035544_02_Null_ExtraData', N'6.0.1');
END;


COMMIT;


BEGIN TRANSACTION;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220903152613_03_Rm_ReadModelNotifications')
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxItems]') AND [c].[name] = N'ReadModelNotifications');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [OutboxItems] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [OutboxItems] DROP COLUMN [ReadModelNotifications];
END;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220903152613_03_Rm_ReadModelNotifications')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220903152613_03_Rm_ReadModelNotifications', N'6.0.1');
END;


COMMIT;


BEGIN TRANSACTION;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230205094717_04_Rm_QueuedCommands')
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxItems]') AND [c].[name] = N'CommandContext_HttpQueued');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [OutboxItems] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [OutboxItems] DROP COLUMN [CommandContext_HttpQueued];
END;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230205094717_04_Rm_QueuedCommands')
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxItems]') AND [c].[name] = N'CommandContext_WSQueued');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [OutboxItems] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [OutboxItems] DROP COLUMN [CommandContext_WSQueued];
END;


IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230205094717_04_Rm_QueuedCommands')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230205094717_04_Rm_QueuedCommands', N'6.0.1');
END;


COMMIT;



END
