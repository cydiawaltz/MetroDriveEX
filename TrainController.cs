using BveEx.PluginHost;
using BveTypes.ClassWrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetroDriveEX.MapPlugin
{
    internal class TrainController
    {
        internal IBveHacker Hacker;
        double Speed; double OldSpeed;
        double Now; double Arrive;
        double NextLoc; double CurrentLoc;
        bool IsPass; bool OldPass;
        LifeInfo Life;
        public AssistantUIInfo AssistInfo = new AssistantUIInfo();
        public List<double> Locations = new List<double>();//駅到着時にリプレイするための位置保存
        TimeSpan Timer = TimeSpan.Zero;
        Station station;

        //フラグ
        bool IsResultWindow; bool IsKeyDowned;
        public LifeInfo Tick(Station sta,LifeInfo life,TimeSpan elapsed)
        { 
            Speed = Hacker.Scenario.VehicleLocation.Speed*3.6;//km/hに変更
            station = sta;
            IsPass = station.Pass;
            Life = life;
            if (Speed == 0 && Speed < OldSpeed)
            {
                OnStop();
            }
            if(Speed> 0 && !IsPass == OldPass)
            {
                OnPass();
            }
            Now = Hacker.Scenario.TimeManager.Time.TotalSeconds;
            Arrive = station.DepartureTime.TotalSeconds;
            NextLoc = station.Location;
            CurrentLoc = Hacker.Scenario.VehicleLocation.Location;
            if(NextLoc - CurrentLoc <10)
            {
                Locations.Add(CurrentLoc);
            }
            Timer += elapsed;
            if(Timer > TimeSpan.FromSeconds(1))
            {
                OnSeconds();
                Timer = TimeSpan.Zero;
            }
            if (Life.Life < 0)
            OldSpeed = Hacker.Scenario.VehicleLocation.Speed * 3.6;
            OldPass = station.Pass;
            return Life;
        }
        void OnStop()
        {
            if (Math.Abs(Now - Arrive) < 5 && Math.Abs(NextLoc - CurrentLoc) < 3 && !IsPass)//Good
            {
                AssistInfo.GoodInvoke();
                Life.Life += Life.Good;//Good

            }
            if (Math.Abs(Now - Arrive) < 2 && Math.Abs(NextLoc - CurrentLoc) < 1 && !IsPass)//Great
            {
                Life.Life += Life.Great;
                AssistInfo.GreatInvoke();//Great
            }
            if (CurrentLoc - NextLoc > Life.Margin)
            {
                Life.Life -= (int)(CurrentLoc - NextLoc) - (int)Life.Margin;
                AssistInfo.OverInvoke();//過走
            }
            if(NextLoc - CurrentLoc <120 && NextLoc - CurrentLoc > Life.Margin && !IsPass)
            {
                AssistInfo.IsRestarted = true;
            }
        }
        void OnPass()
        {
            if(Math.Abs(Now - Arrive)<1)//定通
            {
                Life.Life += Life.Teitsu;
                AssistInfo.TeituInvoke();
            }
        }
        void OnSeconds()//１秒毎に呼ばれる
        {
            if(station.ArrivalTime.TotalSeconds < Hacker.Scenario.TimeManager.Time.TotalSeconds)
            {//現在時刻が到着予定時刻を越しているとき
                Life.Life -= Life.DelayTime;
                //Functions.PlaySound()
                //biみたいな音
            }
            if(station.ArrivalTime.TotalSeconds < Hacker.Scenario.TimeManager.Time.TotalSeconds+5)
            {
                //ticktack音
            }
            if (Hacker.Scenario.Vehicle.Instruments.Cab.Handles.BrakeNotch == Hacker.Scenario.Vehicle.Instruments.Cab.Handles.NotchInfo.EmergencyBrakeNotch)
            {//EBブレーキ使用時(EB使用停車の減点はこれで代替)
                Life.Life -= Life.EBBrake;
                AssistInfo.EBusedInvoke();
            }
        }
        async void OnGameOver()
        {
            while(true)
            {
                if(Speed == 0)break;
                Hacker.Scenario.VehicleLocation.SetSpeed(Speed / 3.6 - 2);
            }
            await Task.Delay(5000);
            //ゲーム終了処理
        }
        async void OnGameClear()
        {
            await Task.Yield();
        }

        async void SetGameState(bool isEnter)//リプレイか否かの情報を更新
        {
            if(isEnter)//停止後
            {
                Hacker.Scenario.TimeManager.State = TimeManager.GameState.Paused;
                while (!Hacker.Scenario.Vehicle.Doors.AreAllClosed) await Task.Yield();
                await Task.Delay(1000);
                AssistInfo.FadeOutUIInvoke(); //ここでUIを引っ込める
                AssistInfo.AlphaInInvoke();//暗転
                //ここでカメラを切り替える
                int delayTime = (int)(Now - Arrive);//遅れ
                int over = (int)(CurrentLoc - NextLoc);//過走
                AssistInfo.AlphaOutInvoke();//明転
                //「(駅名),(駅名)です。～」の放送
                //減点
                if(delayTime > 0) Life.Life -= delayTime;
                if(over > 0) Life.Life -= over;
                if (AssistInfo.IsRestarted) Life.Life -= Life.ReStart;
                for (int i = 0;i<Locations.Count;i++)//ここからリプレイ(10m前から記録？)
                {
                    if (IsKeyDowned)
                    {
                        IsKeyDowned = !IsKeyDowned;
                        //ゲームを再開させる処理
                        if (Life.Life == 0) OnGameOver();
                        else SetGameState(false);
                        break;
                    }
                    Hacker.Scenario.VehicleLocation.SetLocation(Locations[i*2],true);
                    //await Task.Yield(); <= 1f飛ばせない説あり?
                    await Task.Delay(1);
                    //↑2倍速で処理
                }
                AssistInfo.IsRestarted = false;
                await Task.Delay(2000);
                if (Life.Life == 0) OnGameOver();
                else SetGameState(false);
            }
            else//ゲーム再開
            {
                AssistInfo.AlphaInInvoke();
                //カメラ設定変更
                AssistInfo.AlphaOutInvoke();
                //次駅案内表示
                while (!IsKeyDowned) await Task.Yield();
                IsKeyDowned = !IsKeyDowned;
                AssistInfo.FadeInUIInvoke(); //UI戻し
                Hacker.Scenario.TimeManager.State = TimeManager.GameState.Forward;
                //発車メロディ鳴らし（終了まで待機）=> メロディと「ドアが締まります」は別
                //(メロディ終了後)「ドアが締まります」
                await Task.Delay(200);//調節しとく
                SideDoorSet sideDoorSet = Hacker.Scenario.Vehicle.Doors.GetSide((DoorSide)station.DoorSide);
                sideDoorSet.CloseDoors(2);//引数は割とテキトー
                while (!Hacker.Scenario.Vehicle.Doors.AreAllClosed) await Task.Yield();
                //await Task.Delay(500);//ドア閉まった後、一瞬待機 => PilotLampの点灯原理不明？

            }
        }
        public void OnkeyDown(object sender, KeyEventArgs e)
        {
            IsKeyDowned = true;
        }
    }
}
