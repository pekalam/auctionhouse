$ErrorActionPreference = "Stop"
$ver = "$($args[1])"
$backupFolder = "AuctionhouseDatabase\testbackups\$ver"
if ($backupFolder.Length -eq ("AuctionhouseDatabase\testbackups\".Length)){
    throw 'Invalid version arg'
}

if (Test-Path -Path $backupFolder){
    Remove-Item -Path $backupFolder -Recurse
}
mkdir "$backupFolder"

$cid = "$($args[0])"
docker cp "${cid}:/var/opt/mssql/data/AuctionhouseDatabase.mdf" "$backupFolder"
docker cp "${cid}:/var/opt/mssql/data/AuctionhouseDatabase_log.ldf" "$backupFolder"
./Generate-Compose.ps1 "$ver" > "AuctionhouseDatabase\testbackups\$ver\docker-compose.yml"