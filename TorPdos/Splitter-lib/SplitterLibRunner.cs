using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Splitter_lib{


    public class SplitterLibrary{

        /// <summary>
        /// Split a file into chunks of specified sizes.
        /// </summary>
        /// <param name="inputFilePath">Path to file that needs splitting</param>
        /// <param name="inputFileHash">File hash</param>
        /// <param name="outputFolderPath">Path to store chunks</param>
        /// <param name="chunkSize">Chunksize</param>
        /// <returns>A list of chunk hashes</returns>
        public List<string> SplitFile(string inputFilePath, string inputFileHash, string outputFolderPath,
            int chunkSize = 1000000){
            //If the folder for the chunk files do not exist make said folder
            if (!Directory.Exists(outputFolderPath)){
                Directory.CreateDirectory(outputFolderPath);
            }

            List<string> currentFiles = new List<string>();

            // https://stackoverflow.com/questions/3967541/how-to-split-large-files-efficiently

            //If the file to be splitted exists it will begin splitting the file and hashing it
            if (File.Exists(inputFilePath)){
                using (Stream input = File.OpenRead(inputFilePath)){
                    while (input.Position < input.Length){
                        var writingBuffer = fileStreamReader(input, chunkSize);
                        Console.WriteLine(writingBuffer.Length);

                        using (Stream output = File.Create(outputFolderPath + CreateMD5(writingBuffer))){
                            output.Write(writingBuffer, 0, writingBuffer.Length);
                        }

                        currentFiles.Add(CreateMD5(writingBuffer));
                    }
                }
            } else{
                throw new FileNotFoundException();
            }

            return currentFiles;
        }

        /// <summary>
        /// Helper function which opens a memorystream and reads it into a buffer and returns an array
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <param name="chunkSize">Chunksize</param>
        /// <returns></returns>
        private byte[] fileStreamReader(Stream input, int chunkSize){
            using (MemoryStream ms = new MemoryStream()){
                const int BufferSize = 1024;
                byte[] buffer = new byte[BufferSize];

                int remaining = chunkSize, bytesRead;
                while (remaining > 0 &&
                       (bytesRead = input.Read(buffer, 0, Math.Min(remaining, BufferSize))) > 0){
                    ms.Write(buffer, 0, bytesRead);
                    remaining -= bytesRead;
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Function to merge the files when downloaded from the network in chunks
        /// Takes an input directory where all the split files are and puts them in the output filepath.
        /// </summary>
        /// <param name="inputDir">Path to folder containing the file chunks</param>
        /// <param name="outputFilePath">Path where the merged file should be outputted</param>
        /// <param name="fileList">List of files</param>
        /// <returns>Returns true if file was merged successfully false if not</returns>
        public void MergeFiles(string inputDir, string outputFilePath, List<string> fileList){
            if (Directory.Exists(inputDir)){
                if (fileList.Count > 0){
                    if (!File.Exists(outputFilePath)){
                        using (File.Create(outputFilePath)){ }
                    }

                    using (Stream output = File.OpenWrite(outputFilePath)){
                        foreach (string chunkPath in fileList){
                            byte[] contentBuffer = File.ReadAllBytes(inputDir + chunkPath);
                            output.Write(contentBuffer, 0, contentBuffer.Length);
                        }
                    }
                }
            } else{
                throw new FileNotFoundException();
            }
        }

        /// <summary>
        /// Generate MD5 Hash based on byte array.
        /// </summary>
        /// <param name="input">The byte array</param>
        /// <returns>MD5 hash as string</returns>
        private string CreateMD5(byte[] input){
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