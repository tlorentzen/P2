using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Encryption;
using Microsoft.Win32;
using P2P_lib;

namespace P2P_lib{
    public static class IdHandler{
        private const string UserDataFile = "userdata";

        private const string HiddenFolder = @".hidden\";

        private static readonly int iterations = 10000, hashlength = 20, saltlength = 16;
        private static string _keyMold;
        private static string _uuId;

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

                string path = DiskHelper.GetRegistryValue("Path") + HiddenFolder + UserDataFile;

                _keyMold = GenerateKeyMold(uuid, password);
                string output = _keyMold + "\n" + uuid;
                FileEncryption.UserDataEncrypt(password, output, path);

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

            return DiskHelper.CreateMd5(guid);
        }

        //Check if UUID and password match existing local user
        //Compare keymolds (hashes)
        //Returns true if user details are valid, false if not
        public static bool IsValidUser(string password){
            try{
                string path = DiskHelper.GetRegistryValue("Path") + HiddenFolder + UserDataFile;

                FileEncryption.UserDataDecrypt(password, path);
                return true;
            }
            catch (Exception){
                return false;
            }
        }


        //Return UUID if present, else return null
        public static string GetUuid(string password){
            string path = DiskHelper.GetRegistryValue("Path") + HiddenFolder + UserDataFile;
            if (UserExists()){
                try{
                    if (_uuId != null){
                        return _uuId;
                    }
                    string[] userdata = FileEncryption.UserDataDecrypt(password, path);
                    _uuId = userdata[1];
                    _keyMold = userdata[0];
                    return _uuId;
                }
                catch (Exception){
                    //return null;
                    return "Invalid Password";
                }
            } else{
                return null;
            }
        }


        public static string GetUuid(){
            if (UserExists()){
                try{
                    return _uuId;
                }
                catch (Exception){
                    return null;
                }
            } else{
                return null;
            }
        }

        public static string GetKeyMold(){
            if (UserExists()){
                try{
                    return _keyMold;
                }
                catch (Exception){
                    return null;
                }
            } else{
                return null;
            }
        }

        public static bool UserExists(){
            string path = DiskHelper.GetRegistryValue("Path") + HiddenFolder + UserDataFile;

            if (File.Exists(path)){
                return true;
            } else{
                return false;
            }
        }

        //Removes userdata file, return false if failed
        public static bool RemoveUser(){
            try{
                string path = DiskHelper.GetRegistryValue("Path") + HiddenFolder + UserDataFile;

                File.Delete(path);
                return true;
            }
            catch (Exception){
                return false;
            }
        }
    }
}