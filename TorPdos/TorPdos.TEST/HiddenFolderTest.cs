using NUnit.Framework;
using Index_lib;
using System.IO;
using System.Text;

namespace TorPdos.TEST{
    [TestFixture]
    public class HiddenFolderTest{
        [Test]
        public void FolderIsHidden()
        {
            var hidden = MakeHiddenFolder();
            bool expected = Directory.Exists("TEST/.hidden");

            if (expected){
                DirectoryInfo dir = new DirectoryInfo("TEST/.hidden");
                expected &= ((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden);
            }

            Assert.IsTrue(expected);
        }
        
        private HiddenFolder MakeHiddenFolder(){
            Helpers.MakeDirectory("TEST");
            return new HiddenFolder("TEST/.hidden");
        }
    }
}