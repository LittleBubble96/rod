using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Rod
{
    public sealed class RodViewModel
    {
        public RodSaveData save;
        public bool home=true,busy,adReady,adLoading;
        public string popup,message,rerollCost,skipCost;
        public int selectedFace=-1;
        public HashSet<int> revival=new HashSet<int>();
    }
    /// <summary>仅布局与展示；业务由Action事件交给Presenter。设计尺寸750×1500。</summary>
    public sealed class RodGameView : MonoBehaviour
    {
        public RodUISkin skin;
        public event Action<string,int> Action;
        private RectTransform frame,content;
        private Rect lastSafe;private int lastW,lastH;
        private float helpUntil;
        private GameObject help;
        private RodViewModel model;
        public void Initialize(RodUISkin value){skin=value;EnsureRoot();}
        private void EnsureRoot()
        {
            if(frame!=null)return;
            var canvas=GetComponent<Canvas>();if(canvas==null)canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            if(!GetComponent<GraphicRaycaster>())gameObject.AddComponent<GraphicRaycaster>();
            var scaler=GetComponent<CanvasScaler>();if(scaler==null)scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            frame=Rect(transform,"SafeFrame",0,0,750,1500);frame.anchorMin=frame.anchorMax=frame.pivot=new Vector2(.5f,.5f);Fit();
        }
        private void Fit(){if(frame==null)return;var safe=Screen.safeArea;float scale=Mathf.Min(safe.width/750f,safe.height/1500f);frame.localScale=Vector3.one*scale;frame.anchoredPosition=safe.center-new Vector2(Screen.width,Screen.height)*.5f;lastSafe=safe;lastW=Screen.width;lastH=Screen.height;}
        private void Update(){if(Screen.safeArea!=lastSafe||Screen.width!=lastW||Screen.height!=lastH)Fit();if(help!=null&&Time.unscaledTime>=helpUntil){help.SetActive(false);help=null;}}
        public void Render(RodViewModel vm)
        {
            model=vm;EnsureRoot();
            if(content!=null){content.gameObject.SetActive(false);if(Application.isPlaying)Destroy(content.gameObject);else DestroyImmediate(content.gameObject);}
            content=Rect(frame,"Content",0,0,750,1500);content.anchorMin=content.anchorMax=content.pivot=new Vector2(.5f,.5f);content.anchoredPosition=Vector2.zero;
            Panel(content,"Background",0,0,750,1500,skin.mint,null);
            if(vm.save==null){Text(content,"LoadingTitle","六面算筹",65,360,620,110,64);Text(content,"LoadingMessage",vm.message??"正在准备棋盘",65,610,620,160,30);Panel(content,"ProgressTrack",95,820,560,30,skin.ink,skin.slot);Panel(content,"ProgressFill",100,825,vm.busy?180:545,20,skin.gold,skin.slot);return;}
            if(vm.home)Home();else Board();
            if(!string.IsNullOrEmpty(vm.popup))Modal(vm.popup);
            Header();
            if(!string.IsNullOrEmpty(vm.message)){var toast=Panel(content,"Toast",45,1370,660,90,skin.ink,skin.panel);Text(toast,"Message",vm.message,18,8,624,72,23,Color.white);}
            if(vm.busy){var block=Panel(content,"Busy",0,0,750,1500,new Color(0,0,0,.45f),null,true);Text(block,"BusyLabel","请稍候",150,680,450,80,36,Color.white);Icon(block,RodIcon.Loading,333,790,84,Color.white);}
        }
        private void Header()
        {
            var s=model.save;var coin=Button(content,"Coin","",28,30,230,72,skin.cream,()=>Emit("coins"));Icon(coin,RodIcon.Coin,8,12,46,skin.gold);Text(coin,"Count",s.coins.ToString(),56,8,122,55,29);Icon(coin,RodIcon.Plus,180,18,34,skin.ink);
            Text(content,"Level",model.home?"六面算筹":$"第 {s.activeRun?.levelId??s.currentLevelId} 关",276,38,230,60,31);
            if(!model.home&&string.IsNullOrEmpty(model.popup)){var back=Button(content,"Back","",646,28,74,74,skin.cream,()=>Emit("home"));Icon(back,RodIcon.Back,14,14,46,skin.ink);}
        }
        private void Home()
        {
            Text(content,"Brand","六 面 算 筹",55,300,640,125,67);
            Text(content,"Tagline","风 · 花 · 雪 · 月 · 方 · 圆",55,435,640,65,31);
            for(int i=0;i<6;i++)Die(content,i%3+1, RodCatalog.Faces[i],150+i%3*150,590+i/3*140,110,null);
            Button(content,"Start",model.save.activeRun==null?"开始游戏":"继续游戏",150,980,450,100,skin.gold,()=>Emit("start"));
            Button(content,"Rules","玩法说明",150,1110,210,80,skin.cream,()=>Emit("rules"));
            Button(content,"GetCoin","获取金币",390,1110,210,80,skin.cream,()=>Emit("coins"));
            Text(content,"Progress",$"已解锁 {model.save.highestUnlockedLevel} / 200 关",120,1240,510,60,26);
        }
        private void Board()
        {
            var r=model.save.activeRun;if(r==null)return;
            var scores=RodRules.ScoreRun(r);Text(content,"Scores",$"你 {scores[0]}     甲 {scores[1]}     乙 {scores[2]}     丙 {scores[3]}",25,130,700,60,28);
            for(int f=0;f<6;f++){
                int face=f;float x=30+(f%2)*355,y=220+(f/2)*202;
                var card=Button(content,"Region_"+f,"",x,y,335,183,model.selectedFace==f?skin.gold:skin.cream,()=>Emit("select",face));
                int[] prizes=r.prizes??RodRules.LegacyPrizes;
                Text(card,"Face",RodCatalog.Faces[f],20,10,66,57,38);Text(card,"Awards",$"第一 {prizes[f*2]}   第二 {prizes[f*2+1]}",104,20,210,40,22);
                int mine=r.dice.Where(d=>d.region==f).Sum(d=>d.value);
                Text(card,"Power",$"你 {mine}    甲 {r.opponentPower[f]}    乙 {r.opponentPower[6+f]}    丙 {r.opponentPower[12+f]}",12,76,310,36,21);
                var placed=r.dice.Where(d=>d.region==f).ToArray();
                for(int n=0;n<placed.Length;n++)Text(card,"Placed_"+n,placed[n].value.ToString(),16+n*32,123,30,36,23);
            }
            var tray=Panel(content,"Tray",30,855,690,310,skin.cream,skin.panel);
            Text(tray,"TrayTitle",model.save.tutorialCompleted?"选择同花色，整组落区":"点击一枚算筹选中同花色，再点确认",18,15,654,43,24);
            var hand=r.dice.Where(d=>d.region<0).ToArray();
            for(int n=0;n<hand.Length;n++){var d=hand[n];int face=d.face;Die(tray,d.value,RodCatalog.Faces[face],25+n%5*128,78+n/5*110,96,()=>Emit("select",face),model.selectedFace==face);}
            Tool(content,"Reroll","重投",RodIcon.Reroll,model.rerollCost,32,1200,()=>Emit("reroll"));
            Tool(content,"Skip","下一关",RodIcon.Next,model.skipCost,202,1200,()=>Emit("skip"),r.levelId<200);
            Button(content,"Confirm","确认落区",386,1210,328,118,skin.gold,()=>Emit("place"),model.selectedFace>=0&&r.phase==RodPhase.AwaitGroup);
        }
        private void Tool(Transform parent,string name,string label,RodIcon icon,string cost,float x,float y,Action click,bool enabled=true)
        {
            var b=Button(parent,name,"",x,y,148,148,skin.cream,click,enabled);
            Icon(b,icon,48,12,52,skin.ink);Text(b,"Label",label,8,66,132,37,25);
            if(cost=="广告"){Icon(b,model.adLoading?RodIcon.Loading:RodIcon.Ad,10,110,27,skin.ink);Text(b,"Cost",model.adReady?"广告领取":"加载中",40,108,100,29,17);}
            else Text(b,"Cost",cost??"",5,110,138,28,19);
        }
        private void Modal(string kind)
        {
            Panel(content,"ModalShade",0,118,750,1382,new Color(0,0,0,.35f),null,true);
            var panel=Panel(content,"Modal_"+kind,45,280,660,985,skin.cream,skin.panel);
            string title=kind=="rules"?"玩法说明":kind=="coins"?"获取金币":kind=="won"?"挑战成功":kind=="failed"?"本局未通关":kind=="revive"?"选择复活算筹":"已跳过本关";
            Text(panel,"Title",title,40,32,580,72,40);
            var close=Button(panel,"Close","",571,24,62,62,skin.mint,()=>Emit("close"));Icon(close,RodIcon.Close,12,12,38,skin.ink);
            if(kind=="rules"){
                Text(panel,"Rules","每人9枚算筹，共13点筹力。\n\n投掷后选择一种花色，整组放入对应区域。剩余算筹自动重投。\n\n同一区域中，相同正筹力的参与者全部抵消；剩余者按筹力排名领取区域奖励。\n\n全部落完后，总分并列最高也通关。\n\n每局2次免费重投。失败后可选恰好6点算筹，看完广告取回复活。",44,145,572,640,29);
                Button(panel,"Understood","知道了",135,835,390,86,skin.gold,()=>Emit("close"));
            }else if(kind=="coins"){
                Text(panel,"DailyDesc","每日领取 · 每日一次",48,165,560,55,28);Button(panel,"Daily",model.save.dailyClaimed?"今日已领取":"领取 100 金币",95,235,470,85,skin.gold,()=>Emit("daily"),!model.save.dailyClaimed);
                Text(panel,"ShareDesc",$"分享领取 · 剩余 {3-model.save.sharesClaimed} 次",48,385,560,55,28);Button(panel,"ShareCoin","分享领取 50 金币",95,455,470,85,skin.mint,()=>Emit("shareCoin"),model.save.sharesClaimed<3);
                Text(panel,"AdDesc","看广告领取 · 每日不限",48,605,560,55,28);AdButton(panel,"CoinAd","领取 30 金币",95,675,470,90,()=>Emit("adCoin"));
            }else if(kind=="won"||kind=="skipped"){
                var r=model.save.activeRun;Text(panel,"RewardTitle",kind=="skipped"?"跳过不发放通关奖励":r.baseRewardClaimed?"本关奖励已入账":"本关已领取过首通奖励",30,145,600,55,27);
                if(kind=="won"){
                    var items=new[]{r.rewardCoin,r.rewardReroll,r.rewardSkip};var icons=new[]{RodIcon.Coin,RodIcon.Reroll,RodIcon.Next};var labels=new[]{"金币","重投","下一关"};
                    for(int i=0;i<3;i++){Icon(panel,icons[i],85+i*185,250,80,skin.ink);Text(panel,"Reward"+i,labels[i]+" ×"+items[i],50+i*185,345,160,65,25);}
                    AdButton(panel,"Double",r.doubleRewardClaimed?"已领取双倍":"奖励双倍",95,455,470,80,()=>Emit("double"),r.baseRewardClaimed&&!r.doubleRewardClaimed);
                }
                Button(panel,"Next",r.levelId<200?"下一关":"已通关全部关卡",95,585,470,95,skin.gold,()=>Emit("next"),r.levelId<200);
                Button(panel,"Home","返回首页",95,720,225,80,skin.mint,()=>Emit("home"));Button(panel,"Share","分享",340,720,225,80,skin.mint,()=>Emit("share"));
            }else if(kind=="failed"){
                var scores=RodRules.ScoreRun(model.save.activeRun);Text(panel,"FailedScore",$"你的总分 {scores[0]}\n本局最高 {scores.Max()}",55,190,550,190,42);
                Button(panel,"Revive","选择算筹复活",95,480,470,100,skin.gold,()=>Emit("revive"));Button(panel,"Restart","免费重开",95,625,470,85,skin.mint,()=>Emit("restart"));
            }else if(kind=="revive"){
                var r=model.save.activeRun;int points=r.dice.Where(d=>model.revival.Contains(d.id)).Sum(d=>d.value);
                Text(panel,"Selection",$"已选 {points} / 6 点",75,145,460,60,34);
                var info=Button(panel,"Info","",552,145,60,60,skin.mint,()=>ShowHelp(panel));Icon(info,RodIcon.Help,8,8,44,skin.ink);
                for(int n=0;n<9;n++){var d=r.dice[n];int id=d.id;Die(panel,d.value,RodCatalog.Faces[d.face],91+n%3*173,255+n/3*145,116,()=>Emit("pick",id),model.revival.Contains(id));}
                AdButton(panel,"ReviveAd","看广告复活",95,795,470,90,()=>Emit("reviveAd"),points==6);
            }
        }
        private void ShowHelp(Transform parent)
        {if(help!=null)Destroy(help);var p=Panel(parent,"ReviveHelp",65,210,530,135,skin.ink,skin.panel);Text(p,"Tip","广告完成后，才会取回\n所选的这几枚算筹",18,16,494,103,26,Color.white);help=p.gameObject;helpUntil=Time.unscaledTime+3;}
        private void AdButton(Transform p,string name,string label,float x,float y,float w,float h,Action action,bool enabled=true)
        {var b=Button(p,name,model.adLoading?"广告加载中":label,x,y,w,h,skin.gold,action,enabled&&!model.adLoading);Icon(b,model.adLoading?RodIcon.Loading:RodIcon.Ad,12,(h-38)/2,38,skin.ink);}
        private void Die(Transform p,int value,string face,float x,float y,float size,Action action,bool selected=false)
        {value=Mathf.Clamp(value,1,3);var b=Button(p,"Die_"+face,"",x,y,size,size,selected?skin.gold:Color.white,action??(()=>{}));Text(b,"Face",face,3,2,size-6,size*.58f,36);Text(b,"Points",new string('●',value),2,size*.59f,size-4,size*.27f,17);}
        private void Emit(string action,int value=0)=>Action?.Invoke(action,value);
        public RectTransform Rect(Transform p,string name,float x,float y,float w,float h)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
        private RectTransform Panel(Transform p,string n,float x,float y,float w,float h,Color color,Sprite sprite,bool hit=false)
        {var r=Rect(p,n,x,y,w,h);var i=r.gameObject.AddComponent<Image>();i.color=color;i.sprite=sprite;i.type=sprite?Image.Type.Sliced:Image.Type.Simple;i.raycastTarget=hit;return r;}
        private RectTransform Button(Transform p,string n,string text,float x,float y,float w,float h,Color color,Action click,bool enabled=true)
        {var r=Panel(p,n,x,y,w,h,color,skin.button,true);var b=r.gameObject.AddComponent<Button>();b.interactable=enabled;b.onClick.AddListener(()=>click());if(text!="")Text(r,"Label",text,12,7,w-24,h-14,30);return r;}
        private void Text(Transform p,string n,string text,float x,float y,float w,float h,float size,Color? color=null)
        {var r=Rect(p,n,x,y,w,h);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=skin.font;t.text=text;t.fontSize=size;t.color=color??skin.ink;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.enableWordWrapping=true;t.overflowMode=TextOverflowModes.Ellipsis;}
        private void Icon(Transform p,RodIcon icon,float x,float y,float size,Color color)
        {var r=Rect(p,icon.ToString(),x,y,size,size);var g=r.gameObject.AddComponent<RodIconGraphic>();g.icon=icon;g.color=color;g.raycastTarget=false;}
    }
}
