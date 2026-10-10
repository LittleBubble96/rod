using System;
using System.IO;
using System.Globalization;
using GameLogic.Rod;

/// <summary>
/// 离线分数敏感性实验。输入由正式关卡 JSON 导出的布局，使用生产计分与随机函数。
/// 贪心策略只看当前比分差，不预测未来；结果不能解释为玩家胜率或好玩程度。
/// 此工具只输出报告，不写游戏配置、见证或玩家存档。
/// </summary>
internal static class RodPrizeAnalysis
{
    private static readonly int[] Values = { 3, 2, 2, 1, 1, 1, 1, 1, 1 };
    private static readonly string[] Names = { "原图", "当前统一（对照）", "差异化候选A", "强差异候选B" };
    private static readonly int[][] Prizes = {
        new[] {10,5,10,8,9,6,8,7,8,5,7,5},
        new[] {10,5,10,5,10,5,10,5,10,5,10,5},
        new[] {12,4,10,8,11,5,9,6,8,5,7,3},
        new[] {14,3,10,8,12,4,9,6,8,4,7,3}
    };

    private sealed class Stats
    {
        internal int Games, Wins, Close, Ties, InvalidWitness, Actions, Takeovers, Cancellations;
        internal readonly long[] Investment = new long[6];
    }

    public static void Main(string[] args)
    {
        int trials = int.Parse(args[1]);
        string[] layouts = File.ReadAllLines(args[0]);
        Console.WriteLine("# 差异化计分敏感性实验\n");
        Console.WriteLine("使用当前正式 JSON 的200个电脑布局；每关每表" + trials + "局，四表共" + (layouts.Length * trials * 4) + "局。相同起始种子、两次免费重投，无广告/复活/付费，不应用教学首投。布局原按统一10/5筛选，有选择偏差。");
        Console.WriteLine("\n策略：最大化当前玩家分减最高电脑分，其次玩家分、组枚数、面序。没有改善且仍有免费次数则重投。使用生产 RodRules.Score/NextFace。\n");
        Console.WriteLine("争夺代理指标：夺首率=落子后在该区升为第一且挤掉原电脑第一的动作比例；同值抵消率=落子后自身与任意电脑同正筹力的动作比例。仅描述本策略行为，不代表真人竞争强度。接近结局=玩家分与最高电脑分差绝对值≤3。\n");
        Console.WriteLine("|方案|模拟通关率|接近结局|夺首动作|同值抵消动作|原见证失效关数|六区筹力投入占比（风花雪月方圆）|");
        Console.WriteLine("|---|---:|---:|---:|---:|---:|---|");
        for (int scheme = 0; scheme < Prizes.Length; scheme++)
        {
            var stats = new Stats();
            foreach (string line in layouts)
            {
                string[] cells = line.Split(',');
                int id = int.Parse(cells[0]);
                var power = new int[4, 6];
                for (int p = 1; p < 4; p++) for (int f = 0; f < 6; f++) power[p, f] = int.Parse(cells[1 + (p - 1) * 6 + f]);
                for (int f = 0; f < 6; f++) power[0, f] = int.Parse(cells[19 + f]);
                if (!RodRules.PlayerWins(RodRules.Score(power, Prizes[scheme]))) stats.InvalidWitness++;
                for (int trial = 0; trial < trials; trial++)
                    Simulate(power, Prizes[scheme], unchecked((uint)(20261010 + id * 100003 + trial * 97)), stats);
            }
            string[] investments = new string[6];
            for (int f = 0; f < 6; f++) investments[f] = Percent(stats.Investment[f], stats.Games * 13);
            Console.WriteLine("|" + Names[scheme] + "|" + Percent(stats.Wins, stats.Games) + "|" + Percent(stats.Close, stats.Games) + "|" + Percent(stats.Takeovers, stats.Actions) + "|" + Percent(stats.Cancellations, stats.Actions) + "|" + stats.InvalidWitness + "|" + string.Join(" / ", investments) + "|");
        }
        Console.WriteLine("\n各差异化表最大发分均为88，统一对照为90。原见证失效只说明原解不可复用，不等于关卡无解。不同策略会消耗不同投掷轮次；相同初始种子并不保证同一时间作同一决策。未模拟电脑中途反应，因为当前玩法电脑先固定落筹。\n");
    }

    private static string Percent(long n, long d) => (100.0 * n / d).ToString("F1", CultureInfo.InvariantCulture) + "%";
    private static int Margin(int[] scores) => scores[0] - Math.Max(scores[1], Math.Max(scores[2], scores[3]));

    /// <summary>返回抵消后的区域第一名；没人有资格时返回 -1。</summary>
    private static int First(int[,] power, int face)
    {
        int leader = -1, best = 0;
        for (int p = 0; p < 4; p++)
        {
            int value = power[p, face], count = 0;
            if (value <= best) continue;
            for (int q = 0; q < 4; q++) if (power[q, face] == value) count++;
            if (count == 1) { leader = p; best = value; }
        }
        return leader;
    }

    private static void Simulate(int[,] power, int[] prizes, uint random, Stats stats)
    {
        for (int f = 0; f < 6; f++) power[0, f] = 0;
        var hand = new bool[9];
        var rolled = new int[9];
        var strength = new int[6];
        var count = new int[6];
        for (int i = 0; i < 9; i++) hand[i] = true;
        int remaining = 9, free = 2;
        while (remaining > 0)
        {
            Array.Clear(strength, 0, 6); Array.Clear(count, 0, 6);
            // 每轮为九枚生成完整随机样本，避免手牌枚数改变同轮其他骰子的随机索引。
            for (int i = 0; i < 9; i++)
            {
                rolled[i] = RodRules.NextFace(ref random);
                if (hand[i]) { strength[rolled[i]] += Values[i]; count[rolled[i]]++; }
            }
            int best = -1, bestMargin = int.MinValue, bestScore = -1, bestCount = -1;
            int current = Margin(RodRules.Score(power, prizes));
            for (int f = 0; f < 6; f++)
            {
                if (count[f] == 0) continue;
                power[0, f] += strength[f];
                int[] score = RodRules.Score(power, prizes);
                power[0, f] -= strength[f];
                int margin = Margin(score);
                if (margin > bestMargin || (margin == bestMargin && (score[0] > bestScore || (score[0] == bestScore && count[f] > bestCount))))
                { best = f; bestMargin = margin; bestScore = score[0]; bestCount = count[f]; }
            }
            if (free > 0 && bestMargin <= current) { free--; continue; }
            int previous = First(power, best);
            power[0, best] += strength[best];
            stats.Actions++;
            if (previous > 0 && First(power, best) == 0) stats.Takeovers++;
            for (int p = 1; p < 4; p++)
                if (power[p, best] == power[0, best]) { stats.Cancellations++; break; }
            stats.Investment[best] += strength[best];
            for (int i = 0; i < 9; i++) if (hand[i] && rolled[i] == best) { hand[i] = false; remaining--; }
        }
        int finalMargin = Margin(RodRules.Score(power, prizes));
        stats.Games++;
        if (finalMargin >= 0) stats.Wins++;
        if (finalMargin == 0) stats.Ties++;
        if (Math.Abs(finalMargin) <= 3) stats.Close++;
    }
}
