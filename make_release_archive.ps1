Set-Location -Path $PSScriptRoot

MSBuild.exe -target:Build -property:Configuration=Release

$Name = "KSPN.GameDataDisabler"
$Artifacts = ".\artifacts\Release"
$Archives = ".\archives"
$Working = "$Archives\working"
$GameData = "$Working\GameData"
$Destination = "$GameData\$Name"
$Version = (Get-Item -Path $Artifacts\$Name.dll).VersionInfo.ProductVersion

if (-Not (Test-Path -Path $Archives)) {
    New-Item -Name $Archives -ItemType Directory
}
if (Test-Path -Path $Working) {
    Remove-Item -Recurse -Force -Path $Working
}

New-Item -Name $Destination -ItemType Directory
Copy-Item `
    -Path .\README.md,
        .\LICENSE.txt,
        $Artifacts\$Name.dll,
        $Artifacts\Microsoft.Extensions.FileSystemGlobbing.dll `
    -Destination $Destination
dotnet-thirdpartynotices.exe --output-filename $Destination\THIRD-PARTY-NOTICES.txt

if (Test-Path -Path $Archives\$Name-$Version.zip) {
    Remove-Item -Path $Archives\$Name-$Version.zip
}
Compress-Archive -Path $GameData -DestinationPath $Archives\$Name-$Version.zip
