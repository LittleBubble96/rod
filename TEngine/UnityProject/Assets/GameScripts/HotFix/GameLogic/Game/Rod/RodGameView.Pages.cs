using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Rod
{
    public sealed partial class RodGameView
    {
        private static readonly string[] PlayerNames = { "你", "电脑甲", "电脑乙", "电脑丙" };
        private readonly int[] rankOrder = new int[4];
        private readonly int[] rankNumbers = new int[4];
        private readonly int[] ownPower = new int[6];
        private static readonly Color Paper = new Color32(255, 249, 232, 255);
        private static readonly Color Dark = new Color32(0, 62, 66, 255);
        private static readonly Color Amber = new Color32(159, 66, 9, 255);

        /// <summary>正式页面入口。底板、图案、动态文字与独立命中区分开，装饰不接收点击。</summary>
        private void RenderPagesArt(RodViewModel vm)
        {
            var backdrop = Panel(transform, "PageBackdrop", 0, 0, 0, 0, Paper, null);
            backdrop.anchorMin = Vector2.zero; backdrop.anchorMax = Vector2.one;
            backdrop.offsetMin = backdrop.offsetMax = Vector2.zero;
            if (vm.home) HomeArt(); else BoardArt();
            HeaderArt();
            if (!string.IsNullOrEmpty(vm.popup)) ModalArt(vm.popup);
            // 金币弹窗保留顶部余额可读，但不允许再次触发底层按钮。
            if (vm.popup == "coins") CoinHeader(false);
            if (!string.IsNullOrEmpty(vm.message))
            {
                var toast = Panel(content, "Toast", 75, referenceSize.y - 74, 703, 62, Dark, skin.panel);
                Text(toast, "Message", vm.message, 10, 5, 683, 52, 24, Paper);
            }
            if (vm.busy)
            {
                var shade = Panel(content, "Busy", 0, 0, 853, referenceSize.y, new Color(0, 0, 0, .4f), null, true);
                Text(shade, "BusyLabel", "请稍候", 200, referenceSize.y * .5f - 40, 453, 80, 38, Paper);
            }
        }

        private string LevelCaption()
        {
            int id = model.save.activeRun?.levelId ?? model.save.currentLevelId;
            string title = string.IsNullOrEmpty(model.levelName) ? "" : " · " + model.levelName;
            return "第 " + id.ToString("00") + " 关" + title;
        }

        private void HomeArt()
        {
            // 原稿静态品牌和插画已在Loading阶段拆出，不将首页按钮烘焙进背景。
            if (skin.loadingArt != null)
            {
                Panel(content, "HomeBackground", 0, 0, 853, 1844, Color.white,
                    skin.loadingArt.homeBackground ? skin.loadingArt.homeBackground : skin.loadingArt.background);
                Panel(content, "GameLogo", 135, 170, 600, 250, Color.white, skin.loadingArt.logo);
            }
            float bottom = referenceSize.y - 1844;
            Text(content, "Progress", LevelCaption(), 135, 1340 + bottom, 583, 67, 37, Dark);
            ArtButton(content, "Start", RodArt.HomeStart, model.save.activeRun == null ? "开始挑战" : "继续挑战",
                103, 1410 + bottom, 648, 180, () => Emit("start"), true, 65, Paper);
            ArtButton(content, "Rules", RodArt.HomeRules, "玩法说明", 310, 1602 + bottom, 235, 87, () => Emit("rules"), true, 30, Paper);
            HomeShortcut("Club", "游戏圈", RodArt.ClubIcon, 576, "club");
            HomeShortcut("Share", "分享", RodArt.ShareIcon, 771, "share");
            HomeShortcut("Settings", "设置", RodArt.SettingsIcon, 965, "settings");
        }

        private void HomeShortcut(string name, string label, RodArt icon, float y, string action)
        {
            var button = ArtButton(content, name, RodArt.HomeRound, "", 687, y, 128, 128, () => Emit(action));
            ArtImage(button, "Icon", icon, 27, 25, 74, 74);
            Text(content, name + "Text", label, 678, y + 125, 144, 50, 30, Dark);
        }

        private void HeaderArt()
        {
            CoinHeader(string.IsNullOrEmpty(model.popup));
            if (model.home) return;
            ArtImage(content, "LevelPlate", RodArt.HeaderLevel, 293, 84, 434, 82);
            Text(content, "Level", LevelCaption(), 347, 95, 323, 56, 32, Dark);
            if (string.IsNullOrEmpty(model.popup))
            {
                var back = ArtButton(content, "Back", RodArt.HeaderBack, "", 746, 83, 87, 83, () => Emit("home"));
                Panel(back, "Icon", 21, 20, 45, 45, Dark, skin.pageArt[RodArt.BackIcon]);
            }
        }

        private void CoinHeader(bool enabled)
        {
            bool home = model.home;
            float x = home ? 37 : 24, y = home ? 54 : 86, w = home ? 239 : 264;
            var coin = ArtButton(content, "GetCoin", home ? RodArt.HomeCoin : RodArt.HeaderCoin, "", x, y, w, 78,
                () => Emit("coins"), enabled);
            Panel(coin, "CoinSymbol", home ? 0 : 2, home ? 0 : 2, home ? 78 : 72, home ? 78 : 72,
                Color.white, home ? skin.pageArt.homeCoinIcon : skin.pageArt[RodArt.Coin]);
            Panel(coin, "AddBackground", home ? 172 : 197, 3, 65, 70, Color.white, skin.pageArt.headerAdd);
            Text(coin, "Count", model.save.coins.ToString(), 73, 6, home ? 105 : 125, 62, 36, Paper);
            Panel(coin, "Plus", home ? 186 : 211, 22, 35, 35, Paper, skin.pageArt[RodArt.PlusIcon]);
        }

        /// <summary>三列两行落区与三列三行手牌；只读取快照，不为预览修改实际筹力。</summary>
        private void BoardArt()
        {
            ArtImage(content, "BoardBackground", RodArt.Background, 0, 0, 853, referenceSize.y);
            var run = model.save.activeRun;
            if (run == null) return;
            var scores = RodRules.ScoreRun(run);
            for (int p = 0; p < 4; p++)
            {
                float x = p == 0 ? 38 : 248 + (p - 1) * 192;
                float w = p == 0 ? 200 : 182;
                var plate = ArtImage(content, "Score_" + p, p == 0 ? RodArt.ScoreYou : RodArt.ScoreBot, x, 219, w, 126);
                Text(plate, "Name", PlayerNames[p], 5, 12, w - 10, 39, 28, p == 0 ? Paper : Dark);
                Text(plate, "Score", scores[p].ToString(), 5, 56, w - 10, 60, 49, p == 0 ? Paper : Dark);
            }
            Array.Clear(ownPower, 0, ownPower.Length);
            int selectedPoints = 0, selectedCount = 0;
            for (int i = 0; i < run.dice.Length; i++)
            {
                var die = run.dice[i];
                if (die.region >= 0) ownPower[die.region] += die.value;
                else if (die.face == model.selectedFace) { selectedPoints += die.value; selectedCount++; }
            }
            var prizes = run.prizes ?? RodRules.LegacyPrizes;
            for (int f = 0; f < 6; f++)
            {
                int face = f;
                float x = 47 + f % 3 * 256, y = 369 + f / 3 * 203;
                bool selected = f == model.selectedFace && selectedCount > 0;
                var card = ArtButton(content, "Region_" + f, f < 3 ? RodArt.Region : RodArt.RegionSecond, "", x, y, 250, 185,
                    () => Emit("select", face), run.phase == RodPhase.AwaitGroup);
                if (selected) Frame(card, "Selected", RodArt.RegionSelection, 0, 0, 250, 185);
                Face(card, f, 22, 15, 61, selected && f == 1 ? Amber : Dark);
                Text(card, "Face", RodCatalog.Faces[f], 96, 15, 46, 51, 35, Dark);
                Text(card, "Awards", prizes[f * 2] + "/" + prizes[f * 2 + 1], 157, 21, 87, 42, 27, Dark);
                Text(card, "Power", "你 " + ownPower[f] + (selected ? "<color=#AC4A10>+" + selectedPoints + "</color>" : ""), 12, 78, 157, 52, 35, Dark);
                Text(card, "PowerLabel", "筹力", 169, 88, 74, 37, 25, Dark);
                Text(card, "Opponents", "甲 " + run.opponentPower[f] + "   乙 " + run.opponentPower[6 + f] + "   丙 " + run.opponentPower[12 + f], 9, 137, 232, 39, 25, Dark);
            }
            // 短屏优先压缩托盘与操作栏之间的留白，保证算筹命中区，不把整页一起缩小。
            float compactSpace = 78 * Mathf.Clamp01((1844 - referenceSize.y) / 344);
            float trayHeight = referenceSize.y - 1159 + compactSpace;
            var tray = ArtImage(content, "Tray", RodArt.Tray, 20, 777, 816, trayHeight);
            float dieSize = Mathf.Min(183, (trayHeight - 105) / 3);
            float rowStep = Mathf.Min(179, dieSize + 3);
            int hand = 0;
            for (int i = 0; i < run.dice.Length; i++)
            {
                var die = run.dice[i];
                if (die.region >= 0) continue;
                int face = die.face;
                DieArt(tray, die, 94 + hand % 3 * 218 + (183 - dieSize) * .5f, 35 + hand / 3 * rowStep, dieSize, () => Emit("select", face), model.selectedFace == face);
                hand++;
            }
            var summary = ArtImage(content, "SelectionSummary", RodArt.Summary, 93, referenceSize.y - 450 + compactSpace, 667, 82);
            string selection = selectedCount == 0 ? (hand == 0 ? "本局算筹已全部落区" : "点击算筹，选择同花色整组") :
                "已选：" + RodCatalog.Faces[model.selectedFace] + " · " + selectedCount + "枚 · " + selectedPoints + "点筹力";
            Text(summary, "Selection", selection, 14, 12, 639, 55, 31, selectedCount == 0 ? Dark : Amber);
            bool active = run.phase == RodPhase.AwaitGroup;
            ToolArt("Reroll", "重投", RodIcon.Reroll, model.rerollCost, 41, active, "reroll");
            ToolArt("Skip", "下一关", RodIcon.Next, model.skipCost, 245, active && run.levelId < 200, "skip");
            ArtButton(content, "Confirm", RodArt.Confirm, selectedCount > 0 ? "确认" + RodCatalog.Faces[model.selectedFace] + "组落区" : "确认落区",
                445, referenceSize.y - 263, 375, 160, () => Emit("place"), active && selectedCount > 0, 39, Paper);
        }

        private void ToolArt(string name, string label, RodIcon icon, string cost, float x, bool enabled, string action)
        {
            bool advert = cost == "广告";
            var button = ArtButton(content, name, RodArt.Tool, "", x, referenceSize.y - 272, 182, 174, () => Emit(action), enabled && (!advert || model.adReady));
            Panel(button, "Icon", 36, 22, 110, 110, Dark, skin.pageArt[icon == RodIcon.Reroll ? RodArt.RerollIcon : RodArt.NextIcon]);
            Text(button, "Label", label, 12, 124, 158, 43, 32, Dark);
            var badge = Panel(button, "CostPlate", 99, -2, 83, 42, Dark, skin.button);
            string display = cost ?? "";
            if (display.StartsWith("数量 ") || display.StartsWith("免费 ")) display = "×" + display.Substring(3);
            else if (display.StartsWith("金币 ")) display = display.Substring(3) + "币";
            if (advert) Panel(badge, "Ad", 25, 5, 32, 32, Paper, skin.pageArt[RodArt.AdIcon]);
            else Text(badge, "Cost", display, 2, 2, 79, 36, 22, Paper);
        }

        private void DieArt(Transform parent, RodDie die, float x, float y, float size, Action click, bool selected)
        {
            var body = die.value == 3 ? RodArt.DieThree : die.value == 2 ? RodArt.DieTwo : RodArt.DieOne;
            var button = ArtButton(parent, "Die_" + RodCatalog.Faces[die.face], body, "", x, y, size, size, click);
            Face(button, die.face, size * .23f, size * .19f, size * .55f, selected && die.face == 1 ? Amber : Dark);
            Text(button, "Points", die.value.ToString(), size * .71f, size * .03f, size * .22f, size * .3f, size * .23f, new Color32(65, 39, 12, 255));
            if (!selected) return;
            Frame(button, "SelectionFrame", RodArt.DieSelection, 0, 0, size, size);
            ArtImage(button, "Check", RodArt.Check, -10, -7, size * .32f, size * .32f);
        }

        private RectTransform ArtImage(Transform parent, string name, RodArt art, float x, float y, float w, float h)
            => Panel(parent, name, x, y, w, h, Color.white, skin.pageArt[art]);

        private RectTransform ArtButton(Transform parent, string name, RodArt art, string label, float x, float y, float w, float h,
            Action click, bool enabled = true, float fontSize = 30, Color? color = null)
        {
            var node = Rent(parent, name, RodUINodePool.Kind.Button, x, y, w, h);
            SetImage(node.Image, Color.white, skin.pageArt[art], true);
            node.Callback = click;
            node.Button.interactable = enabled;
            if (label.Length > 0) Text(node.Rect, "Label", label, 12, 6, w - 24, h - 12, fontSize, color ?? Dark);
            return node.Rect;
        }

        private void Face(Transform parent, int face, float x, float y, float size, Color color)
        {
            var node = Rent(parent, "FaceIcon", RodUINodePool.Kind.Panel, x, y, size, size);
            SetImage(node.Image, color, skin.pageArt[(RodArt)((int)RodArt.Wind + face)], false);
            node.Image.preserveAspect = true;
        }

        private void Frame(Transform parent, string name, RodArt art, float x, float y, float w, float h)
        {
            var node = Rent(parent, name, RodUINodePool.Kind.Panel, x, y, w, h);
            SetImage(node.Image, Color.white, skin.pageArt[art], false);
            node.Image.fillCenter = false;
        }
    }
}
