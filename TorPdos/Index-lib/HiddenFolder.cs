using System;
using System.IO;

namespace Index_lib{
    public class HiddenFolder{
        private readonly string _path;
        private static NLog.Logger logger = NLog.LogManager.GetLogger("HiddenFolderLogger");

        
        public HiddenFolder(string path){
            DirectoryInfo directory;
            _path = path;
            //Create the hidden folder at the given path if it does not exist already
            if (!Directory.Exists(_path)){
                directory = Directory.CreateDirectory(_path);
                //Get information about the folder if it does already exist
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
            //If it is not an absolute path it will be turned into an absolute path
            if (!path.Contains(".hidden"))
            {
                path = _path + "\\" + path;
            }
            //If a file exists on the path the file will be deleted
            if (File.Exists(path)) {
                File.Delete(path);
                //Else if the path is a directory it will find all the files in the directory and delete said files. 
            } else if (Directory.Exists(path)) {
                string[] files = Directory.GetFiles(path);
                foreach(string p in files) {
                    File.Delete(p);
                }
                string[] paths = Directory.GetDirectories(path);
                foreach(string p in paths) {
                    Remove(p);
                }
                //Deletes the entire directory
                Directory.Delete(path);
                //If the path doesn't exist it will throw the exception that it's not a valid path
            } else
                throw new ArgumentException("Path invalid", path);
        }

        //Deletes a file at the inputted path
        public static void RemoveFile(string path) {
            File.Delete(path);
        }

        //Makes it possible to edit files located in hidden folders
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