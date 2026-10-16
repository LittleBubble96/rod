using System;

namespace GameLogic.Rod
{
    /// <summary>页面交互边界及展示排名；不持久化、不发奖励，也不代替 Session 的规则校验。</summary>
    public static class RodPresentationRules
    {
        /// <summary>弹窗只能触发本层操作，防止遮罩下按钮、旧回调或快捷输入穿透。</summary>
        public static bool Allows(string popup, string action)
        {
            if (string.IsNullOrEmpty(popup)) return true;
            if (action == "close") return popup != "won" && popup != "skipped";
            switch (popup)
            {
                case "rules": return false;
                case "coins": return action == "daily" || action == "shareCoin" || action == "adCoin";
                case "settings": return action == "music" || action == "sound";
                case "won": return action == "next" || action == "home" || action == "share" || action == "double";
                case "skipped": return action == "next" || action == "home";
                case "failed": return action == "restart" || action == "revive";
                case "revive": return action == "pick" || action == "reviveAd";
                default: return false;
            }
        }

        /// <summary>再次点击当前花色取消；不能选已落完或终局算筹，不改变对局本身。</summary>
        public static int ToggleFace(RodRun run, int current, int requested)
        {
            if (run == null || run.phase != RodPhase.AwaitGroup || requested < 0 || requested > 5) return current;
            for (int i = 0; i < run.dice.Length; i++)
                if (run.dice[i].region < 0 && run.dice[i].face == requested)
                    return current == requested ? -1 : requested;
            return current;
        }

        /// <summary>
        /// 将四方按分数降序写入调用方缓存。并列保持你、甲、乙、丙顺序；同分同名次，下一名跳号。
        /// 四项插入排序无需临时集合，禁止用显示排序修改原始分数或胜负规则。
        /// </summary>
        public static void Rank(int[] scores, int[] order, int[] ranks)
        {
            if (scores == null || scores.Length != 4 || order == null || order.Length != 4 || ranks == null || ranks.Length != 4)
                throw new ArgumentException("排名需要三个长度为4的独立数组。");
            if (ReferenceEquals(scores, order) || ReferenceEquals(scores, ranks) || ReferenceEquals(order, ranks))
                throw new ArgumentException("排名输出不能复用输入或彼此共用数组。");
            for (int i = 0; i < 4; i++)
            {
                int j = i;
                while (j > 0 && scores[order[j - 1]] < scores[i]) { order[j] = order[j - 1]; j--; }
                order[j] = i;
            }
            for (int i = 0; i < 4; i++)
                ranks[i] = i > 0 && scores[order[i]] == scores[order[i - 1]] ? ranks[i - 1] : i + 1;
        }
    }
}
