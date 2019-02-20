using System;

namespace Encryption{
    internal static class Program{
        public static void Main(string[] args){
            string Path = "./EncryptMe";
            string Extension = ".png";
            FileEncryption File = new FileEncryption(Path,Extension);
            Console.WriteLine("Please enter your username");
            var password = Console.ReadLine();
            Console.WriteLine("Encryption started");
            File.doEncrypt(password);
            Console.WriteLine("Encryption done");
            Console.WriteLine("Decryption Started");
            File.doDecrypt(password);
            Console.WriteLine("Encryption done");
        }
    }
}