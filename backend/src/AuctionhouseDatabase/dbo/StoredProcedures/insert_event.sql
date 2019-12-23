CREATE PROCEDURE [dbo].[insert_event]
@AggId char(36),
@AggName varchar(100),
@EventName varchar(100),
@Date datetime2,
@Data nvarchar(max),
@ExpectedVersion bigint,
@NewVersion bigint
as
begin
	declare
		@versionToInsert bigint;
	declare
		@currentVer bigint;
	declare
		@errmsg char(100);


	if @ExpectedVersion >= @NewVersion
		begin
			set @errmsg = FORMATMESSAGE('Cannot insert event with version %I64d', @NewVersion);
			throw 51000, @errmsg, 0;
		end
	if @ExpectedVersion < 0
		begin
			if @NewVersion is null
				set @versionToInsert = 1;
			else
				set @versionToInsert = @NewVersion;
		end
	else if @ExpectedVersion > 0
		begin
			set @currentVer = (select TOP 1 e.Version FROM Events e where e.AggId = @AggId ORDER BY e.Version desc);
			if @currentVer = @ExpectedVersion
				begin
					if @NewVersion is null
						set @versionToInsert = @currentVer + 1;
					else
						if @NewVersion > @currentVer
							set @versionToInsert = @NewVersion;
				end
			else
				begin
					set @errmsg = FORMATMESSAGE('Cannot find event with expected version %I64d', @ExpectedVersion);
					throw 51000, @errmsg, 0;
				end
		end
	else
		begin
			set @currentVer = (select TOP 1 e.Version FROM Events e where e.AggId = @AggId ORDER BY e.Version desc);
			if @currentVer is null
				set @versionToInsert = 1;
			else
				if @NewVersion is null
					set @versionToInsert = @currentVer + 1;
				else
					if @NewVersion > @currentVer
						set @versionToInsert = @NewVersion;
		end

	insert into Events
		(Id, AggId, AggName, EventName, Date, Data, Version)
	values (NEWID(), @AggId, @AggName, @EventName, @Date, @Data, @versionToInsert);
end;
