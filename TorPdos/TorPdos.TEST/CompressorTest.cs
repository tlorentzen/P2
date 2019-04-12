using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Compression;
using System.Security.Cryptography;
using System.IO;

namespace TorPdos.TEST
{
    [TestClass]
    public class CompressorTest
    {
        
       
        [TestMethod]
        public void CompressAndDecompressSameArray()
        {
            byte[] expected = new byte[100];
            Random rand = new Random();
            rand.NextBytes(expected);

            byte[] compressed = ByteCompressor.compressBytes(expected);

            byte[] result = ByteCompressor.decompressBytes(compressed);

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CompressedArrayNotSame()
        {
            byte[] notExpected = new byte[100];
            Random rand = new Random();
            rand.NextBytes(notExpected);

            byte[] actual = ByteCompressor.compressBytes(notExpected);

            CollectionAssert.AreNotEqual(notExpected, actual);

        }

        [TestMethod]
        public void CompressAndDecompressSameFile()
        {
            string FilePath = "TESTFILE.md";

            byte[] expected = HashFile(FilePath);

            ByteCompressor.compressFile(FilePath, "comp.lzma");
            ByteCompressor.decompressFile("comp.lzma", "result");

            byte[] result = HashFile("result.md");

            File.Delete("comp.lzma");
            File.Delete("result.md");

            CollectionAssert.AreEqual(expected, result);

        }

        [TestMethod]
        public void CompressedFileNotSame()
        {
            string FilePath = "TESTFILE.md";

            byte[] notExpected = HashFile(FilePath);
            ByteCompressor.compressFile(FilePath, "comp.lzma");
            byte[] actual = HashFile("comp.lzma");
            File.Delete("comp.lzma");

            CollectionAssert.AreNotEqual(notExpected, actual);
        }

        static byte[] HashFile(string filename)
        {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(filename)) {
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}
