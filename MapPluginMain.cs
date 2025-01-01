using BveEx.Extensions.Native;
using BveEx.Extensions.SoundFactory;
using BveEx.PluginHost;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using FastMember;
using HarmonyLib;
using ObjectiveHarmonyPatch;
using System;
using System.Collections;
using TypeWrapping;

namespace MetroDriveEX.MapPlugin
{
    [Plugin(PluginType.MapPlugin)]
    internal class MapPluginMain : AssemblyPluginBase
    {
        INative Native;
        IBveHacker Hacker;//他スクリプトでBveHacker使えるように
        
        Functions Function;
        Life Life;//減点内容etc
        Drawer Draw;
        public MapPluginMain(PluginBuilder builder) : base(builder)
        {
            Function.SoundFactory = Extensions.GetExtension<ISoundFactory>();
            Native = Extensions.GetExtension<INative>();
            Native.Opened += NativeOpened;
            BveHacker.MainFormSource.KeyDown += Function.keyDown;
            Function.MoveHandle(-2, 15, Hacker);
            Life = ReadFile.ReadSettings(Location);
            //DirectX系処理
            ClassMemberSet set = BveHacker.BveTypes.GetClassInfoOf<AssistantSet>();
            FastMethod drawMethod = set.GetSourceMethodOf(nameof(AssistantSet.Draw));
            Draw.DrawPatch = ObjectiveHarmonyPatch.HarmonyPatch.Patch(Name, drawMethod.Source, PatchType.Prefix);
            Draw.DrawPatch.Invoked += Draw.DrawPatch_Invoked;
        }
        void NativeOpened(object sender,EventArgs e)
        {
            Native.BeaconPassed += Function.BeaconPassed;
            Native.HornBlown += Function.HornBlown;
        }
        public override void Dispose()
        {
            
        }
        public override void Tick(TimeSpan elapsed)
        {
            
        }
    }
}
