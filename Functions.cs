using BveEx.Extensions.Native;
using BveEx.Extensions.SoundFactory;
using BveEx.PluginHost;
using BveTypes.ClassWrappers;
using Mackoy.Bvets;
using System.Collections;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MetroDriveEX.MapPlugin
{
    internal class Functions
    {
        public ISoundFactory SoundFactory;
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

        //便利関数系
        public static IEnumerator MoveHandle(int axis,int value,IBveHacker b)
        {
            InputEventArgs inputEventArgs = new InputEventArgs(axis, value);
            b.InputManager.KeyDown_Invoke(inputEventArgs);
            yield return null;
            b.InputManager.KeyUp_Invoke(inputEventArgs);
        }

    }
}
