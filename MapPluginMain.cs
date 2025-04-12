using BveEx.Extensions.Native;
using BveEx.Extensions.SoundFactory;
using BveEx.PluginHost;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using FastMember;
using NAudio.Wave;
using ObjectiveHarmonyPatch;
using System;
using System.Windows.Forms;
using TypeWrapping;

namespace MetroDriveEX.MapPlugin
{
    [Plugin(PluginType.MapPlugin)]
    public class MapPluginMain : AssemblyPluginBase
    {
        INative Native;
        //他スクリプト系
        Functions Function = new Functions();
        Drawer Draw = new Drawer();
        TrainController TController = new TrainController();
        ExchangeTest ex = new ExchangeTest();
        LifeInfo Life;//減点内容etcが入ったやつ
        public string ShareMes = "none";
        public MapPluginMain(PluginBuilder builder) : base(builder)
        {
            /*if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }*/
            ShareMes = "ready";
            Functions.SoundFactory = Extensions.GetExtension<ISoundFactory>();
            Native = Drawer.Native = Functions.Native = Extensions.GetExtension<INative>();
            Native.Opened += NativeOpened;
            BveHacker.ScenarioCreated += ScenarioCreated;
            //BveHacker.MainFormSource.KeyDown += Function.keyDown;
            Functions.Hacker = Drawer.Hacker = CameraManager.Hacker = TrainController.Hacker = BveHacker;
            Functions.Location = Drawer.Location = ReadFile.Location = Location;
            Life = ReadFile.ReadLifeSettings();//設定ファイルからLife情報を読取る
            Draw.initialize(Life); TController.Initialize();
            //DirectX系処理
            ClassMemberSet set = BveHacker.BveTypes.GetClassInfoOf<AssistantSet>();
            FastMethod drawMethod = set.GetSourceMethodOf(nameof(AssistantSet.Draw));
            Draw.DrawPatch = HarmonyPatch.Patch(Name, drawMethod.Source, PatchType.Prefix);
            Draw.DrawPatch.Invoked += Draw.DrawPatch_Invoked;
            //TrainController => Drawerへのイベント渡し
            TController.AssistInfo.OnGood += Draw.OnGood;
            TController.AssistInfo.OnGreat += Draw.OnGreat;
            TController.AssistInfo.OnTeitu += Draw.OnTeitu;
            TController.AssistInfo.FadeInUI += Draw.FadeIn;
            TController.AssistInfo.FadeOutUI += Draw.FadeOut;
            TController.AssistInfo.OnEBUsed += Draw.OnEBUsed;
            TController.AssistInfo.AlphaIn += Draw.AlphaInAnimation;
            TController.AssistInfo.AlphaOut += Draw.AlphaOutAnimation;
            //test
            ex.Initialize();
            BveHacker.MainFormSource.KeyDown += ex.KeyDowned;
            ex.hacker = BveHacker;
        }
        void NativeOpened(object sender,EventArgs e)
        {
            Native.BeaconPassed += Function.BeaconPassed;
            Native.HornBlown += Function.HornBlown;
            //書き直せよ以下の命令
            /*BveHacker.MainFormSource.KeyDown += Function.keyDown;
            Function.MoveHandle(-2, 15);//計器(QWERTYの0)
            Function.MoveHandle(-3, 8); //時刻表off*/
        }
        void ScenarioCreated(ScenarioCreatedEventArgs e)
        {
            BveHacker.MainFormSource.KeyDown += Function.keyDown;
            BveHacker.MainFormSource.KeyDown += TController.OnkeyDown;
            Function.MoveHandle(-2, 15);//計器(QWERTYの0)
            Function.MoveHandle(-3, 8); //時刻表off*/
            Draw.OnScenarioCreated(e);
        }
        public override void Dispose()
        {
            Draw.Dispose();

            BveHacker.MainFormSource.KeyDown -= Function.keyDown;

            //debug
            BveHacker.MainFormSource.KeyDown -= ex.KeyDowned;
        }
        public override void Tick(TimeSpan elapsed)
        {
            int brake = BveHacker.Scenario.Vehicle.Instruments.Cab.Handles.BrakeNotch;
            int atcBrake = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.AtsHandles.PowerNotch;
            int index = BveHacker.Scenario.Map.Stations.CurrentIndex + 2;
            Station station = (Station)BveHacker.Scenario.Map.Stations[index];
            Life = TController.Tick(station, Life, elapsed);
            if (!(brake == atcBrake)) { Draw.Tick(Life,false,station);}
            else { Draw.Tick(Life,true,station); }
        }
    }
}
