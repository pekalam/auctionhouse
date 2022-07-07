$backupFolder = "AuctionhouseDatabase\testbackups\$($args[0])"

if(!(Test-Path -Path "$backupFolder")){
	throw "Cannot find $backupFolder"
}

Write-Host "Creating image for $($args[0]) db version"
Write-Host "db content description:"
Get-Content "$backupFolder\description.txt"
Write-Host "`r`n"

xcopy 'AuctionhouseDatabase\bin\Debug\*' AuctionhouseDatabase.Docker\buildArtifacts /i /y
$backupDest = "AuctionhouseDatabase.Docker\testbackups\$($args[0])"
xcopy "$backupFolder\*" "$backupDest\" /i /y


docker build --target frombackup --build-arg BACKUP_LOCATION="testbackups/$($args[0])/" -t pekalam/auctionhouse-sqlserver-backup:"$($args[0])" .\AuctionhouseDatabase.Docker

Remove-Item -Recurse .\AuctionhouseDatabase.Docker\buildArtifacts
Remove-Item -Recurse "$backupDest"
