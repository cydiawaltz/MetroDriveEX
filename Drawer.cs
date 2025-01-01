using BveTypes.ClassWrappers;
using ObjectiveHarmonyPatch;

namespace MetroDriveEX.MapPlugin
{
    internal class Drawer
    {
        public HarmonyPatch DrawPatch;
        float Width; float Height;//縦横の解像度
        int?[] Arrive = new int?[9]; int?[] Now = new int?[9]; int?[] Life = new int?[3];
        //数字部分のみ入れ、他はnull Arv. 12:34:56 => Arv.が[0],:が[3][6]とか
        Model[] yellow = new Model[12]; Model[] green = new Model[12]; Model[] sky = new Model[12];
        //[0]~[9] には 0~9 ,[10]はarv. now.とか [11]は:(コロン)
        public PatchInvokationResult DrawPatch_Invoked(object sender, PatchInvokedEventArgs e)
        {
            
            return PatchInvokationResult.DoNothing(e);
        }

        public void Tick(string arr,string now)//Main側で呼び出す
        {

        }
    }
}
