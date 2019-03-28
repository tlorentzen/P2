using System;
using System.Collections.Generic;
using System.IO;

namespace FileSplitter{
    static class Program{
        static void Main(string[] args){
            String filePath = "ruskursus.pdf";
            String outputFolder = "output/";
            String newFile = "ruskursus-new.pdf";
            int chuncSize = 1000000;
            splitterLibary splitterLibary= new splitterLibary();
            

            Console.WriteLine("Splitting file into chunks:");
            List<string> filelist =  splitterLibary.splitFile(filePath, outputFolder, chuncSize);

            Console.WriteLine("Merging file again:");
            splitterLibary.mergeFiles(outputFolder, newFile,filelist);
            Console.WriteLine("File Merged again.");
        }
    }
}