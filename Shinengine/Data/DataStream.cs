using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Shinengine.Data
{
    public class DataStream
    {
        public XDocument doc;
        public IEnumerable<XNode> atr;

        public struct PageInfo
        {
            public string Illustration;
            public string BackgroundMusic;
            public List<string> Contents;
        }

        public DataStream(string path)
        {
            doc = XDocument.Load(path);
            atr = doc.Root.Nodes();
        }

        public PageInfo getSignalPage(int id)
        {
            PageInfo result = new PageInfo();
            foreach (XElement item in atr)  //遍历节点
            {
                if (item.Attribute("id").Value.ToString() == id.ToString())
                {
                    result.BackgroundMusic = item.Element("BackgroundMusic").Value.ToString();
                    result.Illustration = item.Element("Illustration").Value.ToString();
                    result.Contents = item.Element("Contents").Value.ToString().Split('\n').ToList();
         
                    for (var i = 0; i < result.Contents.Count; )
                    {
                        bool succ_flag = false;
                        foreach(var p in result.Contents[i])
                        {
                            if(p!=' ' && p != '\n')
                            {
                                succ_flag = true;
                                break;
                            }
                        }
                        if (!succ_flag)
                        {
                            result.Contents.RemoveAt(i);
                            continue;
                        }
                        i++;
                    }
                    for (var index_i = 0; index_i < result.Contents.Count; index_i++)
                    {
                        while (result.Contents[index_i][0] == ' ')
                        {
                            string newi = result.Contents[index_i].Substring(1, result.Contents[index_i].Length - 1);
                            result.Contents[index_i] = newi;
                        }
                    }
                    break;
                }
                else
                    continue;
            }

            return result;
        }
    }

    public static class SaveData 
    {
        static XDocument sysData = XDocument.Load("saves.xml");
        static IEnumerable<XNode> atrs = sysData.Root.Nodes();
        public struct SaveInfo 
        {
            public int chapter;
            public int frames;
            public string comment;
            public Bitmap imp;
        }

        public static SaveInfo save1
        {
            get
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "data1")
                    {
                        var mLp = i.Value.ToString().Split(':').ToList();
                        var ms_file = new FileStream("data\\save004.png", FileMode.Open);
                        var img = Bitmap.FromStream(ms_file);
                        ms_file.Dispose();

                        return new SaveInfo() { chapter = Convert.ToInt32(mLp[0]), frames = Convert.ToInt32(mLp[1]), comment = i.Attribute("comment").Value.ToString(), imp = (Bitmap)img };
                    }
                }
                throw new Exception();
            }
            set
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "data1")
                    {
                        string data_save = value.chapter.ToString() + ":" + value.frames.ToString();
                        i.Value = data_save;
                        i.Attribute("comment").Value = value.comment;
                        value.imp.Save("data\\save001.png", System.Drawing.Imaging.ImageFormat.Png);
                        sysData.Save("saves.xml");
                        value.imp.Dispose();
                    }
                }
            }

        }
        public static SaveInfo save2
        {
            get
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "data2")
                    {
                        var mLp = i.Value.ToString().Split(':').ToList();
                        var ms_file = new FileStream("data\\save002.png", FileMode.Open);
                        var img = Bitmap.FromStream(ms_file);
                        ms_file.Dispose();
                        return new SaveInfo() { chapter = Convert.ToInt32(mLp[0]), frames = Convert.ToInt32(mLp[1]), comment = i.Attribute("comment").Value.ToString(), imp = (Bitmap)img };
                    }
                }
                throw new Exception();
            }
            set
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "data2")
                    {
                        string data_save = value.chapter.ToString() + ":" + value.frames.ToString();
                        i.Value = data_save;
                        i.Attribute("comment").Value = value.comment;
                        value.imp.Save("data\\save002.png", System.Drawing.Imaging.ImageFormat.Png);
                        sysData.Save("saves.xml");
                        value.imp.Dispose();
                    }
                }
            }

        }
        public static SaveInfo save3
        {
            get
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "data3")
                    {
                        var mLp = i.Value.ToString().Split(':').ToList();
                        var ms_file = new FileStream("data\\save003.png", FileMode.Open);
                        var img = Bitmap.FromStream(ms_file);
                        ms_file.Dispose();
                        return new SaveInfo() { chapter = Convert.ToInt32(mLp[0]), frames = Convert.ToInt32(mLp[1]), comment = i.Attribute("comment").Value.ToString(), imp = (Bitmap)img };
                    }
                }
                throw new Exception();
            }
            set
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "data3")
                    {
                        string data_save = value.chapter.ToString() + ":" + value.frames.ToString();
                        i.Value = data_save;
                        i.Attribute("comment").Value = value.comment;
                        value.imp.Save("data\\save003.png", System.Drawing.Imaging.ImageFormat.Png);
                        sysData.Save("saves.xml");
                        value.imp.Dispose();
                    }
                }
            }

        }
        public static SaveInfo save4
        {
            get
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "data4")
                    {
                        var mLp = i.Value.ToString().Split(':').ToList();
                        var ms_file = new FileStream("data\\save004.png", FileMode.Open);
                        var _img = Bitmap.FromStream(ms_file);
                        ms_file.Dispose();

                        return new SaveInfo() { chapter = Convert.ToInt32(mLp[0]), frames = Convert.ToInt32(mLp[1]), comment = i.Attribute("comment").Value.ToString(), imp = (Bitmap)_img };
                    }
                }
                throw new Exception();
            }
            set
            {
                foreach (XElement i in atrs)
                {
                    if (i.Name.ToString() == "data4")
                    {
                        string data_save = value.chapter.ToString() + ":" + value.frames.ToString();
                        i.Value = data_save;
                        i.Attribute("comment").Value = value.comment;
                        value.imp.Save("data\\save004.png", System.Drawing.Imaging.ImageFormat.Png);
                        sysData.Save("saves.xml");
                        value.imp.Dispose();
                    }
                }
            }

        }
    }

}