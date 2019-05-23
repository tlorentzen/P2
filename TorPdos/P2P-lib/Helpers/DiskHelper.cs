using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace P2P_lib.Helpers{
    public static class DiskHelper{

        /// <summary>
        /// Get how much space is available on the chosen drive
        /// </summary>
        /// <param name="driveName">The name of the drive to be checked</param>
        /// <returns>Returns amount of space available on the drive</returns>
        public static long GetTotalAvailableSpace(string driveName){
            driveName = driveName.Split('\\')[0] + '\\';
            foreach (DriveInfo drive in DriveInfo.GetDrives()){
                if (drive.IsReady && drive.Name == driveName){
                    long space = drive.TotalFreeSpace - (long)(drive.TotalSize * 0.2);
                    return space > 0 ? space : 0;
                }
            }

            return -1;
        }

        /// <summary>
        /// This returns a MD5 hash of the given string input.
        /// </summary>
        /// <param name="input">The input for which to return the Hash</param>
        /// <returns></returns>
        public static string CreateMd5(string input){
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create()){
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();

                foreach (var currentByte in hashBytes){
                    sb.Append(currentByte.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// This allows for timestamps being added to console output.
        /// Use the same way as Console.WriteLine
        /// </summary>
        /// <param name="message">The message to get printed.</param>
        public static void ConsoleWrite(string message){
            Console.WriteLine(DateTime.Now +" "+ message);
        }

        /// <summary>
        /// Function to get a value from the registry.
        /// </summary>
        /// <param name="key">The key from the registry we want to work with</param>
        /// <returns>Returns the value stored in the key</returns>
        public static string GetRegistryValue(string key){
            RegistryKey registry = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");
            return registry?.GetValue(key) == null ? null : registry.GetValue(key).ToString();
        }

        /// <summary>
        /// Set value in registry
        /// </summary>
        /// <param name="key">The key which has to be saved</param>
        /// <param name="value">The value which has to be saved in the key</param>
        public static void SetRegistryValue(string key, string value){
            RegistryKey registry = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");
            registry?.SetValue(key, value);
        }
    }
}