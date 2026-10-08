using System.IO;
using GameLogic.Rod;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rod.Editor
{
    public static class RodUIBuilder
    {
        public const string SkinPath="Assets/AssetRaw/UI/Rod/RodSkin.asset";
        public const string PrefabPath="Assets/AssetRaw/UI/Rod/RodGameRoot.prefab";
        public const string ScenePath="Assets/Scenes/RodPreview.unity";
        [MenuItem("六面算筹/生成UI与预览场景")]
        public static void Generate()
        {
            Directory.CreateDirectory("Assets/AssetRaw/UI/Rod");Directory.CreateDirectory("Assets/AssetRaw/UIRaw/Atlas/Rod");Directory.CreateDirectory("Assets/Scenes");
            WritePlate("panel_9s",new Color32(255,255,255,255),new Color32(120,154,127,255));
            WritePlate("button_9s",new Color32(255,255,255,255),new Color32(174,160,123,255));
            WritePlate("slot_9s",new Color32(255,255,255,255),new Color32(142,159,145,255));
            AssetDatabase.Refresh();
            var skin=AssetDatabase.LoadAssetAtPath<RodUISkin>(SkinPath);
            if(skin==null){skin=ScriptableObject.CreateInstance<RodUISkin>();AssetDatabase.CreateAsset(skin,SkinPath);}
            skin.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/AssetRaw/Fonts/AlimamaFangYuanTiVF SDF.asset");
            skin.panel=Sprite("panel_9s");skin.button=Sprite("button_9s");skin.slot=Sprite("slot_9s");EditorUtility.SetDirty(skin);
            var root=new GameObject("RodGameRoot",typeof(RectTransform));
            var host=root.AddComponent<RodGameHost>();host.skin=skin;host.levelConfig=AssetDatabase.LoadAssetAtPath<TextAsset>(RodLevelEditor.CatalogPath);
            root.AddComponent<RodGameView>().skin=skin;
            PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);Object.DestroyImmediate(root);AssetDatabase.SaveAssets();
            // 单独预览场景不覆盖当前打开场景；保留原场景设置。
            var previous=EditorSceneManager.GetSceneManagerSetup();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            instance.GetComponent<RodGameHost>().usePreviewSave=true;
            new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            var camera=new GameObject("Camera",typeof(Camera));camera.GetComponent<Camera>().backgroundColor=skin.mint;
            EditorSceneManager.SaveScene(scene,ScenePath);
            if(previous.Length>0)EditorSceneManager.RestoreSceneManagerSetup(previous);
            Debug.Log("[Rod] 已生成3张96px九宫底板、游戏入口Prefab和独立预览场景。");
        }
        [MenuItem("六面算筹/打开游戏预览场景")]
        public static void OpenPreview(){if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene(ScenePath);}
        private static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AssetRaw/UIRaw/Atlas/Rod/"+name+".png");
        private static void WritePlate(string name,Color fill,Color border)
        {
            const int n=96;var texture=new Texture2D(n,n,TextureFormat.RGBA32,false);
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){
                float dx=Mathf.Max(Mathf.Abs(x-47.5f)-27,0),dy=Mathf.Max(Mathf.Abs(y-47.5f)-27,0),dist=Mathf.Sqrt(dx*dx+dy*dy);
                Color c=dist>20?Color.clear:dist>17?border:Color.Lerp(fill*.86f,fill,Mathf.Clamp01(y/28f));
                if(c.a>0)c.a=1;texture.SetPixel(x,y,c);
            }
            texture.Apply();File.WriteAllBytes("Assets/AssetRaw/UIRaw/Atlas/Rod/"+name+".png",texture.EncodeToPNG());Object.DestroyImmediate(texture);
        }
    }
}
