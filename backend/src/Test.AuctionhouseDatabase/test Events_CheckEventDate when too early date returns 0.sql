CREATE PROCEDURE [test Events_CheckEventDate when too early date returns 0]
AS
BEGIN
	DECLARE @d1 smalldatetime = DATEADD(mi, -10, GETUTCDATE());
	DECLARE @actual bit;

	SELECT @actual = dbo.Event_CheckEventDate(@d1);

	DECLARE @expected bit = 0;

	EXEC tSQLt.AssertEquals @actual, @expected;
END;
