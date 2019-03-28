using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
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


            if (File.Exists(inputFilePath)){
                using (Stream input = File.OpenRead(inputFilePath)){
                    int index = 0;
                    while (input.Position < input.Length){
                        var writingBuffer = fileStreamReader(input, chunkSize);
                        Console.WriteLine(writingBuffer.Length);

                        using (Stream output = File.Create(OutputFolderpath + CreateMD5(writingBuffer))){
                            output.Write(writingBuffer, 0, writingBuffer.Length);
                        }

                        _files.Add(CreateMD5(writingBuffer));
                    }
                }
            } else{
                throw new FileNotFoundException();
            }

            return _files;
        }
        
        public List<string> splitFile(String inputFilePath, string OutputFolderpath, int chunkSize,NetworkStream outputStream){
            if (!Directory.Exists(OutputFolderpath)){
                Directory.CreateDirectory(OutputFolderpath);
            }

            //Based on https://stackoverflow.com/questions/3967541/how-to-split-large-files-efficiently


            if (File.Exists(inputFilePath)){
                using (Stream input = File.OpenRead(inputFilePath)){
                    int index = 0;
                    while (input.Position < input.Length){
                        var writingBuffer = fileStreamReader(input, chunkSize);
                        Console.WriteLine(writingBuffer.Length);
                        //TODO PLZ ADD MSG TO ZEND HASH TO RECEIVER
                        outputStream.Write(writingBuffer, 0, writingBuffer.Length);
                        
                        _files.Add(CreateMD5(writingBuffer));
                    }
                }
            } else{
                throw new FileNotFoundException();
            }

            return _files;
        }

        private byte[] fileStreamReader(Stream input, int chunkSize){
            using (MemoryStream ms = new MemoryStream()){
                const int BUFFER_SIZE = 1024;
                byte[] buffer = new byte[BUFFER_SIZE];

                int remaining = chunkSize, bytesRead;
                while (remaining > 0 &&
                       (bytesRead = input.Read(buffer, 0, Math.Min(remaining, BUFFER_SIZE))) > 0){
                    ms.Write(buffer, 0, bytesRead);
                    remaining -= bytesRead;
                }

                return ms.ToArray();
            }
        }

        public void mergeFiles(string inputDir, string outputFilePath, List<string> fileList){
            if (Directory.Exists(inputDir)){
                if (fileList.Count > 0){
                    if (!File.Exists(outputFilePath)){
                        using (File.Create(outputFilePath)){ }
                    }

                    using (Stream output = File.OpenWrite(outputFilePath)){
                        foreach (String chunkPath in fileList){
                            byte[] contentBuffer = File.ReadAllBytes(inputDir + chunkPath);
                            output.Write(contentBuffer, 0, contentBuffer.Length);
                        }
                    }
                }
            } else{
                throw new FileNotFoundException();
            }
        }

        public static string CreateMD5(byte[] input){
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create()){
                byte[] hashBytes = md5.ComputeHash(input);

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