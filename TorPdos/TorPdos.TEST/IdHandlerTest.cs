using NUnit.Framework;
using P2P_lib;
using P2P_lib.Handlers;

namespace TorPdos.TEST{
    [TestFixture]
    public class IdHandlerTest{
        public static string path = "TEST/";
        private const string Password = "Password";

        [Test]
        public void GenerateUuidIsRandom(){
            string notExpected = IdHandler.CreateUser(Password);
            System.Threading.Thread.Sleep(5000);
            string actual = IdHandler.CreateUser(Password);

            IdHandler.RemoveUser();

            Assert.AreNotEqual(notExpected, actual);
        }

        /*[Test]
        public void UserDataFileCreated()
        {
            IdHandler.CreateUser(Password);

            bool result = File.Exists(path + @"\userdata");
            IdHandler.RemoveUser();

            Assert.IsTrue(result);
        }*/

        [Test]
        public void ValidateUser(){
            IdHandler.CreateUser(Password);
            bool result = IdHandler.IsValidUser(Password);
            IdHandler.RemoveUser();

            Assert.IsTrue(result);
        }

        [Test]
        public void WrongPasswordInvalidUser(){
            IdHandler.CreateUser(Password);
            bool result = IdHandler.IsValidUser( "wrong");
            IdHandler.RemoveUser();

            Assert.IsFalse(result);
        }

        [Test]
        public void GetUUidIsCorrect(){
            string expected = IdHandler.CreateUser(Password);
            string result = IdHandler.GetUuid(Password);
            IdHandler.RemoveUser();

            Assert.AreEqual(expected, result);
        }

        /*[Test]
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