﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Encryption;
using P2P_lib.Helpers;

namespace P2P_lib.Handlers{
    public static class IdHandler{
        private const string UserDataFile = "userdata";
        private const string HiddenFolder = @".hidden\";
        private static readonly int iterations = 10000, hashlength = 20, saltlength = 16;
        private static string _keyMold;
        private static string _uuId;

        /// <summary>
        /// Create user file using generated UUID and input password.
        /// UUID can be sent in as input, for a custom UUID.
        /// </summary>
        /// <param name="password">The input password from user.</param>
        /// <param name="uuid">User UUID (if the user already has an UUID).</param>
        /// <returns>UUID</returns>
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

        /// <summary>
        /// Generate keymold (hash) from key.
        /// </summary>
        /// <param name="key1">First string to base keymold off of.</param>
        /// <param name="key2">Second string to base keymold off of.</param>
        /// <returns>Keymold.</returns>
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

        /// <summary>
        /// Generate UUID based on mac addresses and current time.
        /// </summary>
        /// <returns>UUID.</returns>
        private static string GenerateUuid(){
            string guid = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            List<string> macAddresses = NetworkHelper.GetMacAddresses();

            foreach (string mac in macAddresses){
                guid += mac;
            }

            return DiskHelper.CreateMd5(guid);
        }

        /// <summary>
        /// Check if UUID and password match existing local user.
        /// Compare keymolds (hashes).
        /// </summary>
        /// <param name="password">The password input of the user.</param>
        /// <returns>Returns true if user details are valid, false if not.</returns>
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


        /// <summary>
        /// Return UUID if present, else return null.
        /// </summary>
        /// <param name="password"></param>
        /// <returns>UUID if present, else null.</returns>
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

        /// <summary>
        /// Gets the UUID, if the user exists.
        /// </summary>
        /// <returns>UUID if user exists, else null.</returns>
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

        /// <summary>
        /// Gets the keymold, if the user exists.
        /// </summary>
        /// <returns>Keymold if user exists, else null.</returns>
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

        /// <summary>
        /// Checks rather the user exists.
        /// </summary>
        /// <returns>Rather the user exists or not.</returns>
        public static bool UserExists(){
            string path = DiskHelper.GetRegistryValue("Path") + HiddenFolder + UserDataFile;

            if (File.Exists(path)){
                return true;
            } else{
                return false;
            }
        }

        /// <summary>
        /// Removes userdata file.
        /// </summary>
        /// <returns>Rather the removal was successful or not.</returns>
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