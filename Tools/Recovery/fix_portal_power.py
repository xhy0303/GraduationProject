"""Insert abs before Portal's positive powerScale base; keep all valid values unchanged."""
from pathlib import Path
import json,copy
p=Path('Assets/Material/Portal.shadergraph')
s=p.read_text();decoder=json.JSONDecoder();objs=[]
while s.strip():
 s=s.lstrip();o,i=decoder.raw_decode(s);objs.append(o);s=s[i:]
byid={o['m_ObjectId']:o for o in objs};root=objs[0]
node_id='cd741a0d4804486fa3037e385bc5f78a'
if node_id not in byid:
 power=next(o for o in objs if o['m_Type'].endswith('.PowerNode'))
 edge=next(e for e in root['m_Edges'] if e['m_InputSlot']['m_Node']['m_Id']==power['m_ObjectId'] and e['m_InputSlot']['m_SlotId']==0)
 original=copy.deepcopy(edge['m_OutputSlot'])
 inslot=copy.deepcopy(byid[power['m_Slots'][0]['m_Id']]);inslot.update(m_ObjectId='8e3f64c73b134fa981a7b58c6b785b44',m_Id=0,m_DisplayName='In',m_ShaderOutputName='In')
 outslot=copy.deepcopy(byid[power['m_Slots'][2]['m_Id']]);outslot.update(m_ObjectId='5e824ca88c0c471ab41dba17aa140b34',m_Id=1)
 node=copy.deepcopy(power);node.update(m_Type='UnityEditor.ShaderGraph.AbsoluteNode',m_ObjectId=node_id,m_Name='Absolute',m_Slots=[{'m_Id':inslot['m_ObjectId']},{'m_Id':outslot['m_ObjectId']}])
 node['m_DrawState']['m_Position']['x']=790.0;node['m_DrawState']['m_Position']['y']=-150.0
 edge['m_OutputSlot']={'m_Node':{'m_Id':node_id},'m_SlotId':1}
 root['m_Edges'].append({'m_OutputSlot':original,'m_InputSlot':{'m_Node':{'m_Id':node_id},'m_SlotId':0}})
 root['m_Nodes'].append({'m_Id':node_id});objs.extend([node,inslot,outslot])
 p.write_text('\n\n'.join(json.dumps(o,ensure_ascii=False,indent=4) for o in objs)+'\n')
