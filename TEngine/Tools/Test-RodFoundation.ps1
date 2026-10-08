param([string]$UnityData = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3c1/Editor/Data')
$ErrorActionPreference = 'Stop'
$taskEngine = Split-Path $PSScriptRoot -Parent
$taskRoot = Split-Path $taskEngine -Parent
$taskOutput = Join-Path $taskRoot 'work/rod-validation'
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
$taskExe = Join-Path $taskOutput 'RodFoundationTests.exe'
$taskArguments = @('/nologo', '/target:exe', "/out:$taskExe")
foreach ($taskAssembly in @('mscorlib','System','System.Core')) {
    $taskArguments += "/r:$UnityData/MonoBleedingEdge/lib/mono/4.5/$taskAssembly.dll"
}
$taskArguments += Join-Path $PSScriptRoot 'RodFoundationTests.cs'
$taskArguments += Join-Path $taskEngine 'UnityProject/Assets/GameScripts/HotFix/GameLogic/Game/Data/SnapshotJournal.cs'
$taskArguments += Join-Path $taskEngine 'UnityProject/Assets/GameScripts/HotFix/GameLogic/Game/Rod/RodState.cs'
& "$UnityData/NetCoreRuntime/dotnet.exe" "$UnityData/DotNetSdkRoslyn/csc.dll" @taskArguments
if ($LASTEXITCODE -ne 0) { throw 'Rod foundation compilation failed.' }
& "$UnityData/MonoBleedingEdge/bin/mono.exe" $taskExe
if ($LASTEXITCODE -ne 0) { throw 'Rod foundation tests failed.' }
