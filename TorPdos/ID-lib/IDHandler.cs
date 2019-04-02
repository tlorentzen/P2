using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Index_lib;

namespace ID_lib
{
    public class IDHandler
    {
        public static void CreateUserFolder()
        {
            string userdata = "userdata";
            DirectoryInfo userdataDirectory = Directory.CreateDirectory(userdata);
            userdataDirectory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            string uid = "0123456789abcdef";
            string pass = "let_us_go";
            using (StreamWriter userFile = File.CreateText(userdata + "\\" + uid))
            {
                userFile.WriteLine(pass);
            }

        }

        public static void CreateUser(string password)
        {
            //Generate UID
            //
        }

        public static bool IsUserPresent()
        {
            //Test for validators in hidden subfolder
            if (true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool ValidateUser(string uid, string password)
        {
            //find matching UID file
            //compare password hash
            if (true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



    }
}
