﻿using NUnit.Framework;

namespace TorPdos.TEST{
    [TestFixture]
    public class FileSplitterTest{
        /*[Test]
        public void SplitFileToFolderCorrectNumberOfFiles()
        {
            SplitterLibary Splitter = new SplitterLibary();
            Splitter.splitFile("TESTFILE.md", HashFile("TESTFILE.md").ToString(), "TEST/", 100);

            string[] files = Directory.GetFiles("TEST");

            int expected = (int)new FileInfo("TESTFILE.md").Length / 100 + 1;

            int result = files.Length;

            foreach(string s in files) {
                File.Delete(s);
            }

            Assert.AreEqual(expected, result);
        }

        /*
        [Test]
        public void MergedFilesAreSame()
        {
            SplitterLibary Splitter = new SplitterLibary();
            byte[] expected = HashFile("TESTFILE.md");
            var dict = Splitter.splitFile("TESTFILE.md", HashFile("TESTFILE.md").ToString(), "TEST/", 100);
            string[] files = Directory.GetFiles("TEST");
            var list = new List<string>();
            dict.TryGetValue(HashFile("TESTFILE.md").ToString(), out list);
            Splitter.mergeFiles("TEST/", "Merged.md",list);
            byte[] result = HashFile("Merged.md");

            File.Delete("Merged.md");
            foreach (string s in files) {
                File.Delete(s);
            }
            Console.WriteLine(expected);
            Console.WriteLine(result);

            Assert.AreEqual(expected.Length, result.Length);

        }
        */
    }
}