using Shinengine.Data;
using Shinengine.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Shinengine.Surface
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Page
    { 
        public Setting(ImageSource bkGround)//,AudioPlayer ioPt,AudioPlayer ioPo)
        { 
            InitializeComponent();
            Bkgnd.Source = bkGround;

            SwitchSpeed.Value = (int)(SharedSetting.SwitchSpeed * 10.0);
            TextSpeed.Value = (int)(SharedSetting.TextSpeed * 10.0);
            BGMVm.Value = (int)(SharedSetting.BGMVolum * 100.0);
            VoiceVm.Value = (int)(SharedSetting.VoiceVolum * 100.0);
            AutoSpeed.Value = SharedSetting.AutoTime;
            EmVm.Value = (int)(SharedSetting.EmVolum * 100.0);

            this.Bkgnd.Loaded += (E, V) => 
            {
                if (MainWindow.Title==null)
                MainWindow.m_window.ResizeMode = ResizeMode.NoResize;
            };
            this.Bkgnd.Unloaded += (E, V) =>
            {
                if (MainWindow.Title == null)
                    MainWindow.m_window.ResizeMode = ResizeMode.CanResize;
            };
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SharedSetting.SwitchSpeed = e.NewValue / 10.0;
        }

        private void TextSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SharedSetting.TextSpeed = e.NewValue / 10.0;
        }

        private void BGMVm_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SharedSetting.BGMVolum = (float)(e.NewValue / 100.0);
            if (MainWindow.TheatreMode.m_theatre.m_player != null) MainWindow.TheatreMode.m_theatre.m_player.outputDevice.Volume = SharedSetting.BGMVolum;
        }

        private void VoiceVm_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SharedSetting.VoiceVolum = (float)(e.NewValue / 100.0);
        }

        private void AutoSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SharedSetting.AutoTime = e.NewValue;
        }

        private void EmVm_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SharedSetting.EmVolum= (float)(e.NewValue / 100.0);
            if (MainWindow.TheatreMode.m_theatre.m_em_player != null) MainWindow.TheatreMode.m_theatre.m_em_player.outputDevice.Volume = SharedSetting.EmVolum;
        }

        private void Exitlpg_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
