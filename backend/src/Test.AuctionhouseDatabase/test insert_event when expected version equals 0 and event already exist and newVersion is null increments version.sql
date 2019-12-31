CREATE PROCEDURE [dbo].[test insert_event when expected version equals 0 and event already exist and newVersion is null increments version]
AS
BEGIN
    declare @count int;
    declare @expectedCount int = 2;
    declare @version bigint;
    declare @expectedVersion bigint = 2;

    EXEC tSQLt.FakeTable 'dbo.Events';

    declare @aggId char(36) = N'10A2C48F-BABC-412C-B9BD-6CAA2B4C36BA';
    insert into Events(Id, AggId, AggName, EventName, Date, Data, Version) values (NEWID(), @aggId, 'agg1', 'event 1', GETDATE(), '{"a": 1}', 1);

    declare @now datetime2 = GETDATE();
    execute dbo.insert_event @AggId = @aggId, @AggName = 'agg1', @EventName = 'event 1', @Date = @now, @Data = '{"a": 1}', @ExpectedVersion = 0, @NewVersion = NULL

    select TOP 1 @version = e.Version from Events e order by e.Version desc;
    select @count = count(*) from Events e;

    EXEC tSQLt.AssertEquals @expectedVersion, @version;
    EXEC tSQLt.AssertEquals @expectedCount, @count;
END;
