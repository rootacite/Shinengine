using Shinengine.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Shinengine.Theatre
{
    public class Usage
    {
        public static bool locked = false;
        public Grid UsageArea { get; private set; } = null;
        public Usage(Grid ua)
        {
            UsageArea = ua;
        }
        public void Show(double? time = null, bool isAsyn = false)
        {
            if (locked) return;
            if (time == null) time = SharedSetting.SwitchSpeed / 2.0;
            EasyAmal amsc = new EasyAmal(UsageArea, "(Opacity)", 0.0, 1.0, (double)time);
            amsc.Start(isAsyn);
        }
        public void Hide(double? time = null, bool isAsyn = false)
        {
            if (locked) return;
            if (time == null) time = SharedSetting.SwitchSpeed / 2.0;
            EasyAmal amsc = new EasyAmal(UsageArea, "(Opacity)", 1.0, 0.0, (double)time);
            amsc.Start(isAsyn);
        }
    }//这个类没有使用过非托管资源
}
