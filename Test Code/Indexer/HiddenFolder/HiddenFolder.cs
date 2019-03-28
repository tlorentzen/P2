﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenFolders
{
    public class HiddenFolder
    {
        public HiddenFolder(string path)
        {
            _path = path;
            if (!Directory.Exists(_path)) {
                _directory = Directory.CreateDirectory(_path);
            } else {
                _directory = new DirectoryInfo(_path);
            }
            //See if directory has hidden flag, if not, make hidden
            if ((_directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
                //Add Hidden flag    
                _directory.Attributes |= FileAttributes.Hidden;
            }
        }

        private string _path;
        private DirectoryInfo _directory;

        //Adds file or directory from path to the hidden directory
        public void Add(string path)
        {
            string nameExt = path.Split('\\').Last();
            Directory.Move(path, _path + nameExt);
        }

        //Deletes file or directory form the relative path from hidden directory
        public void Remove(string path)
        {
            Directory.Delete(_path + path);
        }

        public FileStream WriteToFile(string path)
        {
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public FileStream ReadFromFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        




    }

}
