CREATE TABLE [dbo].[AuctionIdToAuctionBidsId]
(
	[Id] INT PRIMARY KEY IDENTITY, 
    [AuctionId] UNIQUEIDENTIFIER NOT NULL, 
    [AggregateId] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [FK_AuctionIdToAuctionBidsId_ToAggregatesAggregateId] FOREIGN KEY ([AggregateId]) REFERENCES [Aggregate]([AggregateId]) ON DELETE CASCADE,
    CONSTRAINT CHK_AuctionIdAggregateId CHECK (NOT (AuctionId = AggregateId)),
    UNIQUE(AuctionId),
    UNIQUE(AggregateId)
)
