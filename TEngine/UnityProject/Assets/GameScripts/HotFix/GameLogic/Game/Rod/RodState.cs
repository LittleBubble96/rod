using System;
using System.Collections.Generic;

namespace GameLogic.Rod
{
    public enum RodPhase { AwaitGroup, Won, Failed, Skipped }

    [Serializable]
    public sealed class RodDie
    {
        public int id;
        public int value;
        public int face;
        public int region = -1;
    }

    [Serializable]
    public sealed class RodRun
    {
        public string runId;
        public int levelId;
        public string contentVersion;
        public RodPhase phase;
        public int round = 1;
        public int freeRerolls = 2;
        public uint randomState;
        public RodDie[] dice;
        // 按甲、乙、丙顺序，每人连续6区；快照固定，不引用当前热更表。
        public int[] opponentPower;
        // 每区第一/第二连续存储；旧v1无此字段时按旧88分规则恢复。
        public int[] prizes;
        public bool baseRewardClaimed;
        public bool doubleRewardClaimed;
        public int rewardCoin;
        public int rewardReroll;
        public int rewardSkip;
        public RodRun Copy()
        {
            var r=(RodRun)MemberwiseClone();r.opponentPower=opponentPower==null?null:(int[])opponentPower.Clone();r.prizes=prizes==null?null:(int[])prizes.Clone();
            if(dice!=null){r.dice=new RodDie[dice.Length];for(int i=0;i<dice.Length;i++){var d=dice[i];r.dice[i]=d==null?null:new RodDie{id=d.id,value=d.value,face=d.face,region=d.region};}}
            return r;
        }
    }

    [Serializable]
    public sealed class RodLevelProgress
    {
        public int levelId;
        public bool won;
        public bool skipped;
        public bool firstRewardClaimed;
    }

    [Serializable]
    public sealed class RodSaveData
    {
        public int schemaVersion = 1;
        public string contentVersion = "rod-200-draft-1";
        public int currentLevelId = 1;
        public int highestUnlockedLevel = 1;
        public int coins;
        public int rerollItems;
        public int skipItems;
        public string dayKey;
        public bool dailyClaimed;
        public int sharesClaimed;
        public long highestSeenUtcTicks;
        public string tutorialStep = "welcome";
        public bool tutorialCompleted;
        // 默认不静音，兼容没有这些字段的历史存档；只保存用户选择，不依赖UI状态。
        public bool musicMuted;
        public bool soundMuted;
        public List<RodLevelProgress> levels = new List<RodLevelProgress>();
        public RodRun activeRun;
        public bool hasActiveRun; // JsonUtility不能可靠保留内联对象null，写入显式标记。
        public string pendingActionId;
        public string pendingActionRunId;
        public string pendingActionKind;
        public bool pendingAdCompleted;
        public int[] pendingDieIds;
        public string lastAppliedActionId;

        public RodSaveData Copy()
        {
            var s=(RodSaveData)MemberwiseClone();s.activeRun=activeRun?.Copy();s.hasActiveRun=activeRun!=null;
            s.pendingDieIds=pendingDieIds==null?null:(int[])pendingDieIds.Clone();
            s.levels=levels==null?null:new List<RodLevelProgress>();
            if(levels!=null)foreach(var p in levels)s.levels.Add(p==null?null:new RodLevelProgress{levelId=p.levelId,won=p.won,skipped=p.skipped,firstRewardClaimed=p.firstRewardClaimed});
            return s;
        }

        public void Validate(int maxLevel = 200)
        {
            if (schemaVersion != 1) throw new InvalidOperationException("不支持的算筹存档版本，禁止覆写。");
            if (maxLevel < 1 || currentLevelId < 1 || currentLevelId > maxLevel ||
                highestUnlockedLevel < currentLevelId || highestUnlockedLevel > maxLevel)
                throw new InvalidOperationException("关卡进度无效。");
            if (coins < 0 || coins > 2000000000 || rerollItems < 0 || skipItems < 0 || sharesClaimed < 0 || sharesClaimed > 3)
                throw new InvalidOperationException("资源或每日次数无效。");
            if (levels == null) throw new InvalidOperationException("关卡记录缺失。");
            var ids = new HashSet<int>();
            foreach (var level in levels)
                if (level == null || level.levelId < 1 || level.levelId > maxLevel || !ids.Add(level.levelId) ||
                    (level.firstRewardClaimed && !level.won)) throw new InvalidOperationException("关卡记录冲突。");
            if (activeRun != null) RodRules.ValidateRun(activeRun, maxLevel);
        }
    }

    public static class RodRules
    {
        private static readonly int[] Values = {3,2,2,1,1,1,1,1,1};

        public static int[] LegacyPrizes => new[] {10,5,10,8,9,6,8,7,8,5,7,5};
        public static int[] UniformPrizes => new[] {10,5,10,5,10,5,10,5,10,5,10,5};
        /// <summary>2026-10-13 确认的差异化配置；每次返回独立数组，防止修改污染其他对局。仅用于新配置，旧局仍用存档快照。</summary>
        public static int[] CurrentPrizes => new[] {12,4,10,8,11,5,9,6,8,5,7,3};

        public static int[] Score(int[,] power, int[] prizes = null)
        {
            prizes = prizes ?? LegacyPrizes;
            ValidatePrizes(prizes);
            if (power == null || power.GetLength(0) != 4 || power.GetLength(1) != 6)
                throw new ArgumentException("需要4人×6区筹力。");
            var score = new int[4];
            for (int f = 0; f < 6; f++)
            {
                var unique = new List<int>();
                for (int p = 0; p < 4; p++)
                {
                    int v = power[p,f];
                    if (v < 0) throw new ArgumentException("筹力不可为负。");
                    if (v == 0) continue;
                    int count = 0;
                    for (int q = 0; q < 4; q++) if (power[q,f] == v) count++;
                    if (count == 1) unique.Add(p);
                }
                unique.Sort((a,b) => power[b,f].CompareTo(power[a,f]));
                for (int i = 0; i < Math.Min(2,unique.Count); i++) score[unique[i]] += prizes[f*2+i];
            }
            return score;
        }

        public static bool PlayerWins(int[] scores)
        {
            if (scores == null || scores.Length != 4) throw new ArgumentException("需要4人胜分。");
            return scores[0] >= scores[1] && scores[0] >= scores[2] && scores[0] >= scores[3];
        }

        public static int[] ScoreRun(RodRun run)
        {
            var power = new int[4,6];
            foreach (var die in run.dice) if (die.region >= 0) power[0,die.region] += die.value;
            for (int p = 1; p < 4; p++) for (int f = 0; f < 6; f++) power[p,f] = run.opponentPower[(p-1)*6+f];
            return Score(power, run.prizes);
        }

        public static void ValidatePrizes(int[] prizes)
        {
            if (prizes == null || prizes.Length != 12) throw new ArgumentException("六区奖励需要12个整数。");
            for (int i=0;i<6;i++) if (prizes[i*2]<1 || prizes[i*2]>100 || prizes[i*2+1]<0 || prizes[i*2+1]>prizes[i*2])
                throw new ArgumentException("区域奖励要求100>=第一>=第二>=0，第一>0。");
        }

        public static int[] SelectGroup(RodRun run, int face)
        {
            ValidateRun(run);
            if (run.phase != RodPhase.AwaitGroup || face < 0 || face >= 6) throw new InvalidOperationException("当前不能选组。");
            var result = new List<int>();
            foreach (var die in run.dice) if (die.region == -1 && die.face == face) result.Add(die.id);
            return result.ToArray();
        }

        public static bool CanRevive(RodRun run, int[] ids)
        {
            if (run == null || run.phase != RodPhase.Failed || ids == null) return false;
            ValidateRun(run);
            var unique = new HashSet<int>(); int total = 0;
            foreach (int id in ids)
            {
                if (!unique.Add(id)) return false;
                RodDie found = null;
                foreach (var die in run.dice) if (die.id == id) found = die;
                if (found == null || found.region < 0) return false;
                total += found.value;
            }
            return total == 6;
        }

        public static int NextFace(ref uint state)
        {
            if (state == 0) throw new ArgumentException("随机状态不可为0。");
            uint value;
            do { state ^= state << 13; state ^= state >> 17; state ^= state << 5; value = state - 1; }
            while (value >= 4294967292u);
            return (int)(value % 6);
        }

        public static void ValidateRun(RodRun run, int maxLevel = 200)
        {
            if (run != null && run.prizes != null) ValidatePrizes(run.prizes);
            if (run == null || string.IsNullOrEmpty(run.runId) || string.IsNullOrEmpty(run.contentVersion) ||
                run.levelId < 1 || run.levelId > maxLevel || run.randomState == 0 || run.round < 1 ||
                run.freeRerolls < 0 || run.freeRerolls > 2 || !Enum.IsDefined(typeof(RodPhase),run.phase))
                throw new InvalidOperationException("对局头信息无效。");
            if (run.dice == null || run.dice.Length != 9 || run.opponentPower == null || run.opponentPower.Length != 18)
                throw new InvalidOperationException("算筹或电脑数据缺失。");
            var ids = new HashSet<int>(); int hand = 0;
            foreach (var die in run.dice)
            {
                if (die == null || die.id < 0 || die.id >= 9 || !ids.Add(die.id) || die.value != Values[die.id] ||
                    die.face < 0 || die.face >= 6 || die.region < -1 || die.region >= 6 ||
                    (die.region >= 0 && die.region != die.face)) throw new InvalidOperationException("算筹数据不守恒或位置无效。");
                if (die.region == -1) hand++;
            }
            for (int p=0;p<3;p++)
            {
                int total=0;
                for (int f=0;f<6;f++) { int n=run.opponentPower[p*6+f]; if(n<0 || n>13) throw new InvalidOperationException("电脑筹力非法。"); total+=n; }
                if(total!=13) throw new InvalidOperationException("电脑筹力不守恒。");
            }
            if ((run.phase==RodPhase.Won || run.phase==RodPhase.Failed) && hand!=0) throw new InvalidOperationException("未落完不能结算。");
            if (run.phase==RodPhase.AwaitGroup && hand==0) throw new InvalidOperationException("落完必须结算。");
            if (run.phase==RodPhase.Won && !PlayerWins(ScoreRun(run))) throw new InvalidOperationException("胜利标记与比分不符。");
            if (run.phase==RodPhase.Failed && PlayerWins(ScoreRun(run))) throw new InvalidOperationException("失败标记与比分不符。");
            if(run.rewardCoin<0 || run.rewardReroll<0 || run.rewardSkip<0 || (run.doubleRewardClaimed && !run.baseRewardClaimed))
                throw new InvalidOperationException("结算奖励无效。");
        }
    }
}
