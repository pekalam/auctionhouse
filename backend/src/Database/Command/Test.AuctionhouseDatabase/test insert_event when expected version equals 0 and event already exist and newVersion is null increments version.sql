CREATE PROCEDURE [dbo].[test insert_event when expected version equals existing inserts new event with incremented version of existing]
AS
BEGIN
    declare @count int;
    declare @expectedCount int = 2;
    declare @version bigint;
    declare @expectedVersion bigint = 2;

    EXEC tSQLt.FakeTable 'dbo.Event';
    EXEC tSQLt.FakeTable 'dbo.Aggregate';

    declare @aggId char(36) = N'10A2C48F-BABC-412C-B9BD-6CAA2B4C36BA';
    insert into Aggregate(AggregateId, AggregateName, Version) VALUES (@aggId, 'test', 1);
    insert into Event(AggId, EventName, Date, Data, Version) values (@aggId, 'event 1', GETDATE(), '{"a": 1}', 1);

    declare @now datetime2 = GETDATE();
    execute dbo.insert_event @AggId = @aggId, @EventName = 'event 1', @Data = '{"a": 1}', @ExpectedVersion = 1

    select TOP 1 @version = e.Version from Event e order by e.Version desc;
    select @count = count(*) from Event e;

    EXEC tSQLt.AssertEquals @expectedVersion, @version;
    EXEC tSQLt.AssertEquals @expectedCount, @count;
END;
