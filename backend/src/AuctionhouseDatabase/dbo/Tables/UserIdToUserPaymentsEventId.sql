CREATE TABLE [dbo].[UserIdToUserPaymentsEventId]
(
	[Id] INT PRIMARY KEY IDENTITY, 
    [UserId] UNIQUEIDENTIFIER NOT NULL, 
    [AggId] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [FK_UserIdToUserPaymentsEventId_ToAggregatesUserId] FOREIGN KEY ([AggId]) REFERENCES [Aggregate]([AggregateId]) ON DELETE CASCADE,
    CONSTRAINT CHK_UserIdAggId CHECK (NOT (UserId = AggId)),
    UNIQUE(UserId, AggId)
)
