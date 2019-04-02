using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Index_lib;
using P2P_lib;

namespace ID_lib
{
    public class IDHandler
    {
        private static string userdatafolder = "userdata";

        public static void CreateUser(string password)
        {
            if (!Directory.Exists(userdatafolder))
            {
                DirectoryInfo userdataDirectory = Directory.CreateDirectory(userdatafolder);
                userdataDirectory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            string uuid = GenerateUUID();
            using (StreamWriter userFile = File.CreateText(userdatafolder + "\\" + uuid))
            {
                userFile.WriteLine(password);
            }

        }

        public static string GenerateUUID()
        {
            String guid = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            List<string> macAddresses = NetworkHelper.getMacAddresses();

            foreach(string mac in macAddresses){
                guid += mac;
            }
            return DiskHelper.CreateMD5(guid);
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
