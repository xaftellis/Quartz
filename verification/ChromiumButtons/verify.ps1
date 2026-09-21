param([switch]$Benchmark)

$ErrorActionPreference = 'Stop'
$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$output = Join-Path $PSScriptRoot 'bin\updated'
$msbuild = 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe'
$compiler = Join-Path (Split-Path $msbuild) 'Roslyn\csc.exe'
Push-Location $repo
try {
    & $msbuild Quartz.sln /t:Build /p:Configuration=Debug "/p:OutDir=$output\" /p:UseSharedCompilation=true /v:minimal /nologo
    if ($LASTEXITCODE) { throw 'Solution build failed.' }
    & $compiler /nologo /target:exe "/out:$output\Quartz.ButtonTests.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll "/r:$output\Quartz.exe" "$PSScriptRoot\Verify.cs"
    if ($LASTEXITCODE) { throw 'Verification compilation failed.' }
    Copy-Item -LiteralPath "$output\Quartz.exe.config" -Destination "$output\Quartz.ButtonTests.exe.config"
    & "$output\Quartz.ButtonTests.exe" "$output\rendered-buttons.png"
    if ($LASTEXITCODE) { throw 'Button verification failed.' }
    if ($Benchmark) {
        & $compiler /nologo /target:exe "/out:$output\Quartz.ButtonBenchmark.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll "/r:$output\Quartz.exe" "$PSScriptRoot\Benchmark.cs"
        if ($LASTEXITCODE) { throw 'Benchmark compilation failed.' }
        Copy-Item -LiteralPath "$output\Quartz.exe.config" -Destination "$output\Quartz.ButtonBenchmark.exe.config"
        & "$output\Quartz.ButtonBenchmark.exe"
        if ($LASTEXITCODE) { throw 'Button benchmark failed.' }
    }
}
finally { Pop-Location }
