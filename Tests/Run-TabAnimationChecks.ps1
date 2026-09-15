param([string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$output = Join-Path $repo 'bin\verification\tab-animation\bin'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -products '*' -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (!$msbuild) { throw 'MSBuild was not found.' }
& $msbuild (Join-Path $repo 'Quartz.sln') /m /nologo /v:minimal "/p:Configuration=$Configuration" '/p:Platform=Any CPU' "/p:OutDir=$output\"
if ($LASTEXITCODE) { throw 'Quartz build failed.' }
$compiler = Join-Path (Split-Path $msbuild -Parent) 'Roslyn\csc.exe'
$exe = Join-Path $output 'TabAnimationChecks.exe'
& $compiler /nologo /target:exe /platform:anycpu "/out:$exe" "/r:$output\EasyTabs.dll" /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'TabAnimationChecks.cs')
if ($LASTEXITCODE) { throw 'Animation check compilation failed.' }
Copy-Item -LiteralPath (Join-Path $output 'Quartz.exe.config') -Destination "$exe.config"
& $exe
if ($LASTEXITCODE) { throw 'Animation checks failed.' }
