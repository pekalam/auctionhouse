CREATE FUNCTION [dbo].[Event_CheckEventDate]
(
	@EventDate datetime
)
	RETURNS bit
AS
BEGIN
	DECLARE
		@res bit = 1
	DECLARE
		@now datetime = GETUTCDATE()

	IF (@EventDate > @now OR @EventDate < DATEADD(minute, -5, @now))
		SET @res = 0

	RETURN @res
END
