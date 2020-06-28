using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Shinengine.Media;
using Shinengine.Surface;

namespace Shinengine.Scripts
{
    public static class Chapter1
    {
        private static StaticCharacter.ChangeableAreaInfo[] thi_02 = new StaticCharacter.ChangeableAreaInfo[]
        {
            new StaticCharacter.ChangeableAreaInfo()
            {
                 area = new Rect(469, 174, 275, 273),
                 pics = new string[]
                 {
                      "assets\\Characters\\thi\\level_2\\a000.png",
                      "assets\\Characters\\thi\\level_2\\a000h.png",

                      "assets\\Characters\\thi\\level_2\\a001.png",
                      "assets\\Characters\\thi\\level_2\\a001h.png",

                      "assets\\Characters\\thi\\level_2\\a002.png",
                      "assets\\Characters\\thi\\level_2\\a002h.png",

                      "assets\\Characters\\thi\\level_2\\a004.png",
                      "assets\\Characters\\thi\\level_2\\a004h.png",

                      "assets\\Characters\\thi\\level_2\\a005h.png",

                      "assets\\Characters\\thi\\level_2\\a006.png",
                      "assets\\Characters\\thi\\level_2\\a006h.png",

                      "assets\\Characters\\thi\\level_2\\a007.png",
                      "assets\\Characters\\thi\\level_2\\a007h.png",

                      "assets\\Characters\\thi\\level_2\\a008.png",
                      "assets\\Characters\\thi\\level_2\\a008h.png",

                      "assets\\Characters\\thi\\level_2\\a009.png",
                      "assets\\Characters\\thi\\level_2\\a009h.png",

                      "assets\\Characters\\thi\\level_2\\a010.png",
                      "assets\\Characters\\thi\\level_2\\a010h.png",

                      "assets\\Characters\\thi\\level_2\\a011h.png",
                      "assets\\Characters\\thi\\level_2\\a012h.png"
                 }
            }
        };

        private static StaticCharacter.ChangeableAreaInfo[] tmi_02 = new StaticCharacter.ChangeableAreaInfo[]
        {
            new StaticCharacter.ChangeableAreaInfo()
            {
                 area = new Rect(307, 194, 290, 288),
                 pics = new string[]
                 {
                      "assets\\Characters\\tmi\\level_2\\a000h.png",

                      "assets\\Characters\\tmi\\level_2\\a001h.png",

                      "assets\\Characters\\tmi\\level_2\\a002h.png",

                      "assets\\Characters\\tmi\\level_2\\a004.png",

                      "assets\\Characters\\tmi\\level_2\\a005.png",

                      "assets\\Characters\\tmi\\level_2\\a006.png",
                      "assets\\Characters\\tmi\\level_2\\a006h.png",

                      "assets\\Characters\\tmi\\level_2\\a007h.png",

                      "assets\\Characters\\tmi\\level_2\\a009.png",

                      "assets\\Characters\\tmi\\level_2\\a011.png",
                      "assets\\Characters\\tmi\\level_2\\a011h.png",
                      "assets\\Characters\\tmi\\level_2\\a012h.png"
                 }
            }
        };

        private static string mvp(int i)
        {
            string result = "assets\\Voice\\miori_";
            string value = i.ToString();

            for (int p = 0; p < 4 - value.Length; p++)
            {
                result += "0";
            }

            result += value  + ".wma";
            return result;
        }

        static public GamingTheatre.ScriptHandle Chapter1Script = new GamingTheatre.ScriptHandle((theatre)=> 
        {
            try
             {
                theatre.setBackground(Colors.White);
                theatre.stage.setAsImage("assets\\Background\\bg02a.png", 0, false);
                theatre.stage.Show(null, true);
                theatre.usage.Show();

                theatre.SetBackgroundMusic("assets\\BGM\\pcpc006_bgm_06.wma");
                theatre.cts.Add(new StaticCharacter("美织", "assets\\Characters\\tmi\\level_2\\tmi_z2a0000.png", theatre.CharacterLayer, false, tmi_02, null, false, 400));
                theatre.cts[0].SwitchTo(0, 6, 0, false);

                
                do
                {
                    switch (theatre.saved_frame) 
                    {
                        case 0:
                            theatre.cts[0].Show();
                            theatre.cts[0].Say(theatre.airplant, "那个...我有点话要跟你说。", mvp(1));
                            break;
                        case 1:
                            theatre.airplant.Say("什么？", "悠辅");
                            break;
                        case 2:
                            theatre.airplant.Say("晚饭后，我的妻子美织打扫完毕后坐在了我的面前。");
                            break;
                        case 3:
                            theatre.airplant.Say("我放下了嘴边的咖啡杯，听道。");
                            break;
                        case 4:
                            theatre.cts[0].SwitchTo(0,4);
                            theatre.cts[0].Say(theatre.airplant, "那个...那个....", mvp(2));
                            break;
                        case 5:
                            theatre.airplant.Say("我和美织结婚，才刚刚一年");
                            break;
                        case 6:
                            theatre.airplant.Say("从开始交往的时间有两年");
                            break;
                        case 7:
                            theatre.airplant.Say("而我们已经快要相识九年了。");
                            break;
                        case 8:
                            theatre.airplant.Say("虽然我们在学生时代就互相认识，到现在已经很久了");
                            break;
                        case 9:
                            theatre.airplant.Say("但是她用这样的表情说话还是第一次。");
                            break;
                        case 10:
                            theatre.airplant.Say("这么认真...是什么事呢？", "悠辅");
                            break;
                        case 11:
                            theatre.cts[0].SwitchTo(0, 5);
                            theatre.cts[0].Say(theatre.airplant, "嘛啊...该怎么说呢...", mvp(3));
                            break;
                        case 12:
                            theatre.stage.SuperimposeWithImage("assets\\Characters\\tmi\\level_2\\tmi_z2a0000.png");
                            break;
                        default:
                            throw new Exception("NormalOver");
                    }
                    theatre.waitForClick();
                    theatre.saved_frame++;
                }
                while (true);
            }
             catch(Exception e)
             {
                theatre.cts[0].Hide();
                theatre.cts[0].Dispose();
                if (e.Message == "NormalOver")
                    return 0;
                else
                    return -1;
             }
        });
    }
}
