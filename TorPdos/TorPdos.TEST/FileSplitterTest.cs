using System;
using Splitter_lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;

namespace TorPdos.TEST{
    [TestClass]
    public class FileSplitterTest{
        /*[TestMethod]
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
        [TestMethod]
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