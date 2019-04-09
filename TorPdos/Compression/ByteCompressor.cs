﻿using System;
using System.IO;
using System.Text;
using SevenZip.Compression.LZMA;

namespace Compression{
    public class ByteCompressor{
        public static byte[] CompressBytes(byte[] inData){
            Console.WriteLine("Original data is {0} bytes", inData.Length);
            //Compress byte array
            byte[] Compressed = SevenZipHelper.Compress(inData);
            Console.WriteLine("Compressed data is {0} bytes", Compressed.Length);

            return Compressed;
        }

        public static byte[] DecompressBytes(byte[] inData){
            Console.WriteLine("Compressed data is {0} bytes", inData.Length);
            //Decompress byte array
            byte[] Decompressed = SevenZipHelper.Decompress(inData);
            Console.WriteLine("Decompressed data is {0} bytes", Decompressed.Length);

            return Decompressed;
        }

        public static void CompressFile(string inPath, string outPath){
            int pakg = 1;

            if (File.Exists(inPath)){
                //Add .lzma extension to output file, if no other extension provided
                if (!Path.HasExtension(outPath)){
                    outPath = outPath + ".lzma";
                }

                if (!File.Exists(outPath)){
                    //Buffersize 128 MB
                    const long BUFFER_SIZE = 1024 * 1024 * 128;

                    using (FileStream file =
                        new FileStream(inPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                        long fileSize = file.Length;
                        Console.WriteLine(fileSize);

                        if (fileSize > BUFFER_SIZE){
                            pakg = (int) (fileSize / BUFFER_SIZE);
                        }
                        file.Close();
                    }


                    using (FileStream inStream =
                        new FileStream(inPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                        using (FileStream outStream = new FileStream(outPath, FileMode.Create, FileAccess.Write,
                            FileShare.ReadWrite)){
                            // Save file extension
                            string ext = Path.GetExtension(inPath);
                            ext = ext.Length + ext;
                            outStream.Write(Encoding.ASCII.GetBytes(ext), 0, ext.Length);

                            long remaining = inStream.Length - inStream.Position;
                            int Compressed = 1;
                            //Compress file 
                            while (remaining > 0){
                                //Read buffer from file
                                int BytesToRead = (int) (remaining > BUFFER_SIZE ? BUFFER_SIZE : remaining);
                                byte[] buffer = new byte[BytesToRead];
                                int BytesRead = inStream.Read(buffer, 0, BytesToRead);

                                if (BytesRead != BytesToRead){
                                    //throw exception
                                    Console.WriteLine("Woopsie :)");
                                }
                                else{
                                    //Compress buffer and write to outfile
                                    Console.WriteLine($"Compressing {Compressed} out of {pakg}");
                                    byte[] compressed = CompressBytes(buffer);
                                    outStream.Write(compressed, 0, compressed.Length);
                                    Compressed++;
                                }

                                remaining = inStream.Length - inStream.Position;
                            }
                            outStream.Close();
                        }
                        inStream.Close();
                    }
                }
                else{
                    //Outfile already exists, add 2 to name
                    CompressFile(inPath, Path.GetFileNameWithoutExtension(outPath) + "2" + Path.GetExtension(outPath));
                }
            }
            else{
                Console.WriteLine("The file you wish to compress does not exist");
            }
        }

        public static void DecompressFile(string inPath, string outPath){
            //Add .lzma to infile, if extesion not provided
            if (!Path.HasExtension(inPath)){
                inPath = inPath + ".lzma";
            }

            if (File.Exists(inPath)){
                using (Stream inStream = File.OpenRead(inPath)){
                    // Read file extension - Kan måske gøres simplere
                    byte[] extbyte = new byte[1];
                    extbyte[0] = (byte) inStream.ReadByte();
                    int extlen = Convert.ToInt32(Encoding.ASCII.GetString(extbyte));
                    byte[] extbuf = new byte[extlen];
                    inStream.Read(extbuf, 0, extlen);
                    string ext = Encoding.ASCII.GetString(extbuf);
                    if (!Path.HasExtension(outPath) || Path.GetExtension(outPath) != ext){
                        outPath = outPath + ext;
                    }

                    if (!File.Exists(outPath)){
                        //Readbuffer 128MB
                        const int BUFFER_SIZE = 1024 * 1024 * 128;

                        using (Stream outStream = File.Create(outPath)){
                            long remaining = inStream.Length - inStream.Position;
                            //Decompress file
                            while (remaining > 0){
                                //Read buffer from file
                                int BytesToRead = (int) (remaining > BUFFER_SIZE ? BUFFER_SIZE : remaining);
                                byte[] buffer = new byte[BytesToRead];
                                int BytesRead = inStream.Read(buffer, 0, BytesToRead);

                                if (BytesRead != BytesToRead){
                                    //throw exception
                                    Console.WriteLine("Woopsie :)");
                                }
                                else{
                                    //Decompress buffer
                                    byte[] decompressed = DecompressBytes(buffer);
                                    outStream.Write(decompressed, 0, decompressed.Length);
                                }

                                remaining = inStream.Length - inStream.Position;
                            }
                        }
                    }
                    else{
                        //Appends 2 to tile name if file already exists
                        DecompressFile(inPath,
                            Path.GetFileNameWithoutExtension(outPath) + "2" + Path.GetExtension(outPath));
                    }
                }
            }
            else{
                Console.WriteLine("The file you wish to decompress does not exist");
            }
        }
    }
}