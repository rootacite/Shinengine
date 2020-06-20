using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
    /// GamingTheatre.xaml 的交互逻辑
    /// </summary>
    public partial class GamingTheatre : Window
    {
        public Theatre m_theatre = null;
        public GamingTheatre()
        {
            InitializeComponent();

            m_theatre = new Theatre(BG, Usage, SBK);
        }
        public delegate int ScriptHandle(Theatre theatre);
        Task scriptTask = null;
        public void Start(ScriptHandle scriptHandle)
        {
            scriptTask = new Task(()=> { scriptHandle(m_theatre); });
            scriptTask.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }
    }
}
