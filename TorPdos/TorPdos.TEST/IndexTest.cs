using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Index_lib;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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
            string expected = "TEST";
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

        [TestMethod]
        public void AddedFileEventRaised()
        {
            
            bool result = false;
            var index = initIndex();
            index.FileAdded += (f) => { result = true; };
            index.Start();
            WriteFile();
            System.Threading.Thread.Sleep(1000);
            index.save();
            index.stop();
            File.Delete("TEST/TESTFILE.txt");
            File.Delete("TEST/.hidden/index.json");

            Assert.IsTrue(result);

        }

    
        [TestMethod]
        public void DeletedFileEventRaised()
        {
            bool result = false;
            var index = initIndex();
            index.FileDeleted += (f) => { result = true; };
            WriteFile();

            index.reIndex();
            index.Start();
            File.Delete("TEST/TESTFILE.txt");
            System.Threading.Thread.Sleep(1000);
            index.stop();
            index.save();
            File.Delete("TEST/.hidden/index.json");

            Assert.IsTrue(result);

        }

        [TestMethod]
        public void FileChangedEventRaised()
        {
            bool result = false;
            var index = initIndex();
            index.FileChanged += (f) => { result = true; };
            WriteFile();
            index.Start();
            var fs = new FileStream("TEST/TESTFILE.txt", FileMode.Append);
            byte[] text = Encoding.ASCII.GetBytes("THIS IS A TEST TOO");
            fs.Write(text, 0, text.Length);
            fs.Close();
            index.save();
            index.stop();

            File.Delete("TEST/TESTFILE.txt");
            File.Delete("TEST/.hidden/index.json");

            Assert.IsTrue(result);

        }

        [TestMethod]
        public void FileMissingEventRaised()
        {
            bool result = false;
            var index = initIndex();
            index.FileMissing += (f) => { result = true; };
            WriteFile();
            index.reIndex();
            File.Delete("TEST/TESTFILE.txt");
            index.MakeIntegrityCheck();
            index.save();

            File.Delete("TEST/TESTFILE.txt");
            File.Delete("TEST/.hidden/index.json");

            Assert.IsTrue(result);

        }

        [TestMethod]
        public void RenameEventWorks()
        {
            string name = "TEST\\\\NEWNAMETEST.txt";
            var index = initIndex();
            WriteFile();
            index.reIndex();
            index.Start();
            File.Move("TEST/TESTFILE.txt", "TEST/NEWNAMETEST.txt");
            System.Threading.Thread.Sleep(1000);
            index.save();
            index.stop();
            index.reIndex();
            string json = File.ReadAllText("TEST/.hidden/index.json");

            string result = json.Substring(json.Length - name.Length - 4, name.Length);

            File.Delete("TEST/NEWNAMETEST.txt");
            File.Delete("TEST/.hidden/index.json");

            Assert.AreEqual(name,result);

        }

        private Index initIndex()
        {
            Index index = new Index("TEST");
            index.buildIndex();

            return index;
        }

        private void WriteFile()
        {
            var fs = new FileStream("TEST/TESTFILE.txt", FileMode.Create);
            byte[] text = Encoding.ASCII.GetBytes("THIS IS A TEST");
            fs.Write(text, 0, text.Length);
            fs.Close();
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
