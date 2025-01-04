using BveEx.Extensions.Native;
using BveEx.Extensions.SoundFactory;
using BveEx.PluginHost;
using Mackoy.Bvets;
using System;
using System.Collections;
using System.Windows.Forms;


namespace MetroDriveEX.MapPlugin
{
    internal class Functions//Mainの拡張と便利関数系
    {
        public static ISoundFactory SoundFactory;
        public static IBveHacker Hacker;
        public static INative Native;
        //native系
        public void BeaconPassed(object sender,BeaconPassedEventArgs e)
        {
            
        }
        public void HornBlown(object sender, HornBlownEventArgs e)
        {

        }
        //BveHacker系
        public void keyDown(object sender, KeyEventArgs e)
        {

        }

        public void Tick()
        {

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
            // Mathf.Log10(0)はNegativeInfinityを返すため、別途処理する。
            return (num == 0) ? 1 : ((int)Math.Log10(num) + 1);
        }
    }
}
