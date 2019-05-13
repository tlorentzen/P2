using Microsoft.VisualStudio.TestTools.UnitTesting;
using Index_lib;
using System.IO;
using System.Text;

/*namespace TorPdos.TEST{
    [TestClass]
    public class HiddenFolderTest{
        [TestMethod]
        public void FolderIsHidden(){
            bool expected = Directory.Exists("TEST/.hidden");

            if (expected){
                DirectoryInfo dir = new DirectoryInfo("TEST/.hidden");
                expected &= ((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden);
            }

            Assert.IsTrue(expected);
        }

        [TestMethod]
        public void AddFileAddedFileExists(){
            var hidden = MakeHiddenFolder();
            Helpers.MakeAFile("TESTFILE.txt");
            File.Copy("TESTFILE.txt", "TESTCOPY.txt");
            hidden.Add("TESTCOPY.txt");
            bool expected = File.Exists("TEST/.hidden/TESTCOPY.txt");
            hidden.Remove("TESTCOPY.txt");

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

        /*[TestMethod]
        public void WriteToFileCreatedFile(){
            var hidden = MakeHiddenFolder();
            using (FileStream fs = hidden.WriteToFile("TEST/.hidden/TESTFILE.txt")){
                fs.Write(Encoding.ASCII.GetBytes("Hej"), 0, 3);
            }

            bool result = File.Exists("TEST/.hidden/TESTFILE.txt");
            hidden.Remove("TESTFILE.txt");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReadFromFile(){
            var hidden = MakeHiddenFolder();
            byte[] expected = Encoding.ASCII.GetBytes("Hej");
            byte[] result = new byte[3];
            using (FileStream fs = hidden.WriteToFile("TEST/.hidden/TESTFILE.txt")){
                fs.Write(Encoding.ASCII.GetBytes("Hej"), 0, 3);
            }

            using (FileStream fs = hidden.ReadFromFile("TEST/.hidden/TESTFILE.txt")){
                fs.Read(result, 0, 3);
            }

            hidden.Remove("TESTFILE.txt");

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void MakeFolderInHiddenAddFile(){
            var hidden = MakeHiddenFolder();
            Directory.CreateDirectory("TEST/.hidden/HiddenTest");
            Helpers.MakeAFile("TESTFILE.txt");
            File.Copy("TESTFILE.txt", "TESTCOPY.txt");
            hidden.Add("TESTCOPY.txt", "HiddenTest/TESTCOPY.txt");
            bool expected = File.Exists("TEST/.hidden/HiddenTest/TESTCOPY.txt");
            hidden.Remove("HiddenTest/TESTCOPY.txt");

            Assert.IsTrue(expected);
        }

        [TestMethod]
        public void DeleteEntireFolderInHidden(){
            var hidden = MakeHiddenFolder();
            Directory.CreateDirectory("TEST/.hidden/HiddenTest");
            Helpers.MakeAFile("TESTFILE.txt");
            File.Copy("TESTFILE.txt", "TESTCOPY.txt");
            hidden.Add("TESTCOPY.txt", "HiddenTest/TESTCOPY.txt");
            bool expected = File.Exists("TEST/.hidden/HiddenTest/TESTCOPY.txt");
            hidden.Remove("HiddenTest");
            expected &= Directory.Exists("TEST/.hidden/HiddenTest");

            Assert.IsFalse(expected);
        }

        private HiddenFolder MakeHiddenFolder(){
            Helpers.MakeDirectory("TEST");
            return new HiddenFolder("TEST/.hidden");
        }
    }
}*/