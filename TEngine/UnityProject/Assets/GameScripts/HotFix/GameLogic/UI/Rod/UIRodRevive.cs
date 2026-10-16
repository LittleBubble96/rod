using System;
using TEngine;
using TMPro;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>固定1080×1920预制体窗口；仅绑定显示与点击，不承担玩法、存档或机型适配。</summary>
    [Window(UILayer.UI, location: "UIRodRevive")]
    public sealed class UIRodRevive : UIWindow
    {
        /// <summary>将节点路径交给调用方处理业务；窗口只持有自身生命周期内的订阅。</summary>
        public event Action<string> Clicked;
        private Button _button0;
        private Button _button1;
        private Button _button2;
        private Button _button3;
        private Button _button4;
        private Button _button5;
        private Button _button6;
        private Button _button7;
        private Button _button8;
        private Button _button9;
        private Button _button10;
        private Button _button11;
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

        protected override void ScriptGenerator()
        {
            _button0 = FindChildComponent<Button>("Panel/Content/Close");
            _button0.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/Close"));
            _button1 = FindChildComponent<Button>("Panel/Content/Info");
            _button1.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/Info"));
            _button2 = FindChildComponent<Button>("Panel/Content/RevivalTray/Die_方_0");
            _button2.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/RevivalTray/Die_方_0"));
            _button3 = FindChildComponent<Button>("Panel/Content/RevivalTray/Die_风");
            _button3.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/RevivalTray/Die_风"));
            _button4 = FindChildComponent<Button>("Panel/Content/RevivalTray/Die_月");
            _button4.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/RevivalTray/Die_月"));
            _button5 = FindChildComponent<Button>("Panel/Content/RevivalTray/Die_花_0");
            _button5.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/RevivalTray/Die_花_0"));
            _button6 = FindChildComponent<Button>("Panel/Content/RevivalTray/Die_圆_0");
            _button6.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/RevivalTray/Die_圆_0"));
            _button7 = FindChildComponent<Button>("Panel/Content/RevivalTray/Die_花_1");
            _button7.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/RevivalTray/Die_花_1"));
            _button8 = FindChildComponent<Button>("Panel/Content/RevivalTray/Die_圆_1");
            _button8.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/RevivalTray/Die_圆_1"));
            _button9 = FindChildComponent<Button>("Panel/Content/RevivalTray/Die_雪");
            _button9.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/RevivalTray/Die_雪"));
            _button10 = FindChildComponent<Button>("Panel/Content/RevivalTray/Die_方_1");
            _button10.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/RevivalTray/Die_方_1"));
            _button11 = FindChildComponent<Button>("Panel/Content/ReviveAd");
            _button11.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/ReviveAd"));
            _text0 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Title");
            _text1 = FindChildComponent<TextMeshProUGUI>("Panel/Content/SelectionPlate/Selection");
            _text2 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RevivalTray/Die_方_0/Points");
            _text3 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RevivalTray/Die_风/Points");
            _text4 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RevivalTray/Die_月/Points");
            _text5 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RevivalTray/Die_花_0/Points");
            _text6 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RevivalTray/Die_圆_0/Points");
            _text7 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RevivalTray/Die_花_1/Points");
            _text8 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RevivalTray/Die_圆_1/Points");
            _text9 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RevivalTray/Die_雪/Points");
            _text10 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RevivalTray/Die_方_1/Points");
            _text11 = FindChildComponent<TextMeshProUGUI>("Panel/Content/ReviveAd/Label");
        }

        /// <summary>按节点路径更新文案，使用已缓存组件，不重建层级。</summary>
        public bool SetText(string path, string value)
        {
            switch (path)
            {
                case "Panel/Content/Title": _text0.text = value; return true;
                case "Panel/Content/SelectionPlate/Selection": _text1.text = value; return true;
                case "Panel/Content/RevivalTray/Die_方_0/Points": _text2.text = value; return true;
                case "Panel/Content/RevivalTray/Die_风/Points": _text3.text = value; return true;
                case "Panel/Content/RevivalTray/Die_月/Points": _text4.text = value; return true;
                case "Panel/Content/RevivalTray/Die_花_0/Points": _text5.text = value; return true;
                case "Panel/Content/RevivalTray/Die_圆_0/Points": _text6.text = value; return true;
                case "Panel/Content/RevivalTray/Die_花_1/Points": _text7.text = value; return true;
                case "Panel/Content/RevivalTray/Die_圆_1/Points": _text8.text = value; return true;
                case "Panel/Content/RevivalTray/Die_雪/Points": _text9.text = value; return true;
                case "Panel/Content/RevivalTray/Die_方_1/Points": _text10.text = value; return true;
                case "Panel/Content/ReviveAd/Label": _text11.text = value; return true;
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
