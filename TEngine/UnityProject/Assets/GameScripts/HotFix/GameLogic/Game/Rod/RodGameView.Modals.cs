using UnityEngine;

namespace GameLogic.Rod
{
    public sealed partial class RodGameView
    {
        /// <summary>公共弹窗只使用独立框体散图。高度受安全区约束，内部行距重排，文字不随整页压缩。</summary>
        private void ModalArt(string kind)
        {
            Panel(content, "ModalShade", 0, 0, 853, referenceSize.y, new Color(0, 0, 0, .68f), null, true);
            float desired = kind == "rules" ? 1560 : kind == "won" ? 1415 : kind == "revive" ? 1238 : kind == "settings" ? 780 : 1112;
            float h = Mathf.Min(desired, referenceSize.y - 180);
            var panel = ArtImage(content, "Modal_" + kind, RodArt.ModalFrame, 26, (referenceSize.y - h) * .5f, 801, h);
            string title = kind == "rules" ? "玩法说明" : kind == "coins" ? "获取金币" : kind == "won" ? "挑战成功" :
                kind == "failed" ? "本局未通关" : kind == "revive" ? "选择复活算筹" : kind == "settings" ? "设置" : "已跳过本关";
            Text(panel, "Title", title, 70, 43, 661, 78, 52, Dark);
            if (kind != "won" && kind != "skipped")
            {
                var close = ArtButton(panel, "Close", RodArt.HomeRound, "", 725, -15, 82, 82, () => Emit("close"));
                Panel(close, "Icon", 21, 21, 40, 40, Paper, skin.pageArt[RodArt.CloseIcon]);
            }
            switch (kind)
            {
                case "settings": SettingsArt(panel, h); break;
                case "rules": RulesArt(panel, h); break;
                case "coins": CoinsArt(panel, h); break;
                case "revive": ReviveArt(panel, h); break;
                default: ResultArt(panel, kind, h); break;
            }
        }

        private void SettingsArt(RectTransform panel, float h)
        {
            AudioRow(panel, "Music", "音乐", !model.save.musicMuted, 190, "music");
            AudioRow(panel, "Sound", "音效", !model.save.soundMuted, 365, "sound");
            ArtButton(panel, "Understood", RodArt.ModalButton, "完成", 195, h - 150, 411, 95, () => Emit("close"), true, 36, Paper);
        }

        private void AudioRow(RectTransform parent, string name, string label, bool enabled, float y, string action)
        {
            var row = ArtImage(parent, name + "Row", RodArt.ModalRow, 55, y, 691, 140);
            Text(row, "Name", label, 40, 30, 260, 75, 38, Dark);
            ArtButton(row, name, RodArt.ModalButton, enabled ? "开启" : "关闭", 442, 27, 207, 86,
                () => Emit(action), true, 33, Paper);
        }

        private void RulesArt(RectTransform panel, float h)
        {
            float scale = (h - 350) / 1210;
            float y = 137;
            var first = RuleCard(panel, 1, "认识算筹", y, 313 * scale);
            Text(first, "Body", "不同点数的算筹代表\n不同的筹力值。", 24, 100 * scale, 270, 115 * scale, 28, Dark);
            RuleIllustration(first, RodArt.RuleDice, 294, 65 * scale, 464, 158 * scale);
            for (int i = 0; i < 3; i++) Text(first, "Points_" + i, (i + 1) + "点筹力", 290 + i * 155, 228 * scale, 145, 55 * scale, 27, Amber);
            y += 321 * scale;
            var second = RuleCard(panel, 2, "选中同花色", y, 260 * scale);
            Text(second, "Body", "点击一枚，选中同花色整组；\n再点取消，点其他花色切换。", 24, 105 * scale, 355, 115 * scale, 26, Dark);
            RuleIllustration(second, RodArt.RuleSelect, 363, 59 * scale, 418, 192 * scale);
            y += 269 * scale;
            var third = RuleCard(panel, 3, "确认落区", y, 287 * scale);
            Text(third, "Body", "确认后，整组放入\n对应区域。", 24, 110 * scale, 235, 115 * scale, 27, Dark);
            RuleIllustration(third, RodArt.RulePlace, 245, 28 * scale, 535, 238 * scale);
            y += 296 * scale;
            var fourth = RuleCard(panel, 4, "比较胜分", y, 320 * scale);
            Text(fourth, "Body", "各区同筹力抵消后排名获得胜分；\n总胜分最高即通关，并列最高也通关。", 24, 84 * scale, 720, 98 * scale, 27, Dark);
            var scores = model.save.activeRun == null ? null : RodRules.ScoreRun(model.save.activeRun);
            for (int p = 0; p < 4; p++)
            {
                var score = ArtImage(fourth, "ExampleScore_" + p, p == 0 ? RodArt.ScoreYou : RodArt.ScoreBot, 16 + p * 190, 190 * scale, 184, 118 * scale);
                Text(score, "Name", PlayerNames[p], 8, 8, 168, 39 * scale, 25, p == 0 ? Paper : Dark);
                Text(score, "Score", (scores == null ? 0 : scores[p]).ToString(), 8, 48 * scale, 168, 60 * scale, 40, p == 0 ? Paper : Dark);
            }
            var note = ArtImage(panel, "RerollNote", RodArt.Summary, 75, h - 205, 651, 60);
            Text(note, "Text", "重投：重新投掷手中全部算筹。", 20, 5, 611, 50, 27, Dark);
            ArtButton(panel, "Understood", RodArt.ModalButton, "我知道了", 175, h - 125, 451, 95, () => Emit("close"), true, 38, Paper);
        }

        private RectTransform RuleCard(RectTransform panel, int index, string title, float y, float h)
        {
            var row = ArtImage(panel, "Rule_" + index, RodArt.ModalRow, 8, y, 785, h);
            var number = ArtImage(row, "NumberPlate", RodArt.ModalButton, 24, 17, 90, 70);
            Text(number, "Number", index.ToString("00"), 5, 5, 80, 60, 37, Paper);
            Text(row, "Title", title, 130, 17, 320, 70, 38, Dark);
            return row;
        }

        private void RuleIllustration(RectTransform row, RodArt art, float x, float y, float w, float h)
        {
            var node = Rent(row, "Illustration", RodUINodePool.Kind.Panel, x, y, w, h);
            SetImage(node.Image, Color.clear, null, false);
            int section = art == RodArt.RuleDice ? 1 : art == RodArt.RuleSelect ? 2 : 3;
            Vector2 size = RodRulePart.SourceSize(section);
            // 已缓存的池节点逐项更新，规则切页不创建纹理、集合或重新绑定回调。
            foreach (var part in skin.pageArt.ruleParts)
            {
                if (part.section != section) continue;
                Rect rect = part.rect;
                if (string.IsNullOrEmpty(part.text))
                    Panel(node.Rect, part.name, rect.x * w / size.x, rect.y * h / size.y,
                        rect.width * w / size.x, rect.height * h / size.y, part.color, part.sprite);
                else
                    Text(node.Rect, part.name, part.text, rect.x * w / size.x, rect.y * h / size.y,
                        rect.width * w / size.x, rect.height * h / size.y, part.fontSize, part.color);
            }
            node.Rect.SetAsFirstSibling();
        }

        private void CoinsArt(RectTransform panel, float h)
        {
            float rowH = (h - 245) / 3;
            CoinRow(panel, "Daily", "每日领取", "每日一次", "100", RodArt.DailyArt, 180, rowH,
                model.save.dailyClaimed ? "已领取" : "领取", "daily", !model.save.dailyClaimed);
            CoinRow(panel, "ShareCoin", "分享领取", "剩余 " + (3 - model.save.sharesClaimed) + " 次", "50", RodArt.ShareArt, 180 + rowH, rowH,
                "分享", "shareCoin", model.save.sharesClaimed < 3);
            CoinRow(panel, "CoinAd", "广告领取", "每日不限", "30", RodArt.AdArt, 180 + rowH * 2, rowH,
                model.adReady ? "领取" : "准备中", "adCoin", model.adReady);
        }

        private void CoinRow(RectTransform panel, string name, string title, string limit, string amount, RodArt art,
            float y, float h, string label, string action, bool enabled)
        {
            var row = ArtImage(panel, name + "Row", RodArt.ModalRow, 28, y, 746, h - 10);
            float illustration = Mathf.Min(215, h - 35);
            ArtImage(row, "Illustration", art, 10, 15, illustration * 1.28f, illustration);
            Text(row, "Title", title, 296, 15, 400, 55, 37, Dark);
            Text(row, "Limit", limit, 296, 73, 400, 43, 27, Dark);
            Text(row, "Amount", "+" + amount, 306, h - 110, 175, 72, 46, Amber);
            ArtButton(row, name, RodArt.ModalButton, label, 521, h - 116, 208, 92, () => Emit(action), enabled, 32, Paper);
        }

        private void ResultArt(RectTransform panel, string kind, float h)
        {
            var run = model.save.activeRun;
            if (run == null) return;
            Text(panel, "Level", LevelCaption(), 60, 140, 681, 60, 32, Dark);
            RankArt(panel, 220, Mathf.Min(83, (h - 600) / 4));
            if (kind == "won")
            {
                Text(panel, "RewardTitle", run.baseRewardClaimed ? "首通奖励已入账" : "本关已领取过首通奖励", 50, h - 610, 701, 55, 30, Dark);
                RewardArt(panel, RodArt.Coin, "金币 ×" + run.rewardCoin, 85, h - 550);
                RewardArt(panel, RodArt.RerollIcon, "重投 ×" + run.rewardReroll, 300, h - 550);
                RewardArt(panel, RodArt.NextIcon, "下一关 ×" + run.rewardSkip, 515, h - 550);
                ArtButton(panel, "Next", RodArt.ModalButton, run.levelId < 200 ? "下一关" : "已通关全部关卡", 80, h - 425, 641, 100, () => Emit("next"), run.levelId < 200, 38, Paper);
                ArtButton(panel, "Double", RodArt.ModalButton, run.doubleRewardClaimed ? "已领取双倍" : model.adReady ? "广告奖励双倍" : "广告准备中", 80, h - 310, 641, 100,
                    () => Emit("double"), run.baseRewardClaimed && !run.doubleRewardClaimed && model.adReady, 34, Paper);
                ArtButton(panel, "Home", RodArt.ModalButton, "返回首页", 80, h - 160, 321, 95, () => Emit("home"), true, 33, Paper);
                ArtButton(panel, "Share", RodArt.ModalButton, "分享", 425, h - 160, 296, 95, () => Emit("share"), true, 33, Paper);
            }
            else if (kind == "failed")
            {
                Text(panel, "Explanation", "调整落区，再挑战一次", 60, h - 420, 681, 65, 31, Dark);
                ArtButton(panel, "Restart", RodArt.ModalButton, "免费重开", 80, h - 325, 641, 110, () => Emit("restart"), true, 39, Paper);
                ArtButton(panel, "Revive", RodArt.ModalButton, "选择算筹复活", 80, h - 195, 641, 110, () => Emit("revive"), true, 39, Paper);
            }
            else
            {
                Text(panel, "Explanation", "跳过不发放通关奖励", 60, h - 400, 681, 65, 31, Dark);
                ArtButton(panel, "Next", RodArt.ModalButton, "下一关", 80, h - 280, 641, 100, () => Emit("next"), run.levelId < 200, 38, Paper);
                ArtButton(panel, "Home", RodArt.ModalButton, "返回首页", 80, h - 150, 641, 95, () => Emit("home"), true, 34, Paper);
            }
        }

        /// <summary>稳定降序展示，并列使用同一名次；不修改玩法分数或存档。</summary>
        private void RankArt(RectTransform panel, float y, float rowH)
        {
            var scores = RodRules.ScoreRun(model.save.activeRun);
            RodPresentationRules.Rank(scores, rankOrder, rankNumbers);
            for (int row = 0; row < 4; row++)
            {
                int player = rankOrder[row];
                var plate = ArtImage(panel, "Rank_" + row, RodArt.ModalRow, 65, y + row * (rowH + 6), 671, rowH);
                Text(plate, "Rank", rankNumbers[row].ToString(), 20, 6, 85, rowH - 12, 34, Dark);
                Text(plate, "Name", PlayerNames[player], 135, 6, 300, rowH - 12, 34, Dark);
                Text(plate, "Score", scores[player] + "分", 450, 6, 180, rowH - 12, 34, Dark);
            }
        }

        private void RewardArt(RectTransform panel, RodArt art, string label, float x, float y)
        {
            Panel(panel, "RewardIcon_" + art, x + 55, y, 70, 70, art == RodArt.Coin ? Color.white : Dark, skin.pageArt[art]);
            Text(panel, "RewardLabel_" + art, label, x, y + 73, 180, 44, 27, Dark);
        }

        private void ReviveArt(RectTransform panel, float h)
        {
            var run = model.save.activeRun;
            int points = 0;
            for (int i = 0; i < run.dice.Length; i++) if (model.revival.Contains(run.dice[i].id)) points += run.dice[i].value;
            var selection = ArtImage(panel, "SelectionPlate", RodArt.ModalRow, 61, 176, 673, 149);
            Text(selection, "Selection", "已选 " + points + " / 6 点筹力", 20, 38, 633, 70, 42, Dark);
            var info = ArtButton(panel, "Info", RodArt.HomeRound, "", 640, 66, 63, 63, () => ShowHelp(panel));
            Panel(info, "Icon", 10, 10, 43, 43, Paper, skin.pageArt[RodArt.HelpIcon]);
            float trayH = h - 555;
            var tray = ArtImage(panel, "RevivalTray", RodArt.Tray, 14, 341, 773, trayH);
            float size = Mathf.Min(183, (trayH - 65) / 3);
            for (int i = 0; i < run.dice.Length; i++)
            {
                var die = run.dice[i]; int id = die.id;
                DieArt(tray, die, 85 + i % 3 * 214 + (183 - size) * .5f, 25 + i / 3 * (size + 3), size,
                    () => Emit("pick", id), model.revival.Contains(id));
            }
            ArtButton(panel, "ReviveAd", RodArt.ModalButton, model.adReady ? "看广告复活" : "广告准备中", 40, h - 185, 722, 130,
                () => Emit("reviveAd"), points == 6 && model.adReady, 44, Paper);
            var hint = Panel(panel, "ReviveHelp", 68, 158, 667, 223, Paper, skin.pageArt[RodArt.ModalRow]);
            Text(hint, "Title", "广告完成后才取回", 30, 25, 607, 65, 37, Dark);
            Text(hint, "Body", "选中算筹返回手中，整批重新投掷。\n免费重投余额保留，已用次数不恢复。", 28, 95, 611, 103, 28, Dark);
            hint.gameObject.SetActive(Time.unscaledTime < helpUntil);
            if (hint.gameObject.activeSelf) help = hint.gameObject;
        }
    }
}
