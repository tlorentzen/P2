﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using P2P_lib;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace ID_lib
{
    public static class IdHandler
    {
        private static readonly string userdatafile = "userdata";
        private static readonly int iterations = 10000, hashlength = 20, saltlength = 16;
        private static RegistryKey MyReg = Registry.CurrentUser.OpenSubKey("TorPdos\\1.1.1.1", true);

        //Create user file using generated UUID and input password (and UUID, if input)
        public static string createUser(string path, string password, string uuid = null)
        {
            try
            {
                if (uuid == null)
                {
                    uuid = generateUuid();
                }
                else
                {
                    if (uuid.Length > 32)
                    {
                        uuid = uuid.Substring(0, 32);
                    }
                }

                string keymold = generateKeymold(uuid, password);
                using (StreamWriter userFile = File.CreateText(path + userdatafile))
                {
                    userFile.WriteLine(keymold);
                    userFile.WriteLine(uuid);
                    userFile.Close();
                }

                MyReg.SetValue("UUID", uuid);
                Console.WriteLine("NEW USER: " + uuid);
                return uuid;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //Generate keymold (hash) from key
        public static string generateKeymold(string key1, string key2 = null)
        {
            string key = null;

            if (key2 != null)
            {
                key = string.Concat(key1, key2);
            }
            else
            {
                key = key1;
            }

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
        private static string generateUuid()
        {
            string guid = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            List<string> macAddresses = NetworkHelper.getMacAddresses();

            foreach (string mac in macAddresses)
            {
                guid += mac;
            }

            return DiskHelper.createMd5(guid);
        }

        //Check if UUID and password match existing local user
        //Compare keymolds (hashes)
        //Returns true if user details are valid, false if not
        public static bool isValidUser(string path, string uuid, string password)
        {
            try
            {
                using (StreamReader userFile = new StreamReader(path + userdatafile))
                {
                    //Hash userdata, load 
                    //?? Checker for null value
                    byte[] hashBytes = Convert.FromBase64String(userFile.ReadLine() ?? throw new NullReferenceException());
                    byte[] salt = new byte[saltlength];
                    Array.Copy(hashBytes, 0, salt, 0, saltlength);
                    Rfc2898DeriveBytes keymold = new Rfc2898DeriveBytes(string.Concat(uuid, password), salt, iterations);
                    byte[] hash = keymold.GetBytes(hashlength);

                    //Compare hashes
                    for (int i = 0; i < hashlength; i++)
                    {
                        if (hashBytes[i + saltlength] != hash[i])
                        {
                            return false;
                        }
                    }
                    userFile.Close();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        //Return UUID if present, else return null
        public static string getUuid(string path)
        {
            if (userExists(path))
            {
                try
                {
                    return File.ReadAllLines(path + userdatafile).ElementAtOrDefault(1);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        //Return keymold if present, else return null 
        //TODO Skal den godt nok være public? Den burde slet ikke være her, to be honest. Der er intet, der kræver access til keymold (udover validation, som selv klarer det)
        public static string getKeymold(string path)
        {
            if (userExists(path))
            {
                try
                {
                    return File.ReadAllLines(path + userdatafile).ElementAtOrDefault(0);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static bool userExists(string path)
        {
            if (File.Exists(path + userdatafile))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Removes userdata file, return false if failed
        public static bool removeUser(string path)
        {
            try
            {
                File.Delete(path + userdatafile);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}