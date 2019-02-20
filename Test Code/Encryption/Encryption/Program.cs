using System;

namespace Encryption{
    internal static class Program{
        public static void Main(string[] args){
            string Path = "./EncryptMe";
            string Extension = ".png";
            int input;
            FileEncryption File = new FileEncryption(Path, Extension);
            Console.WriteLine("Please enter your username");
            var password = Console.ReadLine();
            Console.WriteLine("Do you want to encrypt(1), or decrypt(2)?");
            try{input = Convert.ToInt16(Console.ReadLine()); }
            catch (Exception e){
                Console.WriteLine("That was not a number");
                Console.WriteLine(e);
                throw;
            }
            switch (input){
                case 1:
                    Console.WriteLine("Encryption started");
                    File.doEncrypt(password);
                    Console.WriteLine("Encryption done");
                    break;
                case 2:

                    Console.WriteLine("Decryption Started");
                    File.doDecrypt(password);
                    Console.WriteLine("Encryption done");
                    break;
                default:
                    Console.WriteLine("Not an option");
                    break;
            }
        }
    }
}