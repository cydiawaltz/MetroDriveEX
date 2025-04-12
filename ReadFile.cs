using SlimDX.Direct3D9;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MetroDriveEX.MapPlugin
{
    internal class ReadFile
    {
        public static string Location;
        public static LifeInfo ReadLifeSettings()
        {
            string path = Path.Combine(Path.GetDirectoryName(Location), @"setting\setting.txt");
            LifeInfo life = new LifeInfo();
            //try
            //{
                using(StreamReader sr = new StreamReader(path))
                {
                    string contents = sr.ReadLine();
                    string[] lines = contents.Split(',');
                    List<int> temp = new List<int>();
                    foreach(string line in lines)
                    {
                        temp.Add(int.Parse(line));
                    }
                    life.Life = temp[0];
                    life.DelayTime = temp[1];
                    life.ReStart = temp[2];
                    life.EBBrake = temp[3];
                    life.EBStop = temp[4];
                    life.Teitsu = temp[5];
                    life.Good = temp[6];
                    life.Great = temp[7];
                    life.Bonus = temp[8];
                    life.Level = temp[9];
                    life.Margin = temp[10];
                }
            /*}
            catch
            {
                MessageBox.Show("なんかどっかしらでエラーon ReadLifeSettings","debug",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/
            return life;
        }
        public static List<UIInfo> ReadUIElementFile(string fileName)
        {
            string path = Path.Combine(Path.GetDirectoryName(Location), @"setting\"+fileName+".txt");
            List<UIInfo> infos = new List<UIInfo>();
            int debug = 0;
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    while(true)
                    {
                        string contents = sr.ReadLine();
                        if(contents.Contains("end"))
                        {
                            break;
                        }
                        string[] lines = contents.Split(',');
                        UIInfo info = new UIInfo();
                        info.x = float.Parse(lines[0]);
                        info.y = float.Parse(lines[1]);
                        info.sizex = float.Parse(lines[2]);
                        info.sizey = float.Parse(lines[3]);
                        infos.Add(info);
                        debug++;
                        continue;
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("なんかどっかでエラー on ReadUIElementFile"+debug+fileName+e.Message);
            }
            return infos;
        }
    }
}
