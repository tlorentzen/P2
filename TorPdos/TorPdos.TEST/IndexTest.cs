using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Index_lib;
using System.IO;
using System.Security.Cryptography;


namespace TorPdos.TEST
{
    [TestClass]
    public class IndexTest
    {
        [TestMethod]
        public void IndexFileCreatedAtRightPath()
        {

            var index = initIndex();
            Assert.IsTrue(File.Exists("TEST/.hidden/index.json"));

        }

        [TestMethod]
        public void GetRightPath()
        {
            var index = initIndex();
            string expected = "TEST/";
            string result = index.getPath();

            Assert.AreEqual(expected, result);

        }

        [TestMethod]
        public void IndexStartSetRunningTrue()
        {
            var index = initIndex();
            index.Start();

            bool result = index.isRunning;

            index.stop();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IndexStopSetRunningFalse()
        {
            var index = initIndex();
            index.Start();
            bool result = index.isRunning;
            index.stop();
            result = result == index.isRunning;

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void rebuildIndexGivesSameIndex()
        {
            for(int i = 0; i < 10; i++) {
                File.Copy("TESTFILE.md", "TEST/TESTCOPY" + i + ".md");
            }
            var index = initIndex();

            string expected = File.ReadAllText("TEST/.hidden/index.json");
            
            index.reIndex();

            string result = File.ReadAllText("TEST/.hidden/index.json");
            
            string[] files = Directory.GetFiles("TEST");
            foreach(string s in files) {
                File.Delete(s);
            }

            Assert.AreEqual(expected, result);

        }

        private Index initIndex()
        {
            Index index = new Index("TEST/");
            index.buildIndex();

            return index;
        }

        static byte[] HashFile(string filename)
        {
            if(!File.Exists(filename))
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(filename)) {
                    return md5.ComputeHash(stream);
                }
            } else {
                throw new ArgumentException(filename);
            }
        }
    }
}
