$ErrorActionPreference = 'Stop'

& .\BuildCsproj.ps1
& .\Add-EventOutboxAdapterScripts.ps1
& .\Add-SagaNotificationsScripts.ps1
& .\Append-ScriptsToDatabaseProj.ps1
& .\CopyArtifacts.ps1 "..\..\AuctionhouseDatabase.Docker\buildArtifacts"
& .\Build-DockerImage.ps1