$ErrorActionPreference='Stop'
$taskRepo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$taskAssets=[IO.Path]::GetFullPath((Join-Path $taskRepo 'TEngine/UnityProject/Assets'))
# 明确列出已由Rod入口替换的旧链，保留SDK、存储、音频及框架。
$taskPaths=@(
 'GameScripts/HotFix/GameLogic/UI',
 'GameScripts/HotFix/GameLogic/Localization/GameLocalizationManager.cs',
 'GameScripts/HotFix/GameLogic/Localization/LocalizationHelper.cs',
 'GameScripts/HotFix/GameLogic/Game/CorePlay',
 'GameScripts/HotFix/GameLogic/Game/Common',
 'GameScripts/HotFix/GameLogic/Game/Effect',
 'GameScripts/HotFix/GameLogic/Game/GM',
 'GameScripts/HotFix/GameLogic/Game/Pool',
 'GameScripts/HotFix/GameLogic/Game/Prop',
 'GameScripts/HotFix/GameLogic/Game/Share',
 'GameScripts/HotFix/GameLogic/Game/System',
 'GameScripts/HotFix/GameLogic/Game/GameManager.cs',
 'GameScripts/HotFix/GameLogic/Game/GameDefine.cs',
 'GameScripts/HotFix/GameLogic/Game/IGamePlay.cs',
 'GameScripts/HotFix/GameLogic/Game/DrawCharacter.cs',
 'GameScripts/HotFix/GameLogic/Game/Triangulator.cs',
 'GameScripts/HotFix/GameLogic/Game/TextLevelDataScriptableObject.cs',
 'GameScripts/HotFix/GameLogic/Game/TextGraphicDataScriptableObject.cs',
 'GameScripts/HotFix/GameLogic/Game/LevelTableScriptableObject.cs',
 'GameScripts/HotFix/GameLogic/Game/LevelCompletionReward.cs',
 'GameScripts/HotFix/GameLogic/Game/SDK/BiMgr.Events.cs',
 'GameScripts/HotFix/GameLogic/Game/Data/GameCacheManager.cs',
 'GameScripts/HotFix/GameLogic/Game/Data/GameCacheData.cs',
 'GameScripts/HotFix/GameLogic/Game/Data/GameSettingsData.cs',
 'GameScripts/HotFix/GameLogic/Game/Data/IRestoreData.cs',
 'GameScripts/HotFix/GameLogic/Game/Data/Ext',
 'Editor/LevelEditor/LevelTools.cs',
 'Editor/LevelEditor/TextGraphicDataScriptableObjectEditor.cs',
 'Editor/Rod/RodDesignMenu.cs',
 'AssetRaw/UI/Ad','AssetRaw/UI/GM','AssetRaw/UI/Guide','AssetRaw/UI/Top',
 'AssetRaw/UI/UICorePlay','AssetRaw/UI/UIFinish','AssetRaw/UI/UIHome','AssetRaw/UI/Common',
 'AssetRaw/UIRaw/Atlas/Common','AssetRaw/UIRaw/Atlas/Game','AssetRaw/UIRaw/Atlas/SingleAtlas',
 'AssetRaw/UIRaw/Raw',
 'AssetRaw/Actor/GameView','AssetRaw/Configs/LevelConfigs','AssetRaw/Materials/StrokeMaterial.mat'
)
$taskReport=@()
foreach($taskRelative in $taskPaths){
 $taskTarget=[IO.Path]::GetFullPath((Join-Path $taskAssets $taskRelative))
 if(-not $taskTarget.StartsWith($taskAssets+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw '删除目标越界'}
 foreach($taskItem in @($taskTarget,($taskTarget+'.meta'))){
  if(-not(Test-Path -LiteralPath $taskItem)){continue}
  $taskFiles=Get-ChildItem -LiteralPath $taskItem -Recurse -File
  foreach($taskFile in $taskFiles){$taskReport+=@{path=$taskFile.FullName.Substring($taskRepo.Length+1);bytes=$taskFile.Length}}
  Remove-Item -LiteralPath $taskItem -Recurse -Force
 }
}
$taskReport | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $taskRepo 'work/rod-validation/retired-game-deleted.json') -Encoding utf8
Write-Output ("Deleted {0} files; {1:N0} source bytes" -f $taskReport.Count,($taskReport.bytes | Measure-Object -Sum).Sum)
