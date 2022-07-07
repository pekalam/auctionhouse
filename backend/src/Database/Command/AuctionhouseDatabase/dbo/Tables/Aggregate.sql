CREATE TABLE [dbo].[Aggregate]
(
	[Id] INT PRIMARY KEY IDENTITY,
	[AggregateId] UNIQUEIDENTIFIER NOT NULL,
	[AggregateName] VARCHAR(100) NOT NULL,
	[Version] BIGINT NOT NULL
	UNIQUE(AggregateId)
)
