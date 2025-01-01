using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MetroDriveEX.MapPlugin
{
    internal class ReadFile
    {
        public static Life ReadSettings(string Location)
        {
            string path = Path.Combine(Path.GetDirectoryName(Location), @"setting\setting.txt");
            Life life = new Life();
            try
            {
                using(StreamReader sr = new StreamReader(path))
                {
                    string contents = sr.ReadLine();
                    string[] lines = contents.Split(',');
                    List<int> temp = new List<int>();
                    foreach(string line in lines)
                    {
                        temp.Add(int.Parse(line));
                    }
                    life.life = temp[0];
                    life.DelayTime = temp[1];
                    life.ReStart = temp[2];
                    life.EBBrake = temp[3];
                    life.EBStop = temp[4];
                    life.Teitsu = temp[5];
                    life.Good = temp[6];
                    life.Great = temp[7];
                    life.Bonus = temp[8];
                }
            }
            catch
            {
                MessageBox.Show("なんかどっかしらでエラー","debug",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return life;
        }
    }
}
