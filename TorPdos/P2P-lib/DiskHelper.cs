using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace P2P_lib{
    public class DiskHelper{
        public static long GetTotalFreeSpace(string driveName){
            foreach (DriveInfo drive in DriveInfo.GetDrives()){
                if (drive.IsReady && drive.Name == driveName){
                    return drive.TotalFreeSpace;
                }
            }

            return -1;
        }

        public static string CreateMD5(string input){
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
    }
}