using System;
using System.Linq;
using GameLogic.Rod;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Rod.Editor
{
    [InitializeOnLoad]
    public static class RodPlaySmoke
    {
        private const string Key="Rod.PlaySmoke.Active";
        private static double nextTick;
        static RodPlaySmoke(){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
        public static void Run()
        {
            SessionState.SetBool(Key,true);SessionState.SetInt("Rod.PlaySmoke.Step",0);SessionState.SetFloat("Rod.PlaySmoke.Start",(float)EditorApplication.timeSinceStartup);
            EditorSceneManager.OpenScene(RodUIBuilder.ScenePath);EditorApplication.isPlaying=true;
        }
        private static void Log(string text,string trace,LogType type)
        {if(SessionState.GetBool(Key,false)&&(type==LogType.Exception||type==LogType.Error))Finish(false,text);}
        private static void Click(RodGameHost host,string name)
        {var b=host.GetComponentsInChildren<Button>().FirstOrDefault(x=>x.name==name&&x.interactable);if(b==null)throw new Exception("Button unavailable: "+name);b.onClick.Invoke();}
        private static void Tick()
        {
            if(!SessionState.GetBool(Key,false))return;
            if(EditorApplication.timeSinceStartup-SessionState.GetFloat("Rod.PlaySmoke.Start",0)>120){Finish(false,"timeout");return;}
            if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<nextTick)return;nextTick=EditorApplication.timeSinceStartup+.25;
            try{
                var host=UnityEngine.Object.FindObjectOfType<RodGameHost>();if(host==null||!host.IsReady)return;
                int step=SessionState.GetInt("Rod.PlaySmoke.Step",0);
                if(step==0){Click(host,"GetCoin");SessionState.SetInt("Rod.PlaySmoke.Step",1);}
                else if(step==1){Click(host,"Daily");if(host.Session.Snapshot.coins!=100)throw new Exception("daily UI reward");Click(host,"Close");Click(host,"Start");SessionState.SetInt("Rod.PlaySmoke.Step",2);}
                else if(step==2){
                    var r=host.Session.Snapshot.activeRun;
                    if(r.phase!=RodPhase.AwaitGroup){SessionState.SetInt("Rod.PlaySmoke.Step",3);return;}
                    var face=r.dice.First(d=>d.region<0).face;Click(host,"Die_"+RodCatalog.Faces[face]);Click(host,"Confirm");
                }else if(step==3){
                    var r=host.Session.Snapshot.activeRun;var modal=host.GetComponentsInChildren<Transform>().Any(t=>t.name=="Modal_won"||t.name=="Modal_failed");
                    if(!modal)throw new Exception("terminal modal missing");
                    if(r.phase==RodPhase.Won)Click(host,"Home");else{Click(host,"Close");Click(host,"Back");}
                    Click(host,"Rules");Click(host,"Understood");
                    // 用真实规则寻找可复现的失败局，再通过正式按钮重走；不修改局面伪造失败。
                    uint seed=FindFailureSeed(host);
                    host.Session.Start(1,seed);Click(host,"Start");SessionState.SetInt("Rod.PlaySmoke.Step",4);
                }else if(step==4){
                    var run=host.Session.Snapshot.activeRun;
                    if(run.phase==RodPhase.AwaitGroup){Click(host,"Die_"+RodCatalog.Faces[run.dice.First(d=>d.region<0).face]);Click(host,"Confirm");return;}
                    if(run.phase!=RodPhase.Failed)throw new Exception("deterministic failure fixture diverged");
                    SessionState.SetString("Rod.PlaySmoke.Opponents",string.Join(",",run.opponentPower));
                    SessionState.SetInt("Rod.PlaySmoke.FreeRerolls",run.freeRerolls);
                    Click(host,"Revive");Click(host,"Close");
                    if(!host.GetComponentsInChildren<Transform>().Any(t=>t.name=="Modal_failed"))throw new Exception("revive close must return to failed modal");
                    Click(host,"Revive");
                    var modal=host.GetComponentsInChildren<Transform>().First(t=>t.name=="Modal_revive");
                    var dice=modal.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Die_")).ToArray();
                    // 每次点击都会刷新 View；持有的按钮必须由节点池保留并正确重绑。
                    foreach(int index in new[]{0,1,3})dice[index].onClick.Invoke();
                    Click(host,"ReviveAd");SessionState.SetInt("Rod.PlaySmoke.Step",5);
                }else if(step==5){
                    var run=host.Session.Snapshot.activeRun;
                    if(run.phase==RodPhase.Failed)return; // 等待真实 Editor SDK 完成回调。
                    if(run.phase!=RodPhase.AwaitGroup||run.dice.Where(d=>d.region<0).Sum(d=>d.value)!=6)throw new Exception("revival must restore exactly six points");
                    if(string.Join(",",run.opponentPower)!=SessionState.GetString("Rod.PlaySmoke.Opponents",""))throw new Exception("revival changed opponent layout");
                    if(run.freeRerolls!=SessionState.GetInt("Rod.PlaySmoke.FreeRerolls",-1))throw new Exception("revival replenished rerolls");
                    if(host.GetComponentsInChildren<Transform>().Any(t=>t.name=="Modal_revive"||t.name=="Modal_failed"))throw new Exception("revival left stale modal");
                    Click(host,"Back");
                    Finish(true,"home, daily, placement, result, rules, revive close/return, six-point Editor-ad revival, unchanged opponents and rerolls");
                }
            }catch(Exception e){Finish(false,e.ToString());}
        }
        /// <summary>在隔离内存 Session 上求失败种子，仅测试使用，不写玩家档或改关卡配置。</summary>
        private static uint FindFailureSeed(RodGameHost host)
        {
            var catalog=JsonUtility.FromJson<RodCatalog>(host.levelConfig.text);
            var baseline=host.Session.Snapshot;
            for(uint seed=1;seed<=256;seed++){
                // 每次从相同教学状态开始，前一个候选局获胜不能改变下一次的首投规则。
                var fixture=new RodSession(catalog,baseline,_=>{},s=>s.Copy());
                fixture.Start(1,seed);
                while(fixture.Snapshot.activeRun.phase==RodPhase.AwaitGroup){
                    var run=fixture.Snapshot.activeRun;fixture.Place(run.dice.First(d=>d.region<0).face);
                }
                if(fixture.Snapshot.activeRun.phase==RodPhase.Failed)return seed;
            }
            throw new Exception("No deterministic failure fixture in first 256 seeds");
        }
        private static void Finish(bool success,string detail)
        {
            SessionState.SetBool(Key,false);
            Debug.Log("[RodPlaySmoke] "+(success?"PASS ":"FAIL ")+detail);
            if(Application.isBatchMode)EditorApplication.Exit(success?0:1);else EditorApplication.isPlaying=false;
        }
    }
}
