using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Encryption{
    public class FileEncryption{
        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("EncryptionLogging");

        //Sets Buffersize for encryption and decryption.
        private const int BufferSize = 100048576;

        private string Path{ get; set; }

        private string Extension{ get; set; }

        public FileEncryption(string path, string extension){
            this.Path = path;
            Extension = extension;
        }

        /// <summary>
        /// Function to encrypt files
        /// </summary>
        /// <param name="password">Keymold from our hashes of UUID and user password</param>
        /// <returns>Returns true if encryption is succesful and false if not</returns>
        public bool DoEncrypt(string password){
            //Uses the GetSalt function to create the salt for the encryption.
            var salt = GetSalt();


            //The encrypted output file.
            using (FileStream fsCrypt = new FileStream(Path + ".aes", FileMode.Create)){
                //Converts password into bytes
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                //Set up AES for encryption
                RijndaelManaged aes = new RijndaelManaged{KeySize = 256, BlockSize = 128, Padding = PaddingMode.PKCS7};


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
                            byte[] buffer = new byte[BufferSize];


                            //Tries and catches regarding opening and reading file
                            try{
                                int read;
                                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0){
                                    cs.Write(buffer, 0, read);
                                }
                            }
                            catch (Exception e){
                                Logger.Fatal(e);
                                return false;
                            }
                            finally{
                                fsIn.Close();
                            }
                        }
                    }
                    catch (FileNotFoundException e){
                        Logger.Fatal(e);
                        return false;
                    }
                    catch (Exception e){
                        Logger.Warn(e);
                        return false;
                    }
                    cs.Flush();
                    cs.Close();
                }

                fsCrypt.Close();
            }

            return true;
        }

        /// <summary>
        /// Function to decrypt 
        /// </summary>
        /// <param name="password">Keymold from our hashes of UUID and user password</param>
        public bool DoDecrypt(string password){
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
                        byte[] buffer = new byte[BufferSize];

                        //Outputs the read file into the output file.
                        try{
                            int read;
                            while ((read = cs.Read(buffer, 0, buffer.Length)) > 0){
                                fsOut.Write(buffer, 0, read);
                            }
                        }
                        catch (Exception e){
                            Logger.Fatal(e);
                            return false;
                        }
                        finally{
                            fsOut.Close();
                        }
                    }
                    cs.Flush();
                    cs.Close();
                }

                fsCrypt.Close();
            }

            return true;
        }

        /// <summary>
        /// Decrypt the Userdata file so it is readable by the program
        /// </summary>
        /// <param name="password">The user's password</param>
        /// <param name="path">Where the Userdata file is stored</param>
        /// <returns></returns>
        public static string[] UserDataDecrypt(string password, string path){
            //Setup to read the salt from the start of the file
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[64];
            string[] output = null;

            //Reading through the file
            using (FileStream fsCrypt =
                new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
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
                    byte[] buffer = new byte[BufferSize];

                    using (var fileRead = new MemoryStream()){
                        //Outputs the read file into the output file.
                        try{int read;
                            while ((read = cs.Read(buffer, 0, buffer.Length)) > 0){
                                fileRead.Write(buffer, 0, read);
                            }

                            var result = Encoding.UTF8.GetString(fileRead.ToArray());
                            Console.WriteLine("Complete result:" + result);
                            output = result.Split('\n');
                            Console.WriteLine("UUID Check: "+ output[1]);
                            Console.WriteLine("KeyLog Check: "+ output[0]);
                        }
                        catch (Exception e){
                            Logger.Fatal(e);
                        }
                        finally{
                            cs.Flush();
                        }
                    }

                    cs.Close();
                }
            }

            return output;
        }

        /// <summary>
        /// Encrypts the userdata
        /// </summary>
        /// <param name="password">User password</param>
        /// <param name="fileInformation">Is the data in the userfile</param>
        /// <param name="path">Path to where the userdata is located</param>
        public static void UserDataEncrypt(string password, string fileInformation, string path){
            //Uses the GetSalt function to create the salt for the encryption.
            var salt = GetSalt();


            //The encrypted output file.
            using (FileStream fsCrypt = new FileStream(path, FileMode.Create)){
                //Converts password into bytes
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                //Set up AES for encryption
                RijndaelManaged aes = new RijndaelManaged{KeySize = 256, BlockSize = 128, Padding = PaddingMode.PKCS7};


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
                        var buffer = Encoding.UTF8.GetBytes(fileInformation);
                        //Tries and catches regarding opening and reading file
                        cs.Write(buffer, 0, buffer.Length);
                    }
                    catch (Exception e){
                        Logger.Fatal(e);
                    }

                    cs.Flush();
                    cs.Close();
                }

                fsCrypt.Close();
            }
        }

        /// <summary>
        /// Fetches the salt from the log
        /// </summary>
        /// <returns></returns>
        private static byte[] GetSalt(){
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