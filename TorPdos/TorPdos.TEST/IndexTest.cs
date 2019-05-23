using NUnit.Framework;
using Index_lib;
using System.IO;
using System.Text;

namespace TorPdos.TEST{
    [TestFixture]
    public class IndexTest{
        [Test]
        public void IndexFileCreatedAtRightPath(){
            initIndex();

            bool result = File.Exists(@"C:\TEST\.hidden\index.json");
            
            System.Threading.Thread.Sleep(1000);
            Directory.Delete(@"C:\TEST\",true);
            
            Assert.IsTrue(result);
        }

        [Test]
        public void GetRightPath(){
            var index = initIndex();
            string expected = @"C:\TEST\";
            string result = index.GetPath();
            
            System.Threading.Thread.Sleep(1000);
            Directory.Delete(@"C:\TEST\",true);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void IndexStartSetRunningTrue(){
            var index = initIndex();
            index.Start();

            bool result = index.isRunning;

            index.Stop();
            
            System.Threading.Thread.Sleep(1000);
            Directory.Delete(@"C:\TEST\",true);

            Assert.IsTrue(result);
        }

        [Test]
        public void IndexStopSetRunningFalse(){
            var index = initIndex();
            index.Start();
            bool result = index.isRunning;
            index.Stop();
            result = result == index.isRunning;
            
            System.Threading.Thread.Sleep(1000);
            Directory.Delete(@"C:\TEST\",true);

            Assert.IsFalse(result);
        }

        [Test]
        public void RebuildIndexGivesSameIndex(){
            System.Threading.Thread.Sleep(1000);
            var index = initIndex();
            Helpers.MakeAFile("TESTFILE.txt");
            for (int i = 0; i < 10; i++){
                File.Copy("TESTFILE.txt", @"C:\TEST\TESTCOPY" + i + ".txt");
            }
            System.Threading.Thread.Sleep(1000);
            index.Save();
            index.Stop();
            string expected = File.ReadAllText(@"C:\TEST\.hidden\index.json");

            index.ReIndex();
            index.Save();
            index.Stop();
            string result = File.ReadAllText(@"C:\TEST\.hidden\index.json");
            
            System.Threading.Thread.Sleep(1000);
            Directory.Delete(@"C:\TEST\",true );
            File.Delete("TESTFILE.txt");

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AddedFileEventRaised(){
            bool result = false;
            var index = initIndex();
            index.FileAdded += (f) => { result = true; };
            index.Start();

            Helpers.MakeAFile(@"C:\TEST\TESTFILE.txt");
            System.Threading.Thread.Sleep(1000);
            index.Stop();
            System.Threading.Thread.Sleep(1000);
            Directory.Delete(@"C:\TEST\",true);

            Assert.IsTrue(result);
        }


        [Test]
        public void DeletedFileEventRaised(){
            bool result = false;
            var index = initIndex();
            index.FileDeleted += (f) => { result = true; };
            Helpers.MakeAFile(@"C:\TEST\TESTFILE.txt");
            System.Threading.Thread.Sleep(1000);
            index.ReIndex();
            //index.Start();
            File.Delete(@"C:\TEST\TESTFILE.txt");
            System.Threading.Thread.Sleep(1000);
            index.Stop();
            //index.Save();
            Directory.Delete(@"C:\TEST\",true);

            Assert.IsTrue(result);
        }

        [Test]
        public void FileChangedEventRaised(){
            bool result = false;
            var index = initIndex();
            index.FileChanged += (f,t) => { result = true; };
            Helpers.MakeAFile(@"C:\TEST\TESTFILE.txt");
            index.Start();
            var fs = new FileStream(@"C:\TEST\TESTFILE.txt", FileMode.Append);
            byte[] text = Encoding.ASCII.GetBytes("THIS IS A TEST TOO");
            fs.Write(text, 0, text.Length);
            fs.Close();
            System.Threading.Thread.Sleep(500);
            index.Save();
            index.Stop();

            Directory.Delete(@"C:\TEST\",true);

            Assert.IsTrue(result);
        }

        [Test]
        public void FileMissingEventRaised(){
            bool result = false;
            var index = initIndex();
            index.FileMissing += (f) => { result = true; };
            Helpers.MakeAFile(@"C:\TEST\TESTFILE.txt");
            index.ReIndex();
            File.Delete(@"C:\TEST\TESTFILE.txt");
            index.MakeIntegrityCheck();
            index.Save();

            System.Threading.Thread.Sleep(1000);
            Directory.Delete(@"C:\TEST\",true);

            Assert.IsTrue(result);
        }

        [Test]
        public void RenameEventWorks(){
            string name = @"C:\TEST\NEWNAMETEST.txt";
            var index = initIndex();
            Helpers.MakeAFile(@"C:\TEST\TESTFILE.txt");
            index.ReIndex();
            File.Move(@"C:\TEST\TESTFILE.txt", name);
            System.Threading.Thread.Sleep(1000);
            index.Stop();
            index.ReIndex();
            System.Threading.Thread.Sleep(1000);
            string json = File.ReadAllText(@"C:\TEST\.hidden\index.json");

            bool result = json.Contains("NEWNAMETEST.txt");

            System.Threading.Thread.Sleep(1000);
            Directory.Delete(@"C:\TEST\",true);

            Assert.IsTrue(result);
        }

        private Index initIndex(){
            Helpers.MakeDirectory(@"C:\TEST\");
            Index index = new Index(@"C:\TEST\");
            index.BuildIndex();
            index.Start();

            return index;
        }
    }
}