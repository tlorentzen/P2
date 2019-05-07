using System;
using System.IO;

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
        public void Add(string path){
            //string nameExt = path.Split('\\').Last();
            Directory.Move(path, _path + "/" + path);
        }

        public void Add(string path, string inpath)
        {
            Directory.Move(path, _path + "/" + inpath);
        }

        //Deletes file or directory form the relative path from hidden directory

        public void Remove(string path)
        {
            if (File.Exists(path)) {
                File.Delete(path);
            } else if (Directory.Exists(path)) {
                string[] files = Directory.GetFiles(path);
                foreach(string p in files) {
                    File.Delete(p);
                }
                string[] paths = Directory.GetDirectories(path);
                foreach(string p in paths) {
                    Remove(p);
                }
                Directory.Delete(path);
            } else
                throw new ArgumentException("Path invalid", path);
        }

        public static void RemoveFile(string path) {
            File.Delete(path);
        }

        public FileStream WriteToFile(string path){
            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }

        public FileStream ReadFromFile(string path){
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