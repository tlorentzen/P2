using System;
using System.IO;
using System.Security.Cryptography;
using FileCompression;

namespace CompleteTest{
    internal class Program{
        public static void Main(string[] args){
            string path = "testFile";
            string extension = ".exe";
            string outExtension = extension;
            string username;
            string password;
            string hash;
            string tempFileName = "TempFile";
            int input;

            ByteCompressor compress = new ByteCompressor();

            
            Console.WriteLine("Please enter your username. (Case sensitive)");
            username = Console.ReadLine();
            Console.WriteLine("Please enter your password. (Case sensitive)");
            password = Console.ReadLine();

            hash = (username + password);
            
            Console.WriteLine("Do you want to encrypt (1) or decrypt (2)");
            input = Convert.ToInt16(Console.ReadLine());
            switch (input){
                case 1:
                    //Compression
                    compress.CompressFile(path+extension,path+".lzma");


                    //Setup for file encryption
                    FileEncryption fileToEncrypt = new FileEncryption("./" + path, ".lzma");
                    //Calling the encryption libary, then encrypts it.
                    fileToEncrypt.doEncrypt(hash);
                    
                    
                    //Splits the file in chuncks. Outputs these in Output.
                    FileSplitter.splitFile("./" + path + ".aes", "./output", 100048576);
                    File.Delete(path + ".aes");
                    break;

                case 2:
                    //Merges the files again
                    FileSplitter.mergeFiles("./output", FileSplitter.createMd5(path + ".aes"),
                        tempFileName + ".lzma");
                    //Checks whether was created.
                    if (File.Exists(tempFileName + ".aes")){
                        FileEncryption fileToDecrypt = new FileEncryption("./" + tempFileName, ".aes");
                       
                        //Decrypts the files, after merge.
                        fileToDecrypt.doDecrypt(hash, path + "-new" + outExtension);
                        
                        compress.DecompressFile(path+".lzma",path+outExtension);


                        //Deletes temporary files.
                        File.Delete(tempFileName + ".aes");
                    } else{
                        Console.WriteLine("Error in creating file");
                    }

                    break;

                default:
                    Console.WriteLine("Not a valid option, terminating software");
                    break;
            }
        }
    }
}