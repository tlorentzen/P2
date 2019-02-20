using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using SevenZip;
using System.Text;

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
            Stream inStream = File.Open(inPath, FileMode.Open);
            Stream outStream = File.Open(outPath, FileMode.CreateNew);

            byte[] buffer = new byte[1024 * 1024];
            MemoryStream bufferStream = new MemoryStream();
            inStream.CopyTo(bufferStream, 1024);

            buffer = bufferStream.ToArray();



        }


    }

}
