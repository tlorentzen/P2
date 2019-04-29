using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Index_lib;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TorPdos.TEST{
    [TestClass]
    public class IndexTest{
        [TestMethod]
        public void IndexFileCreatedAtRightPath(){
            var index = initIndex();
            Assert.IsTrue(File.Exists("TEST/.hidden/index.json"));
        }

        [TestMethod]
        public void GetRightPath(){
            var index = initIndex();
            string expected = "TEST";
            string result = index.GetPath();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void IndexStartSetRunningTrue(){
            var index = initIndex();
            index.Start();

            bool result = index.isRunning;

            index.Stop();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IndexStopSetRunningFalse(){
            var index = initIndex();
            index.Start();
            bool result = index.isRunning;
            index.Stop();
            result = result == index.isRunning;

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void rebuildIndexGivesSameIndex(){
            Helpers.MakeAFile("TESTFILE.txt");
            for (int i = 0; i < 10; i++){
                File.Copy("TESTFILE.txt", "TEST/TESTCOPY" + i + ".txt");
            }

            var index = initIndex();

            string expected = File.ReadAllText("TEST/.hidden/index.json");

            index.ReIndex();

            string result = File.ReadAllText("TEST/.hidden/index.json");

            string[] files = Directory.GetFiles("TEST");
            foreach (string s in files){
                File.Delete(s);
            }

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AddedFileEventRaised(){
            bool result = false;
            var index = initIndex();
            index.FileAdded += (f) => { result = true; };
            index.Start();

            Helpers.MakeAFile("TEST/TESTFILE.txt");
            System.Threading.Thread.Sleep(1000);
            index.Save();
            index.Stop();
            File.Delete("TEST/TESTFILE.txt");
            File.Delete("TEST/.hidden/index.json");

            Assert.IsTrue(result);
        }


        [TestMethod]
        public void DeletedFileEventRaised(){
            bool result = false;
            var index = initIndex();
            index.FileDeleted += (f) => { result = true; };
            Helpers.MakeAFile("TEST/TESTFILE.txt");

            index.ReIndex();
            index.Start();
            File.Delete("TEST/TESTFILE.txt");
            System.Threading.Thread.Sleep(1000);
            index.Stop();
            index.Save();
            File.Delete("TEST/.hidden/index.json");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FileChangedEventRaised(){
            bool result = false;
            var index = initIndex();
            index.FileChanged += (f) => { result = true; };
            Helpers.MakeAFile("TEST/TESTFILE.txt");
            index.Start();
            var fs = new FileStream("TEST/TESTFILE.txt", FileMode.Append);
            byte[] text = Encoding.ASCII.GetBytes("THIS IS A TEST TOO");
            fs.Write(text, 0, text.Length);
            fs.Close();
            index.Save();
            index.Stop();

            File.Delete("TEST/TESTFILE.txt");
            File.Delete("TEST/.hidden/index.json");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FileMissingEventRaised(){
            bool result = false;
            var index = initIndex();
            index.FileMissing += (f) => { result = true; };
            Helpers.MakeAFile("TEST/TESTFILE.txt");
            index.ReIndex();
            File.Delete("TEST/TESTFILE.txt");
            index.MakeIntegrityCheck();
            index.Save();

            File.Delete("TEST/TESTFILE.txt");
            File.Delete("TEST/.hidden/index.json");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RenameEventWorks(){
            string name = "TEST\\\\NEWNAMETEST.txt";
            var index = initIndex();
            Helpers.MakeAFile("TEST/TESTFILE.txt");
            index.ReIndex();
            index.Start();
            File.Move("TEST/TESTFILE.txt", "TEST/NEWNAMETEST.txt");
            System.Threading.Thread.Sleep(1000);
            index.Save();
            index.Stop();
            index.ReIndex();
            string json = File.ReadAllText("TEST/.hidden/index.json");

            string result = json.Substring(json.Length - name.Length - 4, name.Length);

            File.Delete("TEST/NEWNAMETEST.txt");
            File.Delete("TEST/.hidden/index.json");

            Assert.AreEqual(name, result);
        }

        private Index initIndex(){
            Helpers.MakeDirectory("TEST");
            Index index = new Index("TEST");
            index.BuildIndex();

            return index;
        }
    }
}