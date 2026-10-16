using System;
using TEngine;
using TMPro;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>固定1080×1920预制体窗口；仅绑定显示与点击，不承担玩法、存档或机型适配。</summary>
    [Window(UILayer.UI, location: "UIRodHome")]
    public sealed class UIRodHome : UIWindow
    {
        /// <summary>将节点路径交给调用方处理业务；窗口只持有自身生命周期内的订阅。</summary>
        public event Action<string> Clicked;
        private Button _button0;
        private Button _button1;
        private Button _button2;
        private Button _button3;
        private Button _button4;
        private Button _button5;
        private TextMeshProUGUI _text0;
        private TextMeshProUGUI _text1;
        private TextMeshProUGUI _text2;
        private TextMeshProUGUI _text3;
        private TextMeshProUGUI _text4;
        private TextMeshProUGUI _text5;
        private TextMeshProUGUI _text6;

        protected override void ScriptGenerator()
        {
            _button0 = FindChildComponent<Button>("Panel/Start");
            _button0.onClick.AddListener(() => Clicked?.Invoke("Panel/Start"));
            _button1 = FindChildComponent<Button>("Panel/Rules");
            _button1.onClick.AddListener(() => Clicked?.Invoke("Panel/Rules"));
            _button2 = FindChildComponent<Button>("Panel/Club");
            _button2.onClick.AddListener(() => Clicked?.Invoke("Panel/Club"));
            _button3 = FindChildComponent<Button>("Panel/Share");
            _button3.onClick.AddListener(() => Clicked?.Invoke("Panel/Share"));
            _button4 = FindChildComponent<Button>("Panel/Settings");
            _button4.onClick.AddListener(() => Clicked?.Invoke("Panel/Settings"));
            _button5 = FindChildComponent<Button>("Panel/GetCoin");
            _button5.onClick.AddListener(() => Clicked?.Invoke("Panel/GetCoin"));
            _text0 = FindChildComponent<TextMeshProUGUI>("Panel/Progress");
            _text1 = FindChildComponent<TextMeshProUGUI>("Panel/Start/Label");
            _text2 = FindChildComponent<TextMeshProUGUI>("Panel/Rules/Label");
            _text3 = FindChildComponent<TextMeshProUGUI>("Panel/ClubText");
            _text4 = FindChildComponent<TextMeshProUGUI>("Panel/ShareText");
            _text5 = FindChildComponent<TextMeshProUGUI>("Panel/SettingsText");
            _text6 = FindChildComponent<TextMeshProUGUI>("Panel/GetCoin/Count");
        }

        /// <summary>按节点路径更新文案，使用已缓存组件，不重建层级。</summary>
        public bool SetText(string path, string value)
        {
            switch (path)
            {
                case "Panel/Progress": _text0.text = value; return true;
                case "Panel/Start/Label": _text1.text = value; return true;
                case "Panel/Rules/Label": _text2.text = value; return true;
                case "Panel/ClubText": _text3.text = value; return true;
                case "Panel/ShareText": _text4.text = value; return true;
                case "Panel/SettingsText": _text5.text = value; return true;
                case "Panel/GetCoin/Count": _text6.text = value; return true;
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
