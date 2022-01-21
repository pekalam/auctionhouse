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

    $itemGroup = $csproj.SelectSingleNode("//msb:ItemGroup/msb:None/parent::msb:ItemGroup", $ns)

    $scriptFilesArr = [System.Collections.ArrayList]$scriptFiles
    foreach ($existingNone in $itemGroup.SelectNodes("//msb:None", $ns))
    {
        $includeText = $existingNone.Attributes["Include"].Value

        if((!(Test-Path -Path ("$projDir\"+$includeText) -PathType Leaf)) -or (!($includeText.Contains("Generated")))){
            $itemGroup.RemoveChild($existingNone)
            continue  
        }

        if($scriptFiles.Contains($includeText)) {
            $scriptFilesArr.Remove($includeText)
        }
    }

    foreach ($file in $scriptFilesArr)
    {
        $none = $csproj.CreateElement("None", "http://schemas.microsoft.com/developer/msbuild/2003")
        $none.Attributes.RemoveAll()
        $includeAttr = $csproj.CreateAttribute("Include")
        $includeAttr.Value = "$file"
        $none.Attributes.Append($includeAttr)
        $itemGroup.AppendChild($none)
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
    $postDeployScriptContent1 = ":r .\Job_ResetPasswordCode.sql"
    Write-Output $postDeployScriptContent1 > "$PSScriptRoot\AuctionhouseDatabase\dbo\Scripts\Script.PostDeployment.sql"

    foreach($file in $scripts)
    {
        $pathParts = ([System.String]$file).Split('\')
        Write-Output (":r .\" + $pathParts[2] + '\' + $pathParts[3]) >> "$PSScriptRoot\AuctionhouseDatabase\dbo\Scripts\Script.PostDeployment.sql"
    }

    $postDeployScriptContent2 = "ALTER DATABASE AuctionhouseDatabase
    SET READ_COMMITTED_SNAPSHOT ON
    GO
    ALTER DATABASE AuctionhouseDatabase
    SET ALLOW_SNAPSHOT_ISOLATION ON
    GO"
    Write-Output $postDeployScriptContent2 >> "$PSScriptRoot\AuctionhouseDatabase\dbo\Scripts\Script.PostDeployment.sql"
}


$scripts = [String[]](Get-ChildItem .\AuctionhouseDatabase\dbo\Scripts\Generated | Select-Object @{name="Name"; expression={"dbo\Scripts\Generated\"+$_.Name}} | Select-Object -ExpandProperty Name)
$l = $scripts.Length
Write-Host "Found $l scripts"

if($scripts.Length -eq 0){
    return
}


$srcFile = "$PSScriptRoot\AuctionhouseDatabase\AuctionhouseDatabase.sqlproj"
$resultFile = "$PSScriptRoot\AuctionhouseDatabase\AuctionhouseDatabase.sqlproj2"
$projDir = "$PSScriptRoot\AuctionhouseDatabase"
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
$postDeploymentScript = "$PSScriptRoot\AuctionhouseDatabase\dbo\Scripts\Script.PostDeployment.sql"
Add-PostDeploymentScript $postDeploymentScript