CREATE TABLE dbo.Events
(
    Id        uniqueidentifier PRIMARY KEY NONCLUSTERED,
    AggId     uniqueidentifier NOT NULL,
    AggName   varchar(100)     NOT NULL,
    EventName varchar(100)     NOT NULL,
    Date      datetime2        NOT NULL,
    Data      nvarchar(max)    NOT NULL,
    Version   bigint           NOT NULL,

    CONSTRAINT CHK_EventName CHECK (LEN(EventName) > 0),
    CONSTRAINT CHK_AggName CHECK (LEN(AggName) > 0),
    CONSTRAINT CHK_Date CHECK (dbo.Event_CheckEventDate(Date) = 1)
);
GO
create clustered index Events_AggIdInd on dbo.Events (AggId, Date asc);