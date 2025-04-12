using BveEx.PluginHost;
using Mackoy.Bvets;
using SlimDX.Direct3D9;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetroDriveEX
{
    internal class ExchangeTest
    {
        public IBveHacker hacker;
        NamedPipeServerStream server;
        StreamWriter writer; StreamReader reader;
        public string SharedMes = "none";

        bool isrunning = false;
        public void Initialize()
        {
            try
            {
                server = new NamedPipeServerStream("metrodrive", PipeDirection.InOut);
                writer = new StreamWriter(server); reader = new StreamReader(server);
                ExchangeMessage();
                isrunning = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        async void ExchangeMessage()
        {
            while(isrunning)
            {
                SharedMes = await reader.ReadLineAsync();
                await Task.Yield();
                await Task.Delay(1000);
            }
        }
        void SendMesToUnity()
        {
            if(isrunning)
            {
                try
                {
                    writer.Write(SharedMes);
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else MessageBox.Show("パイプが動作中ではない");
        }
        public void Tick()
        {
            if(writer != null)
            {
                if(SharedMes.Contains("Scenario:"))
                {
                    try
                    {
                        int tmp_shMes = int.Parse(SharedMes.Remove(0, 8));
                        MessageBox.Show(tmp_shMes.ToString());
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                    SharedMes = "none";
                }
                switch (SharedMes)
                {
                    case "stay":
                        SharedMes = "ready"; break;
                    //default:
                      //  SharedMes = "none"; break;
                }
            }
        }
        public void KeyDowned(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.E:
                    SharedMes = "End"; SendMesToUnity(); break;
                case Keys.O:
                    SharedMes = "Over"; SendMesToUnity(); break;
                case Keys.C:
                    SharedMes = "Clear"; SendMesToUnity(); break;
                //キー操作簡略化のテスト
                case Keys.Down:
                    hacker.Scenario.Vehicle.Instruments.Cab.Handles.ReverserPosition = BveTypes.ClassWrappers.ReverserPosition.F;
                    if (hacker.Scenario.Vehicle.Instruments.Cab.Handles.BrakeNotch == 0)
                    {
                        SendKeys.Send("Z");
                    }
                    else
                    {
                        SendKeys.Send("<");
                    }
                    //InputEventArgs inputEventArgs = new InputEventArgs(3, 1);
                    //hacker.InputManager.KeyDown_Invoke(inputEventArgs);
                    break;
                case Keys.Up:
                    hacker.Scenario.Vehicle.Instruments.Cab.Handles.ReverserPosition = BveTypes.ClassWrappers.ReverserPosition.F;
                    if (hacker.Scenario.Vehicle.Instruments.Cab.Handles.PowerNotch == 0)
                    {
                        SendKeys.Send(">");
                    }
                    else
                    {
                        SendKeys.Send("A");
                    }
                    break;

            }
            /*
             * メッセージタイプ
             * stay => unity to bve. bve側では認識したら"none"に書き換え(以下同様)　最初に送る
             * ready => bve to unity. 準備中画面を終了し、起動画面にする
             * Scenario:(Num) => unity to bve. Num部分に応じて起動するシナリオを変更
             * End => bve to unity. 途中離脱を示す。
             * Over => bve to uniy. GameOver.
             * Clear => bve to unity GameClear.(流すmovieはUnity側で設定)
             */
        }
        public void Dispose()
        {
            try
            {
                server.Dispose(); writer.Dispose(); reader.Dispose();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
