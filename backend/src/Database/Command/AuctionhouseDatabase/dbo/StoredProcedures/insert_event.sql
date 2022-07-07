CREATE PROCEDURE [dbo].[insert_event]
	@AggId char(36),
	@EventName varchar(100),
	@Data nvarchar(max),
	@ExpectedVersion bigint
AS
BEGIN
	declare
		@errmsg char(100);	
	
	update [dbo].[Aggregate] set Version = (@ExpectedVersion + 1) where AggregateId = @AggId and  Version = @ExpectedVersion

	IF NOT @@ROWCOUNT = 1
	BEGIN
		set @errmsg = FORMATMESSAGE('Optimistic concurrency exception - aggregate with version %I64d doesn''t exist', @ExpectedVersion);
		throw 51000, @errmsg, 0;
	END

	insert into [dbo].[Event] (AggId, EventName, Date, Data, Version) values (@AggId, @EventName, GETUTCDATE(), @Data, (@ExpectedVersion + 1))
END