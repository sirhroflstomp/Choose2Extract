# Builds the plugin and packs it into a zip laid out for the SPT root folder.
# Usage: .\package.ps1 [-GameDir "C:\path\to\SPT"]  (not needed if GameDir.user.props exists)
param([string]$GameDir)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$name = 'sirhroflstomp-Choose2Extract'

# The csproj and the [BepInPlugin] attribute must carry the same version
$csprojVersion = ([xml](Get-Content Choose2Extract.csproj)).Project.PropertyGroup.Version |
    Where-Object { $_ } | Select-Object -First 1
$pluginVersion = [regex]::Match((Get-Content Plugin.cs -Raw), 'BepInPlugin\([^,]+,[^,]+,\s*"([^"]+)"').Groups[1].Value
if ($csprojVersion -ne $pluginVersion) {
    throw "Version mismatch: Choose2Extract.csproj has $csprojVersion, Plugin.cs has $pluginVersion"
}

if ($GameDir) {
    dotnet build -c Release "-p:GameDir=$GameDir"
} else {
    dotnet build -c Release
}
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }

$stage = Join-Path $PSScriptRoot 'dist\stage'
$modDir = Join-Path $stage "BepInEx\plugins\$name"
Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $modDir -Force | Out-Null

Copy-Item "bin\Release\net472\$name.dll" $modDir
Copy-Item LICENSE $modDir

$archive = Join-Path $PSScriptRoot "dist\$name-$csprojVersion.zip"
Remove-Item $archive -ErrorAction SilentlyContinue
tar -a -c -f $archive -C $stage BepInEx

Write-Host "Created $archive"
