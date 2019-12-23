CREATE PROCEDURE [dbo].[drop_events]
	@AggId char(36)
AS
begin
	delete from Events where AggId = @AggId
end;
