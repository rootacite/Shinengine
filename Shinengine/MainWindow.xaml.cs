using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace Shinengine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Direct2DImage DxBkGround = null;
        bool enClose = false;
        public MainWindow()
        {
            InitializeComponent();

            DxBkGround = new Direct2DImage(BackGround, 60);

            new Thread(() => {
                while (!enClose) {

                    Dispatcher.Invoke(new Action(() => { this.Title = DxBkGround.Dpis.ToString(); }));
                    Thread.Sleep(10);
                }
            }).Start();
        }

        private void BackGround_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            DxBkGround.SetRenderTask(()=> {
                DxBkGround.View.BeginDraw();
                DxBkGround.View.Clear(new RawColor4(1, 0, 0, 1));
                DxBkGround.View.EndDraw();
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            enClose = true;
            DxBkGround.Dispose();
        }
    }
}
