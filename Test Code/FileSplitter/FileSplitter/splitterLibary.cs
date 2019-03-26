using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileSplitter{
    public class splitterLibary{
        private List<string> _files = new List<string>();
        Random rnd = new Random();

        public List<string> splitFile(String inputFilePath, string OutputFolderpath, int chunkSize){
            if (!Directory.Exists(OutputFolderpath)){
                Directory.CreateDirectory(OutputFolderpath);
            }

            // https://stackoverflow.com/questions/3967541/how-to-split-large-files-efficiently
            const int BUFFER_SIZE = 1024;
            byte[] buffer = new byte[BUFFER_SIZE];

            if (File.Exists(inputFilePath)){
                String fileHash = CreateMD5(Path.GetFileName(inputFilePath));

                using (Stream input = File.OpenRead(inputFilePath)){
                    int index = 0;
                    string filehashName = CreateMD5(fileHash + rnd.Next(1, 8000));
                    using (Stream output = File.Create(OutputFolderpath + filehashName)){
                        while (input.Position < input.Length){
                            int remaining = chunkSize, bytesRead;
                            while (remaining > 0 &&
                                   (bytesRead = input.Read(buffer, 0, Math.Min(remaining, BUFFER_SIZE))) > 0){
                                output.Write(buffer, 0, bytesRead);
                                remaining -= bytesRead;
                            }

                            _files.Add(filehashName);
                        }
                    }
                }
            } else{
                throw new FileNotFoundException();
            }

            return _files;
        }

        public void mergeFiles(string inputDir, string outputFilePath, List<string> filelist){
            if (Directory.Exists(inputDir)){
                if (filelist.Count > 0){
                    if (!File.Exists(outputFilePath)){
                        using (File.Create(outputFilePath)){ }
                    }

                    using (Stream output = File.OpenWrite(outputFilePath)){
                        foreach (String chunkPath in filelist){
                            byte[] contentBuffer = File.ReadAllBytes(inputDir + chunkPath);
                            output.Write(contentBuffer, 0, contentBuffer.Length);
                        }
                    }
                }
            } else{
                throw new FileNotFoundException();
            }
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