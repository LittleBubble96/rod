using System;
using TEngine;
using TMPro;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>固定1080×1920预制体窗口；仅绑定显示与点击，不承担玩法、存档或机型适配。</summary>
    [Window(UILayer.UI, location: "UIRodCoins")]
    public sealed class UIRodCoins : UIWindow
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
        private TextMeshProUGUI _text6;
        private TextMeshProUGUI _text7;
        private TextMeshProUGUI _text8;
        private TextMeshProUGUI _text9;
        private TextMeshProUGUI _text10;
        private TextMeshProUGUI _text11;
        private TextMeshProUGUI _text12;

        protected override void ScriptGenerator()
        {
            _button0 = FindChildComponent<Button>("Panel/Content/Close");
            _button0.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/Close"));
            _button1 = FindChildComponent<Button>("Panel/Content/DailyRow/Daily");
            _button1.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/DailyRow/Daily"));
            _button2 = FindChildComponent<Button>("Panel/Content/ShareCoinRow/ShareCoin");
            _button2.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/ShareCoinRow/ShareCoin"));
            _button3 = FindChildComponent<Button>("Panel/Content/CoinAdRow/CoinAd");
            _button3.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/CoinAdRow/CoinAd"));
            _text0 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Title");
            _text1 = FindChildComponent<TextMeshProUGUI>("Panel/Content/DailyRow/Title");
            _text2 = FindChildComponent<TextMeshProUGUI>("Panel/Content/DailyRow/Limit");
            _text3 = FindChildComponent<TextMeshProUGUI>("Panel/Content/DailyRow/Amount");
            _text4 = FindChildComponent<TextMeshProUGUI>("Panel/Content/DailyRow/Daily/Label");
            _text5 = FindChildComponent<TextMeshProUGUI>("Panel/Content/ShareCoinRow/Title");
            _text6 = FindChildComponent<TextMeshProUGUI>("Panel/Content/ShareCoinRow/Limit");
            _text7 = FindChildComponent<TextMeshProUGUI>("Panel/Content/ShareCoinRow/Amount");
            _text8 = FindChildComponent<TextMeshProUGUI>("Panel/Content/ShareCoinRow/ShareCoin/Label");
            _text9 = FindChildComponent<TextMeshProUGUI>("Panel/Content/CoinAdRow/Title");
            _text10 = FindChildComponent<TextMeshProUGUI>("Panel/Content/CoinAdRow/Limit");
            _text11 = FindChildComponent<TextMeshProUGUI>("Panel/Content/CoinAdRow/Amount");
            _text12 = FindChildComponent<TextMeshProUGUI>("Panel/Content/CoinAdRow/CoinAd/Label");
        }

        /// <summary>按节点路径更新文案，使用已缓存组件，不重建层级。</summary>
        public bool SetText(string path, string value)
        {
            switch (path)
            {
                case "Panel/Content/Title": _text0.text = value; return true;
                case "Panel/Content/DailyRow/Title": _text1.text = value; return true;
                case "Panel/Content/DailyRow/Limit": _text2.text = value; return true;
                case "Panel/Content/DailyRow/Amount": _text3.text = value; return true;
                case "Panel/Content/DailyRow/Daily/Label": _text4.text = value; return true;
                case "Panel/Content/ShareCoinRow/Title": _text5.text = value; return true;
                case "Panel/Content/ShareCoinRow/Limit": _text6.text = value; return true;
                case "Panel/Content/ShareCoinRow/Amount": _text7.text = value; return true;
                case "Panel/Content/ShareCoinRow/ShareCoin/Label": _text8.text = value; return true;
                case "Panel/Content/CoinAdRow/Title": _text9.text = value; return true;
                case "Panel/Content/CoinAdRow/Limit": _text10.text = value; return true;
                case "Panel/Content/CoinAdRow/Amount": _text11.text = value; return true;
                case "Panel/Content/CoinAdRow/CoinAd/Label": _text12.text = value; return true;
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
