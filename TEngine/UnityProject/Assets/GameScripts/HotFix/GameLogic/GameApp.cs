using System;
using Cysharp.Threading.Tasks;
using GameLogic;
using GameLogic.Rod;
using GameSDK;
using TEngine;
using UnityEngine;
#if ENABLE_OBFUZ
using Obfuz;
#endif

#if ENABLE_OBFUZ
[ObfuzIgnore(ObfuzScope.TypeName | ObfuzScope.MethodName)]
#endif
public partial class GameApp
{
    public static void Entrance(object[] objects)
    {
        GameEventHelper.Init();Utility.Unity.AddDestroyListener(Release);StartGameLogic().Forget();
    }
    private static async UniTask StartGameLogic()
    {
        int step=11;
        try{
            StartupTelemetry.Start(step);
            var root=await GameModule.Resource.LoadGameObjectAsync("RodGameRoot");
            if(root==null)throw new InvalidOperationException("RodGameRoot资源加载失败。");
            UnityEngine.Object.DontDestroyOnLoad(root);
            StartupTelemetry.Success(step);
            for(step=12;step<=15;step++){
                StartupTelemetry.Start(step);
                if(step==12)await root.GetComponent<RodGameHost>().InitializeAsync();
                StartupTelemetry.Success(step);
            }
            step=16;StartupTelemetry.Start(step);
            // 算筹入口独占音乐声道，恢复存档开关后再播放，避免此处提前播放造成双重音轨。
            StartupTelemetry.Success(step);GameEvent.Send(EventDefine.GameStartSuccessEvent);SDK.ReportGameStart();
        }catch(Exception e){StartupTelemetry.Fail(step,"rod_startup_exception",e.Message);Debug.LogException(e);}
    }
    private static void Release()=>SingletonSystem.Release();
}
