from pathlib import Path
import re,json
root=Path(__file__).resolve().parents[2]
design=root/'策划案'
for i in range(1,8):
    assert len(list(design.glob(f'{i:02d}_*.md')))==1
for doc in design.glob('*.md'):
    text=doc.read_text(encoding='utf-8')
    for link in re.findall(r'\]\(([^)]+)\)',text):
        if '://' not in link: assert (doc.parent/link.split('#')[0]).exists(), (doc,link)
assets=root/'TEngine/UnityProject/Assets'
old_guid='d42c7360665c1e7419446dcda682fc35'
for path in assets.rglob('*'):
    if path.suffix in {'.cs','.asset','.prefab','.unity','.meta'}:
        assert old_guid not in path.read_text(encoding='utf-8-sig',errors='replace'),path
export=(assets/'Editor/Builder/Localization/LanguageGenerateEditor.cs').read_text(encoding='utf-8')
for stale in ['CollectLevelChars','LevelSuffix','TextLevelDataScriptableObject','Generate_Test']:
    assert stale not in export,stale
assert not (assets/'AssetRaw/Fonts/Level').exists()
assert not (assets/'GameScripts/HotFix/GameLogic/Game/Data/GameRestoreDataManager.cs').exists()
print('PASS: design links, 7 core documents, retired font GUID closure and obsolete exports.')
