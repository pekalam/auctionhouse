$ErrorActionPreference = 'Stop'

xcopy '..\..\AuctionhouseDatabase\bin\Debug\*' "$($args[0])" /i /y