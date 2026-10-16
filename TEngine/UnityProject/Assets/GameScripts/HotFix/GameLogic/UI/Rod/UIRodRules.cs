using System;
using TEngine;
using TMPro;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>固定1080×1920预制体窗口；仅绑定显示与点击，不承担玩法、存档或机型适配。</summary>
    [Window(UILayer.UI, location: "UIRodRules")]
    public sealed class UIRodRules : UIWindow
    {
        /// <summary>将节点路径交给调用方处理业务；窗口只持有自身生命周期内的订阅。</summary>
        public event Action<string> Clicked;
        private Button _button0;
        private Button _button1;
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
        private TextMeshProUGUI _text22;
        private TextMeshProUGUI _text23;
        private TextMeshProUGUI _text24;
        private TextMeshProUGUI _text25;

        protected override void ScriptGenerator()
        {
            _button0 = FindChildComponent<Button>("Panel/Content/Close");
            _button0.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/Close"));
            _button1 = FindChildComponent<Button>("Panel/Content/Understood");
            _button1.onClick.AddListener(() => Clicked?.Invoke("Panel/Content/Understood"));
            _text0 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Title");
            _text1 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_1/NumberPlate/Number");
            _text2 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_1/Title");
            _text3 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_1/Body");
            _text4 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_1/Points_0");
            _text5 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_1/Points_1");
            _text6 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_1/Points_2");
            _text7 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_2/NumberPlate/Number");
            _text8 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_2/Title");
            _text9 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_2/Body");
            _text10 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_3/NumberPlate/Number");
            _text11 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_3/Title");
            _text12 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_3/Body");
            _text13 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_4/NumberPlate/Number");
            _text14 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_4/Title");
            _text15 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_4/Body");
            _text16 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_4/ExampleScore_0/Name");
            _text17 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_4/ExampleScore_0/Score");
            _text18 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_4/ExampleScore_1/Name");
            _text19 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_4/ExampleScore_1/Score");
            _text20 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_4/ExampleScore_2/Name");
            _text21 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_4/ExampleScore_2/Score");
            _text22 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_4/ExampleScore_3/Name");
            _text23 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Rule_4/ExampleScore_3/Score");
            _text24 = FindChildComponent<TextMeshProUGUI>("Panel/Content/RerollNote/Text");
            _text25 = FindChildComponent<TextMeshProUGUI>("Panel/Content/Understood/Label");
        }

        /// <summary>按节点路径更新文案，使用已缓存组件，不重建层级。</summary>
        public bool SetText(string path, string value)
        {
            switch (path)
            {
                case "Panel/Content/Title": _text0.text = value; return true;
                case "Panel/Content/Rule_1/NumberPlate/Number": _text1.text = value; return true;
                case "Panel/Content/Rule_1/Title": _text2.text = value; return true;
                case "Panel/Content/Rule_1/Body": _text3.text = value; return true;
                case "Panel/Content/Rule_1/Points_0": _text4.text = value; return true;
                case "Panel/Content/Rule_1/Points_1": _text5.text = value; return true;
                case "Panel/Content/Rule_1/Points_2": _text6.text = value; return true;
                case "Panel/Content/Rule_2/NumberPlate/Number": _text7.text = value; return true;
                case "Panel/Content/Rule_2/Title": _text8.text = value; return true;
                case "Panel/Content/Rule_2/Body": _text9.text = value; return true;
                case "Panel/Content/Rule_3/NumberPlate/Number": _text10.text = value; return true;
                case "Panel/Content/Rule_3/Title": _text11.text = value; return true;
                case "Panel/Content/Rule_3/Body": _text12.text = value; return true;
                case "Panel/Content/Rule_4/NumberPlate/Number": _text13.text = value; return true;
                case "Panel/Content/Rule_4/Title": _text14.text = value; return true;
                case "Panel/Content/Rule_4/Body": _text15.text = value; return true;
                case "Panel/Content/Rule_4/ExampleScore_0/Name": _text16.text = value; return true;
                case "Panel/Content/Rule_4/ExampleScore_0/Score": _text17.text = value; return true;
                case "Panel/Content/Rule_4/ExampleScore_1/Name": _text18.text = value; return true;
                case "Panel/Content/Rule_4/ExampleScore_1/Score": _text19.text = value; return true;
                case "Panel/Content/Rule_4/ExampleScore_2/Name": _text20.text = value; return true;
                case "Panel/Content/Rule_4/ExampleScore_2/Score": _text21.text = value; return true;
                case "Panel/Content/Rule_4/ExampleScore_3/Name": _text22.text = value; return true;
                case "Panel/Content/Rule_4/ExampleScore_3/Score": _text23.text = value; return true;
                case "Panel/Content/RerollNote/Text": _text24.text = value; return true;
                case "Panel/Content/Understood/Label": _text25.text = value; return true;
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
