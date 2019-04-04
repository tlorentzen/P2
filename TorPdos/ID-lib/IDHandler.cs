using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Index_lib;
using P2P_lib;
using System.Security.Cryptography;

namespace ID_lib
{
    public class IDHandler
    {
        private static readonly string userdatafolder = "userdata", userdatafile = "userdata";
        private static readonly int iterations = 10000, hashlength = 20, saltlength = 16;

        //Create user file using generated UUID and input password
        public static string CreateUser(string path, string password)
        {
            try
            {
                string uuid = GenerateUUID();

                string keymold = GenerateKeymold(string.Concat(uuid, password));

                using (StreamWriter userFile = File.CreateText(path + "\\" + userdatafile))
                {
                    userFile.WriteLine(keymold);
                    userFile.WriteLine(uuid);
                }
                return uuid;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //Generate keymold (hash) from key
        private static string GenerateKeymold(string key)
        {
            //Randomise salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            //Get hash
            Rfc2898DeriveBytes keymold = new Rfc2898DeriveBytes(key, salt, iterations);
            byte[] hash = keymold.GetBytes(hashlength);

            //Combine salt and hash into key
            byte[] hashBytes = new byte[hashlength + saltlength];
            Array.Copy(salt, 0, hashBytes, 0, saltlength);
            Array.Copy(hash, 0, hashBytes, saltlength, hashlength);

            return Convert.ToBase64String(hashBytes);
        }

        //Generate UUID based on mac addresses and current time
        private static string GenerateUUID()
        {
            String guid = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            List<string> macAddresses = NetworkHelper.getMacAddresses();

            foreach(string mac in macAddresses){
                guid += mac;
            }
            return DiskHelper.CreateMD5(guid);
        }

        //Check if UUID and password match existing local user
        //Compare keymolds (hashes)
        //Returns true if user details are valid, false if not
        public static bool IsValidUser(string path, string uuid, string password)
        {
            try
            {
                using (StreamReader userFile = new StreamReader(path + "\\" + userdatafile))
                {
                    //Hash userdata, load 
                    byte[] hashBytes = Convert.FromBase64String(userFile.ReadLine());
                    byte[] salt = new byte[saltlength];
                    Array.Copy(hashBytes, 0, salt, 0, saltlength);
                    Rfc2898DeriveBytes keymold = new Rfc2898DeriveBytes(string.Concat(uuid, password), salt, iterations);
                    byte[] hash = keymold.GetBytes(hashlength);

                    //Compare hashes
                    for (int i = 0; i < hashlength; i++)
                    {
                        if (hashBytes[i+saltlength] != hash[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        //Return UUID if present, else return null
        public static string GetUUID(string path)
        {
            string uuid = File.ReadAllLines(path + "\\" + userdatafile).ElementAtOrDefault(1);
            if (uuid != null)
            {
                return uuid;
            }
            else
            {
                return null;
            }
        }

        public static bool UserFileExists(string path)
        {
            if (File.Exists(path + "\\" + userdatafile))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /*
        //Returns IEnumberable of all UUIDs in userdatafolder, or empty if error
        public static IEnumerable<string> GetUserList(string path)
        {
            try
            {
                string[] uuidList = new DirectoryInfo(path).GetFiles().Select(o => o.Name).ToArray();
                return uuidList;
            }
            catch (Exception err)
            {
                Console.WriteLine(" * Failed to get user list!\n\n" + err.Message);
                string[] error = new string[0];
                return error;
            }
        }

        //Removes user file from userdatafolder
        //True if success, false if failed
        public static bool RemoveUser(string uuid)
        {
            try
            {
                File.Delete(userdatafolder + "\\" + uuid);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }*/
    }
}
