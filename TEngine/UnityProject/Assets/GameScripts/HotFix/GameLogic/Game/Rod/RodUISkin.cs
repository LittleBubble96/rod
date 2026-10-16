using TMPro;
using UnityEngine;

namespace GameLogic.Rod
{
    public sealed class RodUISkin : ScriptableObject
    {
        public TMP_FontAsset font;
        /// <summary>分批接入的 Loading 素材；未绑定时保留旧工程的功能预览路径。</summary>
        public RodLoadingArt loadingArt;
        /// <summary>首页、对局及公共弹窗的分层资源；按槽位共享，不随每次刷新加载。</summary>
        public RodPageArt pageArt;
        /// <summary>入口持有的音乐与点击音效；音轨由 Host 创建并随入口销毁。</summary>
        public AudioClip musicClip, clickClip;
        public Sprite panel,button,slot;
        public Color ink=new Color32(30,76,65,255),cream=new Color32(255,247,225,255),mint=new Color32(207,230,208,255),gold=new Color32(246,190,70,255);
    }
}
