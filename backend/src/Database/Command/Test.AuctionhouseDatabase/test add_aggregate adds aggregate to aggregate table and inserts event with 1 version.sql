CREATE PROCEDURE [dbo].[test add_aggregate adds aggregate to aggregate table and inserts event with 1 version]
AS
BEGIN
    declare @count int;
    declare @expectedCount int = 1;
    declare @version bigint;
    declare @expectedVersion bigint = 1;
    declare @aggVersion int = 1;
    declare @aggCount int = 1;

    EXEC tSQLt.FakeTable 'dbo.Event';
    EXEC tSQLt.FakeTable 'dbo.Aggregate';
    declare @aggId char(36) = N'10A2C48F-BABC-412C-B9BD-6CAA2B4C36BA';

    execute dbo.add_aggregate @AggId = @aggId, @AggregateName = 'test', @EventName = 'test_event', @Data = '{"a": 1}'

    select TOP 1 @version = e.Version from Event e order by e.Version desc;
    select @count = count(*) from Event e;
    EXEC tSQLt.AssertEquals @expectedVersion, @version;
    EXEC tSQLt.AssertEquals @expectedCount, @count;

    select TOP 1 @aggVersion = a.Version from Aggregate a;
    select @aggCount = count(*) from Aggregate;
    EXEC tSQLt.AssertEquals @expectedVersion, @aggVersion;
    EXEC tSQLt.AssertEquals @expectedCount, @aggCount;
END
