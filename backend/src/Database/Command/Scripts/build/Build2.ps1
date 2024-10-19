$ErrorActionPreference = 'Stop'

& .\BuildCsproj.ps1
& .\Add-EventOutboxAdapterScripts.ps1
& .\Add-SagaNotificationsScripts.ps1
& .\Append-ScriptsToDatabaseProj.ps1