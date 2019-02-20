using System;
using System.IO;

namespace CompleteTest{
    internal class Program{
        public static void Main(string[] args){
            string path = "result";
            string extension = ".zip";
            string password = "40809021";

            FileEncryption fileToEncrypt = new FileEncryption("./" + path, extension);
            Console.WriteLine(fileToEncrypt.Path);
            fileToEncrypt.doEncrypt(password);
            FileSplitter.SplitFile(path + ".aes", "./output", 100048576);

            FileSplitter.MergeFiles("./output", FileSplitter.CreateMD5(path + ".aes"),
                "Newfile.aes");
            if (File.Exists("Newfile.aes")){
                FileEncryption fileToDecrypt = new FileEncryption("./Newfile", ".aes");
                fileToDecrypt.doDecrypt(password, "./Output" + extension);
            } else{
                Console.WriteLine("Error in creating file");
            }
        }
    }
}