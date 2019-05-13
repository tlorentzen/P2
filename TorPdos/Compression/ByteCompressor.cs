using System;
using System.IO;
using System.Text;

namespace Compression{
    
    public class Compressor{
        private static NLog.Logger logger = NLog.LogManager.GetLogger("CompressionLogger");

        //Function to compess bytes using SevenZipHelper
        public static byte[] CompressBytes(byte[] inData){
            Console.WriteLine("Original data is {0} bytes", inData.Length);
            //Compress byte array
            byte[] compressed = SevenZipHelper.Compress(inData);
            Console.WriteLine("Compressed data is {0} bytes", compressed.Length);

            return compressed;
        }

        //Function to decompress bytes using SevenZipHelper
        public static byte[] DecompressBytes(byte[] inData){
            Console.WriteLine("Compressed data is {0} bytes", inData.Length);
            //Decompress byte array
            byte[] decompressed = SevenZipHelper.Decompress(inData);
            Console.WriteLine("Decompressed data is {0} bytes", decompressed.Length);

            return decompressed;
        }

        public static bool CompressFile(string inPath, string outPath){
            int pakg = 1;

            if (File.Exists(inPath)){
                try{
                    //Add .lzma extension to output file, if no other extension provided
                    if (!Path.HasExtension(outPath)){
                        outPath = outPath + ".lzma";
                    }

                    if (!File.Exists(outPath)){
                        //Buffersize 128 MB
                        const long BufferSize = 1024 * 1024 * 128;

                        using (var file = new FileStream(inPath, FileMode.Open, FileAccess.Read,
                            FileShare.Read)){
                            long fileSize = file.Length;

                            if (fileSize > BufferSize){
                                pakg = (int) (fileSize / BufferSize);
                            }

                            file.Close();
                        }


                        using (var inStream =
                            new FileStream(inPath, FileMode.Open, FileAccess.Read, FileShare.Read)){
                            using (var outStream = new FileStream(outPath, FileMode.Create, FileAccess.Write)){
                                // Save file extension
                                string ext = Path.GetExtension(inPath);
                                ext = ext.Length + ext;
                                outStream.Write(Encoding.ASCII.GetBytes(ext), 0, ext.Length);

                                long remaining = inStream.Length - inStream.Position;
                                int compressed = 1;
                                //Compress file 
                                while (remaining > 0){
                                    //Read buffer from file
                                    int bytesToRead = (int) (remaining > BufferSize ? BufferSize : remaining);
                                    byte[] buffer = new byte[bytesToRead];
                                    int bytesRead = inStream.Read(buffer, 0, bytesToRead);

                                    if (bytesRead != bytesToRead){
                                        //throw exception
                                        //TODO throw an actual exception
                                        Console.WriteLine(@"Woopsie :)");
                                    } else{
                                        //Compress buffer and write to outfile
                                        Console.WriteLine($"Compressing {compressed} out of {pakg}");
                                        byte[] compressedBytes = CompressBytes(buffer);
                                        outStream.Write(compressedBytes, 0, compressedBytes.Length);
                                        compressed++;
                                    }

                                    remaining = inStream.Length - inStream.Position;
                                }

                                outStream.Close();
                            }

                            inStream.Close();
                        }
                    } else{
                        //Outfile already exists, add 2 to name
                        CompressFile(inPath,
                            Path.GetFileNameWithoutExtension(outPath) + "2" + Path.GetExtension(outPath));
                    }
                }
                catch (Exception e){
                    logger.Error(e);
                    return false;
                }
            }
            else{
                logger.Fatal(new FileNotFoundException());
                return false;
            }

            return true;
        }

        public static string DecompressFile(string inPath, string outPath){
            //Add .lzma to infile, if extesion not provided
            if (!Path.HasExtension(inPath)){
                inPath += ".lzma";
            }

            if (File.Exists(inPath)){
                try{
                    using (Stream inStream = File.OpenRead(inPath)){
                        // Read file extension - Kan måske gøres simplere
                        byte[] extbyte = new byte[1];
                        extbyte[0] = (byte) inStream.ReadByte();
                        int extlen = Convert.ToInt32(Encoding.ASCII.GetString(extbyte));
                        byte[] extbuf = new byte[extlen];
                        inStream.Read(extbuf, 0, extlen);
                        string ext = Encoding.ASCII.GetString(extbuf);
                        if (!Path.HasExtension(outPath) || Path.GetExtension(outPath) != ext){
                            outPath += ext;
                        }

                        if (!File.Exists(outPath)){
                            //Readbuffer 128MB
                            const int BufferSize = 1024 * 1024 * 128;

                            using (Stream outStream = File.Create(outPath)){
                                long remaining = inStream.Length - inStream.Position;
                                //Decompress file
                                while (remaining > 0){
                                    //Read buffer from file
                                    int bytesToRead = (int) (remaining > BufferSize ? BufferSize : remaining);
                                    byte[] buffer = new byte[bytesToRead];
                                    int bytesRead = inStream.Read(buffer, 0, bytesToRead);

                                    if (bytesRead != bytesToRead){
                                        //throw exception
                                        //TODO throw an actual exception
                                        Console.WriteLine(@"Woopsie :)");
                                    } else{
                                        //Decompress buffer
                                        byte[] decompressed = DecompressBytes(buffer);
                                        outStream.Write(decompressed, 0, decompressed.Length);
                                    }

                                    remaining = inStream.Length - inStream.Position;
                                }

                                outStream.Close();
                            }
                        } else{
                            //Appends 2 to tile name if file already exists
                            DecompressFile(inPath,
                                Path.GetFileNameWithoutExtension(outPath) + "2" + Path.GetExtension(outPath));
                        }

                        inStream.Close();
                    }
                }
                catch(Exception e){
                    logger.Error(e);
                }
            }
            else{
                logger.Fatal(new FileNotFoundException());
            }
            return outPath;
        }
    }
}