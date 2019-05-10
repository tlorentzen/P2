using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace P2P_lib{
    public class DiskHelper{

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

        public static string CreateMd5(string input){
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create()){
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < hashBytes.Length; i++){
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

        //Function to get a valure from the registry.
        public static string GetRegistryValue(string key){
            //Sets the RegistryKey to the TorPdos registry
            RegistryKey registry = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");
            //If there is no current value in the registry the function will return null
            if (registry.GetValue(key) == null){
                return null;
                //Else it will return the value in a string
            } else{
                return registry.GetValue(key).ToString();
            }
        }

        //Function to set the registry value
        public static void SetRegistryValue(string key){
            RegistryKey registry = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");
            registry?.SetValue("Path", key);
        }
    }
}