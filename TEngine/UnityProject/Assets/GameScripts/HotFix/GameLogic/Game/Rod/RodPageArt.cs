using System;
using UnityEngine;

namespace GameLogic.Rod
{
    /// <summary>固定资源槽位；顺序写入皮肤资产，新增只能追加，不能重排已有槽位。</summary>
    public enum RodArt
    {
        Background, HomeCoin, HomeStart, HomeRules, HomeRound, HeaderCoin, HeaderLevel, HeaderBack,
        ScoreYou, ScoreBot, Region, RegionSecond, Tray, Tool, Confirm, Summary,
        DieOne, DieTwo, DieThree, DieSelection, Check, RegionSelection,
        ModalFrame, ModalRow, ModalButton, Coin, DailyArt, ShareArt, AdArt,
        Wind, Flower, Snow, Moon, Square, Circle,
        ClubIcon, ShareIcon, SettingsIcon, RerollIcon, NextIcon, BackIcon, CloseIcon, PlusIcon, HelpIcon, AdIcon,
        RuleDice, RuleSelect, RulePlace, Count
    }

    /// <summary>规则插画的独立图层与原稿局部坐标。资源由页面表持有，显示节点不拥有纹理。</summary>
    [Serializable]
    public sealed class RodRulePart
    {
        public int section;
        public string name;
        public Sprite sprite;
        public Rect rect;
        public string text;
        public float fontSize;
        public Color color = Color.white;

        /// <summary>对应旧插画容器的原稿尺寸，拆图不改变现有界面布局。</summary>
        public static Vector2 SourceSize(int section)
            => section == 1 ? new Vector2(464,158) : section == 2 ? new Vector2(418,199) : new Vector2(535,238);
    }

    /// <summary>页面Sprite与共享字形材质。由资源系统随RodSkin持有，运行时不生成材质或纹理。</summary>
    public sealed class RodPageArt : ScriptableObject
    {
        public Sprite[] sprites = new Sprite[(int)RodArt.Count];
        public Sprite homeCoinIcon;
        public Sprite headerAdd;
        public RodRulePart[] ruleParts = Array.Empty<RodRulePart>();
        public Sprite this[RodArt id] => sprites[(int)id];

        /// <summary>导入时和入口初始化时检查，避免缺图时静默混入旧占位资源。</summary>
        public void Validate()
        {
            if (sprites == null || sprites.Length != (int)RodArt.Count)
                throw new InvalidOperationException("页面散图资源表缺失。");
            for (int i = 0; i < sprites.Length; i++)
            {
                // 三个旧组合截图槽位只保留序号，实际改用独立规则图层。
                if (i >= (int)RodArt.RuleDice && i <= (int)RodArt.RulePlace) continue;
                if (!sprites[i]) throw new InvalidOperationException("页面资源缺失：" + (RodArt)i);
            }
            if (!homeCoinIcon || !headerAdd || ruleParts == null || ruleParts.Length == 0)
                throw new InvalidOperationException("页面拆分图层缺失。");
            foreach (var part in ruleParts)
                if (part == null || (!part.sprite && string.IsNullOrEmpty(part.text)))
                    throw new InvalidOperationException("规则图层引用缺失。");
        }
    }
}
