using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Drawing;
using Shinengine.Data;
using Shinengine.Surface;
namespace Shinengine.Theatre
{

    public class AirPlant
    {
        public Theatre m_father = null;
        private readonly Grid Vist = null;

        private readonly Grid chat_usage = null;

        readonly private TextBlock Lines_Usage = null;
        readonly private TextBlock Character_Usage = null;
        readonly private TextBlock _Contents = null;

        readonly private List<TextBlock> freedomLines = new List<TextBlock>();

        public AirPlant(Grid air, Grid names, TextBlock _Lines, TextBlock _Charecter, Grid _Vist, TextBlock Content)
        {
            chat_usage = names;
            Lines_Usage = _Lines;
            Character_Usage = _Charecter;
            Vist = _Vist;
            _Contents = Content;
            chat_usage.Opacity = 0;
        }

        public void Say(string line, string character = "", double? time = null)
        {
            if (time == null) time = SharedSetting.TextSpeed;

            #region log call
            string load_printed = "";

            string ral_printf_str = "";
            if (character != "")
            {
                ral_printf_str += "[" + character + "] ";
            }
            ral_printf_str += line;

            foreach (var c in ral_printf_str)
            {

                Vist.Dispatcher.Invoke(new Action(() =>
                {
                    load_printed += c;
                    var ap_l = GamingBook.MeasureTextWidth(_Contents, _Contents.FontSize, load_printed);
                    if (ap_l.Width > _Contents.Width)
                    {
                        _Contents.Dispatcher.Invoke(new Action(() => { GamingTheatre.Preparation += '\n'; }));
                        load_printed = "";
                    }
                    GamingTheatre.Preparation += c;
                }));
            }
            GamingTheatre.Preparation += "\n";
            #endregion


            EasyAmal esyn = new EasyAmal(Lines_Usage, "(Opacity)", 1.0, 0.0, (double)time);
            esyn.Start(false);
            Lines_Usage.Dispatcher.Invoke(new Action(() =>
            {
                Lines_Usage.Text = line;

                if (character == "" && chat_usage.Opacity == 1)
                {
                    EasyAmal _st = new EasyAmal(chat_usage, "(Opacity)", 1.0, 0.0, (double)time);
                    _st.Start(true);
                    Character_Usage.Text = character;
                }
                else
                if (character != "" && chat_usage.Opacity == 0)
                {
                    Character_Usage.Text = character;
                    EasyAmal _st = new EasyAmal(chat_usage, "(Opacity)", 0.0, 1.0, (double)time);
                    _st.Start(true);
                }
                else
                {
                    Character_Usage.Text = character;
                }
            }));
            esyn = new EasyAmal(Lines_Usage, "(Opacity)", 0.0, 1.0, (double)time);
            esyn.Start(false);


        }
        public void SayAt(string line, RectangleF location, double? time = null, bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.TextSpeed;
            EasyAmal m_txt = null;
            Vist.Dispatcher.Invoke(new Action(() =>
            {
                TextBlock n_mfLine = new TextBlock
                {
                    Text = line,
                    FontSize = Lines_Usage.FontSize,
                    FontFamily = Lines_Usage.FontFamily,
                    FontStyle = Lines_Usage.FontStyle,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray),
                    Width = location.Width,
                    Height = location.Height,
                    Margin = new Thickness(location.Left, location.Top, 0, 0)
                };

                freedomLines.Add(n_mfLine);
                if (time == 0)
                {
                    Vist.Children.Add(n_mfLine);
                    return;
                }
                n_mfLine.Opacity = 0;
                Vist.Children.Add(n_mfLine);
                m_txt = new EasyAmal(n_mfLine, "(Opacity)", 0.0, 1.0, (double)time);
            }));


            m_txt.Start(isAsyn);
        }

        public void CleanAllFreedom(double? time = null)
        {
            if (time == null) time = SharedSetting.TextSpeed;
            for (int i = 0; i < freedomLines.Count; i++)
            {
                EasyAmal mns = new EasyAmal(freedomLines[i], "(Opacity)", 1.0, 0.0, (double)time);
                if (i != freedomLines.Count - 1) mns.Start(true); else mns.Start(false);

            }
            for (int i = 0; i < freedomLines.Count; i++)
            {
                Vist.Children.Remove(freedomLines[i]);
            }
            freedomLines.Clear();
        }
    }//这个类的常用方法中没有使用过非托管资源,暂时搁置
}
