using Shinengine.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;

namespace Shinengine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ~App()
        {
            Directory.Delete(PackStream.TempPath[0..^1], true);
        }
        private App()
        {
            if (!Directory.Exists("data"))
                Directory.CreateDirectory( @"data");

            if(!File.Exists("sysdata.xml"))
            {
                MessageBox.Show("Error : File: sysdata.xml is missing","error", MessageBoxButton.OK, MessageBoxImage.Error);
                PInvoke.Kernel32.ExitProcess(-1);
            }
            if (!File.Exists("saves.xml"))
            {
                MessageBox.Show("Error : File: saves.xml is missing", "error",MessageBoxButton.OK,MessageBoxImage.Error);
                PInvoke.Kernel32.ExitProcess(-1);
            }
        }
    }
}
