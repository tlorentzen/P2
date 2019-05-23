using System;
using System.IO;
using Compression;
using NUnit.Framework;

namespace TorPdos.TEST
{
    [TestFixture]
    public class CompressorTest
    {
        [Test]
        public void CompressAndDecompressSameArray()
        {
            byte[] expected = new byte[100];
            Random rand = new Random();
            rand.NextBytes(expected);

            byte[] compressed = Compressor.CompressBytes(expected);

            byte[] result = Compressor.DecompressBytes(compressed);

            CollectionAssert.AreEqual(expected, result);
        }

        [Test]
        public void CompressedArrayNotSame()
        {
            byte[] notExpected = new byte[100];
            Random rand = new Random();
            rand.NextBytes(notExpected);

            byte[] actual = Compressor.CompressBytes(notExpected);

            CollectionAssert.AreNotEqual(notExpected, actual);
        }

        [Test]
        public void CompressAndDecompressSameFile()
        {
            string FilePath = "TESTFILE.txt";
            Helpers.MakeAFile(FilePath);

            byte[] expected = Helpers.HashFile(FilePath);

            Compressor.CompressFile(FilePath, "comp.lzma");
            Compressor.DecompressFile("comp.lzma", "result");

            byte[] result = Helpers.HashFile("result.txt");

            File.Delete("comp.lzma");
            File.Delete("result.txt");

            CollectionAssert.AreEqual(expected, result);
        }

        [Test]
        public void CompressedFileNotSame()
        {
            string FilePath = "TESTFILE.txt";
            Helpers.MakeAFile(FilePath);

            byte[] notExpected = Helpers.HashFile(FilePath);
            Compressor.CompressFile(FilePath, "comp.lzma");
            byte[] actual = Helpers.HashFile("comp.lzma");
            File.Delete("comp.lzma");

            CollectionAssert.AreNotEqual(notExpected, actual);
        }
    }
}