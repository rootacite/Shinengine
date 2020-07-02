using Shinengine.Surface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using static Shinengine.Data.SaveData;

namespace Shinengine.Data
{
    static public class SharedSetting
    {
        static readonly XDocument sysData = XDocument.Load("sysdata.xml");
        static readonly IEnumerable<XNode> atrs = sysData.Root.Nodes();
        public static double TextSpeed 
        {

            get 
            {
                if (GamingTheatre.isSkiping)
                    return 0;
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
        public static double SwitchSpeed
        {
            get
            {

                if (GamingTheatre.isSkiping)
                    return 0;
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

        public static float EmVolum
        {
            get
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "EmVm")
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
                    if (i.Name.ToString() == "EmVm")
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

        public static bool FullS
        {
            get
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "full")
                    {
                        if (i.Value.ToString() == "true")
                            return true;
                        else
                            return false;
                    }
                }
                return false;
            }
            set
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "full")
                    {
                        i.Value = value ? "true" : "false";
                        sysData.Save("sysdata.xml");
                    }
                }
            }
        }

        public static SaveInfo? Last
        {
            get
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "last")
                    {
                        if (i.Value.ToString() == "null")
                        {
                            return null;
                        }
                        var mLp = i.Value.ToString().Split(':').ToList();

                        return new SaveInfo() { chapter = Convert.ToInt32(mLp[0]), frames = Convert.ToInt32(mLp[1]) };
                    }
                }
                throw new Exception();
            }
            set
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "last")
                    {
                        if (value == null)
                        {
                            i.Value = "null";
                            sysData.Save("sysdata.xml");
                            return;
                        }
                        string data_save = value.Value.chapter.ToString() + ":" + value.Value.frames.ToString();
                        i.Value = data_save;
                       
                        sysData.Save("sysdata.xml");
                    }
                }
            }
        }
    }

}
