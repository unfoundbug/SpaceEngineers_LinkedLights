$newOutDir = Join-Path $env:APPDATA "\SpaceEngineers\Mods\UnFoundBug.LinkedLights\"
if (Test-Path $newOutDir)
{
    Get-ChildItem "$newOutDir\*" -Recurse | Remove-Item
}
else
{
mkdir "$newOutDir"
}

Get-ChildItem .\* -File | Foreach-Object { Copy-Item -Path "$($_.FullName)" -Destination "$(Join-Path $newOutDir $($_.Name))" }

mkdir "$newOutDir\Data"
mkdir "$newOutDir\Data\Scripts"
mkdir "$newOutDir\Data\Scripts\LinkedLights"
Get-ChildItem .\Data\*.sbc -File | Foreach-Object { Copy-Item -Path "$($_.FullName)" -Destination "$(Join-Path $newOutDir "Data" )"}
mkdir "$newOutDir\Data\Scripts"
mkdir "$newOutDir\Data\Scripts\LinkedLights"
Get-ChildItem .\Data\Scripts\LinkedLights\*.cs -File | Foreach-Object { Copy-Item -Path "$($_.FullName)" -Destination "$(Join-Path $newOutDir "Data\Scripts\LinkedLights\" )"}