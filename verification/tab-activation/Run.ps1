$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$visualStudio = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -property installationPath
if (-not $visualStudio) { throw 'Visual Studio MSBuild was not found.' }
$compiler = Join-Path $visualStudio 'MSBuild\Current\Bin\Roslyn\csc.exe'
$outputDirectory = Join-Path $PSScriptRoot 'build'
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$executable = Join-Path $outputDirectory 'TabOpeningChecks.exe'
& $compiler /nologo /target:exe /r:System.dll /r:System.Core.dll /r:System.Windows.Forms.dll "/out:$executable" (Join-Path $repoRoot 'Quartz\TabOpenDisposition.cs') (Join-Path $repoRoot 'Quartz\TabOpenGesture.cs') (Join-Path $PSScriptRoot 'TabOpeningChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'Tab-opening check compilation failed.' }
& $executable
if ($LASTEXITCODE -ne 0) { throw 'Tab-opening checks failed.' }
