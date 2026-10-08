using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameSDK;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameLogic.Rod
{
    /// <summary>Unity生命周期和Presenter；存储/玩法在独立对象中，不由View修改。</summary>
    public sealed class RodGameHost : MonoBehaviour
    {
        public TextAsset levelConfig;
        public RodUISkin skin;
        public bool usePreviewSave;
        private RodGameView view;
        private RodSession session;
        private RodViewModel vm=new RodViewModel();
        private bool starting,disposed;
        private float toastUntil;
        private readonly UniTaskCompletionSource initialized=new UniTaskCompletionSource();
        public RodSession Session=>session;
        private void Start(){InitializeAsync().Forget();}
        public async UniTask InitializeAsync()
        {
            if(starting){await initialized.Task;return;}starting=true;
            try{
                if(skin==null||levelConfig==null)throw new InvalidOperationException("游戏资源缺失，请执行六面算筹/生成UI与预览场景。");
                SDK.InitializeAndLogin();
                if(EventSystem.current==null){var e=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));}
                view=GetComponent<RodGameView>();if(view==null)view=gameObject.AddComponent<RodGameView>();view.Initialize(skin);vm.busy=true;view.Render(vm);
                await UniTask.Yield();
                var catalog=JsonUtility.FromJson<RodCatalog>(levelConfig.text);catalog.Validate();
                var repo=usePreviewSave?(Application.isBatchMode?new RodSaveRepository(new MemoryStorage()):new RodSaveRepository(new WindowReadCacheFileSystem("rod_preview_v1.json"))):RodSaveRepository.CreateLocal();
                session=new RodSession(catalog,repo.Load(),repo.Commit,s=>s.Copy());
                session.RefreshDay();
                view.Action+=Handle;session.Changed+=Refresh;AdSystem.StateChanged+=AdChanged;
                AdSystem.EnterPage(AdIds.RewardedVideo);vm.busy=false;vm.message=null;Refresh();initialized.TrySetResult();
            }catch(Exception e){vm.busy=false;vm.message="启动失败，原存档已保留。\n"+e.Message;view?.Render(vm);initialized.TrySetException(e);Debug.LogException(e);throw;}
        }
        private void Update(){if(session!=null&&!string.IsNullOrEmpty(vm.message)&&Time.unscaledTime>=toastUntil){vm.message=null;Refresh();}}
        private void Toast(string text){vm.message=text;toastUntil=Time.unscaledTime+4;}
        private uint Seed(){var bytes=Guid.NewGuid().ToByteArray();uint n=BitConverter.ToUInt32(bytes,0);return n==0?1:n;}
        private void Handle(string action,int value)
        {
            if(session==null||vm.busy)return;
            try{
                vm.message=null;
                var s=session.Snapshot;
                switch(action){
                case "start":if(s.activeRun==null)session.Start(s.currentLevelId,Seed());vm.home=false;ShowTerminal();break;
                case "home":session.Checkpoint();vm.home=true;vm.popup=null;vm.selectedFace=-1;break;
                case "rules":vm.popup="rules";break;
                case "coins":session.RefreshDay();vm.popup="coins";break;
                case "close":vm.popup=null;break;
                case "select":if(s.activeRun!=null&&s.activeRun.phase==RodPhase.AwaitGroup&&RodRules.SelectGroup(s.activeRun,value).Length>0)vm.selectedFace=value;break;
                case "place":session.Place(vm.selectedFace);vm.selectedFace=-1;ShowTerminal();break;
                case "restart":session.Start(s.activeRun.levelId,Seed());vm.selectedFace=-1;vm.popup=null;break;
                case "next":session.Start(s.activeRun.levelId+1,Seed());vm.selectedFace=-1;vm.popup=null;vm.home=false;break;
                case "reroll":case "skip":
                    if(session.NeedsAd(action=="skip")){ShowAd(action).Forget();return;}
                    session.UseTool(action=="skip");vm.selectedFace=-1;ShowTerminal();break;
                case "daily":session.ClaimDaily();Toast("已领取100金币");break;
                case "shareCoin":
                    // 沿用旧业务的发起分享即奖励口径，平台未提供可靠分享完成证明。
                    if(s.sharesClaimed>=3)throw new InvalidOperationException("今日次数已用完。");
                    SDK.ShareAppMessage("一起来玩六面算筹");session.ClaimShare();Toast("已领取50金币");break;
                case "share":SDK.ShareAppMessage("我在六面算筹完成了挑战");break;
                case "adCoin":ShowAd("coin").Forget();return;
                case "double":ShowAd("double").Forget();return;
                case "revive":vm.revival.Clear();vm.popup="revive";break;
                case "pick":if(!vm.revival.Add(value))vm.revival.Remove(value);break;
                case "reviveAd":ShowAd("revive").Forget();return;
                }
            }catch(Exception e){Toast("操作未完成："+e.Message);Debug.LogWarning(e);}
            Refresh();
        }
        private async UniTaskVoid ShowAd(string kind)
        {
            if(vm.busy)return;
            if(!AdSystem.IsAdAvailable(AdIds.RewardedVideo)){AdSystem.Preload(AdIds.RewardedVideo);Toast("广告准备中，请稍后再试");Refresh();return;}
            string token=null;vm.busy=true;
            try{
                token=session.BeginAd(kind,kind=="revive"?vm.revival.ToArray():null);Refresh();
                bool complete=await AdSystem.ShowRewardedAdAsync(AdIds.RewardedVideo);
                if(disposed)return;
                session.CompleteAd(token,complete);
                if(complete){vm.selectedFace=-1;if(kind=="revive")vm.popup=null;if(kind=="skip")ShowTerminal();Toast("已领取奖励");}
                else Toast("广告未完成，未消耗资源");
            }catch(Exception e){Toast("奖励未完成："+e.Message+"。请重启恢复。");Debug.LogException(e);}
            finally{vm.busy=false;if(!disposed)Refresh();}
        }
        private void ShowTerminal(){var r=session.Snapshot.activeRun;vm.popup=r.phase==RodPhase.Won?"won":r.phase==RodPhase.Failed?"failed":r.phase==RodPhase.Skipped?"skipped":null;}
        private void AdChanged(string id){if(!disposed&&session!=null)Refresh();}
        private void Refresh()
        {
            if(disposed||session==null||view==null)return;
            vm.save=session.Snapshot;vm.rerollCost=session.ToolCost(false);vm.skipCost=session.ToolCost(true);
            vm.adReady=AdSystem.IsAdAvailable(AdIds.RewardedVideo);vm.adLoading=AdSystem.IsLoading(AdIds.RewardedVideo);view.Render(vm);
        }
        private void Save(){if(session==null)return;try{session.Checkpoint();}catch(Exception e){Toast("保存失败，请重试："+e.Message);Debug.LogException(e);Refresh();}}
        private void OnApplicationPause(bool paused){if(paused)Save();else if(session!=null&&!vm.busy){try{session.RefreshDay();}catch(Exception e){Toast(e.Message);}}}
        private void OnApplicationFocus(bool focused){if(!focused)Save();}
        private void OnApplicationQuit()=>Save();
        private void OnDestroy(){disposed=true;if(view!=null)view.Action-=Handle;if(session!=null)session.Changed-=Refresh;AdSystem.StateChanged-=AdChanged;AdSystem.Release(AdIds.RewardedVideo);}
        private sealed class MemoryStorage:IReadCacheFileSystem
        {private string json;public string ReadCache()=>json;public void WriteCache(string value)=>json=value;public void DeleteAll()=>json=null;}
    }
}
