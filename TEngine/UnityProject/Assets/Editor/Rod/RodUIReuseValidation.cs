using System;
using System.IO;
using System.Linq;
using GameLogic.Rod;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Rod.Editor
{
    /// <summary>仅验证 UI 复用，不调用生成器，不覆盖字体、场景或手工美术。</summary>
    public static class RodUIReuseValidation
    {
        [MenuItem("六面算筹/验证UI节点复用（不生成资源）")]
        public static void Run()
        {
            var skin = AssetDatabase.LoadAssetAtPath<RodUISkin>("Assets/AssetRaw/UI/Rod/RodSkin.asset");
            if (skin == null || skin.font == null) throw new InvalidOperationException("缺少现有 UI 皮肤或字体。");
            var catalog = JsonUtility.FromJson<RodCatalog>(File.ReadAllText("Assets/AssetRaw/Configs/Rod/RodLevels.json"));
            Verify(skin, catalog);
        }

        /// <summary>验证真实 View，可由独立 Unity 测试工程传入皮肤和关卡数据。</summary>
        public static void Verify(RodUISkin skin, RodCatalog catalog)
        {
            var session = new RodSession(catalog, new RodSaveData(), _ => { }, s => s.Copy());
            session.Start(1, 43);
            var model = new RodViewModel { save = session.Snapshot, home = false, adReady = true };
            var root = new GameObject("UIReuseValidation", typeof(RectTransform));
            try
            {
                var view = root.AddComponent<RodGameView>();
                view.Initialize(skin);
                string[] pages = { null, "rules", "coins", "won", "failed", "revive", "skipped" };
                // 将所有有限页面和同名算筹的最大数量预热，再检查池是否保持稳定。
                for (int face = 0; face < 6; face++)
                {
                    foreach (var die in model.save.activeRun.dice) { die.face = face; die.region = -1; }
                    RenderPages(view, model, pages);
                }
                var before = root.GetComponentsInChildren<Transform>(true).Select(t => t.GetInstanceID()).OrderBy(id => id).ToArray();
                for (int pass = 0; pass < 20; pass++)
                {
                    foreach (var die in model.save.activeRun.dice) die.face = pass % 6;
                    RenderPages(view, model, pages);
                }
                var after = root.GetComponentsInChildren<Transform>(true).Select(t => t.GetInstanceID()).OrderBy(id => id).ToArray();
                Require(before.SequenceEqual(after), "重复切页创建或销毁了节点");

                model.home = false;
                model.popup = null;
                int clicks = 0, selected = -1;
                view.Action += (action, value) => { if (action == "select") { clicks++; selected = value; } };
                for (int face = 0; face < 6; face++)
                {
                    foreach (var die in model.save.activeRun.dice) die.face = face;
                    view.Render(model);
                    var dice = root.GetComponentsInChildren<Button>().Where(b => b.name == "Die_" + RodCatalog.Faces[face]).ToArray();
                    Require(dice.Length == 9, "同花色算筹未独立复用");
                    int previous = clicks;
                    dice[0].onClick.Invoke();
                    Require(clicks == previous + 1 && selected == face, "按钮重复监听或保留旧花色");
                }

                var stale = root.GetComponentsInChildren<Button>().First(b => b.name.StartsWith("Die_"));
                model.home = true;
                view.Render(model);
                int oldClicks = clicks;
                stale.onClick.Invoke();
                Require(clicks == oldClicks, "闲置按钮仍触发业务");
                model.home = false;
                model.popup = "revive";
                model.revival.UnionWith(new[] { 0, 1, 3 });
                model.adReady = false;
                view.Render(model);
                Require(!FindButton(root, "ReviveAd").interactable, "广告未就绪仍可复活");
                model.adReady = true;
                view.Render(model);
                Require(FindButton(root, "ReviveAd").interactable, "六点且广告就绪不能复活");
                FindButton(root, "Info").onClick.Invoke();
                Require(root.GetComponentsInChildren<Transform>().Any(t => t.name == "ReviveHelp"), "提示未显示");
                model.popup = "rules";
                view.Render(model);
                Require(!root.GetComponentsInChildren<Transform>().Any(t => t.name == "ReviveHelp"), "切页遗留复活提示");
                // 启动失败不应像旧占位逻辑一样把进度条强行填满。
                model.save = null;
                model.loadingProgress = 0;
                view.Render(model);
                var fill = root.GetComponentsInChildren<RectTransform>().First(t => t.name == "ProgressFill");
                Require(fill.sizeDelta.x == 0, "初始加载进度非零");
                model.loadingProgress = 3f / 5f;
                model.busy = false;
                model.message = "启动失败，原存档已保留";
                view.Render(model);
                Require(Mathf.Approximately(fill.sizeDelta.x, (skin.loadingArt != null ? RodLoadingArt.FillWidth : 550) * .6f), "进度未绑定已完成阶段");
                Require(root.GetComponentsInChildren<TextMeshProUGUI>().First(t => t.name == "LoadingPercent").text == "60%", "百分比与进度条不同步");
                Debug.Log("[RodUIReuseValidation] PASS: stable instances, duplicate faces, single callbacks, inactive callbacks, ad readiness, help lifetime, stage progress and failure progress.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static void RenderPages(RodGameView view, RodViewModel model, string[] pages)
        {
            model.home = true;
            model.popup = null;
            view.Render(model);
            model.home = false;
            foreach (string page in pages) { model.popup = page; view.Render(model); }
        }

        private static Button FindButton(GameObject root, string name) =>
            root.GetComponentsInChildren<Button>().First(b => b.name == name);

        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }
    }
}
