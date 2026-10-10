<#
.SYNOPSIS
在隔离的 Unity 工程运行真实 UI 节点复用测试，主工程不导入、不升级。
.DESCRIPTION
仅复制所需生产源码、现有皮肤与关卡，以及验证入口。依赖使用本地已还原的
UGUI/TMP 包，不修改主工程 manifest。传入其他补丁版本的结果仅为兼容性证据。
#>
param([Parameter(Mandatory = $true)][string]$UnityExe, [switch]$PlaySmoke)
$ErrorActionPreference = 'Stop'
$rodRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$rodProject = Join-Path $rodRoot 'TEngine/UnityProject'
$rodHarness = Join-Path $rodRoot 'work/rod-validation/ui-reuse-harness'
foreach ($folder in @('Assets/Runtime', 'Assets/Editor', 'Packages', 'ProjectSettings')) {
    New-Item -ItemType Directory -Force -Path (Join-Path $rodHarness $folder) | Out-Null
}
# .meta 一并保留，使皮肤、字体、底板对现有脚本和资源的引用保持有效。
foreach ($source in @('RodState', 'RodCatalog', 'RodSession', 'RodUISkin', 'RodIconGraphic', 'RodGameView', 'RodUINodePool')) {
    $rodSource = Join-Path $rodProject "Assets/GameScripts/HotFix/GameLogic/Game/Rod/$source.cs"
    Copy-Item -LiteralPath $rodSource -Destination "$rodHarness/Assets/Runtime/"
    if (Test-Path -LiteralPath "$rodSource.meta") { Copy-Item -LiteralPath "$rodSource.meta" -Destination "$rodHarness/Assets/Runtime/" }
}
Copy-Item -LiteralPath "$rodProject/Assets/Editor/Rod/RodUIReuseValidation.cs" -Destination "$rodHarness/Assets/Editor/"
if ($PlaySmoke) {
    foreach ($folder in @('Assets/GameScripts/HotFix/GameLogic/Game/Rod', 'Assets/GameScripts/HotFix/GameLogic/Game/Ad', 'Assets/GameScripts/GameSDK', 'Assets/TEngine/Runtime/Core/Log')) {
        Get-ChildItem (Join-Path $rodProject $folder) -File |
            Where-Object { $_.Extension -in @('.cs', '.meta') -and $_.Name -notmatch 'WXSDK|asmdef' } |
            Copy-Item -Destination "$rodHarness/Assets/Runtime/" -Force
    }
    foreach ($source in @(
        'Assets/GameScripts/HotFix/GameLogic/Game/SDK/BiMgr.cs',
        'Assets/GameScripts/HotFix/GameLogic/Game/Data/SnapshotJournal.cs',
        'Assets/GameScripts/HotFix/GameLogic/Game/Data/Storage/IReadCacheFileSystem.cs',
        'Assets/GameScripts/HotFix/GameLogic/Game/Data/Storage/WindowReadCacheFileSystem.cs',
        'Assets/TEngine/Runtime/Core/Utility/Utility.Text.cs',
        'Assets/TEngine/Runtime/Core/Utility/Utility.Text.ITextHelper.cs',
        'Assets/TEngine/Runtime/Core/Utility/DefaultHelper/DefaultTextHelper.cs',
        'Assets/TEngine/Runtime/Core/Utility/DefaultHelper/DefaultLogHelper.cs',
        'Assets/TEngine/Runtime/Core/Utility/GameFrameworkException.cs'
    )) { Copy-Item -LiteralPath (Join-Path $rodProject $source) -Destination "$rodHarness/Assets/Runtime/" -Force }
    foreach ($source in @('RodPlaySmoke', 'RodUIBuilder', 'RodLevelEditor')) {
        Copy-Item -LiteralPath "$rodProject/Assets/Editor/Rod/$source.cs" -Destination "$rodHarness/Assets/Editor/" -Force
    }
    foreach ($resource in @('Assets/Scenes/RodPreview.unity', 'Assets/AssetRaw/UI/Rod/RodGameRoot.prefab')) {
        $rodDestination = Join-Path $rodHarness $resource
        New-Item -ItemType Directory -Force -Path (Split-Path $rodDestination -Parent) | Out-Null
        Copy-Item -LiteralPath (Join-Path $rodProject $resource) -Destination $rodDestination -Force
        Copy-Item -LiteralPath (Join-Path $rodProject "$resource.meta") -Destination "$rodDestination.meta" -Force
    }
}
foreach ($resource in @('Assets/AssetRaw/UI/Rod/RodSkin.asset', 'Assets/AssetRaw/Configs/Rod/RodLevels.json', 'Assets/AssetRaw/Fonts/AlimamaFangYuanTiVF SDF.asset')) {
    $rodDestination = Join-Path $rodHarness $resource
    New-Item -ItemType Directory -Force -Path (Split-Path $rodDestination -Parent) | Out-Null
    Copy-Item -LiteralPath (Join-Path $rodProject $resource) -Destination $rodDestination
    Copy-Item -LiteralPath (Join-Path $rodProject "$resource.meta") -Destination "$rodDestination.meta"
}
foreach ($resourceFolder in @('Assets/TextMesh Pro', 'Assets/AssetRaw/UIRaw/Atlas/Rod')) {
    $rodDestination = Join-Path $rodHarness $resourceFolder
    New-Item -ItemType Directory -Force -Path $rodDestination | Out-Null
    Get-ChildItem -LiteralPath (Join-Path $rodProject $resourceFolder) -Force |
        Copy-Item -Destination $rodDestination -Recurse -Force
}
$rodDependencies = [ordered]@{}
foreach ($package in @('com.unity.textmeshpro', 'com.unity.ugui')) {
    $rodPackage = Get-ChildItem "$rodProject/Library/PackageCache" -Directory -Filter "$package@*" |
        Sort-Object Name | Select-Object -Last 1
    if (!$rodPackage) { throw "Missing restored package: $package" }
    $rodDependencies[$package] = 'file:' + $rodPackage.FullName.Replace('\', '/')
}
foreach ($module in @('ui', 'imgui', 'jsonserialize', 'imageconversion', 'physics')) { $rodDependencies["com.unity.modules.$module"] = '1.0.0' }
if ($PlaySmoke -or (Test-Path "$rodHarness/Assets/Runtime/RodGameHost.cs")) {
    $rodDependencies['com.cysharp.unitask'] = 'file:' + "$rodProject/Packages/UniTask".Replace('\', '/')
    $rodDependencies['com.tuyoogame.yooasset'] = 'file:' + "$rodProject/Packages/YooAsset".Replace('\', '/')
}
@{ dependencies = $rodDependencies } | ConvertTo-Json -Depth 4 | Set-Content "$rodHarness/Packages/manifest.json" -Encoding utf8
# 不复制主工程 ProjectVersion：隔离工程由指定 Editor 创建自己的版本记录。
$rodLog = Join-Path $rodRoot $(if ($PlaySmoke) { 'work/rod-validation/play-smoke-isolated.log' } else { 'work/rod-validation/ui-reuse-unity.log' })
$rodMethod = if ($PlaySmoke) { 'Rod.Editor.RodPlaySmoke.Run' } else { 'Rod.Editor.RodUIReuseValidation.Run' }
$rodMarker = if ($PlaySmoke) { '[RodPlaySmoke] PASS' } else { '[RodUIReuseValidation] PASS' }
$rodArguments = @('-batchmode', '-nographics', '-projectPath', ('"' + $rodHarness + '"'), '-executeMethod', $rodMethod, '-logFile', ('"' + $rodLog + '"'))
if (!$PlaySmoke) { $rodArguments += '-quit' }
$rodProcess = Start-Process -FilePath $UnityExe -WindowStyle Hidden -PassThru -ArgumentList $rodArguments
# 编译失败时 executeMethod 可能根本不会进入；只终止本脚本创建的超时实例。
if (!$rodProcess.WaitForExit(180000)) {
    $rodProcess.Kill()
    $rodProcess.WaitForExit()
    throw "Unity validation timed out. See $rodLog"
}
if ($rodProcess.ExitCode -ne 0) { throw "Unity UI reuse failed: exit $($rodProcess.ExitCode). See $rodLog" }
if (!(Select-String -LiteralPath $rodLog -SimpleMatch $rodMarker)) { throw "Missing PASS marker: $rodLog" }
Write-Output "PASS: $rodMethod executed in isolated Unity project. Log: $rodLog"
