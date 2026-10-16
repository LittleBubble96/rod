# UI散图与Prefab · 2026-10-14

2026-10-15 更新：十张反馈截图对应的资源已拆分修复并接回现有窗口；本文原资源数量及组合切图描述为历史状态。最新文件、补底差异和验证记录见 [散图修复记录](2026-10-15散图修复.md)。

## 本次范围

按用户提供的 NewText UI 示例组织独立 `UIWindow` 类和静态 Prefab。设计画布为1080×1920；不新增机型适配。原稿853×1844的主体保持等比，放置在设计画布中央。左右余量未重新设计。

本次只交付散图、可编辑层级及节点绑定；没有把新窗口切换进游戏启动流程。原 `RodGameHost` 与旧程序布局仍是已有运行入口，不代表本次静态 Prefab 的效果。没有修改本次的计分或关卡配置。

## 目录

- 小图：`TEngine/UnityProject/Assets/AssetRaw/UIRaw/Atlas/RodHome`、`RodBoard`、`RodRules`、`RodCoins`、`RodLoading`。
- 公共图：同级 `RodCommon`，包括公共框体、算筹底图、符号、关闭按钮等；结果、复活、设置复用公共资源，不重复复制。
- 大图：`TEngine/UnityProject/Assets/AssetRaw/UIRaw/RodBackgrounds`。
- Prefab：`TEngine/UnityProject/Assets/AssetRaw/UI/Rod/Windows`。
- 窗口脚本：`TEngine/UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/Rod`。
- 中间图与坐标清单：`Art/分层源`。这些源图不作为新窗口的整页交互背景。
- 静态窗口截图：`work/rod-validation/window-captures`。

## 使用方式

直接打开 `UIRodHome.prefab` 等资源编辑层级。窗口类使用 `[Window(...location: "UIRodHome")]`；`ScriptGenerator` 缓存组件，`SetText` 根据层级路径更新已缓存文本，`Clicked` 交给调用方处理业务。关闭时清除外部订阅。没有在刷新中生成、销毁页面节点。

编辑器搭建工具 `RodPrefabAssembler` 仅供初次导出，不能在手工调整之后直接重跑，因为会重写工具负责的九个窗口。后续手调以 Prefab 为准。运行时窗口不包含 `RodGameView`、安全区适配器或布局生成组件。

## 验证边界与未完成还原

隔离 Unity 2022.3.62f2 导出九个静态 Prefab，并以1080×1920离屏渲染截图；检查资源尺寸及运行时布局组件未混入。此检查不是完整入口运行、微信真机或一比一视觉验收。

最终资源为51张小图、3张大图。九个绑定脚本针对当前缓存框架程序集编译通过；最终导出日志没有缺字警告。选中框、勾选图标及复活说明保留为静态隐藏节点。日志：`work/rod-validation/window-assembly.log`。

仍有明确差异：现有中文字体与原稿字形不同；扁平稿恢复底板、圆角透明度、阴影及插画矩形边缘尚未达到严格还原；部分公共框体与结果页装饰仍是共用拆分资源，不能声称所有页面均已一比一完成。此前生成的中间素材不等于原始分层美术。
