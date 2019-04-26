using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Encryption;
using System.Security.Cryptography;
using System.IO;

namespace TorPdos.TEST
{
    [TestClass]
    public class EncryptionTest
    {
        static string FilePath = "TESTFILE.md";
        static FileEncryption Crypt = new FileEncryption("TESTFILE", ".md");

        static string password = "Password";

        [TestMethod]
        public void EncryptedFileNotSame()
        {
            byte[] notExpected = HashFile(FilePath);
            Crypt.DoEncrypt(password);

            byte[] actual = HashFile("TESTFILE.aes");

            File.Delete("TESTFILE.aes");

            CollectionAssert.AreNotEqual(notExpected, actual);

        }

        [TestMethod]
        public void EncryptDecryptSameFile()
        {
            byte[] expected = HashFile(FilePath);
            Crypt.DoEncrypt(password);
            Crypt.DoDecrypt(password);

            byte[] result = HashFile("TESTFILE.md");
            File.Delete("TESTFILE.aes");


            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void EncryptDecryptDifferentEncryptorSameFile()
        {
            byte[] expected = HashFile(FilePath);
            Crypt.DoEncrypt(password);
            Crypt = new FileEncryption("TESTFILE", ".md");
            Crypt.DoDecrypt(password);

            byte[] result = HashFile("TESTFILE.md");
            File.Delete("TESTFILE.aes");


            CollectionAssert.AreEqual(expected, result);
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
