"""固定布局与随机种子，比较占位分数与两个统一分数方案。不会覆写关卡。"""
import importlib.util, json
from pathlib import Path
root = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('levels', Path(__file__).with_name('生成与校验关卡.py'))
m = importlib.util.module_from_spec(spec); spec.loader.exec_module(m)
data = json.loads((root/'关卡/首批200关.json').read_text(encoding='utf-8'))
result = {}
for name, prizes in [('原占位', [(10,5),(10,8),(9,6),(8,7),(8,5),(7,5)]), ('统一10比5', [(10,5)]*6), ('统一10比6', [(10,6)]*6)]:
    m.PRIZES = prizes
    rates = [m.simulate([m.totals(f) for f in l['opponentFaces']], l['simulationSeed'], 80) for l in data['levels']]
    result[name] = dict(prizes=prizes, trialsPerLevel=80, rates=rates,
        average=sum(rates)/len(rates), below20=sum(r<.2 for r in rates),
        chapterRates=[sum(rates[i:i+20])/20 for i in range(0,200,20)])
    print(name, result[name]['average'], result[name]['below20'], flush=True)
(root/'关卡/分数对比实验.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
lines=['# 六区分数对比实验','', '同一批200个电脑布局、同一随机种子、每关80局、每局2次免费重投，同一个贪心算法随评分表重新选择动作。共48,000局。未包含广告、复活、付费和真人；首关教学固定首投不参与。原关卡按旧分数筛选，存在选择偏差，结果仅用于观察改分影响，不用于证明哪个方案更好玩。','', '|方案|平均模拟胜率|低于20%的关卡数|六区最高总发分|','|---|---|---|---|']
for name,r in result.items(): lines.append(f"|{name}|{r['average']:.1%}|{r['below20']}|{sum(sum(p) for p in r['prizes'])}|")
lines += ['', '|章|原占位|统一10/5|统一10/6|','|---|---|---|---|']
for i in range(10): lines.append('|'+str(i+1)+'|'+'|'.join(f"{r['chapterRates'][i]:.1%}" for r in result.values())+'|')
lines += ['', '选方案后必须重新生成/排序200关并重新验证可胜见证；不能只改画面分数。设计建议详见《10_六区计分决策》。']
(root/'关卡/分数对比实验.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
