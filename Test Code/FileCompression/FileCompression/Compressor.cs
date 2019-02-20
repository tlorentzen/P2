using System;
using System.IO.Compression;
namespace FileCompression
{
    public class Compressor
    {
        public Compressor()
        {
        }

        private string StartPath = @"./start";
        private string ZipPath = @"./result.zip";
        private string ExtractPath = @"./extract";

        public void Compress(string startPath, string zipPath)
        {
            if(startPath == "")
            {
                StartPath = @"./start";
            }
            else
            {
                StartPath = @"./" + startPath;
            }
            if(zipPath == "")
            {
                ZipPath = @"./result.zip";
            }
            else
            {
                ZipPath = @"./" + zipPath + ".zip";
            }
            Console.WriteLine("Compressing");
            ZipFile.CreateFromDirectory(StartPath, ZipPath);
        }

        public void Extract(string zipPath, string extractPath)
        {
            if (zipPath == "")
            {
                ZipPath = @"./result.zip";
            }
            else
            {
                ZipPath = @"./" + ZipPath + ".zip";
            }
            if (extractPath == "")
            {
                ExtractPath = @"./extract";
            }
            else
            {
                ExtractPath = @"./" + extractPath;
            }
            Console.WriteLine("Extracting");
            ZipFile.ExtractToDirectory(ZipPath, ExtractPath);
        }
    }
}
