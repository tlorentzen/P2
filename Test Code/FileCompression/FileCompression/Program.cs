using System;
using System.IO;
using System.Text;

namespace FileCompression
{
    public class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Provide path to compress:");
            string comPath = Console.ReadLine();

            Console.WriteLine("Provide result path:");
            string outPath = Console.ReadLine();

            ByteCompressor comp = new ByteCompressor();

            comp.CompressFile(@"" + comPath,@"" + outPath);

            Console.WriteLine("done :)");

        }
    }

}