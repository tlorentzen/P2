﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using P2P_lib;

namespace TorPdos.TEST{
    [TestClass]
    public class IdHandlerTest{
        public static string path = "TEST/";
        private const string Password = "Password";

        [TestMethod]
        private void GenerateUuidIsRandom(){
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
            IdHandler.CreateUser(Password);
            bool result = IdHandler.IsValidUser(Password);
            IdHandler.RemoveUser();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WrongPasswordInvalidUser(){
            IdHandler.CreateUser(Password);
            bool result = IdHandler.IsValidUser( "wrong");
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