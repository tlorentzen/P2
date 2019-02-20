using System;
using System.IO;
using System.Security.Cryptography;

namespace Encryption{
    public class FileEncryption{
        private string _path;
        private string _extension;
        //Sets Buffersize for encryption and decryption.
        private const int BUFFERSIZE = 100048576;

        private string Path{
            get{ return _path; }
            set{ _path = value; }
        }

        private string Extension{
            get{ return _extension; }
            set{ _extension = value; }
        }

        public FileEncryption(string path, string extension){
            _path = path;
            _extension = extension;
        }

        public void doEncrypt(string password){
            //Uses the GetSalt function to create the salt for the encryption.
            byte[] salt = getSalt();


            //The encrypted output file.
            FileStream fsCrypt = new FileStream(this._path + ".aes", FileMode.Create);

            //Converts password into bytes
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            //Set up AES for encryption
            RijndaelManaged AES = new RijndaelManaged();

            //Keysize is the 
            AES.KeySize = 256;
            AES.BlockSize = 128;

            //Padding modes helps mask the length of the plain text.
            AES.Padding = PaddingMode.PKCS7;

            //Password, used for encryption key of the file.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            
            //Ciphermode helps mask potential patterns within the encrypted text.
            AES.Mode = CipherMode.CFB;

            //Adds the random salt to the start of the output file.
            fsCrypt.Write(salt, 0, salt.Length);

            //Runs through the file using CryptoStream
            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            //The input stream, this is the input file, based on the inputs given in the file creation.
            FileStream fsIn = new FileStream(this._path + this._extension, FileMode.Open);

            //Buffer on 1 mb
            byte[] buffer = new byte[BUFFERSIZE];


            //Tries and catches regarding opening and reading file
            try{
                int read;
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0){
                    cs.Write(buffer, 0, read);
                }

                fsIn.Close();
            }
            catch (Exception e){
                Console.WriteLine(e);
                throw;
            }
            finally{
                cs.Close();
                fsCrypt.Close();
            }
        }

        public void doDecrypt(string password){
            //Setup to read the salt from the start of the file
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[64];


            //Reading through the file
            FileStream fsCrypt = new FileStream(this._path + ".aes", FileMode.Open);

            //Reads the random salt of the file.
            fsCrypt.Read(salt, 0, salt.Length);


            //Opening a new instance of Rijandeal AES
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;

            //Takes the password, and verifies it towards the salt, read from the start of the file.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            
            //The padding is used to make it harder to see the length of the encrypted text.
            AES.Padding = PaddingMode.PKCS7;
            
            //Cipher mode is a way to mask potential patterns within the encrypted text, to make it harder to decrypt.
            AES.Mode = CipherMode.CFB;
            
            //Runs through the encrypted files, and decrypts it using AES.
            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
            
            //Creates the output file
            FileStream fsOut = new FileStream("./Output" + this._extension, FileMode.Create);

            byte[] buffer = new byte[BUFFERSIZE];

            //Outputs the read file into the output file.
            try{
                int read;
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0){
                    fsOut.Write(buffer, 0, read);
                }
            }
            catch (Exception e){
                Console.WriteLine(e);
                throw;
            }
            finally{
                fsOut.Close();
                fsCrypt.Close();
            }
        }

        private static byte[] getSalt(){
            byte[] data = new byte[64];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()){
                for (int i = 0; i < 10; i++){
                    rng.GetBytes(data);
                }
            }

            return data;
        }
    }
}