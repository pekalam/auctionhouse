$ErrorActionPreference = "Stop"
$backupFolder = "AuctionhouseDatabase\testbackups\$($args[1])"
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
