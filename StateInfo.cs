using System;
using System.Threading.Tasks;

namespace MetroDriveEX.MapPlugin
{
    public class LifeInfo //Life関連(加点減点)に必要な変数
    {
        public int Life;
        //減点系
        public int DelayTime;//遅延
        public bool IsDelayTime;
        public int ReStart;//再加速
        public bool IsRestart;
        public int EBBrake;//EB使用時
        public bool IsEBBrake;
        public int EBStop;//EB停止
        public bool IsEBStop;
        //加点系
        public int Teitsu;//定通
        public bool IsTeitsu;
        public int Good;//Good停車
        public bool IsGood;
        public int Great;//Great停車
        public bool IsGreat;
        public int Bonus;//ボーナス点(警笛とか)
        public bool IsBonus;
        //難易度設計
        public int Level;//難易度(1~5)
        //その他
        public float Margin;//合格範囲
    }
    public class UIInfo //UI要素の解像度、ピポットの設定
    {
        public float x;
        public float y;
        public float sizex;
        public float sizey;
    }
    public class AssistantUIInfo//Drawer.csとTrainController.csで情報を授受するやつ
    {
        public bool IsRestarted = false;
        public double over;
        public event EventHandler OnEBUsed;
        public event EventHandler OnGood;
        public event EventHandler OnGreat;
        public event EventHandler OnOver;
        public event EventHandler OnTeitu;
        public event EventHandler FadeInUI;//外にシュッと動かす
        public event EventHandler FadeOutUI;
        public event EventHandler AlphaIn;
        public event EventHandler AlphaOut;
        public void EBusedInvoke() => OnEBUsed.Invoke(this, EventArgs.Empty);
        public void GoodInvoke() => OnGood.Invoke(this, EventArgs.Empty);
        public void GreatInvoke() => OnGreat.Invoke(this, EventArgs.Empty);
        public void OverInvoke() => OnOver.Invoke(this, EventArgs.Empty);
        public void TeituInvoke() => OnTeitu.Invoke(this, EventArgs.Empty);
        public void FadeInUIInvoke() => FadeInUI.Invoke(this, EventArgs.Empty);
        public void FadeOutUIInvoke() => FadeOutUI.Invoke(this, EventArgs.Empty);
        public void AlphaInInvoke() => AlphaIn.Invoke(this, EventArgs.Empty);
        public void AlphaOutInvoke() => AlphaOut.Invoke(this, EventArgs.Empty);
    }

}
