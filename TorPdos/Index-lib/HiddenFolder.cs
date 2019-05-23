using System;
using System.IO;

namespace Index_lib{
    public class HiddenFolder{
        private readonly string _path;
        private static NLog.Logger logger = NLog.LogManager.GetLogger("HiddenFolderLogger");

        /// <summary>
        /// Creates a hidden folder
        /// </summary>
        /// <param name="path">Path to the hidden folder</param>
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

        /// <summary>
        /// Deletes file or directory form the relative path from hidden directory
        /// </summary>
        /// <param name="path">Path to the file/directory which has to be removed, can be both relative and absolute</param>
        public void Remove(string path)
        {
            //If it is not an absolute path it will be turned into an absolute path
            if (!path.Contains(".hidden"))
            {
                path = _path + path;
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

        /// <summary>
        /// Makes it possible to edit files located in hidden folders
        /// </summary>
        /// <param name="path">Path to the file which has to be edited</param>
        /// <returns>Returns the file with the modifications</returns>
        public FileStream WriteToFile(string path){
            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }
    }
}