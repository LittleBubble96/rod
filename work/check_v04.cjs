const fs=require('fs');
const assert=require('assert/strict');
const src=fs.readFileSync('E:/xx/Rods/work/make_prototype_v04.cjs','utf8').split('const css=')[0];
const {scores,rivals}=new Function('require',src+';return {scores,rivals};')(require);
assert.deepEqual(scores([4,5,4,0,0,0]).total,[29,27,16,6]);
assert.deepEqual(scores([3,5,0,0,3,2]).total,[10,20,21,19]);
// Order is player, A, B, C. Only the wind region is populated in these fixtures.
const fixtures=[[[3,4,4,2],[10,0,0,5]],[[3,4,3,2],[0,10,0,5]],[[4,4,4,2],[0,0,0,10]],[[2,4,4,2],[0,0,0,0]],[[2,4,3,0],[0,10,5,0]],[[0,0,0,0],[0,0,0,0]],[[1,0,0,0],[10,0,0,0]]];
for(const [v,want] of fixtures){rivals.forEach((a,i)=>{a.fill(0);a[0]=v[i+1]});assert.deepEqual(scores([v[0],0,0,0,0,0]).total,want);}
console.log('Validated 7 cancellation scenarios and both displayed final scoreboards.');
