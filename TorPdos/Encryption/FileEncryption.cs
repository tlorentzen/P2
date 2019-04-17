using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Encryption{
    public class FileEncryption{
        private static NLog.Logger logger = NLog.LogManager.GetLogger("EncryptionLogging");

        //Sets Buffersize for encryption and decryption.
        private const int BUFFERSIZE = 100048576;

        private string Path{ get; set; }

        private string Extension{ get; set; }

        public FileEncryption(string path, string extension){
            Path = path;
            Extension = extension;
        }

        public void doEncrypt(string password){
            //Uses the GetSalt function to create the salt for the encryption.
            byte[] salt = getSalt();


            //The encrypted output file.
            using (FileStream fsCrypt = new FileStream(Path + ".aes", FileMode.Create)){
                //Converts password into bytes
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                //Set up AES for encryption
                RijndaelManaged aes = new RijndaelManaged();

                //Keysize is the 
                aes.KeySize = 256;
                aes.BlockSize = 128;

                //Padding modes helps mask the length of the plain text.
                aes.Padding = PaddingMode.PKCS7;

                //Password, used for encryption key of the file.
                var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);

                //Ciphermode helps mask potential patterns within the encrypted text.
                aes.Mode = CipherMode.CFB;

                //Adds the random salt to the start of the output file.
                fsCrypt.Write(salt, 0, salt.Length);

                //Runs through the file using CryptoStream
                using (CryptoStream cs = new CryptoStream(fsCrypt, aes.CreateEncryptor(), CryptoStreamMode.Write)){
                    try{
                        //The input stream, this is the input file, based on the inputs given in the file creation.
                        using (FileStream fsIn = new FileStream(Path + Extension, FileMode.Open, FileAccess.Read,
                            FileShare.ReadWrite)){
                            //Buffer on 1 mb
                            byte[] buffer = new byte[BUFFERSIZE];


                            //Tries and catches regarding opening and reading file
                            try{
                                int read;
                                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0){
                                    cs.Write(buffer, 0, read);
                                }
                            }
                            catch (Exception e){
                                logger.Fatal(e);
                            }
                            finally{
                                fsIn.Close();
                            }
                        }
                    }
                    catch (FileNotFoundException e){
                        logger.Fatal(e);
                    }
                    catch (Exception e){
                        logger.Warn(e);
                    }

                    cs.Close();
                }

                fsCrypt.Close();
            }
        }

        public void doDecrypt(string password){
            //Setup to read the salt from the start of the file
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[64];


            //Reading through the file
            using (FileStream fsCrypt =
                new FileStream(Path + ".aes", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                //Reads the random salt of the file.
                fsCrypt.Read(salt, 0, salt.Length);


                //Opening a new instance of Rijandeal AES
                RijndaelManaged aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 128;

                //Takes the password, and verifies it towards the salt, read from the start of the file.
                var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);

                //The padding is used to make it harder to see the length of the encrypted text.
                aes.Padding = PaddingMode.PKCS7;

                //Cipher mode is a way to mask potential patterns within the encrypted text, to make it harder to decrypt.
                aes.Mode = CipherMode.CFB;

                //Runs through the encrypted files, and decrypts it using AES.
                using (CryptoStream cs = new CryptoStream(fsCrypt, aes.CreateDecryptor(), CryptoStreamMode.Read)){
                    //Creates the output file
                    using (FileStream fsOut = new FileStream(Path + Extension, FileMode.Create)){
                        byte[] buffer = new byte[BUFFERSIZE];

                        //Outputs the read file into the output file.
                        try{
                            int read;
                            while ((read = cs.Read(buffer, 0, buffer.Length)) > 0){
                                fsOut.Write(buffer, 0, read);
                            }
                        }
                        catch (Exception e){
                            logger.Fatal(e);
                        }
                        finally{
                            fsOut.Close();
                        }
                    }

                    cs.Close();
                }

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