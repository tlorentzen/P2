using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Index_lib;
using System.IO;
using System.Text;

namespace TorPdos.TEST{
    [TestClass]
    public class HiddenFolderTest{
        [TestMethod]
        public void FolderIsHidden(){
            var Hidden = MakeHiddenFolder();
            bool expected = Directory.Exists("TEST/.hidden");

            if (expected){
                DirectoryInfo dir = new DirectoryInfo("TEST/.hidden");
                expected &= ((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden);
            }

            Assert.IsTrue(expected);
        }

        [TestMethod]
        public void AddFileAddedFileExists(){
            var Hidden = MakeHiddenFolder();
            Helpers.MakeAFile("TESTFILE.txt");
            File.Copy("TESTFILE.txt", "TESTCOPY.txt");
            Hidden.Add("TESTCOPY.txt");
            bool expected = File.Exists("TEST/.hidden/TESTCOPY.txt");
            Hidden.Remove("TESTCOPY.txt");

            Assert.IsTrue(expected);
        }
        /*
        [TestMethod]
        public void RemovedFileNotExists()
        {
            Hidden.RemoveFile(@"TEST/.hidden/TESTCOPY.md");

            Assert.IsFalse(File.Exists("TEST/.hidden/TESTCOPY.md"));
        }
        */

        [TestMethod]
        public void WriteToFileCreatedFile(){
            var Hidden = MakeHiddenFolder();
            using (FileStream fs = Hidden.WriteToFile("TEST/.hidden/TESTFILE.txt")){
                fs.Write(Encoding.ASCII.GetBytes("Hej"), 0, 3);
            }

            bool result = File.Exists("TEST/.hidden/TESTFILE.txt");
            Hidden.Remove("TESTFILE.txt");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReadFromFile(){
            var Hidden = MakeHiddenFolder();
            byte[] expected = Encoding.ASCII.GetBytes("Hej");
            byte[] result = new byte[3];
            using (FileStream fs = Hidden.WriteToFile("TEST/.hidden/TESTFILE.txt")){
                fs.Write(Encoding.ASCII.GetBytes("Hej"), 0, 3);
            }

            using (FileStream fs = Hidden.ReadFromFile("TEST/.hidden/TESTFILE.txt")){
                fs.Read(result, 0, 3);
            }

            Hidden.Remove("TESTFILE.txt");

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void MakeFolderInHiddenAddFile(){
            var Hidden = MakeHiddenFolder();
            Directory.CreateDirectory("TEST/.hidden/HiddenTest");
            Helpers.MakeAFile("TESTFILE.txt");
            File.Copy("TESTFILE.txt", "TESTCOPY.txt");
            Hidden.Add("TESTCOPY.txt", "HiddenTest/TESTCOPY.txt");
            bool expected = File.Exists("TEST/.hidden/HiddenTest/TESTCOPY.txt");
            Hidden.Remove("HiddenTest/TESTCOPY.txt");

            Assert.IsTrue(expected);
        }

        [TestMethod]
        public void DeleteEntireFolderInHidden(){
            var Hidden = MakeHiddenFolder();
            Directory.CreateDirectory("TEST/.hidden/HiddenTest");
            Helpers.MakeAFile("TESTFILE.txt");
            File.Copy("TESTFILE.txt", "TESTCOPY.txt");
            Hidden.Add("TESTCOPY.txt", "HiddenTest/TESTCOPY.txt");
            bool expected = File.Exists("TEST/.hidden/HiddenTest/TESTCOPY.txt");
            Hidden.Remove("HiddenTest");
            expected &= Directory.Exists("TEST/.hidden/HiddenTest");

            Assert.IsFalse(expected);
        }

        private HiddenFolder MakeHiddenFolder(){
            Helpers.MakeDirectory("TEST");
            return new HiddenFolder("TEST/.hidden");
        }
    }
}