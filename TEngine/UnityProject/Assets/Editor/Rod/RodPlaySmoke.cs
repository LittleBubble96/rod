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
                var host=UnityEngine.Object.FindObjectOfType<RodGameHost>();if(host==null||host.Session==null)return;
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
                    Click(host,"Rules");Click(host,"Understood");Finish(true,"home, daily claim, whole-group placement, result modal, return, rules");
                }
            }catch(Exception e){Finish(false,e.ToString());}
        }
        private static void Finish(bool success,string detail)
        {
            SessionState.SetBool(Key,false);
            Debug.Log("[RodPlaySmoke] "+(success?"PASS ":"FAIL ")+detail);
            if(Application.isBatchMode)EditorApplication.Exit(success?0:1);else EditorApplication.isPlaying=false;
        }
    }
}
