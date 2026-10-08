using System;
using System.IO;
using GameLogic.Rod;
using UnityEditor;
using UnityEngine;

namespace Rod.Editor
{
    public sealed class RodLevelEditor : EditorWindow
    {
        public const string CatalogPath="Assets/AssetRaw/Configs/Rod/RodLevels.json";
        private RodCatalog catalog;
        private int index;
        private int[] player;
        private Vector2 listScroll,bodyScroll;
        private string message;
        [MenuItem("六面算筹/关卡编辑器")]
        public static void Open()=>GetWindow<RodLevelEditor>("算筹关卡编辑器");
        private void OnEnable(){minSize=new Vector2(900,650);Load();}
        private void Load()
        {
            try {catalog=JsonUtility.FromJson<RodCatalog>(File.ReadAllText(CatalogPath));catalog.Validate();index=0;Select(0);hasUnsavedChanges=false;message="已读取游戏实际配置。";}
            catch(Exception e){message=e.Message;catalog=null;}
        }
        private void Select(int n){index=n;player=(int[])catalog.levels[n].witness.Clone();}
        private void Changed(bool layout=false)
        {hasUnsavedChanges=true;saveChangesMessage="关卡改动尚未保存。";if(layout){catalog.levels[index].needsSimulation=true;catalog.levels[index].estimatedWinRate=-1;}}
        public override void SaveChanges()
        {
            try {
                catalog.Validate();
                catalog.contentVersion="rod-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                string tmp=CatalogPath+".tmp";File.WriteAllText(tmp,JsonUtility.ToJson(catalog,true));
                if(File.Exists(CatalogPath))File.Replace(tmp,CatalogPath,null);else File.Move(tmp,CatalogPath);
                AssetDatabase.ImportAsset(CatalogPath);base.SaveChanges();message="校验通过，已保存实际配置。进行中的局保留原快照；新开局使用新配置。";
            }catch(Exception e){message="保存失败："+e.Message;Debug.LogException(e);}
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if(GUILayout.Button("重新读取",EditorStyles.toolbarButton) && (!hasUnsavedChanges || EditorUtility.DisplayDialog("未保存","放弃当前未保存改动？","放弃","取消")))Load();
            GUI.enabled=catalog!=null;
            if(GUILayout.Button("校验并保存",EditorStyles.toolbarButton))SaveChanges();
            if(GUILayout.Button("定位JSON",EditorStyles.toolbarButton))EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<TextAsset>(CatalogPath));
            if(GUILayout.Button("打开游戏预览场景",EditorStyles.toolbarButton))RodUIBuilder.OpenPreview();
            GUI.enabled=true;GUILayout.FlexibleSpace();GUILayout.Label(hasUnsavedChanges?"未保存":"已保存",EditorStyles.miniLabel);EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(message??"",MessageType.Info);
            if(catalog==null)return;
            EditorGUILayout.BeginHorizontal();
            listScroll=EditorGUILayout.BeginScrollView(listScroll,GUILayout.Width(150));
            for(int i=0;i<catalog.levels.Length;i++)if(GUILayout.Toggle(index==i,$"第{i+1}关  难度{catalog.levels[i].difficulty}","Button"))if(index!=i)Select(i);
            EditorGUILayout.EndScrollView();
            bodyScroll=EditorGUILayout.BeginScrollView(bodyScroll);
            var l=catalog.levels[index];
            GUILayout.Label($"第{l.id}关 · {catalog.contentVersion}",EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            l.name=EditorGUILayout.TextField("名称",l.name);l.difficulty=EditorGUILayout.IntSlider("设计难度",l.difficulty,1,10);l.tactic=EditorGUILayout.TextField("破局思路",l.tactic);
            l.coinReward=EditorGUILayout.IntField("首通金币",l.coinReward);l.rerollReward=EditorGUILayout.IntField("首通重投",l.rerollReward);l.skipReward=EditorGUILayout.IntField("首通下一关",l.skipReward);
            if(EditorGUI.EndChangeCheck())Changed();
            GUILayout.Label(l.needsSimulation?"布局已修改，难度需复测。":$"离线贪心估计胜率：{l.estimatedWinRate:P1}（非真人胜率）");
            EditorGUILayout.Space();GUILayout.Label("全局六区奖励（每关相同；旧局用快照）",EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            for(int f=0;f<6;f++)GUILayout.Label($"{RodCatalog.Faces[f]} {catalog.prizes[f*2]}/{catalog.prizes[f*2+1]}");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();GUILayout.Label("电脑布局：每枚算筹选择花色，点值不可改",EditorStyles.boldLabel);
            for(int p=0;p<3;p++)DrawDice("电脑"+new[]{"甲","乙","丙"}[p],l.opponentFaces,p*9,true);
            DrawDice("玩家试摆",player,0,false);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("载入可胜见证"))player=(int[])l.witness.Clone();
            if(GUILayout.Button("保存当前试摆为可胜见证")){
                var s=RodRules.Score(Power(),catalog.prizes);
                if(!RodRules.PlayerWins(s))message="当前试摆没有获胜，不能保存见证。";
                else {l.witness=(int[])player.Clone();if(l.id==1)l.firstRoll=(int[])player.Clone();Changed();message="见证已更新，请校验并保存。";}
            }
            EditorGUILayout.EndHorizontal();
            var power=Power();var scores=RodRules.Score(power,catalog.prizes);
            for(int p=0;p<4;p++){
                EditorGUILayout.BeginHorizontal("box");GUILayout.Label(p==0?"玩家":new[]{"","甲","乙","丙"}[p],GUILayout.Width(55));
                for(int f=0;f<6;f++){int n=power[p,f],same=0;for(int q=0;q<4;q++)if(power[q,f]==n)same++;
                    GUILayout.Label($"{RodCatalog.Faces[f]} {n}"+(n>0&&same>1?" 抵消":""),GUILayout.MinWidth(65));}
                GUILayout.Label($"总分 {scores[p]}",GUILayout.Width(70));EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.HelpBox(RodRules.PlayerWins(scores)?"试摆可胜（并列最高也通关）":"试摆未通关",MessageType.Info);
            EditorGUILayout.HelpBox("试摆是终局评估，不是随机投掷模拟。修改电脑后须重新复测难度。保存写入游戏配置，运行中的旧局不会被修改。",MessageType.None);
            EditorGUILayout.EndScrollView();EditorGUILayout.EndHorizontal();
        }
        private int[,] Power()
        {var p=new int[4,6];for(int n=0;n<9;n++)p[0,player[n]]+=RodCatalog.Values[n];var bots=catalog.levels[index].OpponentPower();for(int n=0;n<18;n++)p[n/6+1,n%6]=bots[n];return p;}
        private void DrawDice(string label,int[] faces,int offset,bool editsLayout)
        {
            EditorGUILayout.BeginHorizontal();GUILayout.Label(label,GUILayout.Width(65));
            EditorGUI.BeginChangeCheck();
            for(int n=0;n<9;n++){EditorGUILayout.BeginVertical(GUILayout.MinWidth(50));GUILayout.Label($"{n+1}号/{RodCatalog.Values[n]}点",EditorStyles.miniLabel);faces[offset+n]=EditorGUILayout.Popup(faces[offset+n],RodCatalog.Faces);EditorGUILayout.EndVertical();}
            if(EditorGUI.EndChangeCheck()&&editsLayout)Changed(true);
            EditorGUILayout.EndHorizontal();
        }
    }
}
