using System;
using TEngine;
using TMPro;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>固定1080×1920预制体窗口；仅绑定显示与点击，不承担玩法、存档或机型适配。</summary>
    [Window(UILayer.UI, location: "UIRodBoard")]
    public sealed class UIRodBoard : UIWindow
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
        private Button _button12;
        private Button _button13;
        private Button _button14;
        private Button _button15;
        private Button _button16;
        private Button _button17;
        private Button _button18;
        private Button _button19;
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
        private TextMeshProUGUI _text26;
        private TextMeshProUGUI _text27;
        private TextMeshProUGUI _text28;
        private TextMeshProUGUI _text29;
        private TextMeshProUGUI _text30;
        private TextMeshProUGUI _text31;
        private TextMeshProUGUI _text32;
        private TextMeshProUGUI _text33;
        private TextMeshProUGUI _text34;
        private TextMeshProUGUI _text35;
        private TextMeshProUGUI _text36;
        private TextMeshProUGUI _text37;
        private TextMeshProUGUI _text38;
        private TextMeshProUGUI _text39;
        private TextMeshProUGUI _text40;
        private TextMeshProUGUI _text41;
        private TextMeshProUGUI _text42;
        private TextMeshProUGUI _text43;
        private TextMeshProUGUI _text44;
        private TextMeshProUGUI _text45;
        private TextMeshProUGUI _text46;
        private TextMeshProUGUI _text47;
        private TextMeshProUGUI _text48;
        private TextMeshProUGUI _text49;
        private TextMeshProUGUI _text50;
        private TextMeshProUGUI _text51;
        private TextMeshProUGUI _text52;
        private TextMeshProUGUI _text53;
        private TextMeshProUGUI _text54;

        protected override void ScriptGenerator()
        {
            _button0 = FindChildComponent<Button>("Panel/Region_0");
            _button0.onClick.AddListener(() => Clicked?.Invoke("Panel/Region_0"));
            _button1 = FindChildComponent<Button>("Panel/Region_1");
            _button1.onClick.AddListener(() => Clicked?.Invoke("Panel/Region_1"));
            _button2 = FindChildComponent<Button>("Panel/Region_2");
            _button2.onClick.AddListener(() => Clicked?.Invoke("Panel/Region_2"));
            _button3 = FindChildComponent<Button>("Panel/Region_3");
            _button3.onClick.AddListener(() => Clicked?.Invoke("Panel/Region_3"));
            _button4 = FindChildComponent<Button>("Panel/Region_4");
            _button4.onClick.AddListener(() => Clicked?.Invoke("Panel/Region_4"));
            _button5 = FindChildComponent<Button>("Panel/Region_5");
            _button5.onClick.AddListener(() => Clicked?.Invoke("Panel/Region_5"));
            _button6 = FindChildComponent<Button>("Panel/Tray/Die_方_0");
            _button6.onClick.AddListener(() => Clicked?.Invoke("Panel/Tray/Die_方_0"));
            _button7 = FindChildComponent<Button>("Panel/Tray/Die_风");
            _button7.onClick.AddListener(() => Clicked?.Invoke("Panel/Tray/Die_风"));
            _button8 = FindChildComponent<Button>("Panel/Tray/Die_月");
            _button8.onClick.AddListener(() => Clicked?.Invoke("Panel/Tray/Die_月"));
            _button9 = FindChildComponent<Button>("Panel/Tray/Die_花_0");
            _button9.onClick.AddListener(() => Clicked?.Invoke("Panel/Tray/Die_花_0"));
            _button10 = FindChildComponent<Button>("Panel/Tray/Die_圆_0");
            _button10.onClick.AddListener(() => Clicked?.Invoke("Panel/Tray/Die_圆_0"));
            _button11 = FindChildComponent<Button>("Panel/Tray/Die_花_1");
            _button11.onClick.AddListener(() => Clicked?.Invoke("Panel/Tray/Die_花_1"));
            _button12 = FindChildComponent<Button>("Panel/Tray/Die_圆_1");
            _button12.onClick.AddListener(() => Clicked?.Invoke("Panel/Tray/Die_圆_1"));
            _button13 = FindChildComponent<Button>("Panel/Tray/Die_雪");
            _button13.onClick.AddListener(() => Clicked?.Invoke("Panel/Tray/Die_雪"));
            _button14 = FindChildComponent<Button>("Panel/Tray/Die_方_1");
            _button14.onClick.AddListener(() => Clicked?.Invoke("Panel/Tray/Die_方_1"));
            _button15 = FindChildComponent<Button>("Panel/Reroll");
            _button15.onClick.AddListener(() => Clicked?.Invoke("Panel/Reroll"));
            _button16 = FindChildComponent<Button>("Panel/Skip");
            _button16.onClick.AddListener(() => Clicked?.Invoke("Panel/Skip"));
            _button17 = FindChildComponent<Button>("Panel/Confirm");
            _button17.onClick.AddListener(() => Clicked?.Invoke("Panel/Confirm"));
            _button18 = FindChildComponent<Button>("Panel/GetCoin");
            _button18.onClick.AddListener(() => Clicked?.Invoke("Panel/GetCoin"));
            _button19 = FindChildComponent<Button>("Panel/Back");
            _button19.onClick.AddListener(() => Clicked?.Invoke("Panel/Back"));
            _text0 = FindChildComponent<TextMeshProUGUI>("Panel/Score_0/Name");
            _text1 = FindChildComponent<TextMeshProUGUI>("Panel/Score_0/Score");
            _text2 = FindChildComponent<TextMeshProUGUI>("Panel/Score_1/Name");
            _text3 = FindChildComponent<TextMeshProUGUI>("Panel/Score_1/Score");
            _text4 = FindChildComponent<TextMeshProUGUI>("Panel/Score_2/Name");
            _text5 = FindChildComponent<TextMeshProUGUI>("Panel/Score_2/Score");
            _text6 = FindChildComponent<TextMeshProUGUI>("Panel/Score_3/Name");
            _text7 = FindChildComponent<TextMeshProUGUI>("Panel/Score_3/Score");
            _text8 = FindChildComponent<TextMeshProUGUI>("Panel/Region_0/Face");
            _text9 = FindChildComponent<TextMeshProUGUI>("Panel/Region_0/Awards");
            _text10 = FindChildComponent<TextMeshProUGUI>("Panel/Region_0/Power");
            _text11 = FindChildComponent<TextMeshProUGUI>("Panel/Region_0/PowerLabel");
            _text12 = FindChildComponent<TextMeshProUGUI>("Panel/Region_0/Opponents");
            _text13 = FindChildComponent<TextMeshProUGUI>("Panel/Region_1/Face");
            _text14 = FindChildComponent<TextMeshProUGUI>("Panel/Region_1/Awards");
            _text15 = FindChildComponent<TextMeshProUGUI>("Panel/Region_1/Power");
            _text16 = FindChildComponent<TextMeshProUGUI>("Panel/Region_1/PowerLabel");
            _text17 = FindChildComponent<TextMeshProUGUI>("Panel/Region_1/Opponents");
            _text18 = FindChildComponent<TextMeshProUGUI>("Panel/Region_2/Face");
            _text19 = FindChildComponent<TextMeshProUGUI>("Panel/Region_2/Awards");
            _text20 = FindChildComponent<TextMeshProUGUI>("Panel/Region_2/Power");
            _text21 = FindChildComponent<TextMeshProUGUI>("Panel/Region_2/PowerLabel");
            _text22 = FindChildComponent<TextMeshProUGUI>("Panel/Region_2/Opponents");
            _text23 = FindChildComponent<TextMeshProUGUI>("Panel/Region_3/Face");
            _text24 = FindChildComponent<TextMeshProUGUI>("Panel/Region_3/Awards");
            _text25 = FindChildComponent<TextMeshProUGUI>("Panel/Region_3/Power");
            _text26 = FindChildComponent<TextMeshProUGUI>("Panel/Region_3/PowerLabel");
            _text27 = FindChildComponent<TextMeshProUGUI>("Panel/Region_3/Opponents");
            _text28 = FindChildComponent<TextMeshProUGUI>("Panel/Region_4/Face");
            _text29 = FindChildComponent<TextMeshProUGUI>("Panel/Region_4/Awards");
            _text30 = FindChildComponent<TextMeshProUGUI>("Panel/Region_4/Power");
            _text31 = FindChildComponent<TextMeshProUGUI>("Panel/Region_4/PowerLabel");
            _text32 = FindChildComponent<TextMeshProUGUI>("Panel/Region_4/Opponents");
            _text33 = FindChildComponent<TextMeshProUGUI>("Panel/Region_5/Face");
            _text34 = FindChildComponent<TextMeshProUGUI>("Panel/Region_5/Awards");
            _text35 = FindChildComponent<TextMeshProUGUI>("Panel/Region_5/Power");
            _text36 = FindChildComponent<TextMeshProUGUI>("Panel/Region_5/PowerLabel");
            _text37 = FindChildComponent<TextMeshProUGUI>("Panel/Region_5/Opponents");
            _text38 = FindChildComponent<TextMeshProUGUI>("Panel/Tray/Die_方_0/Points");
            _text39 = FindChildComponent<TextMeshProUGUI>("Panel/Tray/Die_风/Points");
            _text40 = FindChildComponent<TextMeshProUGUI>("Panel/Tray/Die_月/Points");
            _text41 = FindChildComponent<TextMeshProUGUI>("Panel/Tray/Die_花_0/Points");
            _text42 = FindChildComponent<TextMeshProUGUI>("Panel/Tray/Die_圆_0/Points");
            _text43 = FindChildComponent<TextMeshProUGUI>("Panel/Tray/Die_花_1/Points");
            _text44 = FindChildComponent<TextMeshProUGUI>("Panel/Tray/Die_圆_1/Points");
            _text45 = FindChildComponent<TextMeshProUGUI>("Panel/Tray/Die_雪/Points");
            _text46 = FindChildComponent<TextMeshProUGUI>("Panel/Tray/Die_方_1/Points");
            _text47 = FindChildComponent<TextMeshProUGUI>("Panel/SelectionSummary/Selection");
            _text48 = FindChildComponent<TextMeshProUGUI>("Panel/Reroll/Label");
            _text49 = FindChildComponent<TextMeshProUGUI>("Panel/Reroll/CostPlate/Cost");
            _text50 = FindChildComponent<TextMeshProUGUI>("Panel/Skip/Label");
            _text51 = FindChildComponent<TextMeshProUGUI>("Panel/Skip/CostPlate/Cost");
            _text52 = FindChildComponent<TextMeshProUGUI>("Panel/Confirm/Label");
            _text53 = FindChildComponent<TextMeshProUGUI>("Panel/GetCoin/Count");
            _text54 = FindChildComponent<TextMeshProUGUI>("Panel/Level");
        }

        /// <summary>按节点路径更新文案，使用已缓存组件，不重建层级。</summary>
        public bool SetText(string path, string value)
        {
            switch (path)
            {
                case "Panel/Score_0/Name": _text0.text = value; return true;
                case "Panel/Score_0/Score": _text1.text = value; return true;
                case "Panel/Score_1/Name": _text2.text = value; return true;
                case "Panel/Score_1/Score": _text3.text = value; return true;
                case "Panel/Score_2/Name": _text4.text = value; return true;
                case "Panel/Score_2/Score": _text5.text = value; return true;
                case "Panel/Score_3/Name": _text6.text = value; return true;
                case "Panel/Score_3/Score": _text7.text = value; return true;
                case "Panel/Region_0/Face": _text8.text = value; return true;
                case "Panel/Region_0/Awards": _text9.text = value; return true;
                case "Panel/Region_0/Power": _text10.text = value; return true;
                case "Panel/Region_0/PowerLabel": _text11.text = value; return true;
                case "Panel/Region_0/Opponents": _text12.text = value; return true;
                case "Panel/Region_1/Face": _text13.text = value; return true;
                case "Panel/Region_1/Awards": _text14.text = value; return true;
                case "Panel/Region_1/Power": _text15.text = value; return true;
                case "Panel/Region_1/PowerLabel": _text16.text = value; return true;
                case "Panel/Region_1/Opponents": _text17.text = value; return true;
                case "Panel/Region_2/Face": _text18.text = value; return true;
                case "Panel/Region_2/Awards": _text19.text = value; return true;
                case "Panel/Region_2/Power": _text20.text = value; return true;
                case "Panel/Region_2/PowerLabel": _text21.text = value; return true;
                case "Panel/Region_2/Opponents": _text22.text = value; return true;
                case "Panel/Region_3/Face": _text23.text = value; return true;
                case "Panel/Region_3/Awards": _text24.text = value; return true;
                case "Panel/Region_3/Power": _text25.text = value; return true;
                case "Panel/Region_3/PowerLabel": _text26.text = value; return true;
                case "Panel/Region_3/Opponents": _text27.text = value; return true;
                case "Panel/Region_4/Face": _text28.text = value; return true;
                case "Panel/Region_4/Awards": _text29.text = value; return true;
                case "Panel/Region_4/Power": _text30.text = value; return true;
                case "Panel/Region_4/PowerLabel": _text31.text = value; return true;
                case "Panel/Region_4/Opponents": _text32.text = value; return true;
                case "Panel/Region_5/Face": _text33.text = value; return true;
                case "Panel/Region_5/Awards": _text34.text = value; return true;
                case "Panel/Region_5/Power": _text35.text = value; return true;
                case "Panel/Region_5/PowerLabel": _text36.text = value; return true;
                case "Panel/Region_5/Opponents": _text37.text = value; return true;
                case "Panel/Tray/Die_方_0/Points": _text38.text = value; return true;
                case "Panel/Tray/Die_风/Points": _text39.text = value; return true;
                case "Panel/Tray/Die_月/Points": _text40.text = value; return true;
                case "Panel/Tray/Die_花_0/Points": _text41.text = value; return true;
                case "Panel/Tray/Die_圆_0/Points": _text42.text = value; return true;
                case "Panel/Tray/Die_花_1/Points": _text43.text = value; return true;
                case "Panel/Tray/Die_圆_1/Points": _text44.text = value; return true;
                case "Panel/Tray/Die_雪/Points": _text45.text = value; return true;
                case "Panel/Tray/Die_方_1/Points": _text46.text = value; return true;
                case "Panel/SelectionSummary/Selection": _text47.text = value; return true;
                case "Panel/Reroll/Label": _text48.text = value; return true;
                case "Panel/Reroll/CostPlate/Cost": _text49.text = value; return true;
                case "Panel/Skip/Label": _text50.text = value; return true;
                case "Panel/Skip/CostPlate/Cost": _text51.text = value; return true;
                case "Panel/Confirm/Label": _text52.text = value; return true;
                case "Panel/GetCoin/Count": _text53.text = value; return true;
                case "Panel/Level": _text54.text = value; return true;
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
