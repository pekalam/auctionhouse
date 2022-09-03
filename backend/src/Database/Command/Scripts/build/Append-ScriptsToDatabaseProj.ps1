function Add-ScriptsToProj {
    param(
        [Parameter(Mandatory,Position=0)]
        [String[]]$scriptFiles,
        [Parameter(Mandatory,Position=1)]
        [String]$srcFile,
        [Parameter(Mandatory,Position=2)]
        [String]$resultFile,
        [Parameter(Mandatory,Position=3)]
        [String]$projDir
    )
    [xml]$csproj = Get-Content "$srcFile"
    $ns = new-object Xml.XmlNamespaceManager $csproj.NameTable
    $ns.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003")

    $scriptFilesArr = [System.Collections.ArrayList]$scriptFiles

    $existingItemGroups = $csproj.SelectNodes("//msb:ItemGroup/msb:None/parent::msb:ItemGroup", $ns)

    foreach($existingItemGroup in $existingItemGroups)
    {
        foreach ($existingNone in $existingItemGroup.SelectNodes("//msb:None", $ns)) 
        { 
            $includeText = $existingNone.Attributes["Include"].Value

            if($scriptFiles.Contains($includeText)) { 
                $scriptFilesArr.Remove($includeText) 
            } 
        }
    }

    if($scriptFilesArr.Length -gt 0)
    {
        $rootProj = $csproj.SelectSingleNode("//msb:Project", $ns)
        $itemGroup = $csproj.CreateElement("ItemGroup", "http://schemas.microsoft.com/developer/msbuild/2003")
        $rootProj.AppendChild($itemGroup)

        foreach ($file in $scriptFilesArr)
        {
            $none = $csproj.CreateElement("None", "http://schemas.microsoft.com/developer/msbuild/2003")
            $none.Attributes.RemoveAll()
            $includeAttr = $csproj.CreateAttribute("Include")
            $includeAttr.Value = "$file"
            $none.Attributes.Append($includeAttr)
            $itemGroup.AppendChild($none)
        }
    }


    $xmlsettings = New-Object System.Xml.XmlWriterSettings
    $xmlsettings.Indent = $true
    $xmlsettings.IndentChars = " "
    $xmlWriter = [System.XML.XmlWriter]::Create($resultFile, $xmlsettings)

    $csproj.WriteTo($xmlWriter)
    $xmlWriter.Flush()
    $xmlWriter.Dispose()
}

function Add-PostDeploymentScript {
    param (
        [Parameter(Mandatory,Position=0)]
        [String]$srcFile
    )
    $postDeployScriptContent1 = "--:r .\Job_ResetPasswordCode.sql"
    Write-Output $postDeployScriptContent1 > (Resolve-Path "..\..\AuctionhouseDatabase\dbo\Scripts\Script.PostDeployment.sql")

    foreach($file in $scripts)
    {
        $pathParts = ([System.String]$file).Split('\')
        Write-Output (":r .\" + $pathParts[2] + '\' + $pathParts[3]) >> (Resolve-Path "..\..\AuctionhouseDatabase\dbo\Scripts\Script.PostDeployment.sql")
    }

    $postDeployScriptContent2 = "ALTER DATABASE AuctionhouseDatabase
    SET READ_COMMITTED_SNAPSHOT ON
    GO
    ALTER DATABASE AuctionhouseDatabase
    SET ALLOW_SNAPSHOT_ISOLATION ON
    GO"
    Write-Output $postDeployScriptContent2 >> (Resolve-Path "..\..\AuctionhouseDatabase\dbo\Scripts\Script.PostDeployment.sql")
}


$generatedPath = (Resolve-Path "..\..\AuctionhouseDatabase\dbo\Scripts\Generated")
$scripts = [String[]](Get-ChildItem $generatedPath | Select-Object @{name="Name"; expression={"dbo\Scripts\Generated\"+$_.Name}} | Select-Object -ExpandProperty Name)
$l = $scripts.Length
Write-Host "Found $l scripts"

if($scripts.Length -eq 0){
    return
}


$projDir = (Resolve-Path "..\..\AuctionhouseDatabase")
$srcFile = (Join-Path "$projDir" "AuctionhouseDatabase.sqlproj")
$resultFile = (Join-Path "$projDir" "AuctionhouseDatabase.sqlproj2")
Write-Host "Adding scripts to $projDir"
Add-ScriptsToProj $scripts $srcFile $resultFile $projDir

if(!($?)){
    throw "sqlproj file processing error"
}

# replace proj files
Write-Host "Replacing original file with generated"
Remove-Item "$srcFile"
Rename-Item "$resultFile" "$srcFile"

# add generated scripts to post deployment script
Write-Host "Generating post deployment script"
$postDeploymentScript = (Resolve-Path "..\..\AuctionhouseDatabase\dbo\Scripts\Script.PostDeployment.sql")
Add-PostDeploymentScript $postDeploymentScript