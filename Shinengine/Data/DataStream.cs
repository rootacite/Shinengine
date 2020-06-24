using System;
using System.Collections.Generic;
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
}