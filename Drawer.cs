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
using Zbx1425.DXDynamicTexture;
using System.Threading.Tasks;

namespace MetroDriveEX.MapPlugin
{
    internal class Drawer
    {
        public HarmonyPatch DrawPatch;
        public static IBveHacker Hacker;
        public static INative Native;
        public static string Location;//AssemblyPluginBase.Location
        float Width; float Height;//縦横の解像度
        int PowerNotch; int BrakeNotch;//ノッチ段数
        Model[] Power = new Model[6]; Model[] Brake = new Model[11];//ノッチハンドル+ATC作動中
        int[] Arrive = new int[6]; int[] Now = new int[6]; int[] Life = new int[3];//数字部分のみ
        Model[,] White = new Model[11,2]; Model[,] Red = new Model[11,2]; Model[,] Blue = new Model[11,2];
        //[0]~[9] には 0~9 ,[10]は:(コロン) [u,0]は大きめ、[u,1]は小さめ(秒+次駅距離)
        int[] Next = new int[4]; double NextLoc = 0; double NowLoc = 0; bool Pass; LifeInfo LifeInfo; //次駅関係
        Model[] Texts = new Model[4];//0:[あと] 1:[合格範囲or停止位置] 2:[過走] 3:ベース
        bool IsAtcMoved = false;  bool isUIOff = false;
        Model[] Tutorials;
        Model[] UIElements;//その他のUI要素(詳細は初期化場所でコメント)
        Model[] SubUIs;//「定通」とかのちょっとしか出さないUI
        //0:EB使用 1:good 2:great 3:over 4:定通
        //処理用
        double RestTime;//次駅までの残り時間(ミリ秒)
        Model[,] TextFont;//描画する文字色(中身は動的に変える)
        Model BackGroundModel;
        int BackGroundAlpha = 0;//背景画像のα値
        float UIPos = -1;//UIの位置の設定　-2(定位置)~0(完全フェードアウト)~1（通常） => -1~0に
        //フラグ
        bool isDelaying = false;
        bool[] SubUIFlag;
        //DXDynamicTexture
        TextureHandle TextureHandle;
        GDIHelper GDIHelper;
        //debug
        Model grid;

        public void initialize(LifeInfo lifeinfo)
        {
            Width = Direct3DProvider.Instance.PresentParameters.BackBufferWidth;
            Height = Direct3DProvider.Instance.PresentParameters.BackBufferHeight;
            LifeInfo = lifeinfo;
            List<UIInfo> fontL = ReadFile.ReadUIElementFile("fontL");
            List<UIInfo> fontS = ReadFile.ReadUIElementFile("fontS");
            for (int i = 0; i < 11; i++)
            {
                fontL[i] = SetAspect(fontL[i]); fontS[i] = SetAspect(fontS[i]);
                White[i,0] = CreateModel(@"picture\white\" + i + ".png", fontL[i].x, fontL[i].y, fontL[i].sizex, fontL[i].sizey);
                Red[i,0] = CreateModel(@"picture\red\" + i + ".png", fontL[i].x, fontL[i].y, fontL[i].sizex, fontL[i].sizey);
                Blue[i, 0] = CreateModel(@"picture\blue\" + i + ".png", fontL[i].x, fontL[i].y, fontL[i].sizex, fontL[i].sizey);
                White[i, 1] = CreateModel(@"picture\white\" + i + ".png", fontS[i].x, fontS[i].y, fontS[i].sizex, fontS[i].sizey);
                Red[i, 1] = CreateModel(@"picture\red\" + i + ".png", fontS[i].x, fontS[i].y, fontS[i].sizex, fontS[i].sizey);
                Blue[i, 1] = CreateModel(@"picture\blue\" + i + ".png", fontS[i].x, fontS[i].y, fontS[i].sizex, fontS[i].sizey);
            }
            List<UIInfo> power = ReadFile.ReadUIElementFile("power");
            List<UIInfo> brake = ReadFile.ReadUIElementFile("brake");
            for (int i = 0; i < 6; i++)
            {
                power[i] = SetAspect(power[i]);
                Power[i] = CreateModel(@"picture\power\" + i + ".png", power[i].x, power[i].y, power[i].sizex, power[i].sizey);
            }
            for (int i = 0; i < 11; i++)
            {
                brake[i] = SetAspect(brake[i]);
                Brake[i] = CreateModel(@"picture\brake\" + i + ".png", brake[i].x, brake[i].y, brake[i].sizex, brake[i].sizey);
            }
            List<UIInfo> texts = ReadFile.ReadUIElementFile("texts");
            for (int i = 0; i < 4; i++) Texts[i] = CreateModel(@"picture\texts\" + i + ".png", texts[i].x, texts[i].y, texts[i].sizex, texts[i].sizey);
            //for (int i = 0; i < 4; i++) Texts[i] = CreateModel(@"picture\texts\" + i + ".png", -100,30,30,-100);
            List<UIInfo> tutorialInfo = ReadFile.ReadUIElementFile("tutorialUIList");
            Tutorials = new Model[tutorialInfo.Count];
            for(int i = 0; i < tutorialInfo.Count; i++)
            {
                UIInfo info = SetAspect(tutorialInfo[i]);
                Tutorials[i] = CreateModel(@"picture\tutorial\"+i+".png",info.x,info.y,info.sizex,info.sizey);
            }
            List<UIInfo> assistantInfo = ReadFile.ReadUIElementFile("UIList");
            UIElements = new Model[assistantInfo.Count];
            for (int i = 0; i < assistantInfo.Count; i++)
            {
                UIInfo info = SetAspect(assistantInfo[i]);
                UIElements[i] = CreateModel(@"picture\ui\" + i + ".png", info.x, info.y, info.sizex, info.sizey);
            }
            List<UIInfo> subUI = ReadFile.ReadUIElementFile("sub");
            SubUIs = new Model[subUI.Count]; SubUIFlag = new bool[subUI.Count];
            for (int i = 0; i< subUI.Count; i++)
            {
                UIInfo info = SetAspect(subUI[i]);
                SubUIFlag[i] = false;
                SubUIs[i] = CreateModel(@"picture\sub\" + i + ".png", info.x, info.y, info.sizex, info.sizey);
            }
            BackGroundModel = CreateModel(@"picture\other\background.png", -Width / 2, Height / 2, Width, Height);
            BackGroundModel.SetAlpha(BackGroundAlpha);
            Model CreateModel(string path, float x, float y, float sizex, float sizey)
            {
                string texFilePath = Path.Combine(Path.GetDirectoryName(Location), path);
                RectangleF rectangleF = new RectangleF(x, y, sizex, sizey);
                return Model.CreateRectangleWithTexture(rectangleF, 0, 0, texFilePath);//四角形の3Dモデル
            }
            //debug
            grid = CreateModel(@"picture\debug\grid.png", 0, 0, Width, -Height);
        }
        public void OnScenarioCreated(ScenarioCreatedEventArgs e)
        {
            //Model target_03_3 = e.Scenario.Map.StructureModels["03_3"];
            //TextureHandle = target_03_3.
            FadeIn(new object(), new EventArgs());
        }
        public async void OnEBUsed(object sender, EventArgs e)
        {
            SubUIFlag[0] = true;
            //音鳴らす
            await Task.Delay(1000);
            SubUIFlag[0] = false;
        }
        public async void FadeOut(object sender, EventArgs e)
        {
            float fadeOutValue = 0.03f;//1fで移動する距離(MAX1に注意)
            for(int i = 0;i > 1/fadeOutValue; i++)
            {
                UIPos -= fadeOutValue;
                await Task.Delay(1);
                if(UIPos < 0)
                {
                    UIPos = 0;
                    break;
                }
            }
        }
        public async void FadeIn(object sender, EventArgs e)
        {
            float fadeOutValue = 0.05f;//1fで移動する距離(MAX1に注意) => Maxは0
            while(true)
            {
                UIPos += fadeOutValue;
                //await Task.Yield();
                await Task.Delay(1);
                if (UIPos > 0)
                {
                    UIPos = 0;
                    break;
                }
            }
        }
        public async void OnGood(object sender, EventArgs e)
        {
            Hacker.Scenario.TimeManager.State = TimeManager.GameState.Paused;
            SubUIFlag[1] = true;
            //音を鳴らす
            await Task.Delay(3000);
            SubUIFlag[1] = false;
            Hacker.Scenario.TimeManager.State = TimeManager.GameState.Forward;
        }
        public async void OnGreat(object sender, EventArgs e)
        {
            Hacker.Scenario.TimeManager.State = TimeManager.GameState.Paused;
            SubUIFlag[2] = true;
            //音を鳴らす
            await Task.Delay(3000);//ここは調整
            SubUIFlag[2] = false;
            Hacker.Scenario.TimeManager.State = TimeManager.GameState.Forward;
        }
        public async void OnTeitu(object sender, EventArgs e)
        {
            SubUIFlag[4] = true;
            await Task.Delay(3000);
            SubUIFlag[4] = false;
        }
        public async void AlphaInAnimation(object sender, EventArgs e)
        {
            for (int i = 0; i < 64; i++)//ここでジョジョに暗転させる
            {
                BackGroundAlpha = BackGroundAlpha += 4;
                if (BackGroundAlpha > 255)
                {
                    BackGroundAlpha = 255;
                }
                BackGroundModel.SetAlpha(BackGroundAlpha);
                await Task.Yield();
            }
        }
        public async void AlphaOutAnimation(object sender,EventArgs e)
        {
            for (int i = 0; i < 64; i++)
            {
                BackGroundAlpha = BackGroundAlpha -= 4;
                if (BackGroundAlpha < 0)
                {
                    BackGroundAlpha = 0;
                }
                BackGroundModel.SetAlpha(BackGroundAlpha);
                await Task.Yield();
            }
        }
        public PatchInvokationResult DrawPatch_Invoked(object sender, PatchInvokedEventArgs e)
        {
            Device device = Direct3DProvider.Instance.Device;
            device.SetTransform(TransformState.View, Matrix.Identity);
            device.SetTransform(TransformState.Projection, Matrix.OrthoOffCenterLH(-Width / 2, Width / 2, -Height / 2, Height / 2, 0, 1));
            DrawingUI();//後ろにまとめる
            return PatchInvokationResult.DoNothing(e);
        }
        public void Tick(LifeInfo info,bool isAtcMoved,Station station)//Main側のtickで呼び出す
        {
            PowerNotch = Hacker.Scenario.Vehicle.Instruments.Cab.Handles.PowerNotch; 
            BrakeNotch = Hacker.Scenario.Vehicle.Instruments.Cab.Handles.BrakeNotch;
            string now = Hacker.Scenario.TimeManager.Time.ToString("hhmmss");
            string arr = station.DepartureTime.ToString("hhmmss");
            RestTime = station.DepartureTime.TotalMilliseconds - Hacker.Scenario.TimeManager.Time.TotalMilliseconds;
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
            NextLoc = Hacker.Scenario.Map.Stations[Hacker.Scenario.Map.Stations.CurrentIndex + 1].Location; 
            NowLoc = Hacker.Scenario.VehicleLocation.Location;
            string next = "0000";//暫定で代入してるだけ
            switch(Functions.Digit((int)(NowLoc - NextLoc)))
            {
                case 4: next = ((int)Math.Abs(NextLoc - NowLoc)).ToString(); break;
                case 3: next = "0" + ((int)Math.Abs(NextLoc - NowLoc)).ToString(); break;
                case 2: next = "00" + ((int)Math.Abs(NextLoc - NowLoc)).ToString(); break;
                case 1: next = "000" + ((int)Math.Abs(NextLoc - NowLoc)).ToString(); break;
            }
            for(int i = 0; i < 4; i++)
            {
                Next[i] = int.Parse(next.Substring(i, 1));
            }
            if (NowLoc - NextLoc > LifeInfo.Margin)//「over」
            {
                SubUIFlag[3] = true;//「over」表示（必要なときのみONにする）
            }
            else SubUIFlag[3] = false;
        }
        UIInfo SetAspect(UIInfo def)//横幅の長さを設定(基準1920px)
        {
            float x = def.x * Width / 1920;
            float y = x * def.sizey / def.sizex;
            float sizex = def.sizex * Width / 1920;
            float sizey = sizex * def.sizey/def.sizex;
            UIInfo info = new UIInfo();
            info.x = x; info.y = y; info.sizex = sizex; info.sizey = sizey;
            return info;
        }

        public void Dispose()
        { 
            foreach(Model power in Power) power.Dispose(); 
            foreach(Model brake in Brake) brake.Dispose(); 
            foreach(Model yellow in White) yellow.Dispose();
            foreach(Model greeeen in Red) greeeen.Dispose();
            foreach(Model black in Blue) black.Dispose();
            foreach(Model text in Texts) text.Dispose();    
            foreach(Model tutorial in Tutorials) tutorial.Dispose();
            foreach(Model uiElement in UIElements) uiElement.Dispose();
            if (!(TextFont == null)) { foreach (Model textfont in TextFont) textfont.Dispose(); }
        }
        void ChangeSigns()//車両の幕をDXDynamicTextureでどうにかする
        {
            if(TextureHandle.HasEnoughTimePassed(10))
            {

            }
        }
        void DrawingUI()//3K!!
        {
            if(!isUIOff)
            {
                Device device = Direct3DProvider.Instance.Device;
                //力行(power)
                device.SetTransform(TransformState.World, Matrix.Translation(-Width / 2, Height / 2, 0));
                /*if (IsAtcMoved) { Power[PowerNotch].Draw(Direct3DProvider.Instance, false); }
                else { Power[5].Draw(Direct3DProvider.Instance, false); }//ATC作動
                                                                         //制動(Brake)*/
                Power[PowerNotch].Draw(Direct3DProvider.Instance, false);
                device.SetTransform(TransformState.World, Matrix.Translation( Width/ 2, Height / 2-225*Width/1920, 0));//(float)Math.Sin(Native.VehicleState.Time.TotalSeconds)*100
                /*if (IsAtcMoved) { Brake[BrakeNotch].Draw(Direct3DProvider.Instance, false); }
                else { Brake[10].Draw(Direct3DProvider.Instance, false); }*/
                Brake[BrakeNotch].Draw(Direct3DProvider.Instance, false);
                //Life => UIデザイン検討
                //到着時刻
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2, -Height / 2 + 20 * UIPos*Height/1080, 0));
                UIElements[3].Draw(Direct3DProvider.Instance, false);//[現在時刻]
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2-300, -Height / 2 + 30 * UIPos, 0));
                White[Arrive[0],0].Draw(Direct3DProvider.Instance, false);
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2-250, -Height / 2 + 30 * UIPos, 0));
                White[Arrive[1], 0].Draw(Direct3DProvider.Instance, false);
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 200, -Height / 2 + 30 * UIPos, 0));
                White[Arrive[2], 0].Draw(Direct3DProvider.Instance, false);
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 220, -Height / 2 + 30 * UIPos, 0));
                White[10, 0].Draw(Direct3DProvider.Instance, false);
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 150, -Height / 2 + 30 * UIPos, 0));
                White[Arrive[3], 0].Draw(Direct3DProvider.Instance, false);
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 100, -Height / 2 + 30 * UIPos, 0));
                White[Arrive[4], 1].Draw(Direct3DProvider.Instance, false);
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 50, -Height / 2 + 30 * UIPos, 0));
                White[Arrive[5], 1].Draw(Direct3DProvider.Instance, false);
                //現在時刻
                if (RestTime <= 5000 && RestTime > 0) TextFont = Blue;
                else if (RestTime <= 0) TextFont = Red;
                else TextFont = White;
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2, -Height / 2 + 90 * UIPos * Height / 1080, 0));
                UIElements[0].Draw(Direct3DProvider.Instance, false);//[現在時刻]
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2-300, -Height / 2 + 90 * UIPos, 0));
                TextFont[Now[0],0].Draw(Direct3DProvider.Instance, false);//1桁目
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 250, -Height / 2 + 90 * UIPos, 0));
                TextFont[Now[1], 0].Draw(Direct3DProvider.Instance, false);//2桁目
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 225, -Height / 2 + 90 * UIPos, 0));
                TextFont[10, 0].Draw(Direct3DProvider.Instance, false);//コロン(:)
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 200, -Height / 2 + 90 * UIPos, 0));
                TextFont[Now[2], 0].Draw(Direct3DProvider.Instance, false);//3桁目
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 150, -Height / 2 + 90 * UIPos, 0));
                TextFont[Now[3], 0].Draw(Direct3DProvider.Instance, false);//4桁目
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 100, -Height / 2 + 90 * UIPos, 0));
                TextFont[Now[4], 1].Draw(Direct3DProvider.Instance, false);//5桁目
                device.SetTransform(TransformState.World, Matrix.Translation(Width / 2 - 50, -Height / 2 + 90 * UIPos, 0));
                TextFont[Now[5], 1].Draw(Direct3DProvider.Instance, false);//6桁目
                //距離表示
                device.SetTransform(TransformState.World, Matrix.Translation(-Width / 2 + 277, -Height / 2 + 90*UIPos, 0));
                Texts[3].Draw(Direct3DProvider.Instance, false);
                device.SetTransform(TransformState.World, Matrix.Translation(-Width / 2+140, -Height / 2 + 150 * UIPos, 0));
                if (Math.Abs(NowLoc - NextLoc) < LifeInfo.Margin) Texts[1].Draw(Direct3DProvider.Instance, false);//合格範囲
                else if (NowLoc < NextLoc) Texts[2].Draw(Direct3DProvider.Instance, false);//「過走」
                else Texts[0].Draw(Direct3DProvider.Instance, false);//「あと」
                for (int i = 0; i < 4; i++)
                {
                    device.SetTransform(TransformState.World, Matrix.Translation(-Width / 2 + 50 * i, -Height / 2 + 100 * UIPos, 0));
                    White[Next[i], 1].Draw(Direct3DProvider.Instance, false);
                }
                //debug
                device.SetTransform(TransformState.World, Matrix.Translation(-Width / 2, Height / 2, 0));
                grid.Draw(Direct3DProvider.Instance, false);
            }

        }
    }
}
