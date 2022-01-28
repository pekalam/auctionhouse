$backupFolder = "testbackups/$($args[0])/"

if(!(Test-Path -Path ".\AuctionhouseDatabase\$backupFolder")){
	throw "Cannot find $backupFolder"
}

Write-Host "Creating image for $($args[0]) db version"
Write-Host "db content description:"
Get-Content ".\AuctionhouseDatabase\$backupFolder\description.txt"
Write-Host "`r`n"

docker build -f .\AuctionhouseDatabase\Dockerfile-frombackup --build-arg BACKUP_LOCATION="$backupFolder" -t marekbf3/auctionhouse-sqlserver-backup:"$($args[0])" .\AuctionhouseDatabase