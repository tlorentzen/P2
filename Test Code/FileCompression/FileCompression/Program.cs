using System;
using System.IO.Compression;

class Program
{
    static void Main(string[] args)
    {
        string startPath = @"./start";
        string zipPath = @"./result.zip";
        string extractPath = @"./extract";
        string mode = "";

        while (mode != "E" && mode != "C")
        {
            Console.WriteLine("Compress or extract? (C/E)");
            mode = Console.ReadLine();
        }

        if (mode == "C")
        {

            Console.WriteLine("Compressing");
            ZipFile.CreateFromDirectory(startPath, zipPath);
        }
        else if (mode == "E")
        {
            Console.WriteLine("Extracting");
            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }
        else
        {
            Console.WriteLine("Unknown error");
        }
    }
}