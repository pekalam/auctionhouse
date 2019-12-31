CREATE PROCEDURE [dbo].[test insert_event when expected version lower than 0 and events with given aggId doesnt exists inserts events]
AS
BEGIN
    declare @count int;
    declare @expectedCount int = 1;
    declare @insertedAggId varchar(100);
    declare @version bigint;
    declare @expectedVersion bigint = 1;

    EXEC tSQLt.FakeTable @TableName = 'dbo.Events';

    declare @aggId char(36) = N'10A2C48F-BABC-412C-B9BD-6CAA2B4C36BA';
    declare @now datetime2 = GETDATE();
    execute AuctionhouseDatabase.dbo.insert_event @AggId = @aggId, @AggName = 'agg1', @EventName = 'event 1', @Date = @now, @Data = '{"a": 1}', @ExpectedVersion = -1, @NewVersion = NULL

    select @count = count(*) from Events e;
    select TOP 1 @version = e.Version, @insertedAggId = e.AggId from Events e;

    EXEC tSQLt.AssertEquals @expectedVersion, @version;
    EXEC tSQLt.AssertEquals @aggId, @insertedAggId;
    EXEC tSQLt.AssertEquals @expectedCount, @count;
END;
