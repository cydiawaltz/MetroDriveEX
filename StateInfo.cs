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
}
