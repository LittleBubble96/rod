using System;
using TEngine;
using TMPro;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>固定1080×1920预制体窗口；仅绑定显示与点击，不承担玩法、存档或机型适配。</summary>
    [Window(UILayer.UI, location: "UIRodSettings")]
    public sealed class UIRodSettings : UIWindow
    {
        /// <summary>将节点路径交给调用方处理业务；窗口只持有自身生命周期内的订阅。</summary>
        public event Action<string> Clicked;
        private Button _button0;
        private Button _button1;
        private Button _button2;
        private Button _button3;
        private TextMeshProUGUI _text0;
        private TextMeshProUGUI _text1;
        private TextMeshProUGUI _text2;
        private TextMeshProUGUI _text3;
        private TextMeshProUGUI _text4;
        private TextMeshProUGUI _text5;

        protected override void ScriptGenerator()
        {
            _button0 = FindChildComponent<Button>("Panel/Content/Close");
            _button0.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/Close"));
            _button1 = FindChildComponent<Button>("Panel/Content/MusicRow/Music");
            _button1.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/MusicRow/Music"));
            _button2 = FindChildComponent<Button>("Panel/Content/SoundRow/Sound");
            _button2.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/SoundRow/Sound"));
            _button3 = FindChildComponent<Button>("Panel/Content/Understood");
            _button3.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/Understood"));
            _text0 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Title");
            _text1 = FindChildComponent<TextMeshProUGUI>("Panel/Content/MusicRow/Name");
            _text2 = FindChildComponent<TextMeshProUGUI>("Panel/Content/MusicRow/Music/Label");
            _text3 = FindChildComponent<TextMeshProUGUI>("Panel/Content/SoundRow/Name");
            _text4 = FindChildComponent<TextMeshProUGUI>("Panel/Content/SoundRow/Sound/Label");
            _text5 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Understood/Label");
        }

        /// <summary>按节点路径更新文案，使用已缓存组件，不重建层级。</summary>
        public bool SetText(string path, string value)
        {
            switch (path)
            {
                case "Panel/Content/Title": _text0.text = value; return true;
                case "Panel/Content/MusicRow/Name": _text1.text = value; return true;
                case "Panel/Content/MusicRow/Music/Label": _text2.text = value; return true;
                case "Panel/Content/SoundRow/Name": _text3.text = value; return true;
                case "Panel/Content/SoundRow/Sound/Label": _text4.text = value; return true;
                case "Panel/Content/Understood/Label": _text5.text = value; return true;
                default: return false;
            }
        }

        /// <summary>关闭时释放外部业务订阅，缓存窗口再次打开由调用方重新订阅。</summary>
        protected override void OnClose()
        {
            Clicked = null;
            base.OnClose();
        }
    }
}
