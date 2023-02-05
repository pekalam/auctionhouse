$ErrorActionPreference = 'Stop'

& "${Env:MSBuildBinPath}MSBuild.exe" ..\..\AuctionhouseDatabase\AuctionhouseDatabase.sqlproj

& .\Add-EventOutboxAdapterScripts.ps1
& .\Add-SagaNotificationsScripts.ps1
& .\Append-ScriptsToDatabaseProj.ps1
& .\Build-DockerImage.ps1