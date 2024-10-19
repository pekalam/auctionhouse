$ErrorActionPreference = 'Stop'

$env:ConnectionString = 'Data Source=127.0.0.1;Initial Catalog=AuctionhouseDatabase;MultipleActiveResultSets=True;TrustServerCertificate=True;User ID=sa;Password=Qwerty1234;'
dotnet ef migrations script -p '..\..\..\..\Adapters\EventOutboxStorage\Adapter.SqlServer.EventOutboxStorage\Adapter.SqlServer.EventOutboxStorage.csproj' -i --configuration Test -o ..\..\AuctionhouseDatabase\dbo\Scripts\Generated\EventOutbox.sql

#patch to allow idempotency
$generatedFile = "..\..\AuctionhouseDatabase\dbo\Scripts\Generated\EventOutbox.sql"
$existingContent = [String]((Get-Content $generatedFile) -join "`n")
$existingContent = $existingContent.Replace('GO', '')

Write-Output "IF OBJECT_ID(N'[OutboxItems]') IS NULL
BEGIN" > $generatedFile
Write-Output $existingContent >> $generatedFile
Write-Output "
END" >> $generatedFile