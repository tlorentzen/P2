using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace TorPdos.TEST
{
    class Helpers
    {
        public static void MakeAFile(string path)
        {
            var fs = new FileStream(path,FileMode.Create);
            byte[] content = new byte[100];
            Random rand = new Random();
            rand.NextBytes(content);
            fs.Write(content, 0, content.Length);
            fs.Close();

        }
        public static byte[] HashFile(string filename)
        {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(filename)) {
                    return md5.ComputeHash(stream);
                }
            }
        }

        public static void MakeDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
    }
   
}
