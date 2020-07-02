using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Shinengine.Data
{
    static class PackStream
    {
        static public void CreateIfNotExist(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        static public string UnZip(string target, string to, string password)
        {
            var filename = target.Split(':').ToArray();

            FileStream fileStreamIn = new FileStream(filename[0], FileMode.Open, FileAccess.Read);

            ZipInputStream zipInStream = new ZipInputStream(fileStreamIn)
            {
                Password = password
            };
            ZipEntry entry = zipInStream.GetNextEntry();
            do
            {
                if (entry.IsDirectory)
                    CreateIfNotExist(to + entry.Name);
                if (entry.Name != filename[1])
                {
                    continue;
                }
                if (File.Exists(to + filename[1]))
                {
                    return to + filename[1];
                }
                using FileStream fileStreamOut = new FileStream(to + filename[1], FileMode.Create, FileAccess.Write,FileShare.ReadWrite);
                int size = 2048;
                byte[] buffer = new byte[2048];
                do
                {
                    size = zipInStream.Read(buffer, 0, buffer.Length);
                    fileStreamOut.Write(buffer, 0, size);
                } while (size > 0);
                return to + filename[1];
            } while ((entry = zipInStream.GetNextEntry()) != null);

            zipInStream.Dispose();
            return null;
        }
        static public readonly string TempPath;
        static PackStream()
        {
            TempPath = "Temp/ShinengineData/";
        }
 
        static public string Locate(string source)
        {
           
            string result = UnZip(source, TempPath, "t3o(!8.8%%:y>b4|,/3/F=");
            if (result == null)
            {
                throw new Exception("failed to locate file :" + source);
            }
            Debug.WriteLine("Locate " + source +" -> "+ result);
            return result;
        } 
    }
}
