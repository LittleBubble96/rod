"""从生成器候选转换为Unity JsonUtility可读结构。手工编辑后的运行时文件不要无意覆盖。"""
import argparse
import hashlib
import json
from pathlib import Path
root=Path(__file__).resolve().parents[2]
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--apply',action='store_true',help='显式覆盖运行配置；必须同时提供当前文件哈希')
parser.add_argument('--expected-runtime-sha256',help='调用前核对的运行配置 SHA256，防止旧设计副本覆盖编辑器修改')
args=parser.parse_args()
data=json.loads((root/'策划案/关卡/首批200关.json').read_text(encoding='utf-8'))
levels=[]
for l in data['levels']:
    levels.append(dict(id=l['id'],name=l['name'],difficulty=l['difficulty'],tactic=l['tactic'],
        coinReward=l['reward']['coin'],rerollReward=l['reward']['reroll'],skipReward=l['reward']['skip'],
        opponentFaces=sum(l['opponentFaces'],[]),witness=l['winningWitness']['facesByDieId'],
        firstRoll=l.get('firstRollOverride') or [],estimatedWinRate=l['estimatedWinRate'] if l['estimatedWinRate'] is not None else -1,
        needsSimulation=l.get('balanceStatus')=='needs_resimulation' or l['estimatedWinRate'] is None))
out=root/'TEngine/UnityProject/Assets/AssetRaw/Configs/Rod/RodLevels.json'
if args.apply:
    if not args.expected_runtime_sha256 or hashlib.sha256(out.read_bytes()).hexdigest()!=args.expected_runtime_sha256.lower():
        parser.error('运行配置哈希未提供或不匹配，禁止覆盖；请先比较候选与编辑器配置')
else:
    out=root/'work/rod-validation/RodLevels.export-candidate.json'
out.parent.mkdir(parents=True,exist_ok=True)
staged=out.with_suffix('.pending')
staged.write_text(json.dumps(dict(schemaVersion=1,contentVersion=data['contentVersion'],prizes=sum(data['prizes'],[]),levels=levels),ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
staged.replace(out)
print('Exported 200 Unity levels:', out)
