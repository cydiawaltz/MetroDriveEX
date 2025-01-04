using BveEx.Extensions.Native;
using BveEx.Extensions.SoundFactory;
using BveEx.PluginHost;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using FastMember;
using ObjectiveHarmonyPatch;
using System;
using TypeWrapping;

namespace MetroDriveEX.MapPlugin
{
    [Plugin(PluginType.MapPlugin)]
    internal class MapPluginMain : AssemblyPluginBase
    {
        INative Native;
        //他スクリプト系
        Functions Function;
        Drawer Draw;
        LifeInfo Life;//減点内容etcが入ったやつ
        public MapPluginMain(PluginBuilder builder) : base(builder)
        {
            Functions.SoundFactory = Extensions.GetExtension<ISoundFactory>();
            Native = Drawer.Native = Functions.Native = Extensions.GetExtension<INative>();
            Native.Opened += NativeOpened;
            BveHacker.MainFormSource.KeyDown += Function.keyDown;
            Functions.Hacker = Drawer.Hacker = BveHacker;
            Function.MoveHandle(-2, 15);//計器(QWERTYの0)
            Function.MoveHandle(-3, 8); //時刻表off
            Life = ReadFile.ReadLifeSettings(Location);//設定ファイルからLife情報を読取る
            Draw.initialize(Location);
            //DirectX系処理
            ClassMemberSet set = BveHacker.BveTypes.GetClassInfoOf<AssistantSet>();
            FastMethod drawMethod = set.GetSourceMethodOf(nameof(AssistantSet.Draw));
            Draw.DrawPatch = HarmonyPatch.Patch(Name, drawMethod.Source, PatchType.Prefix);
            Draw.DrawPatch.Invoked += Draw.DrawPatch_Invoked;
        }
        void NativeOpened(object sender,EventArgs e)
        {
            Native.BeaconPassed += Function.BeaconPassed;
            Native.HornBlown += Function.HornBlown;
        }
        public override void Dispose()
        {
            Draw.Dispose();
        }
        public override void Tick(TimeSpan elapsed)
        {
            int brake = BveHacker.Scenario.Vehicle.Instruments.Cab.Handles.BrakeNotch;
            int atcBrake = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.AtsHandles.PowerNotch;
            if(!(brake == atcBrake)) { Draw.Tick(Life,false);}
            else { Draw.Tick(Life,true); }
        }
    }
}
