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
    public partial class Setting : Window
    {
        Window _mainwindow = null;
        AudioPlayer _ioPt = null; 
        public Setting(ImageSource bkGround, Window mainwindow, object cont, AudioPlayer ioPt)
        {
            _ioPt = ioPt;
            InitializeComponent();
            _mainwindow = mainwindow;
            Bkgnd.Source = bkGround;

          //  Loaded += (e, v) => {  };
             Closed += (e, v) => { _mainwindow.Content = cont; };

            SwitchSpeed.Value = SharedSetting.switchSpeed * 10.0;
            TextSpeed.Value = SharedSetting.textSpeed * 10.0;
            BGMVm.Value = SharedSetting.BGMVolum * 100.0;
            VoiceVm.Value = SharedSetting.VoiceVolum * 100.0;
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SharedSetting.switchSpeed = e.NewValue / 10.0;
        }

        private void TextSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SharedSetting.textSpeed = e.NewValue / 10.0;
        }

        private void BGMVm_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SharedSetting.BGMVolum = (float)(e.NewValue / 100.0);
         if(_ioPt!=null)   _ioPt.outputDevice.Volume = SharedSetting.BGMVolum;
        }

        private void VoiceVm_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SharedSetting.VoiceVolum = (float)(e.NewValue / 100.0);
        }
    }
}
