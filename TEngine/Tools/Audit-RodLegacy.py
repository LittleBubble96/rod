"""只读审计：GUID引用、代码资源名、YooAsset自动收集都需人工判断。"""
import re,json
from pathlib import Path
root=Path(__file__).resolve().parents[2]; assets=root/'TEngine/UnityProject/Assets'
texts={p:p.read_text(encoding='utf-8',errors='ignore') for p in assets.rglob('*') if p.is_file() and p.suffix in {'.cs','.asset','.prefab','.unity','.meta','.json','.mat','.spriteatlas','.anim','.controller'}}
refs={}
for p,t in texts.items():
    for g in set(re.findall(r'guid: ([0-9a-f]{32})',t)):
        refs.setdefault(g,[]).append(p)
code='\n'.join(t for p,t in texts.items() if p.suffix=='.cs')
rows=[]
for p in (assets/'AssetRaw/UIRaw').rglob('*.png'):
    meta=Path(str(p)+'.meta'); match=re.search(r'guid: ([0-9a-f]{32})',texts.get(meta,''))
    if not match: continue
    usages=[str(x.relative_to(root)) for x in refs.get(match[1],[]) if x!=meta]
    # AddressByFolderAndFileName使用Common_prop_x，不能只检查斜杠路径。
    address=bool(re.search(r'["\'](?:[^"\']*[/_])?'+re.escape(p.stem)+r'["\']',code))
    rows.append(dict(path=str(p.relative_to(root)),bytes=p.stat().st_size,guid=match[1],references=usages,addressInCode=address))
out=root/'work/rod-validation/ui-reference-audit.json';out.write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8')
for r in rows:
    if not r['references'] and not r['addressInCode']: print(r['path'],r['bytes'])
