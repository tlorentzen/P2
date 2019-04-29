using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ID_lib;
using System.IO;

namespace TorPdos.TEST{
    [TestClass]
    public class IDHandelerTest{
        static string path = "TEST/";
        static string Password = "Password";

        [TestMethod]
        public void GenereateUUIDIsRandom(){
            string notExpected = IdHandler.CreateUser(Password);
            System.Threading.Thread.Sleep(5000);
            string actual = IdHandler.CreateUser(Password);

            IdHandler.RemoveUser();

            Assert.AreNotEqual(notExpected, actual);
        }

        /*[TestMethod]
        public void UserDataFileCreated()
        {
            IdHandler.CreateUser(Password);

            bool result = File.Exists(path + @"\userdata");
            IdHandler.RemoveUser();

            Assert.IsTrue(result);
        }*/

        [TestMethod]
        public void ValidateUser(){
            string uuid = IdHandler.CreateUser(Password);
            bool result = IdHandler.IsValidUser(uuid, Password);
            IdHandler.RemoveUser();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WrongPasswordInvaildUser(){
            string uuid = IdHandler.CreateUser(Password);
            bool result = IdHandler.IsValidUser(uuid, "wrong");
            IdHandler.RemoveUser();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetUUidIsCorrect(){
            string expected = IdHandler.CreateUser(Password);
            string result = IdHandler.GetUuid();
            IdHandler.RemoveUser();

            Assert.AreEqual(expected, result);
        }

        /*[TestMethod]
        public void RemovedUserNotExists()
        {
            IdHandler.CreateUser(Password);
            bool result = File.Exists(path + @"\userdata");
            IdHandler.RemoveUser();
            bool final = result == File.Exists(path + @"\userdata");

            Assert.IsFalse(final);
        }*/
    }
}