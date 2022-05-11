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
Copy-Item ".\Data\" "$newOutDir\Data\" -Recurse -Include "*.sbc"
Copy-Item ".\Data\" "$newOutDir\Data\" -Recurse -Include "*.cs"