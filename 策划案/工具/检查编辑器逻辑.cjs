// 纯代码单元检查，不启动或控制浏览器，不替代视觉验收。
const fs=require('fs'),path=require('path'),vm=require('vm'),assert=require('assert');
const folder=path.join(__dirname,'../关卡');
const html=fs.readFileSync(path.join(folder,'关卡编辑器.html'),'utf8');
const data=JSON.parse(fs.readFileSync(path.join(folder,'首批200关.json'),'utf8'));
const js=html.match(/<script>\s*([\s\S]*?)<\/script>/)[1];
new vm.Script(js);
function extract(name){const start=js.indexOf('function '+name+'(');let brace=js.indexOf('{',start),depth=0; // use function boundaries, avoiding nested template literals
 const next=js.indexOf('\nfunction ',brace); return js.slice(start,next<0?js.length:next);}
const context=vm.createContext({initial:data,data,F:data.faces,V:data.dieValues});
vm.runInContext(extract('totals')+'\n'+extract('score'),context);
const a=js.indexOf('function legalFaces('),b=js.indexOf("$('#loadWitness').onclick",a);
vm.runInContext(js.slice(a,b),context);
assert(context.validate(data));let count=0;
for(const l of data.levels){const result=context.score([context.totals(l.winningWitness.facesByDieId),...l.opponentFaces.map(context.totals)]).scores;assert.deepStrictEqual(Array.from(result),l.winningWitness.finalScores);count++;}
function reject(mutate){const changed=structuredClone(data);mutate(changed);assert.throws(()=>context.validate(changed));}
reject(d=>d.prizes[0][0]=99);reject(d=>d.levels[0].reward.coin=-1);reject(d=>d.levels[0].difficulty=11);reject(d=>d.levels[0].opponentFaces[0][0]=6);reject(d=>d.levels[0].winningWitness.finalScores[0]=-1);reject(d=>d.levels[1].opponentFaces=structuredClone(d.levels[0].opponentFaces));
console.log(`PASS: JavaScript syntax, ${count} score fixtures, 6 invalid imports rejected`);
