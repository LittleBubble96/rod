using System;
using System.IO;
using GameLogic.Data;
using GameLogic.Rod;

internal static class RodFoundationTests
{
    private static int assertions;
    private static void Check(bool condition, string name) { assertions++; if (!condition) throw new Exception(name); }
    private static void Throws<T>(Action action, string name) where T:Exception
    {
        try { action(); } catch(T) { assertions++; return; }
        throw new Exception("Expected " + typeof(T).Name + ": " + name);
    }
    private static int[] Wind(params int[] n) { var p=new int[4,6]; for(int i=0;i<4;i++)p[i,0]=n[i]; return RodRules.Score(p); }
    private static bool Same(int[] x, params int[] y) { if(x.Length!=y.Length)return false; for(int i=0;i<x.Length;i++)if(x[i]!=y[i])return false;return true; }
    public static void Main()
    {
        Check(Same(Wind(4,4,3,2),0,0,10,5),"two cancel and promotion");
        Check(Same(Wind(4,4,4,2),0,0,0,10),"three cancel");
        Check(Same(Wind(4,4,2,2),0,0,0,0),"two repeated groups");
        Check(Same(Wind(0,0,0,0),0,0,0,0),"zeros excluded");
        Check(Same(Wind(4,3,2,0),10,5,0,0),"ordinary ranking");
        Check(Same(Wind(4,0,0,0),10,0,0,0),"single participant");
        Check(RodRules.PlayerWins(new[]{0,0,0,0}),"all zero tie passes");
        Check(RodRules.PlayerWins(new[]{20,20,19,10}),"highest tie passes");
        Check(!RodRules.PlayerWins(new[]{19,20,18,10}),"lower score fails");
        Throws<ArgumentException>(()=>Wind(-1,2,3,4),"negative power");
        uint a=123456,b=123456; int[] frequency=new int[6];
        for(int i=0;i<10000;i++) { int f=RodRules.NextFace(ref a); Check(f>=0 && f<6 && f==RodRules.NextFace(ref b),"random replay"); frequency[f]++; }
        foreach(int n in frequency) Check(n>1400 && n<1900,"face sampling sanity");
        uint zero=0; Throws<ArgumentException>(()=>RodRules.NextFace(ref zero),"zero rng state");
        var run=new RodRun {runId="r1",levelId=1,contentVersion="test",randomState=2,phase=RodPhase.Failed,
            opponentPower=new[]{13,0,0,0,0,0,0,13,0,0,0,0,0,0,13,0,0,0},dice=new RodDie[9]};
        int[] values={3,2,2,1,1,1,1,1,1};
        for(int i=0;i<9;i++)run.dice[i]=new RodDie{id=i,value=values[i],face=5,region=5};
        RodRules.ValidateRun(run);
        Check(RodRules.CanRevive(run,new[]{0,1,3}),"3+2+1 revival");
        Check(!RodRules.CanRevive(run,new[]{0,0}),"duplicate id rejected");
        Check(!RodRules.CanRevive(run,new[]{0,1}),"five points rejected");
        Check(!RodRules.CanRevive(run,new[]{0,1,2}),"seven points rejected");
        Check(!RodRules.CanRevive(run,new[]{0,1,99}),"foreign die rejected");
        run.dice[0].value=4; Throws<InvalidOperationException>(()=>RodRules.ValidateRun(run),"point conservation"); run.dice[0].value=3;
        run.dice[0].id=1; Throws<InvalidOperationException>(()=>RodRules.ValidateRun(run),"unique dice"); run.dice[0].id=0;
        run.phase=RodPhase.Won; Throws<InvalidOperationException>(()=>RodRules.ValidateRun(run),"forged win");
        run.phase=RodPhase.AwaitGroup; foreach(var d in run.dice)d.region=-1;
        Check(RodRules.SelectGroup(run,5).Length==9,"select whole same-face group");
        run.dice[0].region=5; Check(RodRules.SelectGroup(run,5).Length==8,"placed dice excluded");
        var state=new RodSaveData(); state.Validate(); state.schemaVersion=2;
        Throws<InvalidOperationException>(()=>state.Validate(),"future save rejected");
        state.schemaVersion=1;state.currentLevelId=201; Throws<InvalidOperationException>(()=>state.Validate(),"level bound");

        var slots=new string[2]; bool partial=false,io=false;
        var journal=new SnapshotJournal(i=>slots[i],(i,s)=>{if(io)throw new IOException();slots[i]=partial?s.Substring(0,10):s;},i=>slots[i]=null);
        Check(journal.Read()==null,"empty store");
        journal.Write("{\"coins\":10}"); journal.Write("{\"coins\":20}");
        Check(journal.Read()=="{\"coins\":20}","latest revision wins");
        partial=true;Throws<IOException>(()=>journal.Write("{\"coins\":30}"),"partial write rejected");
        Check(journal.Read()=="{\"coins\":20}","last good snapshot survives");
        partial=false; journal.Write("{\"coins\":40}");
        Check(journal.Read()=="{\"coins\":40}","repair older slot");
        io=true;Throws<IOException>(()=>journal.Write("{\"coins\":50}"),"write exception");
        Check(journal.Read()=="{\"coins\":40}","write failure keeps current");io=false;
        slots[0]="truncated";slots[1]="corrupt";
        Throws<InvalidDataException>(()=>journal.Read(),"both damaged surfaced");
        Throws<InvalidDataException>(()=>journal.Write("new game"),"do not overwrite both damaged");
        Check(slots[0]=="truncated" && slots[1]=="corrupt","damaged evidence retained");
        journal.DeleteAll();Check(journal.Read()==null,"explicit reset");
        Throws<ArgumentException>(()=>journal.Write(" "),"empty write rejected");
        journal.Write("中文存档\n包含换行");Check(journal.Read()=="中文存档\n包含换行","utf8 payload");
        var reader=new SnapshotJournal(i=>slots[i],(i,s)=>slots[i]=s,i=>slots[i]=null);
        Check(reader.Read()==journal.Read(),"new process recovery");
        Console.WriteLine("PASS: " + assertions + " assertions (scoring, RNG, revival, validation, save fault injection)");
    }
}
