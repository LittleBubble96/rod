using TMPro;
using UnityEngine;

namespace GameLogic.Rod
{
    public sealed partial class RodGameView
    {
        /// <summary>
        /// 以原稿实际尺寸 853×1844 渲染。静态插画不含可交互 UI，真实进度独立改变填充条宽度。
        /// 原图仅露出空轨道右端，左端使用同一端帽镜像；该恢复差异记录在资源交付说明中。
        /// 全部节点沿用 View 私有池，切页后自动归还，背景和进度条不接收点击。
        /// </summary>
        private void RenderLoadingArt(RodViewModel vm)
        {
            var art = skin.loadingArt;
            // 全屏底色覆盖长屏/安全区余量，避免露出前一页面或主相机颜色。
            var backdrop = Panel(transform, "LoadingBackdrop", 0, 0, 0, 0, new Color32(249, 246, 232, 255), null);
            backdrop.anchorMin = Vector2.zero;
            backdrop.anchorMax = Vector2.one;
            backdrop.offsetMin = backdrop.offsetMax = Vector2.zero;
            Panel(content, "LoadingBackground", 0, 0, RodLoadingArt.Width, RodLoadingArt.Height, Color.white, art.background);
            Panel(content, "GameLogo", 135, 170, 600, 250, Color.white, art.logo);
            const float x = 103, y = 1446, h = 62;
            Panel(content, "ProgressTrack", x, y, RodLoadingArt.TrackWidth, h, Color.white, art.track);

            float progress = Mathf.Clamp01(vm.loadingProgress);
            Panel(content, "ProgressFill", x + 4, y + 4, RodLoadingArt.FillWidth * progress, h - 8, Color.white, art.fill);
            // 常规加载沿用原稿文案；真实失败原因必须可见，不能只显示“正在加载”。
            if (!vm.busy && !string.IsNullOrEmpty(vm.message))
                Text(content, "LoadingMessage", vm.message, 100, 1355, 653, 85, 28, new Color32(14, 83, 79, 255));
            else
                Text(content, "LoadingCaption", "正在加载…", 280, 1360, 300, 75, 32, new Color32(14,83,79,255));
            var percentage = Rent(content, "LoadingPercent", RodUINodePool.Kind.Text, 280, 1515, 293, 64).Text;
            percentage.font = skin.font;
            percentage.fontSize = 37;
            percentage.color = new Color32(14, 83, 79, 255);
            percentage.alignment = TextAlignmentOptions.Center;
            percentage.raycastTarget = false;
            percentage.SetText("{0}%", Mathf.RoundToInt(progress * 100));
        }
    }
}
