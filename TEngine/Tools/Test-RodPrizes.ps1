<# 只读正式关卡，对比差异化方案；产物仅写 work，不更改关卡或评分版本。 #>
param([Parameter(Mandatory=$true)][string]$UnityData, [int]$Trials=100)
$ErrorActionPreference = 'Stop'
$rodRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$rodOutput = Join-Path $rodRoot 'work/rod-validation/prize-analysis'
New-Item -ItemType Directory -Force -Path $rodOutput | Out-Null
$rodConfigPath = Join-Path $rodRoot 'TEngine/UnityProject/Assets/AssetRaw/Configs/Rod/RodLevels.json'
$rodConfig = Get-Content -LiteralPath $rodConfigPath -Raw | ConvertFrom-Json
$rodValues = @(3,2,2,1,1,1,1,1,1)
$rodLines = foreach ($rodLevel in $rodConfig.levels) {
    $rodPower = [int[]]::new(18)
    $rodWitness = [int[]]::new(6)
    for ($rodIndex=0; $rodIndex -lt 27; $rodIndex++) { $rodPower[[int][Math]::Floor($rodIndex/9)*6 + $rodLevel.opponentFaces[$rodIndex]] += $rodValues[$rodIndex%9] }
    for ($rodIndex=0; $rodIndex -lt 9; $rodIndex++) { $rodWitness[$rodLevel.witness[$rodIndex]] += $rodValues[$rodIndex] }
    (@($rodLevel.id) + $rodPower + $rodWitness) -join ','
}
$rodLines | Set-Content "$rodOutput/layouts.csv" -Encoding utf8
$rodArguments = @('/nologo','/target:exe',"/out:$rodOutput/RodPrizeAnalysis.exe")
foreach ($rodAssembly in @('mscorlib','System','System.Core')) { $rodArguments += "/r:$UnityData/MonoBleedingEdge/lib/mono/4.5/$rodAssembly.dll" }
$rodArguments += Join-Path $PSScriptRoot 'RodPrizeAnalysis.cs'
$rodArguments += Join-Path $rodRoot 'TEngine/UnityProject/Assets/GameScripts/HotFix/GameLogic/Game/Rod/RodState.cs'
& "$UnityData/NetCoreRuntime/dotnet.exe" "$UnityData/DotNetSdkRoslyn/csc.dll" @rodArguments
if ($LASTEXITCODE -ne 0) { throw 'Prize analysis compilation failed' }
& "$UnityData/MonoBleedingEdge/bin/mono.exe" "$rodOutput/RodPrizeAnalysis.exe" "$rodOutput/layouts.csv" $Trials | Tee-Object -FilePath "$rodOutput/对照结果.md"
if ($LASTEXITCODE -ne 0) { throw 'Prize simulation failed' }
"Source JSON SHA256: $((Get-FileHash -LiteralPath $rodConfigPath -Algorithm SHA256).Hash)" | Add-Content "$rodOutput/对照结果.md"
