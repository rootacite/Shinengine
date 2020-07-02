using SharpDX;
using Shinengine.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Shinengine.Surface.Extra
{
    /// <summary>
    /// ExtraHome.xaml 的交互逻辑
    /// </summary>
    public partial class ExtraHome : Page
    {
        Page ref_mode = null;
        public ExtraHome()
        {
            InitializeComponent();

             ref_mode = new ExtraMemory(); 
            if (SharedSetting.FullS)
            {
                MainWindow.ResizeEvt((ref_mode as ExtraMemory).root_memory, new Size2(1180, 530), new Size2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));
            }
            cont_os.Content = ref_mode.Content;
        }

       
    }
}
