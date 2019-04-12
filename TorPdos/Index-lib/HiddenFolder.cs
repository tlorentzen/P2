using System;
using System.IO;
using System.Linq;
using System.Text;
using NLog;

namespace Index_lib{
    public class HiddenFolder{
        private readonly string _path;
        private static NLog.Logger logger = NLog.LogManager.GetLogger("HiddenFolderLogger");

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

        public FileStream readFromFile(string path){
            try{
                return new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            catch (FileNotFoundException e){
                logger.Fatal(e);
            }
            catch (Exception e){
                logger.Error(e);
            }

            return null;
        }
    }
}