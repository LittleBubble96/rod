using System;
using UnityEngine;

namespace GameLogic.Rod
{
    /// <summary>
    /// Loading 页的正式资源绑定。所有 Sprite 由编辑器导入，运行时不读文件、不切图、不创建材质。
    /// 原稿上半部与清理后的下半部独立，进度条和文字由 View 按真实进度绘制。
    /// 引用归皮肤及其资源加载句柄所有，View 不单独卸载共享纹理。
    /// </summary>
    public sealed class RodLoadingArt : ScriptableObject
    {
        public const float Width = 853;
        public const float Height = 1844;
        public const float IllustrationHeight = 1340;
        public const float TrackWidth = 648;
        public const float FillWidth = 640;

        public Sprite illustration;
        public Sprite footer;
        public Sprite background;
        public Sprite homeBackground;
        public Sprite logo;
        public Sprite track;
        // 旧序列化字段保留用于迁移；修复工具清空，不再绘制或打包组合切片。
        [HideInInspector]
        public Sprite trackMiddle;
        [HideInInspector]
        public Sprite trackEnd;
        public Sprite fill;
        [HideInInspector]
        public Sprite caption;

        /// <summary>缺失资源应在接入时暴露，不能把空白画面当成加载完成。</summary>
        public void Validate()
        {
            if (!background || !logo || !track || !fill)
                throw new InvalidOperationException("Loading 正式资源引用不完整。");
            if (fill.border == Vector4.zero || track.border == Vector4.zero)
                throw new InvalidOperationException("Loading 填充条缺少九宫边界。");
        }
    }
}
