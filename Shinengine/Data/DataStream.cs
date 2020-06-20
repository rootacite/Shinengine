using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Shinengine.Data
{
    class DataStream
    {
        public XDocument doc;
        public IEnumerable<XNode> atr;

        public struct PageInfo
        {
            public string BkSoure;
            public string music;
            public string text;
            public string name;
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
                    result.music = item.Element("music").Value.ToString();
                    result.BkSoure = item.Element("bks").Value.ToString();
                    result.text = item.Element("text").Value.ToString();
                    result.name = item.Element("name").Value.ToString();

                    break;
                }
                else
                    continue;
            }

            return result;
        }
    }
}