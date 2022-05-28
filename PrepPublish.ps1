param([string] $buildType)
Write-Host "Cleaning existing item"
$newOutDir = Join-Path $env:APPDATA "\SpaceEngineers\Mods\UnFoundBug.LinkedLights\"
Remove-Item -LiteralPath "$newOutDir" -Recurse -Force

$nDir = mkdir "$newOutDir"

Write-Host "Copying Bulk"
Get-ChildItem .\* -File | Foreach-Object { Copy-Item -Path "$($_.FullName)" -Destination "$(Join-Path $newOutDir $($_.Name))" }

Write-Host "Copying Scripts"

$nDir = mkdir "$newOutDir\Data"
$nDir = mkdir "$newOutDir\Data\Scripts"
$nDir = mkdir "$newOutDir\Data\Scripts\LinkedLights"
Get-ChildItem .\Data\*.sbc -File | Foreach-Object { Copy-Item -Path "$($_.FullName)" -Destination "$(Join-Path $newOutDir "Data" )"}
Get-ChildItem .\Data\Scripts\LinkedLights\*.cs -File | Foreach-Object { Copy-Item -Path "$($_.FullName)" -Destination "$(Join-Path $newOutDir "Data\Scripts\LinkedLights\" )"}