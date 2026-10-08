"""从生成器候选转换为Unity JsonUtility可读结构。手工编辑后的运行时文件不要无意覆盖。"""
import json
from pathlib import Path
root=Path(__file__).resolve().parents[2]
data=json.loads((root/'策划案/关卡/首批200关.json').read_text(encoding='utf-8'))
levels=[]
for l in data['levels']:
    levels.append(dict(id=l['id'],name=l['name'],difficulty=l['difficulty'],tactic=l['tactic'],
        coinReward=l['reward']['coin'],rerollReward=l['reward']['reroll'],skipReward=l['reward']['skip'],
        opponentFaces=sum(l['opponentFaces'],[]),witness=l['winningWitness']['facesByDieId'],
        firstRoll=l.get('firstRollOverride') or [],estimatedWinRate=l['estimatedWinRate'],needsSimulation=False))
out=root/'TEngine/UnityProject/Assets/AssetRaw/Configs/Rod/RodLevels.json'
out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(schemaVersion=1,contentVersion=data['contentVersion'],prizes=sum(data['prizes'],[]),levels=levels),ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Exported 200 Unity levels with scoring table snapshot.')
