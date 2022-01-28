CREATE TABLE dbo.Event
(
    Id        BIGINT PRIMARY KEY NONCLUSTERED IDENTITY,
    AggId     UNIQUEIDENTIFIER NOT NULL, --TODO rename aggid to aggregateid
    EventName VARCHAR(100)     NOT NULL,
    Date      DATETIME2        NOT NULL,
    Data      NVARCHAR(MAX)    NOT NULL,
    Version   BIGINT           NOT NULL,

    CONSTRAINT CHK_EventName CHECK (LEN(EventName) > 0),
    CONSTRAINT CHK_Data CHECK (LEN(Data) > 0),
    CONSTRAINT [FK_Events_ToAggregates] FOREIGN KEY ([AggId]) REFERENCES [Aggregate]([AggregateId]) ON DELETE CASCADE,
    UNIQUE CLUSTERED(AggId, Version) --there can be only one event with some aggregate id and particular version
    -- possible optimizaiton - use sequantial aggid
);
GO
