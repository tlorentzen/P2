using System;
using System.IO.Compression;
using SevenZip;
namespace FileCompression
{
    class Program
    {
        static void Main(string[] args)
        {
            string mode = "";

            Compressor coder = new Compressor();

            while (mode != "E" && mode != "C")
            {
                Console.WriteLine("Compress or extract? (C/E)");
                mode = Console.ReadLine();
            }

            if (mode == "C")
            {
                coder.Compress("", "");
            }
            else if (mode == "E")
            {
                coder.Extract("", "");
            }
            else
            {
                Console.WriteLine("Unknown error");
            }
        }
    }
}