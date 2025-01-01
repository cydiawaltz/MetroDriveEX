
namespace MetroDriveEX.MapPlugin
{
    internal class Life
    {
        public int life;
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
    }
}
