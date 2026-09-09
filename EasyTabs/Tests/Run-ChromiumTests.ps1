param([ValidateSet('Debug','Release')][string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$buildPath = & $vswherePath -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/**/Bin/MSBuild.exe' | Select-Object -First 1
if (!$buildPath) { throw 'MSBuild was not found.' }
$solutionRoot = (Resolve-Path (Join-Path $projectRoot '..')).Path + [IO.Path]::DirectorySeparatorChar
& $buildPath (Join-Path $projectRoot 'EasyTabs.csproj') /t:Build "/p:SolutionDir=$solutionRoot" "/p:Configuration=$Configuration" /p:Platform=AnyCPU /nologo /v:minimal
if ($LASTEXITCODE) { throw 'EasyTabs build failed.' }
$compiler = Join-Path (Split-Path $buildPath) 'Roslyn/csc.exe'
$buildDirectory = Join-Path $projectRoot "bin/$Configuration"
$testExe = Join-Path $buildDirectory 'ChromiumTabRendererTests.exe'
$references = @('System.Drawing.dll','System.Windows.Forms.dll',"$buildDirectory/EasyTabs.dll","$buildDirectory/SkiaSharp.dll","$buildDirectory/Win32Interop.User32.dll") | ForEach-Object { '/reference:' + $_ }
& $compiler /nologo /target:exe "/out:$testExe" $references (Join-Path $projectRoot 'ChromiumTabGeometry.cs') (Join-Path $PSScriptRoot 'ChromiumTabRendererTests.cs')
if ($LASTEXITCODE) { throw 'Renderer test compilation failed.' }
& $testExe (Join-Path $buildDirectory 'chromium-test-artifacts')
if ($LASTEXITCODE) { throw 'Renderer regression checks failed.' }
