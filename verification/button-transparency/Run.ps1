$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$visualStudio = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -property installationPath
if (-not $visualStudio) { throw 'Visual Studio MSBuild was not found.' }
$msbuild = Join-Path $visualStudio 'MSBuild\Current\Bin\MSBuild.exe'
$compiler = Join-Path $visualStudio 'MSBuild\Current\Bin\Roslyn\csc.exe'
$outputDirectory = Join-Path $env:TEMP 'Quartz-Transparency-build'
& $msbuild (Join-Path $repoRoot 'Quartz\Quartz.csproj') /t:Build /p:Configuration=Debug /p:Platform=AnyCPU "/p:OutDir=$outputDirectory\" /v:quiet /nologo
if ($LASTEXITCODE -ne 0) { throw 'Quartz build failed.' }
$executable = Join-Path $outputDirectory 'TransparencyChecks.exe'
& $compiler /nologo /target:exe /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll "/r:$outputDirectory\Quartz.exe" "/win32manifest:$repoRoot\Quartz\app.manifest" "/out:$executable" (Join-Path $PSScriptRoot 'Checks.cs')
if ($LASTEXITCODE -ne 0) { throw 'Transparency check compilation failed.' }
Copy-Item -LiteralPath (Join-Path $outputDirectory 'Quartz.exe.config') -Destination "$executable.config" -Force
& $executable
if ($LASTEXITCODE -ne 0) { throw 'Transparency checks failed.' }
