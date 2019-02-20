using System;
using System.IO;

namespace CompleteTest{
    internal class Program{
        public static void Main(string[] args){
            string path = "testFile";
            string extension = ".zip";
            string outExtension = extension;
            string password = "40809021";
            string tempFileName = "TempFile";

            Compressor fileToCompress = new Compressor();

            //Compression
            //fileToCompress.compress(path + extension, path);

            //Setup for file encryption
            FileEncryption fileToEncrypt = new FileEncryption("./" + path, extension);
            //Calling the encryption libary, then encrypts it.
            fileToEncrypt.doEncrypt(password);
            //Splits the file in chuncks. Outputs these in Output.
            FileSplitter.splitFile("./" + path + ".aes", "./output", 100048576);
            File.Delete(path + ".aes");

            //Merges the files again
            FileSplitter.mergeFiles("./output", FileSplitter.createMd5(path + ".aes"),
                tempFileName + ".aes");
            //Checks whether was created.
            if (File.Exists(tempFileName + ".aes")){
                FileEncryption fileToDecrypt = new FileEncryption("./" + tempFileName, ".aes");
                //Decrypts the files, after merge.
                fileToDecrypt.doDecrypt(password, path + "-new" + outExtension);
                // fileToCompress.Extract("Output.zip", "./EndResult");

                //Deletes temporary files.
                File.Delete(tempFileName + ".aes");
            } else{
                Console.WriteLine("Error in creating file");
            }
        }
    }
}