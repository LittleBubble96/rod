using System;
using System.IO;
using System.Linq;
using GameLogic.Rod;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Rod.Editor
{
    public static class RodEditorValidation
    {
        private static int assertions;
        private static void Check(bool condition,string label){assertions++;if(!condition)throw new Exception(label);}
        private static T Copy<T>(T value)
        {if(value is RodSaveData state)return (T)(object)state.Copy();if(value is RodRun run)return (T)(object)run.Copy();return JsonUtility.FromJson<T>(JsonUtility.ToJson(value));}
        private static void Throws(Action a,string label){try{a();}catch(InvalidOperationException){assertions++;return;}throw new Exception(label);}
        [MenuItem("六面算筹/验证玩法与UI")]
        public static void Run()
        {
            assertions=0;
            Builder.LanguageGenerateEditor.Generate();
            RodUIBuilder.Generate();
            var catalog=JsonUtility.FromJson<RodCatalog>(File.ReadAllText(RodLevelEditor.CatalogPath));catalog.Validate();
            RodSaveData disk=null;bool fail=false;int writes=0;
            Action<RodSaveData> store=s=>{if(fail)throw new IOException("injected disk full");disk=Copy(s);writes++;};
            var clock=new DateTime(2026,10,8,2,0,0,DateTimeKind.Utc);
            var session=new RodSession(catalog,new RodSaveData{highestUnlockedLevel=200,tutorialCompleted=true},store,Copy,()=>clock);
            session.ClaimDaily();Check(session.Snapshot.coins==100,"daily reward");Throws(session.ClaimDaily,"duplicate daily");
            clock=clock.AddDays(-1);Throws(session.ClaimDaily,"clock rollback daily");clock=clock.AddDays(2);session.ClaimDaily();Check(session.Snapshot.coins==200,"next day");
            for(int n=0;n<3;n++)session.ClaimShare();Throws(session.ClaimShare,"fourth share");
            session.Start(1,43);var old=session.Snapshot;session.UseTool(false);Check(session.Snapshot.activeRun.freeRerolls==1&&session.Snapshot.coins==old.coins,"first free reroll");
            session.UseTool(false);session.UseTool(false);Check(session.Snapshot.coins==old.coins-100,"paid reroll");
            old=session.Snapshot;fail=true;try{session.UseTool(false);throw new Exception("missing failure");}catch(IOException){assertions++;}fail=false;
            Check(JsonUtility.ToJson(session.Snapshot)==JsonUtility.ToJson(old),"failed commit does not mutate state or RNG");
            var token=session.BeginAd("coin");session.CompleteAd(token,false);Check(session.Snapshot.coins==old.coins,"cancel ad no reward");
            token=session.BeginAd("coin");session.CompleteAd(token,true);session.CompleteAd(token,true);Check(session.Snapshot.coins==old.coins+30,"duplicate callback once");
            token=session.BeginAd("coin");var interrupted=Copy(disk);interrupted.pendingAdCompleted=true;
            var recovered=new RodSession(catalog,interrupted,store,Copy,()=>clock);Check(recovered.Snapshot.coins==old.coins+60&&recovered.Snapshot.pendingActionId==null,"persisted completed ad recovery");
            session=recovered;
            RodRun failed=null,won=null;
            for(int id=1;id<=200;id++){
                session.Start(id,(uint)(id*129+8));int rounds=0;
                while(session.Snapshot.activeRun.phase==RodPhase.AwaitGroup){var r=session.Snapshot.activeRun;session.Place(r.dice.First(d=>d.region<0).face);Check(++rounds<=9,"finite round count");}
                var end=session.Snapshot.activeRun;Check(end.dice.All(d=>d.region>=0),"all placed");
                if(end.phase==RodPhase.Failed)failed=Copy(end);if(end.phase==RodPhase.Won)won=Copy(end);
            }
            Check(failed!=null&&won!=null,"win and failure test fixtures");
            var revival=new RodSession(catalog,new RodSaveData{highestUnlockedLevel=200,currentLevelId=failed.levelId,activeRun=failed},store,Copy);
            token=revival.BeginAd("revive",new[]{0,1,3});revival.CompleteAd(token,true);
            Check(revival.Snapshot.activeRun.dice.Where(d=>d.region<0).Sum(d=>d.value)==6,"revive six points");
            Check(revival.Snapshot.activeRun.opponentPower.SequenceEqual(failed.opponentPower),"revive keeps computers");
            Check(revival.Snapshot.activeRun.freeRerolls==failed.freeRerolls,"revive does not replenish free rolls");
            var skip=new RodSession(catalog,new RodSaveData{coins=200,highestUnlockedLevel=200},store,Copy);skip.Start(1,4);skip.UseTool(true);
            Check(skip.Snapshot.coins==0&&skip.Snapshot.activeRun.phase==RodPhase.Skipped&&!skip.Snapshot.levels[0].won,"skip not victory");
            skip.Start(200,1);Throws(()=>skip.UseTool(true),"last level skip rejected");
            var skin=AssetDatabase.LoadAssetAtPath<RodUISkin>(RodUIBuilder.SkinPath);Check(skin.font!=null&&skin.panel!=null,"skin assets");
            foreach(string page in new[]{"home","game","rules","coins","won","failed","revive","skipped"}){
                var go=new GameObject("Validation_"+page,typeof(RectTransform));var view=go.AddComponent<RodGameView>();view.Initialize(skin);
                var snapshot=session.Snapshot;snapshot.activeRun=page=="won"?won:failed;
                view.Render(new RodViewModel{save=snapshot,home=page=="home",popup=page=="home"||page=="game"?null:page,adReady=true,rerollCost="免费 2",skipCost="广告"});
                Check(go.GetComponentsInChildren<TextMeshProUGUI>().Length>4,"page built "+page);
                foreach(var image in go.GetComponentsInChildren<Image>())if(image.sprite!=null)Check(image.type==Image.Type.Sliced,"9-sliced panels");
                foreach(var text in go.GetComponentsInChildren<TextMeshProUGUI>())foreach(char c in text.text)if(!char.IsWhiteSpace(c))Check(skin.font.HasCharacter(c,true),"missing glyph "+c);
                UnityEngine.Object.DestroyImmediate(go);
            }
            Check(writes>200,"key nodes persisted");
            var memory=new MemoryStorage();var repository=new RodSaveRepository(memory);
            repository.Commit(new RodSaveData());Check(repository.Load().activeRun==null,"JSON empty-run round trip");
            repository.Commit(session.Snapshot);Check(repository.Load().activeRun.runId==session.Snapshot.activeRun.runId,"JSON active-run round trip");
            Debug.Log($"[RodValidation] PASS {assertions} assertions: 200 games, rewards, failure atomicity, ad recovery, revival, 8 UI states, glyphs.");
        }
        private sealed class MemoryStorage:GameLogic.IReadCacheFileSystem
        {private string json;public string ReadCache()=>json;public void WriteCache(string text)=>json=text;public void DeleteAll()=>json=null;}
        public static void CompileHotfix()
        {
#if ENABLE_HYBRIDCLR
            var target=EditorUserBuildSettings.activeBuildTarget;
            HybridCLR.Editor.Commands.CompileDllCommand.CompileDll(target);
            HybridCLR.Editor.Commands.LinkGeneratorCommand.GenerateLinkXml(target);
            BuildDLLCommand.CopyHotUpdateAssembliesToAssetPath();
            Debug.Log("[Rod] Hotfix rebuilt for "+target);
#else
            Debug.Log("[Rod] HybridCLR not enabled; editor compilation validated only.");
#endif
        }
    }
}
