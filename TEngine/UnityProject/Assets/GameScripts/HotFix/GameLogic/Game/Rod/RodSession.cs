using System;
using System.Linq;
using System.Collections.Generic;

namespace GameLogic.Rod
{
    /// <summary>所有动作在副本提交后才公布。没有Unity、UI或广告SDK依赖。</summary>
    public sealed class RodSession
    {
        private RodSaveData state;
        private readonly RodCatalog catalog;
        private readonly Action<RodSaveData> persist;
        private readonly Func<RodSaveData,RodSaveData> clone;
        private readonly Func<DateTime> utc;
        public event Action Changed;
        public RodSaveData Snapshot => clone(state);
        public RodSession(RodCatalog catalog,RodSaveData initial,Action<RodSaveData> persist,
            Func<RodSaveData,RodSaveData> clone,Func<DateTime> utc=null)
        {
            catalog.Validate(); initial.Validate(catalog.levels.Length);
            this.catalog=catalog;this.persist=persist;this.clone=clone;this.utc=utc??(()=>DateTime.UtcNow);state=clone(initial);
            if(!string.IsNullOrEmpty(state.pendingActionId))
            {
                if(state.pendingAdCompleted) CompleteAd(state.pendingActionId,true);
                else Mutate(ClearPending); // 进程退出且未记录成功，不能猜测发奖。
            }
        }
        private void Mutate(Action<RodSaveData> action)
        {
            var next=clone(state); action(next); next.Validate(catalog.levels.Length);
            persist(next); state=next; Changed?.Invoke();
        }
        public void Checkpoint()=>persist(clone(state));
        /// <summary>音频设置与其他存档一样先提交副本；保存失败不得改变当前生效设置。</summary>
        public void SetAudioMuted(bool music, bool muted)=>Mutate(s=>{
            RequireIdle(s);
            if(music)s.musicMuted=muted;else s.soundMuted=muted;
        });
        public void RefreshDay()=>Mutate(UpdateDay);
        private void UpdateDay(RodSaveData s)
        {
            long ticks=Math.Max(utc().ToUniversalTime().Ticks,s.highestSeenUtcTicks);s.highestSeenUtcTicks=ticks;
            string key=new DateTime(ticks,DateTimeKind.Utc).AddHours(8).ToString("yyyy-MM-dd");
            if(s.dayKey!=key){s.dayKey=key;s.dailyClaimed=false;s.sharesClaimed=0;}
        }
        private static void RequireIdle(RodSaveData s)
        { if(!string.IsNullOrEmpty(s.pendingActionId)) throw new InvalidOperationException("请等待当前奖励操作结束。"); }
        public void Start(int id,uint seed)
        {
            Mutate(s=>{
                RequireIdle(s);
                if(id>s.highestUnlockedLevel) throw new InvalidOperationException("该关尚未解锁。");
                var l=catalog.Get(id);
                var r=new RodRun{runId=Guid.NewGuid().ToString("N"),levelId=id,contentVersion=catalog.contentVersion,
                    prizes=(int[])catalog.prizes.Clone(),randomState=seed==0?1:seed,phase=RodPhase.AwaitGroup,
                    opponentPower=l.OpponentPower(),dice=new RodDie[9],rewardCoin=l.coinReward,rewardReroll=l.rerollReward,rewardSkip=l.skipReward};
                for(int n=0;n<9;n++)r.dice[n]=new RodDie{id=n,value=RodCatalog.Values[n],face=RodRules.NextFace(ref r.randomState)};
                if(!s.tutorialCompleted && l.firstRoll!=null && l.firstRoll.Length==9)
                    for(int n=0;n<9;n++)r.dice[n].face=l.firstRoll[n];
                s.activeRun=r;s.currentLevelId=id;s.contentVersion=catalog.contentVersion;
            });
        }
        public void Place(int face)=>Mutate(s=>{
            RequireIdle(s);var r=s.activeRun;var ids=RodRules.SelectGroup(r,face);
            if(ids.Length==0)throw new InvalidOperationException("请选择手中一种花色。");
            foreach(int id in ids)r.dice[id].region=face;
            if(r.dice.Any(d=>d.region<0)){r.round++;Roll(r);}else Finish(s);
            s.tutorialStep="placed";
        });
        private static void Roll(RodRun r){foreach(var d in r.dice)if(d.region<0)d.face=RodRules.NextFace(ref r.randomState);}
        private static RodRun Active(RodSaveData s)
        {
            if(s.activeRun==null || s.activeRun.phase!=RodPhase.AwaitGroup)throw new InvalidOperationException("当前不能使用此道具。");
            return s.activeRun;
        }
        public string ToolCost(bool skip)
        {
            var s=state;var r=s.activeRun;
            if(!skip && r!=null && r.freeRerolls>0)return "免费 "+r.freeRerolls;
            int count=skip?s.skipItems:s.rerollItems;if(count>0)return "数量 "+count;
            int price=skip?200:100;return s.coins>=price?"金币 "+price:"广告";
        }
        public bool NeedsAd(bool skip)=>ToolCost(skip)=="广告";
        public void UseTool(bool skip)=>Mutate(s=>{
            RequireIdle(s);var r=Active(s);if(skip && r.levelId>=catalog.levels.Length)throw new InvalidOperationException("已经是最后一关。");
            if(!skip && r.freeRerolls>0)r.freeRerolls--;
            else if(skip && s.skipItems>0)s.skipItems--;
            else if(!skip && s.rerollItems>0)s.rerollItems--;
            else {int price=skip?200:100;if(s.coins<price)throw new InvalidOperationException("需要观看广告。");s.coins-=price;}
            if(skip)Skip(s);else Roll(r);
        });
        private static RodLevelProgress Record(RodSaveData s,int id)
        {var p=s.levels.Find(x=>x.levelId==id);if(p==null){p=new RodLevelProgress{levelId=id};s.levels.Add(p);}return p;}
        private void Unlock(RodSaveData s,int id){s.highestUnlockedLevel=Math.Max(s.highestUnlockedLevel,Math.Min(id+1,catalog.levels.Length));}
        private void Skip(RodSaveData s){var r=Active(s);r.phase=RodPhase.Skipped;Record(s,r.levelId).skipped=true;Unlock(s,r.levelId);}
        private static void Reward(RodSaveData s,RodRun r)
        {checked{s.coins+=r.rewardCoin;s.rerollItems+=r.rewardReroll;s.skipItems+=r.rewardSkip;}}
        private void Finish(RodSaveData s)
        {
            var r=s.activeRun;r.phase=RodRules.PlayerWins(RodRules.ScoreRun(r))?RodPhase.Won:RodPhase.Failed;
            if(r.phase!=RodPhase.Won)return;
            var p=Record(s,r.levelId);p.won=true;Unlock(s,r.levelId);s.tutorialCompleted=true;
            if(!p.firstRewardClaimed){Reward(s,r);p.firstRewardClaimed=true;r.baseRewardClaimed=true;}
            else{r.rewardCoin=0;r.rewardReroll=0;r.rewardSkip=0;}
        }
        public void ClaimDaily()=>Mutate(s=>{RequireIdle(s);UpdateDay(s);if(s.dailyClaimed)throw new InvalidOperationException("今日已领取。");s.dailyClaimed=true;checked{s.coins+=100;}});
        // 分享SDK无可靠完成回调；本地版本按发起分享计次，文档明确，不伪装验证完成。
        public void ClaimShare()=>Mutate(s=>{RequireIdle(s);UpdateDay(s);if(s.sharesClaimed>=3)throw new InvalidOperationException("今日分享次数已用完。");s.sharesClaimed++;checked{s.coins+=50;}});
        public string BeginAd(string kind,int[] ids=null)
        {
            string token=Guid.NewGuid().ToString("N");
            Mutate(s=>{RequireIdle(s);CheckAd(s,kind,ids);s.pendingActionId=token;s.pendingActionKind=kind;
                s.pendingActionRunId=kind=="coin"?null:s.activeRun.runId;s.pendingDieIds=ids==null?null:(int[])ids.Clone();});
            return token;
        }
        private void CheckAd(RodSaveData s,string kind,int[] ids)
        {
            switch(kind){
            case "coin":return;
            case "reroll":Active(s);return;
            case "skip":if(Active(s).levelId>=catalog.levels.Length)throw new InvalidOperationException("最后一关不可跳过。");return;
            case "revive":if(!RodRules.CanRevive(s.activeRun,ids))throw new InvalidOperationException("请选择恰好6点算筹。");return;
            case "double":var r=s.activeRun;if(r==null||r.phase!=RodPhase.Won||!r.baseRewardClaimed||r.doubleRewardClaimed)throw new InvalidOperationException("没有可翻倍奖励。");return;
            default:throw new ArgumentException("未知广告奖励。");}
        }
        public void CompleteAd(string token,bool completed)
        {
            if(string.IsNullOrEmpty(token)||state.pendingActionId!=token)return;
            if(!completed){Mutate(ClearPending);return;}
            if(!state.pendingAdCompleted)Mutate(s=>s.pendingAdCompleted=true);
            Mutate(s=>{
                string kind=s.pendingActionKind;
                if(kind!="coin" && (s.activeRun==null||s.activeRun.runId!=s.pendingActionRunId)) {ClearPending(s);return;}
                CheckAd(s,kind,s.pendingDieIds);
                switch(kind){
                case "coin":checked{s.coins+=30;}break;
                case "reroll":Roll(s.activeRun);break;
                case "skip":Skip(s);break;
                case "double":Reward(s,s.activeRun);s.activeRun.doubleRewardClaimed=true;break;
                case "revive":foreach(int id in s.pendingDieIds)s.activeRun.dice[id].region=-1;
                    s.activeRun.phase=RodPhase.AwaitGroup;s.activeRun.round++;Roll(s.activeRun);break;
                }
                s.lastAppliedActionId=token;ClearPending(s);
            });
        }
        private static void ClearPending(RodSaveData s)
        {s.pendingActionId=null;s.pendingActionRunId=null;s.pendingActionKind=null;s.pendingDieIds=null;s.pendingAdCompleted=false;}
    }
}
