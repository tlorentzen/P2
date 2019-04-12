using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Index_lib{
    public class HiddenFolder{
        private readonly string _path;

        public HiddenFolder(string path){
            DirectoryInfo directory;
            _path = path;
            if (!Directory.Exists(_path)){
                directory = Directory.CreateDirectory(_path);
            } else{
                directory = new DirectoryInfo(_path);
            }

            //See if directory has hidden flag, if not, make hidden
            if ((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden){
                //Add Hidden flag    
                directory.Attributes |= FileAttributes.Hidden;
            }
        }

        //Adds file or directory from path to the hidden directory
        public void add(string path){
            string nameExt = path.Split('\\').Last();
            Directory.Move(path, _path + "/" + nameExt);
        }

        //Deletes file or directory form the relative path from hidden directory

        public void remove(string path)
        {
            Directory.Delete(_path + "/" + path);
        }

        public void removeFile(string path) {
            File.Delete(path);
        }

        public FileStream writeToFile(string path){
            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }

        public void appendToFileLog(string path, string input){
            FileStream output = new FileStream(path, FileMode.Append);

            byte[] inputBytes;
            inputBytes = Encoding.UTF8.GetBytes(DateTime.Now + "\n" + input + "\n" +
                                                "\n \n -------------------------------------- \n");
            output.Write(inputBytes,0,inputBytes.Length);
            
            output.Close();
        }

        public FileStream readFromFile(string path){
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }
    }
}