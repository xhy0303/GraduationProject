"""Rebuild asset references from the user's own Windows build; do not replace game code."""
from pathlib import Path
import re,json,shutil,ctypes,struct,collections
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'Builds/RecoveredOriginal/ExportedProject/Assets'
DEST=ROOT/'Builds/ResourceRepairCandidate'
ASSETS=DEST/'Assets'
lib=ctypes.CDLL('/usr/lib/system/libcommonCrypto.dylib')
lib.CC_MD4.argtypes=[ctypes.c_void_p,ctypes.c_uint32,ctypes.c_void_p]
def dll_id(type_name):
 ns,sep,name=type_name.rpartition('.');s=('s\0\0\0'+ns+name).encode();o=ctypes.create_string_buffer(16);lib.CC_MD4(s,len(s),o);return str(struct.unpack('<i',o.raw[:4])[0])
def guid(p):
 m=re.search(r'^guid: (\w+)',Path(str(p)+'.meta').read_text(errors='replace'),re.M);return m[1]
def index(base):
 result={}
 for p in base.rglob('*.meta'):
  m=re.search(r'^guid: (\w+)',p.read_text(errors='replace'),re.M)
  if m:result[m[1]]=Path(str(p)[:-5])
 return result
old=index(ROOT/'Assets');recovered=index(SOURCE)
script_map=json.loads((ROOT/'Logs/ProjectDiagnostics/script-map.json').read_text())['entries']
byname={x['type']:x for x in script_map};bydll=collections.defaultdict(list)
for x in script_map:bydll[dll_id(x['type'])].append(x)
refmap={};replacements=[]
for g,p in recovered.items():
 if p.suffix=='.cs' and p.stem in byname:
  t=byname[p.stem];refmap[g]=(str(t['fileId']),t['guid'])
# Match shader sources by their declared name, keeping package shader GUIDs.
shaders={}
for base in [ROOT/'Library/PackageCache',ROOT/'Packages',ROOT/'Assets']:
 for p in base.rglob('*.shader'):
  if not Path(str(p)+'.meta').exists():continue
  m=re.search(r'Shader\s+"([^"]+)"',p.read_text(errors='replace'))
  if m:shaders[m[1]]=('4800000',guid(p))
for p in (ROOT/'Assets/Material').glob('*.shadergraph'):
 shaders['Shader Graphs/'+p.stem]=('-6465566751694194690',guid(p))
# Same original outline code and resources, no duplicate Resources paths.
for g,p in recovered.items():
 if p.name in ['OutlineFill.mat','OutlineMask.mat']:
  t=ROOT/'Assets/Plugins/QuickOutline/Resources/Materials'/p.name;refmap[g]=('2100000',guid(t))
 if p.suffix=='.shader':
  m=re.search(r'Shader\s+"([^"]+)"',p.read_text(errors='replace'));name=m[1]
  if name in shaders:refmap[g]=shaders[name]
  elif name.startswith('Skybox/'):
   # Unity's built-in cubemap shader; resolves through builtin-extra.
   refmap[g]=('103','0000000000000000f000000000000000');replacements.append({'sourceShader':name,'replacement':'Skybox/Cubemap (built-in)'})
  else:
   refmap[g]=shaders['Universal Render Pipeline/Lit'];replacements.append({'sourceShader':name,'replacement':'Universal Render Pipeline/Lit'})
DLLS={g:p for g,p in recovered.items() if p.suffix=='.dll'}
missing_scripts={}
# Preserve scene asset identities and familiar names.
scene_names=['Begin','Main','Dungon','FirstFloor','SecondFloor','ThirdFloor']
scene_map={}
for i,n in enumerate(scene_names):
 p=SOURCE/f'Scenes/level{i}.unity';scene_map[guid(p)]=(ROOT/f'Assets/Scenes/{n}.unity',n)
# Reference mapper replaces both GUID and local file identifier where needed.
REF=re.compile(r'\{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: (\d+)\}')
def rewrite(text):
 def sub(m):
  fid,g,kind=m.groups()
  if g in scene_map:g=guid(scene_map[g][0])
  elif g in refmap:fid,g=refmap[g]
  elif g in DLLS:
   options=bydll.get(fid,[])
   if len(options)==1:fid,g=str(options[0]['fileId']),options[0]['guid']
   else:missing_scripts[g+':'+fid]=[x['type'] for x in options]
  return '{fileID: '+fid+', guid: '+g+', type: '+kind+'}'
 return REF.sub(sub,text)
# Trace only playable scenes' dependencies. Unused debug/pipeline assets are not imported.
selected=set();todo=[guid(SOURCE/f'Scenes/level{i}.unity') for i in range(6)]
text_types={'.asset','.prefab','.unity','.mat','.controller','.overrideController','.playable','.anim'}
while todo:
 g=todo.pop()
 if g in selected or g in refmap or g in DLLS:continue
 p=recovered.get(g)
 if p is None:continue
 selected.add(g)
 if p.suffix in text_types:
  t=p.read_text(errors='replace');todo+=re.findall('guid: ([0-9a-f]{32})',t)
# Copy known source code and original compatible shader graphs/settings.
DEST.mkdir(exist_ok=True)
if ASSETS.exists():raise RuntimeError('Candidate Assets already exists; refuse destructive overwrite')
ASSETS.mkdir()
for folder in ['Scripts','Editor','Plugins','Settings','Material','TextMesh Pro','AddOns/PipeLineSettings']:
 src=ROOT/'Assets'/folder
 if src.exists():
  target=ASSETS/folder;target.parent.mkdir(parents=True,exist_ok=True);shutil.copytree(src,target)
  if Path(str(src)+'.meta').exists():shutil.copy2(str(src)+'.meta',str(target)+'.meta')
# Keep project-configured pipeline assets and their original dependency closure.
keep=set();pending=[]
for n in ['GraphicsSettings.asset','QualitySettings.asset']:
 p=ROOT/'ProjectSettings'/n;pending+=re.findall('guid: ([0-9a-f]{32})',p.read_text(errors='replace'))
while pending:
 g=pending.pop()
 if g in keep or g not in old:continue
 keep.add(g);p=old[g]
 if p.is_file():
  target=ASSETS/p.relative_to(ROOT/'Assets');target.parent.mkdir(parents=True,exist_ok=True)
  shutil.copy2(p,target);shutil.copy2(str(p)+'.meta',str(target)+'.meta')
  if p.suffix in text_types or p.suffix=='.shadergraph':pending+=re.findall('guid: ([0-9a-f]{32})',p.read_text(errors='replace'))
written=[]
for g in sorted(selected):
 p=recovered[g]
 if not p.is_file():continue
 if p.suffix in {'.cs','.dll','.shader'}:continue
 if g in scene_map:
  orig,n=scene_map[g];target=ASSETS/f'Scenes/{n}.unity';meta=Path(str(orig)+'.meta').read_text()
 else:target=ASSETS/'Recovered'/p.relative_to(SOURCE);meta=Path(str(p)+'.meta').read_text()
 target.parent.mkdir(parents=True,exist_ok=True)
 if p.suffix in text_types:target.write_text(rewrite(p.read_text()))
 else:shutil.copy2(p,target)
 Path(str(target)+'.meta').write_text(meta)
 written.append(str(target.relative_to(ASSETS)))
for folder in ['Packages','ProjectSettings']:shutil.copytree(ROOT/folder,DEST/folder,dirs_exist_ok=True)
# Remove the explicitly disabled editor test scene from build settings: its missing assets are archived.
p=DEST/'ProjectSettings/EditorBuildSettings.asset';t=p.read_text();t=re.sub(r'  - enabled: 0\n    path: Assets/Scenes/Test.unity\n    guid: \w+\n','',t);p.write_text(t)
# Removed serialized XRSystemData field: URP 17 owns these resources in graphics settings.
for p in ASSETS.rglob('*.asset'):
 t=p.read_text(errors='replace')
 if '60e1133243b97e347b653163a8c01b64' in t:
  t=re.sub(r'  xrSystemData: \{[^\n]+\}\n', '', t);p.write_text(t)
report={'source':'origin/build c84dcea','scenes':scene_names,'recoveredAssets':len(written),'shaderReplacements':replacements,'unmappedScriptReferences':missing_scripts,'files':written}
(ROOT/'Docs/Review/resource-recovery-manifest.json').write_text(json.dumps(report,ensure_ascii=False,indent=2))
print(json.dumps({k:v for k,v in report.items() if k!='files'},ensure_ascii=False,indent=2))
if missing_scripts:raise RuntimeError('Unmapped DLL script refs; fix before importing')
