CREATE TABLE [dbo].[ResetPasswordCode]
(
	[Id] BIGINT PRIMARY KEY IDENTITY,
	[ResetCode] CHAR(6) NOT NULL DEFAULT NEXT VALUE FOR [dbo].[ResetCodeSeq], 
	[Checked] BIT NOT NULL, 
	[DateCreated] DATETIME2 NOT NULL, 
	[Email] VARCHAR(320) NOT NULL
)
