using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Index_lib;
using System.IO;
using System.Text;

namespace TorPdos.TEST
{
    [TestClass]
    public class HiddenFolderTest
    {

        HiddenFolder Hidden = new HiddenFolder("TEST/.hidden");

        [TestMethod]
        public void FolderIsHidden()
        {

            bool expected = Directory.Exists("TEST/.hidden");

            if (expected) {
                DirectoryInfo dir = new DirectoryInfo("TEST/.hidden");
                expected &= ((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden);
            }

            Assert.IsTrue(expected);
        }

        [TestMethod]
        public void AddFileAddedFileExists()
        {
            File.Copy("TESTFILE.md", "TESTCOPY.md");
            Hidden.Add("TESTCOPY.md");
            bool expected = File.Exists("TEST/.hidden/TESTCOPY.md");
            Hidden.Remove("TESTCOPY.md");

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
        public void WriteToFileCreatedFile()
        {
            
            using (FileStream fs = Hidden.WriteToFile("TEST/.hidden/TESTFILE.txt")) {
                fs.Write(Encoding.ASCII.GetBytes("Hej"), 0, 3);
            }

            Assert.IsTrue(File.Exists("TEST/.hidden/TESTFILE.txt"));
        }

        [TestMethod]
        public void ReadFromFile()
        {
            byte[] expected = Encoding.ASCII.GetBytes("Hej");
            byte[] result = new byte[3];

            using (FileStream fs = Hidden.ReadFromFile("TEST/.hidden/TESTFILE.txt")) {
                
                fs.Read(result, 0, 3);
            }

            CollectionAssert.AreEqual(expected, result);
            
        }

        [TestMethod]
        public void MakeFolderInHiddenAddFile()
        {
            Directory.CreateDirectory("TEST/.hidden/HiddenTest");
            File.Copy("TESTFILE.md", "TESTCOPY.md");
            Hidden.Add("TESTCOPY.md","HiddenTest/TESTCOPY.md");
            bool expected = File.Exists("TEST/.hidden/HiddenTest/TESTCOPY.md");
            Hidden.Remove("HiddenTest/TESTCOPY.md");

            Assert.IsTrue(expected);

        }

        [TestMethod]
        public void DeleteEntireFolderInHidden()
        {
            Directory.CreateDirectory("TEST/.hidden/HiddenTest");
            File.Copy("TESTFILE.md", "TESTCOPY.md");
            Hidden.Add("TESTCOPY.md", "HiddenTest/TESTCOPY.md");
            bool expected = File.Exists("TEST/.hidden/HiddenTest/TESTCOPY.md");
            Hidden.Remove("HiddenTest");
            expected &= Directory.Exists("TEST/.hidden/HiddenTest");

            Assert.IsFalse(expected);
        }
    }

}
