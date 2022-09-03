dotnet ef migrations script -p '..\..\..\..\Adapters\EventOutboxStorage\Adapter.SqlServer.EventOutboxStorage\Adapter.SqlServer.EventOutboxStorage.csproj' --configuration Test -o ..\..\AuctionhouseDatabase\dbo\Scripts\Generated\EventOutbox.sql

#patch to allow idempotency
$generatedFile = "..\..\AuctionhouseDatabase\dbo\Scripts\Generated\EventOutbox.sql"
$existingContent = [String]((Get-Content $generatedFile) -join "`n")
$existingContent = $existingContent.Replace('GO', '')

Write-Output "USE AuctionhouseDatabase
GO
IF OBJECT_ID(N'[OutboxItems]') IS NULL
BEGIN" > $generatedFile
Write-Output $existingContent >> $generatedFile
Write-Output "
END" >> $generatedFile