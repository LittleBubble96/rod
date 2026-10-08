using TMPro;
using UnityEngine;

namespace GameLogic.Rod
{
    public sealed class RodUISkin : ScriptableObject
    {
        public TMP_FontAsset font;
        public Sprite panel,button,slot;
        public Color ink=new Color32(30,76,65,255),cream=new Color32(255,247,225,255),mint=new Color32(207,230,208,255),gold=new Color32(246,190,70,255);
    }
}
