using Shinengine.Surface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace Shinengine.Data
{
    static public class SharedSetting
    {
        static XDocument sysData = XDocument.Load("sysdata.xml");
        static IEnumerable<XNode> atrs = sysData.Root.Nodes();
        public static double textSpeed 
        {

            get 
            {
                if (GamingTheatre.isSkiping)
                    return 0.0;
                foreach (XElement i in atrs)
                {
                    if(i.Name.ToString()== "TextSpeed")
                    {
                        return Convert.ToDouble(i.Value.ToString());
                    }
                }
                return 0.3;
            }
            set
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "TextSpeed")
                    {
                        i.Value = value.ToString();
                        sysData.Save("sysdata.xml");
                    }
                }
            }

        }
        public static double switchSpeed
        {
            get
            {

                if (GamingTheatre.isSkiping)
                    return 0.0;
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "SwitchSpeed")
                    {
                        return Convert.ToDouble(i.Value.ToString());
                    }
                }
                return 0.3;
            }
            set
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "SwitchSpeed")
                    {
                        i.Value = value.ToString();
                        sysData.Save("sysdata.xml");
                    }
                }
            }
        }

        public static float BGMVolum 
        {
            get
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "BGMVm")
                    {
                        return (float)Convert.ToDouble(i.Value.ToString());
                    }
                }
                return 0.3f;
            }
            set
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "BGMVm")
                    {
                        i.Value = value.ToString();
                        sysData.Save("sysdata.xml");
                    }
                }
            }
        }
        public static float VoiceVolum 
        {
            get
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "VoiceVM")
                    {
                        return (float)Convert.ToDouble(i.Value.ToString());
                    }
                }
                return 0.3f;
            }
            set
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "VoiceVM")
                    {
                        i.Value = value.ToString();
                        sysData.Save("sysdata.xml");
                    }
                }
            }
        }

        public static double AutoTime
        {
            get
            {
                if (GamingTheatre.isSkiping)
                    return 0.0;
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "Auto")
                    {
                        return Convert.ToDouble(i.Value.ToString());
                    }
                }
                return 0.3;
            }
            set
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "Auto")
                    {
                        i.Value = value.ToString();
                        sysData.Save("sysdata.xml");
                    }
                }
            }
        }
    }

}
