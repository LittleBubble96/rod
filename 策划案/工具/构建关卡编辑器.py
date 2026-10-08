from pathlib import Path
import json
root=Path(__file__).resolve().parents[1]
data=json.loads((root/'关卡/首批200关.json').read_text(encoding='utf-8'))
template=(root/'工具/关卡编辑器模板.html').read_text(encoding='utf-8')
payload=json.dumps(data,ensure_ascii=False,separators=(',',':')).replace('<','\\u003c')
(root/'关卡/关卡编辑器.html').write_text(template.replace('__LEVEL_DATA__',payload),encoding='utf-8')
print('Built offline editor: 200 levels; no server or external dependency.')
