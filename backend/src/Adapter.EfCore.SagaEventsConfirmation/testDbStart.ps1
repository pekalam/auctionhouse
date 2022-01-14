docker run -d -p 1433:1433 marekbf3/auctionhouse-test-readmodelnotifications-db
sleep 10
$cs = "Data Source=127.0.0.1;Initial Catalog=AuctionhouseDatabase;TrustServerCertificate=True;User ID=sa;Password=Qwerty1234"
dotnet ef database update --configuration Test --connection "$cs" m01
cd ..\SetupTestDb
dotnet run