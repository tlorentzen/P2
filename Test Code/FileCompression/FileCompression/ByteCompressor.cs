using System;
using System.IO;

namespace FileCompression
{
    public class ByteCompressor
    {
        public ByteCompressor()
        {
        }

        public byte[] CompressBytes(byte[] inData)
        {
            Console.WriteLine("Original data is {0} bytes", inData.Length);
            byte[] Compressed = SevenZip.Compression.LZMA.SevenZipHelper.Compress(inData);
            Console.WriteLine("Compressed data is {0} bytes", Compressed.Length);

            return Compressed;

        }

        public byte[] DecompressBytes(byte[] inData)
        {
            Console.WriteLine("Compressed data is {0} bytes", inData.Length);

            byte[] Decompressed = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(inData);
            Console.WriteLine("Decompressed data is {0} bytes", Decompressed.Length);

            return Decompressed;
        }

        public void CompressFile(string inPath, string outPath)
        {
            if (FileExists(inPath))
            {
                if (!FileExists(outPath))
                {
                    const int BUFFER_SIZE = 1024 * 1024 * 12;

                    using (Stream inStream = File.OpenRead(inPath))
                    {
                        using (Stream outStream = File.Create(outPath))
                        {
                            long remaining = inStream.Length - inStream.Position;
                            while (remaining > 0)
                            {

                                int BytesToRead = (int)(remaining > BUFFER_SIZE ? BUFFER_SIZE : remaining);
                                byte[] buffer = new byte[BytesToRead];
                                int BytesRead = inStream.Read(buffer, 0, BytesToRead);
                                if (BytesRead != BytesToRead)
                                {
                                    //throw exception
                                    Console.WriteLine("Woopsie :)");
                                }
                                else
                                {
                                    byte[] compressed = CompressBytes(buffer);
                                    outStream.Write(compressed, 0, compressed.Length);
                                }
                                remaining = inStream.Length - inStream.Position;
                            }
                        }
                    }
                }
                else
                {
                    CompressFile(inPath, outPath + "2");
                }
            }
            else
            {
                Console.WriteLine("The file you wish to compress does not exist");
            }
        }

        public void DecompressFile(string inPath, string outPath)
        {
            if (FileExists(inPath)) { 
                if (!FileExists(outPath))
                {
                    const int BUFFER_SIZE = 1024 * 1024 * 12;
                    using (Stream inStream = File.OpenRead(inPath))
                    {
                        using (Stream outStream = File.Create(outPath))
                        {
                            long remaining = inStream.Length - inStream.Position;
                            while (remaining > 0)
                            {

                                int BytesToRead = (int)(remaining > BUFFER_SIZE ? BUFFER_SIZE : remaining);
                                byte[] buffer = new byte[BytesToRead];
                                int BytesRead = inStream.Read(buffer, 0, BytesToRead);
                                if (BytesRead != BytesToRead)
                                {
                                    //throw exception
                                    Console.WriteLine("Woopsie :)");
                                }
                                else
                                {
                                    byte[] decompressed = DecompressBytes(buffer);
                                    outStream.Write(decompressed, 0, decompressed.Length);
                                }
                                remaining = inStream.Length - inStream.Position;
                            }
                        }
                    }
                }
                else
                {
                    DecompressFile(inPath, outPath + "2");
                }
            }
            else
            {
                Console.WriteLine("The file you wish to decompress does not exist");
            }

        }

        private bool FileExists(string path)
        {
            if (File.Exists(path))
                return true;
            else
                return false;
        }


    }

}
