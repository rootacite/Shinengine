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
        private static StaticCharacter.ChangeableAreaInfo[] m_Infos = new StaticCharacter.ChangeableAreaInfo[]
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

        static public GamingTheatre.ScriptHandle Chapter1Script = new GamingTheatre.ScriptHandle((theatre)=> 
        {
            StaticCharacter character_1 = null;
            try
             {
                theatre.setBackground(Colors.White);
                theatre.stage.setAsImage("assets\\CG\\10.png", 0, false);
                theatre.stage.Show(null, true);
                theatre.usage.Show();

                theatre.m_player = new AudioPlayer("assets\\BGM\\pcpc006_bgm_02.wma", true);

                character_1 = new StaticCharacter("日向", "assets\\Characters\\thi\\level_2\\thi_z2a0200.png", theatre.CharacterLayer, false, m_Infos, null, false, 400);
                character_1.SwitchTo(0, 1, 0, false);
                character_1.Show();

                theatre.waitForClick(theatre.bkSre);
                character_1.SwitchTo(0, 5, null, false);
                character_1.Say(theatre.airplant,"你好呀！");
                theatre.waitForClick(theatre.bkSre);
                character_1.Say(theatre.airplant, "你好呀2！");
                theatre.waitForClick(theatre.bkSre);
                character_1.Say(theatre.airplant, "你好呀！3");
                theatre.waitForClick(theatre.bkSre);
                character_1.Say(theatre.airplant, "你好呀！4");
                theatre.waitForClick(theatre.bkSre);
                character_1.Say(theatre.airplant, "你好呀！5");
                theatre.waitForClick(theatre.bkSre);
                character_1.Say(theatre.airplant, "你好呀！6");
                theatre.waitForClick(theatre.bkSre);
                character_1.Say(theatre.airplant, "你好呀！7");
                theatre.waitForClick(theatre.bkSre);
                character_1.Say(theatre.airplant, "你好呀！8");
                theatre.waitForClick(theatre.bkSre);
                character_1.Say(theatre.airplant, "你好呀！9");
                theatre.waitForClick(theatre.bkSre);
                character_1.Say(theatre.airplant, "你好呀！0");
                theatre.waitForClick(theatre.bkSre);
                character_1.Say(theatre.airplant, "你-好呀！");
                theatre.waitForClick(theatre.bkSre);
            }
             catch
             {
                 character_1.Dispose();
                 return 0;
             }
            return 0;
        });
    }
}
