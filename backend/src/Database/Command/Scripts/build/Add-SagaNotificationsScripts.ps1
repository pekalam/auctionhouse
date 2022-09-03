dotnet ef migrations script -p '..\..\..\..\Adapters\ReadModelNotifications\Adapter.EfCore.ReadModelNotifications\Adapter.EfCore.ReadModelNotifications.csproj' --configuration Test -o ..\..\AuctionhouseDatabase\dbo\Scripts\Generated\SagaNotifications.sql

#patch to allow idempotency
$generatedFile = "..\..\AuctionhouseDatabase\dbo\Scripts\Generated\SagaNotifications.sql"
$existingContent = [String]((Get-Content $generatedFile) -join "`n")
$existingContent = $existingContent.Replace('GO', '')

Write-Output "USE AuctionhouseDatabase
GO
IF OBJECT_ID(N'[SagaEventsConfirmations]') IS NULL
BEGIN" > $generatedFile
Write-Output $existingContent >> $generatedFile
Write-Output "
END" >> $generatedFile