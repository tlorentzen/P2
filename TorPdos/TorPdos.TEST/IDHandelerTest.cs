using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ID_lib;
using System.IO;

namespace TorPdos.TEST
{
    [TestClass]
    public class IDHandelerTest
    {
        static string path = "TEST/";
        static string Password = "Password";

        [TestMethod]
        public void GenereateUUIDIsRandom()
        {
            string notExpected = IdHandler.createUser(path, Password, null);
            System.Threading.Thread.Sleep(5000);
            string actual = IdHandler.createUser(path, Password, null);

            IdHandler.removeUser(path);

            Assert.AreNotEqual(notExpected, actual);
        }

        [TestMethod]
        public void UserDataFileCreated()
        {
            IdHandler.createUser(path, Password, null);

            bool result = File.Exists(path + @"\userdata");
            IdHandler.removeUser(path);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidateUser()
        {
            string uuid = IdHandler.createUser(path, Password, null);
            bool result = IdHandler.isValidUser(path, uuid, Password);
            IdHandler.removeUser(path);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WrongPasswordInvaildUser()
        {
            string uuid = IdHandler.createUser(path, Password, null);
            bool result = IdHandler.isValidUser(path, uuid, "wrong");
            IdHandler.removeUser(path);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetUUidIsCorrect()
        {
            string expected = IdHandler.createUser(path, Password, null);
            string result = IdHandler.getUuid(path);
            IdHandler.removeUser(path);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RemovedUserNotExists()
        {
            IdHandler.createUser(path, Password, null);
            bool result = File.Exists(path + @"\userdata");
            IdHandler.removeUser(path);
            bool final = result == File.Exists(path + @"\userdata");

            Assert.IsFalse(final);

        }

        
    }
}
