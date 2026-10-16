using System;
using System.Collections.Generic;

namespace GameLogic.Rod
{
    [Serializable]
    public sealed class RodLevel
    {
        public int id, difficulty, coinReward, rerollReward, skipReward;
        public string name, tactic;
        public int[] opponentFaces; // 甲乙丙，每人9枚；JsonUtility使用一维数组。
        public int[] witness;
        public int[] firstRoll;
        public float estimatedWinRate;
        public bool needsSimulation;
        public int[] OpponentPower()
        {
            var p=new int[18];
            for(int i=0;i<27;i++) p[(i/9)*6+opponentFaces[i]]+=RodCatalog.Values[i%9];
            return p;
        }
    }

    [Serializable]
    public sealed class RodCatalog
    {
        public int schemaVersion=1;
        public string contentVersion="rod-differentiated-1";
        public int[] prizes=RodRules.CurrentPrizes;
        public RodLevel[] levels;
        public static readonly int[] Values={3,2,2,1,1,1,1,1,1};
        public static readonly string[] Faces={"风","花","雪","月","方","圆"};
        public RodLevel Get(int id)
        {
            if(id<1 || id>levels.Length) throw new ArgumentOutOfRangeException(nameof(id));
            return levels[id-1];
        }
        public void Validate()
        {
            if(schemaVersion!=1 || string.IsNullOrWhiteSpace(contentVersion) || levels==null || levels.Length!=200)
                throw new InvalidOperationException("需要完整200关及配置版本。");
            RodRules.ValidatePrizes(prizes);
            var layouts=new HashSet<string>();
            for(int i=0;i<levels.Length;i++)
            {
                var l=levels[i];
                if(l==null || l.id!=i+1 || l.difficulty<1 || l.difficulty>10 || string.IsNullOrWhiteSpace(l.name) ||
                   l.coinReward<0 || l.coinReward>100000 || l.rerollReward<0 || l.rerollReward>1000 || l.skipReward<0 || l.skipReward>1000)
                    throw new InvalidOperationException($"第{i+1}关头信息/奖励无效。");
                FacesValid(l.opponentFaces,27); FacesValid(l.witness,9);
                if(l.firstRoll!=null && l.firstRoll.Length>0) FacesValid(l.firstRoll,9);
                var bots=l.OpponentPower();
                if(!layouts.Add(string.Join(",",bots))) throw new InvalidOperationException($"第{l.id}关布局重复。");
                var power=new int[4,6];
                for(int n=0;n<9;n++) power[0,l.witness[n]]+=Values[n];
                for(int p=0;p<3;p++) for(int f=0;f<6;f++) power[p+1,f]=bots[p*6+f];
                if(!RodRules.PlayerWins(RodRules.Score(power,prizes))) throw new InvalidOperationException($"第{l.id}关可胜见证不成立。");
            }
        }
        public static void FacesValid(int[] faces,int length)
        {
            if(faces==null || faces.Length!=length) throw new InvalidOperationException("算筹数量错误。");
            foreach(int f in faces) if(f<0 || f>5) throw new InvalidOperationException("花色范围须为0—5。");
        }
    }
}
