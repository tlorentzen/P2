using Encryption;
using System.IO;
using NUnit.Framework;

namespace TorPdos.TEST{
    [TestFixture]
    public class EncryptionTest{
        static string FilePath = "TESTFILE.txt";
        static FileEncryption _crypt = new FileEncryption("TESTFILE", ".txt");

        static string password = "Password";

        [Test]
        public void EncryptedFileNotSame(){
            Helpers.MakeAFile(FilePath);
            byte[] notExpected = Helpers.HashFile(FilePath);
            _crypt.DoEncrypt(password);

            byte[] actual = Helpers.HashFile("TESTFILE.aes");

            File.Delete("TESTFILE.aes");

            CollectionAssert.AreNotEqual(notExpected, actual);
        }

        [Test]
        public void EncryptDecryptSameFile(){
            Helpers.MakeAFile(FilePath);
            byte[] expected = Helpers.HashFile(FilePath);
            _crypt.DoEncrypt(password);
            _crypt.DoDecrypt(password);

            byte[] result = Helpers.HashFile(FilePath);
            File.Delete("TESTFILE.aes");


            CollectionAssert.AreEqual(expected, result);
        }

        [Test]
        public void EncryptDecryptDifferentEncryptorSameFile(){
            Helpers.MakeAFile(FilePath);
            byte[] expected = Helpers.HashFile(FilePath);
            _crypt.DoEncrypt(password);
            _crypt = new FileEncryption("TESTFILE", "txt");
            _crypt.DoDecrypt(password);

            byte[] result = Helpers.HashFile(FilePath);
            File.Delete("TESTFILE.aes");


            CollectionAssert.AreEqual(expected, result);
        }
    }
}