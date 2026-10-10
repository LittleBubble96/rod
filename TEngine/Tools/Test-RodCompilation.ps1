<#
.SYNOPSIS
使用现有 Unity 程序集检查算筹源码，不启动、导入或升级主工程。
.DESCRIPTION
排除迁移缓存中的旧 GameLogic，直接编译当前业务源码；产物仅供编译检查，
不能替代 Unity 完整编译、目标平台构建或正式热更新 DLL。
#>
param(
    [Parameter(Mandatory = $true)][string]$UnityData,
    [string]$OutputDirectory
)
$ErrorActionPreference = 'Stop'
$rodRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$rodProject = Join-Path $rodRoot 'TEngine/UnityProject'
$rodOutput = if ($OutputDirectory) { [IO.Path]::GetFullPath($OutputDirectory) } else { Join-Path $rodRoot 'work/rod-validation/compilation' }
New-Item -ItemType Directory -Force -Path $rodOutput | Out-Null
$rodArguments = @('/nologo', '/target:library', '/define:UNITY_EDITOR', "/out:$rodOutput/RodCompileCheck.dll")
foreach ($assembly in @('mscorlib', 'System', 'System.Core')) {
    $rodArguments += "/r:$UnityData/MonoBleedingEdge/lib/mono/4.5/$assembly.dll"
}
$rodArguments += "/r:$UnityData/MonoBleedingEdge/lib/mono/4.5/Facades/netstandard.dll"
$rodArguments += "/r:$UnityData/Managed/UnityEditor.dll"
$rodArguments += "/r:$UnityData/Managed/UnityEngine.dll"
Get-ChildItem "$UnityData/Managed/UnityEngine" -Filter '*.dll' | ForEach-Object { $rodArguments += '/r:' + $_.FullName }
Get-ChildItem "$rodProject/Library/ScriptAssemblies" -Filter '*.dll' |
    Where-Object Name -notin @('GameLogic.dll', 'Assembly-CSharp-Editor.dll') |
    ForEach-Object { $rodArguments += '/r:' + $_.FullName }
foreach ($folder in @('Assets/GameScripts/HotFix/GameLogic/Game/Rod', 'Assets/GameScripts/HotFix/GameLogic/Game/Ad', 'Assets/GameScripts/GameSDK')) {
    # 此检查是 Editor 分支；微信专用实现须在微信 SDK 与目标平台构建中验证。
    Get-ChildItem (Join-Path $rodProject $folder) -Filter '*.cs' |
        Where-Object Name -ne 'WXSDK.cs' | ForEach-Object { $rodArguments += $_.FullName }
}
foreach ($source in @(
    'Assets/GameScripts/HotFix/GameLogic/Game/SDK/BiMgr.cs',
    'Assets/GameScripts/HotFix/GameLogic/Game/Data/SnapshotJournal.cs',
    'Assets/GameScripts/HotFix/GameLogic/Game/Data/Storage/IReadCacheFileSystem.cs',
    'Assets/GameScripts/HotFix/GameLogic/Game/Data/Storage/WindowReadCacheFileSystem.cs',
    'Assets/Editor/Rod/RodUIReuseValidation.cs',
    'Assets/Editor/Rod/RodUIBuilder.cs',
    'Assets/Editor/Rod/RodLevelEditor.cs',
    'Assets/Editor/Rod/RodPlaySmoke.cs'
)) { $rodArguments += Join-Path $rodProject $source }
& "$UnityData/NetCoreRuntime/dotnet.exe" "$UnityData/DotNetSdkRoslyn/csc.dll" @rodArguments
if ($LASTEXITCODE -ne 0) { throw 'Rod source compilation failed.' }
Write-Output 'PASS: current Rod, Editor SDK, ad, storage and UI validation sources compile (not a Unity build).'
