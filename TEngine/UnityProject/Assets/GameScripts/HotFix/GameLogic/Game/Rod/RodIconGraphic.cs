using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Rod
{
    public enum RodIcon { Coin, Reroll, Next, Ad, Loading, Close, Back, Share, Plus, Help }
    /// <summary>小图标用UGUI网格绘制，无额外贴图和图集。可在Inspector换颜色。</summary>
    public sealed class RodIconGraphic : MaskableGraphic
    {
        public RodIcon icon;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float s=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height);
            Vector2 c=rectTransform.rect.center;
            if(icon==RodIcon.Coin){Ring(vh,c,s*.34f,s*.09f,0,360);Line(vh,c+new Vector2(0,-s*.18f),c+new Vector2(0,s*.18f),s*.07f);}
            else if(icon==RodIcon.Loading || icon==RodIcon.Reroll){Ring(vh,c,s*.30f,s*.07f,35,310);if(icon==RodIcon.Reroll)Tri(vh,c+new Vector2(s*.31f,s*.08f),c+new Vector2(s*.12f,s*.14f),c+new Vector2(s*.24f,s*.30f));}
            else if(icon==RodIcon.Next || icon==RodIcon.Ad){Tri(vh,c+new Vector2(-s*.16f,-s*.24f),c+new Vector2(-s*.16f,s*.24f),c+new Vector2(s*.23f,0));if(icon==RodIcon.Next)Line(vh,c+new Vector2(s*.31f,-s*.26f),c+new Vector2(s*.31f,s*.26f),s*.065f);else{Line(vh,c+new Vector2(-s*.39f,-s*.35f),c+new Vector2(s*.39f,-s*.35f),s*.05f);Line(vh,c+new Vector2(-s*.39f,s*.35f),c+new Vector2(s*.39f,s*.35f),s*.05f);Line(vh,c+new Vector2(-s*.39f,-s*.35f),c+new Vector2(-s*.39f,s*.35f),s*.05f);Line(vh,c+new Vector2(s*.39f,-s*.35f),c+new Vector2(s*.39f,s*.35f),s*.05f);}}
            else if(icon==RodIcon.Close){Line(vh,c+new Vector2(-s*.23f,-s*.23f),c+new Vector2(s*.23f,s*.23f),s*.075f);Line(vh,c+new Vector2(-s*.23f,s*.23f),c+new Vector2(s*.23f,-s*.23f),s*.075f);}
            else if(icon==RodIcon.Plus){Line(vh,c+new Vector2(-s*.25f,0),c+new Vector2(s*.25f,0),s*.075f);Line(vh,c+new Vector2(0,-s*.25f),c+new Vector2(0,s*.25f),s*.075f);}
            else if(icon==RodIcon.Help){Ring(vh,c,s*.34f,s*.055f,0,360);Line(vh,c+new Vector2(0,-s*.04f),c+new Vector2(0,s*.20f),s*.065f);Line(vh,c+new Vector2(0,-s*.20f),c+new Vector2(0,-s*.15f),s*.065f);}
            else{float sign=icon==RodIcon.Back?-1:1;Line(vh,c+new Vector2(-s*.3f,0),c+new Vector2(s*.3f,0),s*.07f);Line(vh,c+new Vector2(sign*s*.3f,0),c+new Vector2(0,s*.26f),s*.07f);Line(vh,c+new Vector2(sign*s*.3f,0),c+new Vector2(0,-s*.26f),s*.07f);}
        }
        private void Update(){if(icon==RodIcon.Loading)rectTransform.Rotate(0,0,-180*Time.unscaledDeltaTime);}
        private void Tri(VertexHelper v,Vector2 a,Vector2 b,Vector2 c){int i=v.currentVertCount;v.AddVert(a,color,Vector2.zero);v.AddVert(b,color,Vector2.zero);v.AddVert(c,color,Vector2.zero);v.AddTriangle(i,i+1,i+2);}
        private void Line(VertexHelper v,Vector2 a,Vector2 b,float width){Vector2 d=b-a,n=new Vector2(-d.y,d.x).normalized*width/2;int i=v.currentVertCount;v.AddVert(a+n,color,Vector2.zero);v.AddVert(b+n,color,Vector2.zero);v.AddVert(b-n,color,Vector2.zero);v.AddVert(a-n,color,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
        private void Ring(VertexHelper v,Vector2 c,float r,float w,float from,float to){for(int n=0;n<32;n++){float a=Mathf.Lerp(from,to,n/32f)*Mathf.Deg2Rad,b=Mathf.Lerp(from,to,(n+1)/32f)*Mathf.Deg2Rad;Line(v,c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,c+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r,w);}}
    }
}
