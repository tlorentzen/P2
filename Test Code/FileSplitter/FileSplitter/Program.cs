using System;
using System.IO;

namespace FileSplitter{
    static class Program{
        static void Main(string[] args){
            String filePath = "4.2.4.zip";
            String outputFolder = "output/";
            String newFile = "4.2.4-new.zip";
            int chuncSize = 1000000;
            splitterLibary splitterLibary= new splitterLibary();

            String filename = Path.GetFileName("pdf-test.pdf");
            

            Console.WriteLine("Splitting file into chunks:");
            splitterLibary.splitFile(filePath, outputFolder, chuncSize);

            Console.WriteLine("Merging file again:");
            splitterLibary.mergeFiles(outputFolder, newFile);
            Console.WriteLine("File Merged again.");

            Console.ReadKey();
        }
    }
}