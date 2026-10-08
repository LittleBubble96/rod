using System;
using UnityEngine;

namespace GameLogic.Rod
{
    /// <summary>新算筹本地档，独立于旧答案索引存档。提交失败向上抛出。</summary>
    public sealed class RodSaveRepository
    {
        private readonly IReadCacheFileSystem storage;
        public RodSaveRepository(IReadCacheFileSystem storage) { this.storage = storage ?? throw new ArgumentNullException(nameof(storage)); }
        public static RodSaveRepository CreateLocal()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return new RodSaveRepository(new WeChatReadCacheFileSystem("rodSave.v1"));
#else
            return new RodSaveRepository(new WindowReadCacheFileSystem("rod_save_v1.json"));
#endif
        }
        public RodSaveData Load()
        {
            string json = storage.ReadCache();
            if (string.IsNullOrEmpty(json)) return new RodSaveData();
            var state = JsonUtility.FromJson<RodSaveData>(json);
            if (state == null) throw new InvalidOperationException("算筹存档解析失败，原数据已保留。");
            // 旧v1没有hasActiveRun。只将空头/零关号/空算筹的内联占位对象还原为null。
            if(!state.hasActiveRun && state.activeRun!=null && string.IsNullOrEmpty(state.activeRun.runId) &&
                state.activeRun.levelId==0 && (state.activeRun.dice==null || state.activeRun.dice.Length==0))state.activeRun=null;
            if(state.activeRun!=null && (state.activeRun.prizes==null || state.activeRun.prizes.Length==0) &&
                state.activeRun.contentVersion=="rod-200-draft-1")state.activeRun.prizes=RodRules.LegacyPrizes;
            state.Validate();
            return state;
        }
        public void Commit(RodSaveData next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            next.Validate();
            storage.WriteCache(JsonUtility.ToJson(next.Copy()));
        }
    }
}
