-- creates row in Aggregate table and inserts aggregate event
CREATE PROCEDURE [dbo].[add_aggregate]
	@AggId char(36),
	@AggregateName varchar(100),
	@EventName varchar(100),
	@Data nvarchar(max)
AS
BEGIN
	--assuming agg version with only one event has version 1
	INSERT INTO Aggregate (AggregateId, AggregateName, Version) VALUES (@AggId, @AggregateName, 1); 
	INSERT INTO Event (AggId, EventName, Date, Data, Version) VALUES (@AggId, @EventName, GETUTCDATE(), @Data, 1);
END
