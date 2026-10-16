"""标准库即可运行。生成首批200关设计数据；不改动旧文字游戏配置。"""
import json, random, math, hashlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
VALUES = [3, 2, 2, 1, 1, 1, 1, 1, 1]
FACES = ['风', '花', '雪', '月', '方', '圆']
PRIZES = [(12,4),(10,8),(11,5),(9,6),(8,5),(7,3)]
CHAPTERS = ['初识六面','同值抵消','次奖取舍','分散落筹','追加与避碰','多区联动','筹力预算','领先与守分','复杂抵消','综合试炼']
TACTICS = ['争取空区','避开同值','保住次奖','制造抵消','追加逆转','分散收益','绕开强区','比较总分','末轮取舍','综合判断']

def totals(faces):
    out = [0]*6
    for v, f in zip(VALUES, faces): out[f] += v
    return out

def scores(boards):
    result = [0]*4
    for face in range(6):
        values = [b[face] for b in boards]
        valid = sorted(((n,p) for p,n in enumerate(values) if n > 0 and values.count(n)==1), reverse=True)
        for rank, (_,p) in enumerate(valid[:2]): result[p] += PRIZES[face][rank]
    return result

def simulate(bots, seed, trials=80):
    rng = random.Random(seed)
    wins = 0
    for _ in range(trials):
        hand = list(range(9)); placed = [0]*6; free = 2
        while hand:
            rolled = {i:rng.randrange(6) for i in hand}
            while True:
                options=[]
                for face in sorted(set(rolled.values())):
                    group=[i for i in hand if rolled[i]==face]
                    board=placed.copy(); board[face]+=sum(VALUES[i] for i in group)
                    s=scores([board]+bots)
                    options.append((s[0]-max(s[1:]), s[0], len(group), -face, face, group, board))
                best=max(options)
                current=scores([placed]+bots)
                if free and best[0] <= current[0]-max(current[1:]):
                    free-=1; rolled={i:rng.randrange(6) for i in hand}
                else: break
            placed=best[-1]; hand=[i for i in hand if i not in best[-2]]
        s=scores([placed]+bots); wins += s[0]>=max(s[1:])
    return wins/trials

def witness(bots, rng):
    for _ in range(8000):
        faces=[rng.randrange(6) for _ in VALUES]
        s=scores([totals(faces)]+bots)
        if s[0]>=max(s[1:]): return faces,s
    return None

def validate(data):
    assert data['schemaVersion']==1 and len(data['levels'])==200
    assert data['faces']==FACES and data['dieValues']==VALUES
    assert data['prizes']==[list(p) for p in PRIZES], '评分表与当前规则不一致，禁止静默变更'
    seen=set()
    for i,l in enumerate(data['levels'],1):
        assert l['id']==i and l['winRule']=='HighestScoreOrTie'
        assert 1<=l['difficulty']<=10 and l['freeRerolls']==2
        assert len(l['opponentFaces'])==3
        for f in l['opponentFaces']:
            assert len(f)==9 and all(isinstance(x,int) and 0<=x<6 for x in f)
            assert sum(totals(f))==13
        sig=tuple(tuple(totals(f)) for f in l['opponentFaces'])
        assert sig not in seen; seen.add(sig)
        w=l['winningWitness']; assert len(w['facesByDieId'])==9
        assert all(0<=x<6 for x in w['facesByDieId'])
        s=scores([totals(w['facesByDieId'])]+[totals(f) for f in l['opponentFaces']])
        assert s==w['finalScores'] and s[0]>=max(s[1:]) and sum(s)<=sum(map(sum, PRIZES))
        assert all(isinstance(l['reward'][k],int) and 0<=l['reward'][k]<=100000 for k in ('coin','reroll','skip'))
        override=l.get('firstRollOverride')
        assert override is None or (len(override)==9 and all(isinstance(f,int) and 0<=f<6 for f in override))
        rate=l['estimatedWinRate']
        assert (rate is None and l['simulationTrials']==0 and l.get('balanceStatus')=='needs_resimulation') or (isinstance(rate,(int,float)) and 0<=rate<=1)
    assert scores([[4,0,0,0,0,0],[4,0,0,0,0,0],[3,0,0,0,0,0],[2,0,0,0,0,0]])==[0,0,12,4]
    assert scores([[4,0,0,0,0,0]]*3+[[2,0,0,0,0,0]])==[0,0,0,12]
    assert scores([[4,0,0,0,0,0]]*2+[[2,0,0,0,0,0]]*2)==[0,0,0,0]

def build():
    rng=random.Random(20261007); candidates=[]; seen=set()
    while len(candidates)<240:
        fs=[[rng.randrange(6) for _ in VALUES] for _ in range(3)]
        bots=[totals(f) for f in fs]; sig=tuple(map(tuple,bots))
        if sig in seen: continue
        seen.add(sig); w=witness(bots,rng)
        if w is None: continue
        seed=rng.randrange(1,2**31)
        rate=simulate(bots,seed)
        candidates.append((rate,seed,fs,w))
    candidates.sort(key=lambda c:(-c[0],c[1]))
    chosen=[candidates[round(i*239/199)] for i in range(200)]
    levels=[]
    for block in range(40):
        for offset in [0,2,1,4,3]:
            rate,seed,fs,w=chosen[block*5+offset]; i=len(levels)+1; chapter=(i-1)//20
            levels.append(dict(id=i,chapter=chapter+1,name=f'{CHAPTERS[chapter]}{(i-1)%20+1:02d}',difficulty=chapter+1,
                winRule='HighestScoreOrTie',tactic=TACTICS[chapter],freeRerolls=2,
                opponentFaces=fs,estimatedWinRate=rate,simulationSeed=seed,simulationTrials=80,
                reward=dict(coin=40+chapter*5,reroll=1 if i%5==0 else 0,skip=1 if i%20==0 else 0),
                winningWitness=dict(facesByDieId=w[0],finalScores=w[1]),
                tutorial='first_match' if i==1 else ('cancellation_example' if i==2 else ''),
                firstRollOverride=w[0] if i==1 else None))
    data=dict(schemaVersion=1,contentVersion='rod-differentiated-1',status='design_candidate_not_live',
        faces=FACES,dieValues=VALUES,prizes=[list(p) for p in PRIZES],levels=levels)
    validate(data)
    folder=ROOT/'关卡'; folder.mkdir(parents=True,exist_ok=True)
    payload=json.dumps(data,ensure_ascii=False,indent=2)+'\n'
    (folder/'首批200关.json').write_text(payload,encoding='utf-8')
    lines=['# 首批200关逐关表','', '面序：风、花、雪、月、方、圆。电脑列为六区筹力；每方均13点。胜利统一为手中落完后总胜分并列最高也通过。', '',
        '难度为章节设计档，胜率为固定贪心策略、两次免费重投、无付费／广告／复活的80局离线估计；不等于玩家真实胜率。第1关正式引导另使用固定首投，表中仍记录随机基线。', '',
        '|关卡|名称|难度|建议破局思路|甲|乙|丙|估计胜率|首通金币/重投/下一关|',
        '|---|---|---|---|---|---|---|---|---|']
    for l in levels:
        boards=['/'.join(map(str,totals(f))) for f in l['opponentFaces']]; r=l['reward']
        lines.append(f"|{l['id']}|{l['name']}|{l['difficulty']}|{l['tactic']}|"+'|'.join(boards)+f"|{l['estimatedWinRate']:.0%}|{r['coin']}/{r['reroll']}/{r['skip']}|")
    (folder/'首批200关逐关表.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
    report=['# 关卡校验报告','', '200/200通过：ID连续、电脑各9枚13点、合法六面、无重复三电脑筹力组合、可胜布局见证、胜分不超过88、统一胜利规则。',
        '', '可胜见证只证明存在合法非零概率投掷路径，不证明任意随机结果可胜、不证明最优解，也不依赖广告。每次按见证中一种花色整组落区，余下算筹下一轮仍投出各自见证花色即可到达。', '',
        f"JSON SHA256：{hashlib.sha256(payload.encode()).hexdigest()}", '',
        '模拟策略：每轮选择使当前“玩家分－最高电脑分”最大的同色组；再按玩家分、组枚数、面序破同分；若没有改善且有免费重投则重投。每关80次，0.5附近95%误差约±11个百分点，必须后续实测调优。', '',
        '|章节|平均估计胜率|最小—最大|', '|---|---|---|']
    for c in range(10):
        rates=[l['estimatedWinRate'] for l in levels[c*20:c*20+20]]
        report.append(f'|{c+1} {CHAPTERS[c]}|{sum(rates)/20:.1%}|{min(rates):.1%}—{max(rates):.1%}|')
    (folder/'校验报告.md').write_text('\n'.join(report)+'\n',encoding='utf-8')
    print('PASS: 200 levels, 240 candidate layouts, 19200 simulated games, 200 legal winning witnesses.')

if __name__=='__main__':
    import argparse
    parser=argparse.ArgumentParser()
    parser.add_argument('--check',action='store_true')
    parser.add_argument('--file',type=Path,default=ROOT/'关卡/首批200关.json')
    parser.add_argument('--resimulate',action='store_true')
    parser.add_argument('--trials',type=int,default=1000)
    args=parser.parse_args()
    if args.check or args.resimulate:
        data=json.loads(args.file.read_text(encoding='utf-8-sig')); validate(data)
        if args.resimulate:
            if args.trials<80: parser.error('trials至少80，正式调优建议1000以上')
            for l in data['levels']:
                l['estimatedWinRate']=simulate([totals(f) for f in l['opponentFaces']],l['simulationSeed'],args.trials)
                l['simulationTrials']=args.trials; l['balanceStatus']='simulated_not_playtested'
            validate(data)
            output=args.file.with_name(args.file.stem+'_复测.json')
            output.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
            print('Saved resimulated copy:', output)
        print('PASS: stored 200-level data validated')
    else: build()
