using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace FileSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            String filePath = @"C:\Users\Thomas Lorentzen\Desktop\Output\pdf-test.pdf";
            String outputFolder = @"C:\Users\Thomas Lorentzen\Desktop\Output";
            String newFile = @"C:\Users\Thomas Lorentzen\Desktop\Output\pdf-test-new.pdf";
            int chunkFile = 1000000;

            String filename = Path.GetFileName(@"pdf-test.pdf");
            String filenameHash = CreateMD5(filename);

            Console.WriteLine("Splitting file into chunks:");
            SplitFile(filePath, outputFolder, chunkFile);

            Console.WriteLine("Merging file again:");
            MergeFiles(outputFolder, filenameHash, newFile);

            Console.ReadKey();
        }

        public static void SplitFile(String inputFilePath, string OutputFolderpath, int chunkSize)
        {
            // https://stackoverflow.com/questions/3967541/how-to-split-large-files-efficiently
            const int BUFFER_SIZE = 1024;
            byte[] buffer = new byte[BUFFER_SIZE];
            
            if (File.Exists(inputFilePath)){

                String fileHash = CreateMD5(Path.GetFileName(inputFilePath));

                using (Stream input = File.OpenRead(inputFilePath)){
                    int index = 0;

                    while (input.Position < input.Length){
                        using (Stream output = File.Create(OutputFolderpath + @"\" + fileHash+"-"+index)){
                            int remaining = chunkSize, bytesRead;

                            while (remaining > 0 && (bytesRead = input.Read(buffer, 0, Math.Min(remaining, BUFFER_SIZE))) > 0){
                                output.Write(buffer, 0, bytesRead);
                                remaining -= bytesRead;
                            }
                        }
                        index++;
                    }
                }
            }else{
                Console.WriteLine("{0} is not a valid file or directory.", inputFilePath);
            }
        }

        public static void MergeFiles(string inputDir, string chunkName, string outputFilePath) {

            if (Directory.Exists(inputDir)){

                List<String> chunkedFiles = new List<String>();

                foreach (String filePath in Directory.GetFiles(inputDir)){
                    String name = Path.GetFileName(filePath);

                    if(name.StartsWith(chunkName)){
                        chunkedFiles.Add(filePath);
                    }
                }

                if (chunkedFiles.Count > 0) {

                    // ascending sorting to make sure we merge the file in right order
                    chunkedFiles.Sort(delegate (String a, String b) {
                        int av = Convert.ToInt32(a.Split('-')[1]);
                        int bv = Convert.ToInt32(b.Split('-')[1]);
                        return av.CompareTo(bv);
                    });

                    if (!File.Exists(outputFilePath)){
                        using (File.Create(outputFilePath)) { };
                    }

                    using (Stream output = File.OpenWrite(outputFilePath)) {
                        foreach (String chunkPath in chunkedFiles){
                            byte[] contentBuffer = File.ReadAllBytes(chunkPath);
                            output.Write(contentBuffer, 0, contentBuffer.Length);
                        }
                    }
                }
            }else{
                Console.WriteLine("{0} is not a valid file or directory.", inputDir);
            }
        }
        
        public static string CreateMD5(string input){
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create()){

                byte[] inputBytes = Encoding.ASCII.GetBytes(input); 
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < hashBytes.Length; i++){
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

    }
}
