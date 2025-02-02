using BveEx.Extensions.Native;
using BveEx.Extensions.SoundFactory;
using BveEx.PluginHost;
using BveTypes.ClassWrappers;
using Mackoy.Bvets;
using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;

namespace MetroDriveEX.MapPlugin
{
    internal class Functions//Mainの拡張と便利関数系
    {
        public static ISoundFactory SoundFactory;
        public static IBveHacker Hacker;
        public static INative Native;
        public bool IsUIOff = false;//UIをオフる
        public bool IsPosed = false;//ポーズ状態
        public bool IsVoice = true;//指差喚呼音声、UI音声等
        public int PointerIndex = 0;//ポーズ時のインデックス
        public static string Location;
        //native系
        public void BeaconPassed(object sender,BeaconPassedEventArgs e)
        {
            //Hacker.Scenario.TimeManager.State = BveTypes.ClassWrappers.TimeManager.GameState.Paused;
        }
        public void HornBlown(object sender, HornBlownEventArgs e)
        {

        }
        //BveHacker系
        public void keyDown(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.R:
                    IsUIOff = !IsUIOff;
                    break;
                case Keys.P:
                    IsPosed = !IsPosed;
                    if (IsUIOff) IsUIOff = false;
                    else IsUIOff = true;
                    break;
            }
            if(IsPosed)
            {
                switch(e.KeyCode)
                {
                    case Keys.Up:
                        PointerIndex--;
                        break;
                    case Keys.Down:
                        PointerIndex++;
                        break;
                    case Keys.Enter:

                        break;
                }
                if (PointerIndex == 4) PointerIndex = 0;
                if(PointerIndex == -1) PointerIndex = 3;
            }
        }

        public void Tick()
        {

        }
        public void EnbaleSetting()
        {
            switch(PointerIndex)
            {
                case 0://UI切り替え
                    IsUIOff = !IsUIOff; break;
                case 1://運転終了
                    EndDrive(); break;
                case 2://シナリオやり直し
                    Hacker.MainForm.OpenScenario(Path.Combine(Path.GetDirectoryName(Location), Hacker.ScenarioInfo.Path));            
                    break;
                case 3://指差喚呼/UI音声切り替え
                    IsVoice = !IsVoice; break;
                default:
                    MessageBox.Show("PointerIndexの値が不正です"); break;
            }
        }
        public void EndDrive()
        {
            //シナリオを終了させ、ウインドウを透明化
            
        }
        static string OnApplicationExited(string sharedMes)
        {
            switch(sharedMes)
            {
                case "Exit":
                    Application.Exit();
                    break;
                case "End"://シナリオ終了
                    Hacker.MainForm.UnloadScenario();
                    break;
            }
            sharedMes = "none";
            return sharedMes;
        }
        //便利関数系
        public IEnumerator MoveHandle(int axis,int value)//Atsに介入することでハンドルを動かす
        {
            InputEventArgs inputEventArgs = new InputEventArgs(axis, value);
            Hacker.InputManager.KeyDown_Invoke(inputEventArgs);
            yield return null;
            Hacker.InputManager.KeyUp_Invoke(inputEventArgs);
        }
        public static int Digit(int num)//桁数をカウント
        {
            num = Math.Abs(num);
            // Mathf.Log10(0)はNegativeInfinityを返すため、別途処理する?
            return (num == 0) ? 1 : ((int)Math.Log10(num) + 1);
        }
        //↓廃止？
        public static void PlaySound(string path,bool isCab)
        {
            Sound.SoundPosition sp;
            if (isCab) sp = Sound.SoundPosition.Cab;
            else sp = Sound.SoundPosition.Ground;
            Sound sound = SoundFactory.LoadFrom(path, 0, sp);
            //sound.Play();
        }
        public static TimeSpan PlayDepartureMelody(string path)//発車メロディ
        {
            TimeSpan duration;
            using (var af = new AudioFileReader(path))
            {
                duration = af.TotalTime;
            }
            using(Sound sound = SoundFactory.LoadFrom(path, 0, Sound.SoundPosition.Ground, 1))
            {
                sound.Play(1, 1, 0);
            }
            return duration;
        }
    }
}
