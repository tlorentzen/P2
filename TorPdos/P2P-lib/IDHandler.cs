using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Encryption;
using Microsoft.Win32;

namespace P2P_lib{
    public static class IdHandler{
        private static readonly string
            userdatafile = "userdata",
            hiddenfolder = @"\.hidden\";

        private static readonly int iterations = 10000, hashlength = 20, saltlength = 16;
        private static RegistryKey MyReg = Registry.CurrentUser.OpenSubKey("TorPdos\\1.1.1.1", true);
        private static string KeyMold = null;
        private static string UuID = null;

        //Create user file using generated UUID and input password (and UUID, if input)
        public static string CreateUser(string password, string uuid = null){
            try{
                if (uuid == null){
                    uuid = GenerateUuid();
                } else{
                    if (uuid.Length > 32){
                        uuid = uuid.Substring(0, 32);
                    }
                }

                string path = MyReg.GetValue("Path") + hiddenfolder + userdatafile;

                string keyMold = GenerateKeyMold(uuid, password);
                string output = KeyMold + "\n" + uuid;
                FileEncryption.UserDataEncrypt(password,output,path);

                Console.WriteLine("NEW USER: " + uuid);
                return uuid;
            }
            catch (Exception){
                return null;
            }
        }

        //Generate keymold (hash) from key
        public static string GenerateKeyMold(string key1, string key2 = null){
            string key;

            if (key2 != null){
                key = string.Concat(key1, key2);
            } else{
                key = key1;
            }

            //Randomise salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            //Get hash
            Rfc2898DeriveBytes keyMold = new Rfc2898DeriveBytes(key, salt, iterations);
            byte[] hash = keyMold.GetBytes(hashlength);

            //Combine salt and hash into key
            byte[] hashBytes = new byte[hashlength + saltlength];
            Array.Copy(salt, 0, hashBytes, 0, saltlength);
            Array.Copy(hash, 0, hashBytes, saltlength, hashlength);

            return Convert.ToBase64String(hashBytes);
        }

        //Generate UUID based on mac addresses and current time
        private static string GenerateUuid(){
            string guid = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            List<string> macAddresses = NetworkHelper.GetMacAddresses();

            foreach (string mac in macAddresses){
                guid += mac;
            }

            return DiskHelper.createMd5(guid);
        }

        //Check if UUID and password match existing local user
        //Compare keymolds (hashes)
        //Returns true if user details are valid, false if not
        public static bool IsValidUser(string uuid, string password){
            try{
                string path = MyReg.GetValue("Path") + hiddenfolder + userdatafile;

                using (StreamReader userFile = new StreamReader(path)){
                    //Hash userdata, load 
                    //?? Checker for null value
                    byte[] hashBytes =
                        Convert.FromBase64String(userFile.ReadLine() ?? throw new NullReferenceException());
                    byte[] salt = new byte[saltlength];
                    Array.Copy(hashBytes, 0, salt, 0, saltlength);
                    Rfc2898DeriveBytes keymold =
                        new Rfc2898DeriveBytes(string.Concat(uuid, password), salt, iterations);
                    byte[] hash = keymold.GetBytes(hashlength);

                    //Compare hashes
                    for (int i = 0; i < hashlength; i++){
                        if (hashBytes[i + saltlength] != hash[i]){
                            return false;
                        }
                    }

                    userFile.Close();
                    return true;
                }
            }
            catch (Exception){
                return false;
            }
        }

        //Return UUID if present, else return null
        public static string GetUuid(string password){
            string path = MyReg.GetValue("Path") + hiddenfolder + userdatafile;
            if (UserExists()){
                try{
                    if (UuID != null){
                        return UuID;
                    } else{
                        UuID = FileEncryption.UserDataDecrypt(password, path)[1];
                        KeyMold = FileEncryption.UserDataDecrypt(password, path)[0];
                        return UuID;
                    }
                }
                catch (Exception){
                    return null;
                }
            } else{
                return null;
            }
        }


        public static string GetUuid(){
            string path = MyReg.GetValue("Path") + hiddenfolder + userdatafile;
            if (UserExists()){
                try{
                    return UuID;
                }
                catch (Exception){
                    return null;
                }
            } else{
                return null;
            }
        }

        public static string GetKeyMold(){
            string path = MyReg.GetValue("Path") + hiddenfolder + userdatafile;
            if (UserExists()){
                try{
                    return KeyMold;
                }
                catch (Exception){
                    return null;
                }
            } else{
                return null;
            }
        }

        public static bool UserExists(){
            string path = MyReg.GetValue("Path") + hiddenfolder + userdatafile;

            if (File.Exists(path)){
                return true;
            } else{
                return false;
            }
        }

        //Removes userdata file, return false if failed
        public static bool RemoveUser(){
            try{
                string path = MyReg.GetValue("Path") + hiddenfolder + userdatafile;

                File.Delete(path);
                return true;
            }
            catch (Exception){
                return false;
            }
        }
    }
}