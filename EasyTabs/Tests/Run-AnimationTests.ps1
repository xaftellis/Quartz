param(
    [string]$MSBuildPath = '',
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug'
)
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (!$MSBuildPath) {
    $vswhere = Join-Path ([Environment]::GetFolderPath('ProgramFilesX86')) 'Microsoft Visual Studio\Installer\vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (!$MSBuildPath) { throw 'MSBuild not found. Supply -MSBuildPath.' }
& $MSBuildPath (Join-Path $projectRoot 'EasyTabs.csproj') /t:Build "/p:Configuration=$Configuration" /p:Platform=AnyCPU /nologo /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'EasyTabs build failed.' }
$compiler = Join-Path (Split-Path $MSBuildPath) 'Roslyn\csc.exe'
$buildDirectory = Join-Path $projectRoot "bin\$Configuration"
$testExe = Join-Path $buildDirectory 'TabAnimationTests.exe'
& $compiler /nologo /target:exe "/out:$testExe" /reference:System.Drawing.dll /reference:System.Windows.Forms.dll `
    "/reference:$buildDirectory\EasyTabs.dll" "/reference:$buildDirectory\Win32Interop.User32.dll" `
    (Join-Path $projectRoot 'TabLayoutAnimation.cs') (Join-Path $PSScriptRoot 'TabAnimationTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Animation test compilation failed.' }
& $testExe (Join-Path $buildDirectory 'animation-test-artifacts')
if ($LASTEXITCODE -ne 0) { throw 'Animation regression tests failed.' }
