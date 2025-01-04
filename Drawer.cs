using BveTypes.ClassWrappers;
using ObjectiveHarmonyPatch;
using System.IO;
using System.Drawing;
using SlimDX.Direct3D9;
using SlimDX;
using BveEx.PluginHost;
using BveEx.Extensions.Native;
using System;
using System.Collections.Generic;

namespace MetroDriveEX.MapPlugin
{
    internal class Drawer
    {
        public HarmonyPatch DrawPatch;
        public static IBveHacker Hacker;
        public static INative Native;
        float Width; float Height;//縦横の解像度
        int PowerNotch; int BrakeNotch;//ノッチ段数
        Model[] Power = new Model[6]; Model[] Brake = new Model[11];//ノッチハンドル+ATC作動中
        int[] Arrive = new int[6]; int[] Now = new int[6]; int[] Life = new int[3];//数字部分のみ
        Model[] Yellow = new Model[12]; Model[] Green = new Model[12]; Model[] Sky = new Model[12]; Model[] Black = new Model[12];
        //[0]~[9] には 0~9 ,[10]はarv. now.とか [11]は:(コロン)
        int[] Next = new int[4]; double NextLoc; double NowLoc; bool Pass; LifeInfo LifeInfo; //次駅関係
        Model[] Texts = new Model[4];//0:[あと] 1:[合格範囲or停止位置] 2:[過走]　3:[m]
        bool IsAtcMoved = false;
        Model[] Tutorials;
        Model[] AssistantUIs;//「定通」「Good」「Great」
        public void initialize(string Location)
        {
            for (int i = 0; i < 12; i++)
            {
                Yellow[i] = CreateModel(@"picture\yellow\" + i + ".png", 0, 0, 40, -60);
                Green[i] = CreateModel(@"picture\green\" + i + ".png", 0, 0, 40, -60);
                Sky[i] = CreateModel(@"picture\sky\" + i + ".png", 0, 0, 40, -60);
                Black[i] = CreateModel(@"picture\black\" + i + ".png", 0, 0, 40, -60);
            }
            for(int i = 0; i < 6; i++) Power[i] = CreateModel(@"picture\P" + i + ".png", 0, 0, 150, -225); 
            for(int i = 0; i < 11; i++) Brake[i] = CreateModel(@"picture\B" + i + ".png", 0, 0, -150, 225);
            Texts[0] = CreateModel(@"picture\texts\0.png", 0, 0, 150, 80);
            Texts[1] = CreateModel(@"picture\texts\1.png", 0, 0, 250, 80);
            Texts[2] = CreateModel(@"picture\texts\2.png", 0, 0, 200, 80);
            Texts[3] = CreateModel(@"picture\texts\3.png", 0, 0, 50, 60);
            List<UIInfo> tutorialInfo = ReadFile.ReadUIElementFile(Location,"tutorialUIList");
            Tutorials = new Model[tutorialInfo.Count];
            for(int i = 0; i < tutorialInfo.Count; i++)
            {
                UIInfo info = tutorialInfo[i];
                Tutorials[i] = CreateModel(@"picture\tutorial\"+i+".png",info.x,info.y,info.sizex,info.sizey);
            }
            List<UIInfo> assistantInfo = ReadFile.ReadUIElementFile(Location, "AssistantUIList");
            AssistantUIs = new Model[assistantInfo.Count];
            for (int i = 0; i < assistantInfo.Count; i++)
            {
                UIInfo info = assistantInfo[i];
                AssistantUIs[i] = CreateModel(@"picture\assistant\" + i + ".png", info.x, info.y, info.sizex, info.sizey);
            }
            Model CreateModel(string path, float x, float y, float sizex, float sizey)
            {
                string texFilePath = Path.Combine(Path.GetDirectoryName(Location), path);
                RectangleF rectangleF = new RectangleF(x, y, sizex, -sizey);
                return Model.CreateRectangleWithTexture(rectangleF, 0, 0, texFilePath);//四角形の3Dモデル
            }
        }
        public PatchInvokationResult DrawPatch_Invoked(object sender, PatchInvokedEventArgs e)
        {
            Width = Direct3DProvider.Instance.PresentParameters.BackBufferWidth;
            Height = Direct3DProvider.Instance.PresentParameters.BackBufferHeight;
            Device device = Direct3DProvider.Instance.Device;
            device.SetTransform(TransformState.View, Matrix.Identity);
            device.SetTransform(TransformState.Projection, Matrix.OrthoOffCenterLH(-Width / 2, Width / 2, -Height / 2, Height / 2, 0, 1));
            HardCodingZone();//後ろにまとめる
            return PatchInvokationResult.DoNothing(e);
        }
        public void Tick(LifeInfo info,bool isAtcMoved)//Main側のtickで呼び出す
        {
            PowerNotch = Hacker.Scenario.Vehicle.Instruments.Cab.Handles.PowerNotch; 
            BrakeNotch = Hacker.Scenario.Vehicle.Instruments.Cab.Handles.BrakeNotch;
            int index = Hacker.Scenario.Map.Stations.CurrentIndex + 1;
            Station station = (Station)Hacker.Scenario.Map.Stations[index];
            string now = Hacker.Scenario.TimeManager.Time.ToString("hhmmss");
            string arr = station.DepartureTime.ToString("hhmmss");
            LifeInfo = info; IsAtcMoved = isAtcMoved;
            for (int i = 0; i < 6; i++)
            {
                Arrive[i] = int.Parse(arr.Substring(i, 1)); Now[i] = int.Parse(now.Substring(i, 1));
            }
            if(LifeInfo.Life >= 100)
            {
                Life[0] = int.Parse(LifeInfo.Life.ToString().Substring(0, 1));
                Life[1] = int.Parse(LifeInfo.Life.ToString().Substring(1, 1));
                Life[2] = int.Parse(LifeInfo.Life.ToString().Substring(2, 1));
            }
            else if(LifeInfo.Life >= 10)
            {
                Life[0] = int.Parse(LifeInfo.Life.ToString().Substring(0, 1));
                Life[1] = int.Parse(LifeInfo.Life.ToString().Substring(1, 1));
            }
            else
            {
                Life[0] = int.Parse(LifeInfo.Life.ToString().Substring(0, 1));
            }
            NextLoc = Hacker.Scenario.Map.Stations[index].Location; NowLoc = Hacker.Scenario.VehicleLocation.Location;
            int next = (int)Math.Abs(NextLoc - NowLoc);
            Next[0] = int.Parse(next.ToString().Substring(0, 1));
            if(NextLoc - NowLoc >= 10) { Next[1] = int.Parse(next.ToString().Substring(1, 1)); }
            if(NextLoc - NowLoc >= 100) { Next[2] = int.Parse(next.ToString().Substring(2, 1)); }
            if (NextLoc - NowLoc >= 1000) { Next[3] = int.Parse(next.ToString().Substring(3, 1)); }
        }
        public void Dispose()
        { 
            foreach(Model power in Power) { power.Dispose(); }
            foreach(Model brake in Brake) { brake.Dispose(); }
            foreach(Model yellow in Yellow) { yellow.Dispose(); }
            foreach(Model greeeen in Green) {  greeeen.Dispose(); }
            foreach(Model sky in Sky) { sky.Dispose(); }
            foreach(Model black in Black) {  black.Dispose(); }
            foreach(Model text in Texts) {  text.Dispose(); }
        }
        void HardCodingZone()//3K!!
        {
            Device device = Direct3DProvider.Instance.Device;
            //力行(power)
            device.SetTransform(TransformState.World, Matrix.Translation(-Width / 2, Height / 2, 0));
            if (IsAtcMoved) { Power[PowerNotch].Draw(Direct3DProvider.Instance, false); }
            else { Power[5].Draw(Direct3DProvider.Instance, false); }//ATC作動
            //制動(Brake)
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2, Height / 2, 0));//(float)Math.Sin(Native.VehicleState.Time.TotalSeconds)*100
            if (IsAtcMoved) { Brake[BrakeNotch].Draw(Direct3DProvider.Instance, false); }
            else { Brake[10].Draw(Direct3DProvider.Instance, false); }
            //Life
            if (LifeInfo.Life >= 100)
            {
                for (int i = 0; i < 3; i++)
                {
                    device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - (150 - i * 40), -Height / 2 + 250, 0));
                    Sky[Life[i]].Draw(Direct3DProvider.Instance, false);
                }
            }
            else if(LifeInfo.Life >= 10)
            {
                for (int i = 0; i < 2; i++)
                {
                    device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - (110 - i * 40), -Height / 2 + 250, 0));
                    Sky[Life[i]].Draw(Direct3DProvider.Instance, false);
                }
            }
            else
            {
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 70, -Height / 2 + 250, 0));
                Sky[Life[0]].Draw(Direct3DProvider.Instance, false);
            }
            //現在時刻と到着時刻(greenがnow,yellowがarrive)
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 450, -Height / 2 + 90, 0));
            Green[10].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 450, -Height / 2 + 170, 0));
            Yellow[10].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 300, -Height / 2 + 80, 0));//arrive１個目
            Green[Arrive[0]].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 260, -Height / 2 + 80, 0));//arrive２個め
            Green[Arrive[1]].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 230, -Height / 2 + 80, 0));
            Green[11].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 200, -Height / 2 + 80, 0));//arrive３個目
            Green[Arrive[2]].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 160, -Height / 2 + 80, 0));//now
            Green[Arrive[3]].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 130, -Height / 2 + 80, 0));//now
            Green[11].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 100, -Height / 2 + 80, 0));//now
            Green[Arrive[4]].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 60, -Height / 2 + 80, 0));//now
            Green[Arrive[5]].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 300, -Height / 2 + 160, 0));//now
            Yellow[Now[0]].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 260, -Height / 2 + 160, 0));//now
            Yellow[Now[1]].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 230, -Height / 2 + 160, 0));
            Yellow[11].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 200, -Height / 2 + 160, 0));//now
            Yellow[Now[2]].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 160, -Height / 2 + 160, 0));//now
            Yellow[Now[3]].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 130, -Height / 2 + 160, 0));
            Yellow[11].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 100, -Height / 2 + 160, 0));//now
            Yellow[Now[4]].Draw(Direct3DProvider.Instance, false);
            device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 60, -Height / 2 + 160, 0));//now
            Yellow[Now[5]].Draw(Direct3DProvider.Instance, false);
            //距離表示
            device.SetTransform(TransformState.World, Matrix.Translation(-Width / 2 + 10, -Height / 2 + 180, 0));
            if(Math.Abs(NowLoc - NextLoc)<LifeInfo.Margin)
            {
                Texts[1].Draw(Direct3DProvider.Instance, false);//「合格範囲」
            }
            else if(NowLoc < NextLoc)
            {
                Texts[2].Draw(Direct3DProvider.Instance, false);//「過走」
            }
            else
            {
                Texts[0].Draw(Direct3DProvider.Instance, false);//「あと」
            }
            switch(Functions.Digit((int)(NowLoc - NextLoc)))
            {
                case 4:
                    for (int i = 0; i < 4; i++)
                    {
                        device.SetTransform(TransformState.World, Matrix.Translation(-Width / 2 + 50 * i, -Height / 2 + 100, 0));
                        Black[Next[i]].Draw(Direct3DProvider.Instance, false);
                    } break;
                case 3:
                    for (int i = 0; i < 3; i++)
                    {
                        device.SetTransform(TransformState.World, Matrix.Translation(-Width / 2 + 50 * (i + 1), -Height / 2 + 100, 0));
                        Black[Next[i]].Draw(Direct3DProvider.Instance, false);
                    } break;
                case 2:
                    for (int i = 0; i < 2; i++)
                    {
                        device.SetTransform(TransformState.World, Matrix.Translation(-Width / 2 + 50 * (i + 2), -Height / 2 + 100, 0));
                        Black[Next[i]].Draw(Direct3DProvider.Instance, false);
                    } break;
                case 1:
                    device.SetTransform(TransformState.World, Matrix.Translation(-Width / 2 + 150, -Height / 2 + 100, 0));
                    Black[Next[0]].Draw(Direct3DProvider.Instance, false);
                    break;
            }

        }
    }
}
