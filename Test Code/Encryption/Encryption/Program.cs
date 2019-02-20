namespace Encryption{
    internal class Program{
        public static void Main(string[] args){
            string Path = "./EncryptMe";
            string Extension = ".png";
            string password = "400EncryptionMe";
            FileEncryption File = new FileEncryption(Path,Extension);
            File.DoEncrypt(password);
            File.DoDecrypt(password);
        }
    }
}