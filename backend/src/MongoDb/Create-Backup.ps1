$ErrorActionPreference = "Stop"

$cid = "$($args[0])"
$backupFolder = "testbackups\$($args[1])"

if ($args.Count -eq 1){
    throw 'Invalid version arg'
}

if (Test-Path -Path $backupFolder){
    echo "removing $backupFolder"
    Remove-Item -Path $backupFolder -Recurse
}

mkdir "$backupFolder"

docker exec "$cid" mongodump
docker exec "$cid" tar -czvf dump.tar.gz ./dump
docker cp "${cid}:/dump.tar.gz" "$backupFolder"