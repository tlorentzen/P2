using System;
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
    }
}
