using System;
using TEngine;
using TMPro;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>固定1080×1920预制体窗口；仅绑定显示与点击，不承担玩法、存档或机型适配。</summary>
    [Window(UILayer.UI, location: "UIRodWon")]
    public sealed class UIRodWon : UIWindow
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
        private TextMeshProUGUI _text13;
        private TextMeshProUGUI _text14;
        private TextMeshProUGUI _text15;
        private TextMeshProUGUI _text16;
        private TextMeshProUGUI _text17;
        private TextMeshProUGUI _text18;
        private TextMeshProUGUI _text19;
        private TextMeshProUGUI _text20;
        private TextMeshProUGUI _text21;

        protected override void ScriptGenerator()
        {
            _button0 = FindChildComponent<Button>("Panel/Content/Next");
            _button0.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/Next"));
            _button1 = FindChildComponent<Button>("Panel/Content/Double");
            _button1.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/Double"));
            _button2 = FindChildComponent<Button>("Panel/Content/Home");
            _button2.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/Home"));
            _button3 = FindChildComponent<Button>("Panel/Content/Share");
            _button3.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/Share"));
            _text0 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Title");
            _text1 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Level");
            _text2 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_0/Rank");
            _text3 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_0/Name");
            _text4 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_0/Score");
            _text5 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_1/Rank");
            _text6 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_1/Name");
            _text7 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_1/Score");
            _text8 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_2/Rank");
            _text9 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_2/Name");
            _text10 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_2/Score");
            _text11 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_3/Rank");
            _text12 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_3/Name");
            _text13 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rank_3/Score");
            _text14 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RewardTitle");
            _text15 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RewardLabel_Coin");
            _text16 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RewardLabel_RerollIcon");
            _text17 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RewardLabel_NextIcon");
            _text18 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Next/Label");
            _text19 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Double/Label");
            _text20 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Home/Label");
            _text21 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Share/Label");
        }

        /// <summary>按节点路径更新文案，使用已缓存组件，不重建层级。</summary>
        public bool SetText(string path, string value)
        {
            switch (path)
            {
                case "Panel/Content/Title": _text0.text = value; return true;
                case "Panel/Content/Level": _text1.text = value; return true;
                case "Panel/Content/Rank_0/Rank": _text2.text = value; return true;
                case "Panel/Content/Rank_0/Name": _text3.text = value; return true;
                case "Panel/Content/Rank_0/Score": _text4.text = value; return true;
                case "Panel/Content/Rank_1/Rank": _text5.text = value; return true;
                case "Panel/Content/Rank_1/Name": _text6.text = value; return true;
                case "Panel/Content/Rank_1/Score": _text7.text = value; return true;
                case "Panel/Content/Rank_2/Rank": _text8.text = value; return true;
                case "Panel/Content/Rank_2/Name": _text9.text = value; return true;
                case "Panel/Content/Rank_2/Score": _text10.text = value; return true;
                case "Panel/Content/Rank_3/Rank": _text11.text = value; return true;
                case "Panel/Content/Rank_3/Name": _text12.text = value; return true;
                case "Panel/Content/Rank_3/Score": _text13.text = value; return true;
                case "Panel/Content/RewardTitle": _text14.text = value; return true;
                case "Panel/Content/RewardLabel_Coin": _text15.text = value; return true;
                case "Panel/Content/RewardLabel_RerollIcon": _text16.text = value; return true;
                case "Panel/Content/RewardLabel_NextIcon": _text17.text = value; return true;
                case "Panel/Content/Next/Label": _text18.text = value; return true;
                case "Panel/Content/Double/Label": _text19.text = value; return true;
                case "Panel/Content/Home/Label": _text20.text = value; return true;
                case "Panel/Content/Share/Label": _text21.text = value; return true;
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
