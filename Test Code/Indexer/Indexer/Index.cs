using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Permissions;
using Newtonsoft.Json;
using HiddenFolders;


namespace Indexer
{
    public class Index
    {
        private String _path;
        private String _indexFilePath = null;
        private Boolean _debug = false;
        private HiddenFolder _hiddenFolder;
        
        // Events
        public event EventHandler FileAdded;
        public event EventHandler FileDeleted;
        public event EventHandler FileChanged;
        public event EventHandler FileRenamed;

        List<IndexFile> index = new List<IndexFile>();
        FileSystemWatcher watcher = new FileSystemWatcher();

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Index(String path) {
            if (Directory.Exists(path)){
                this._path = path;
            }else{
                throw new DirectoryNotFoundException();
            }

            watcher.Path = this._path;
            watcher.IncludeSubdirectories = true;

            // Make hidden directory
            _hiddenFolder = new HiddenFolder(_path + @"\.hidden");
            

            // Add event handlers.
            watcher.Changed += OnChanged;
            watcher.Created += OnCreate;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
                
            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        public void reIndex() {
            this.index.Clear();
            this.buildIndex();
        }

        public void buildIndex() {

            string[] files = Directory.GetFiles(this._path, "*", SearchOption.AllDirectories); //TODO rewrite windows functionality D://
            
            foreach (String filePath in files) {
                if (!IgnoreHidden(filePath)) {
                    if (!this._indexFilePath.Equals(filePath)) {
                        Boolean foundInIndex = false;
                        IndexFile file = new IndexFile(filePath);

                        foreach (IndexFile ifile in index) {
                            if (ifile.Equals(file)) {
                                ifile.addPath(file.getPath());
                                foundInIndex = true;
                            }
                        }

                        if (!foundInIndex) {
                            index.Add(file);
                        }

                        if (this._debug) {
                            Console.WriteLine((foundInIndex ? "Path added: " : "File Added: ") + file.getHash() + " - " + filePath);
                        }
                    }
                }
            }

            this.save();
        }

        public void setIndexFilePath(String path){
            this._indexFilePath = path;
        }

        public void debug(Boolean value) {
            this._debug = value;
        }

        public void listFiles() {
            foreach (IndexFile file in index) {
                Console.WriteLine(file.getPath() + " Paths: "+file.paths.Count);
            }
        }

        public int getIndexSize(){
            return this.index.Count;
        }

        private void OnCreate(object source, FileSystemEventArgs e) {

            // Ignore hidden folder
            if (IgnoreHidden(e.FullPath))
                return;

            // Ignore folder changes
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                return;

            Boolean foundInIndex = false;
            IndexFile eventFile = new IndexFile(e.FullPath);

            foreach (IndexFile file in index) {
                if (file.Equals(eventFile)) {
                    foundInIndex = true;
                }
            }

            if (foundInIndex) {
                foreach (IndexFile file in index) {
                    if (file.Equals(eventFile)) {
                        file.addPath(eventFile.paths[0]);
                    }
                }
            } else {
                index.Add(eventFile);
            }
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            //Ignore hidden folder
            if (IgnoreHidden(e.FullPath))
                return;
            // Ignore folder changes
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                return;

            Boolean foundInIndex = false;
            Boolean fileRemoved = false;
            IndexFile eventFile = new IndexFile(e.FullPath);
            IndexFile foundMatch = eventFile;

            // Handle change
            foreach (IndexFile file in index) {
                if (file.Equals(eventFile)) {
                    foundInIndex = true;
                    foundMatch = file;
                }
            }

            // handle create event
            if (!foundInIndex) {
                foreach (IndexFile file in index){
                    if (file.paths.Contains(e.FullPath)){
                        if (file.paths.Count > 1){
                            file.paths.Remove(e.FullPath);
                        }else{
                            index.Remove(file);
                        }
                    }
                }

                index.Add(eventFile);
            } else {
                foreach (IndexFile file in index) {
                    if(file.paths.Count > 1) {
                        foreach (string path in file.paths) {
                            if (path == eventFile.paths[0] && !eventFile.Equals(file)) {
                                file.paths.Remove(path);
                                break;
                            }
                        }
                    } else {
                        foreach (string path in file.paths) {
                            if (path == eventFile.paths[0] && !eventFile.Equals(file)) {
                                index.Remove(file);
                                fileRemoved = true;
                            }
                        }
                    }
                    if (fileRemoved) {
                        break;
                    }
                }
                if (!foundMatch.paths.Contains(eventFile.paths[0])) {
                    foundMatch.addPath(eventFile.paths[0]);
                }
            }
        }
        
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            //Ignore hidden folder
            if (IgnoreHidden(e.FullPath))
                return;

            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory)){
                foreach (IndexFile file in index) {
                    for (int i = 0; i < file.paths.Count; i++){
                        if (file.paths[i].StartsWith(this._path + @"\" + e.OldName)){
                            file.paths[i] = file.paths[i].Replace(this._path + @"\" + e.OldName, this._path + @"\" + e.Name);
                        }
                    }
                }
            }else{
                IndexFile renamedFile = new IndexFile(e.FullPath);

                foreach (IndexFile file in index){
                    if (file.Equals(renamedFile)) {
                        for (int i = 0; i < file.paths.Count; i++){
                            if (file.paths[i].Equals(e.OldFullPath)) {
                                file.paths.RemoveAt(i);
                                file.addPath(e.FullPath);
                            }
                        }
                    }
                }
            }
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            //Ignore hidden folder
            if (IgnoreHidden(e.FullPath))
                return;

            List<IndexFile> toDelete = new List<IndexFile>();

            foreach (IndexFile file in index){
                if (file.paths.Contains(e.FullPath)){
                    if (file.paths.Count > 1){
                        file.paths.Remove(e.FullPath);
                    }else{
                        toDelete.Add(file);
                    }
                }
            }

            if (toDelete.Count > 0) {
                foreach (IndexFile file in toDelete){
                    index.Remove(file);
                }
            }
        }
        
        //Ignore file events in .hidden folder
        private bool IgnoreHidden(string filePath)
        {
            string[] parents = filePath.Split('\\');
            foreach(string path in parents) {
                if (path == ".hidden") {
                    return true;
                }
            }
            return false;
        }

        public Boolean load()
        {
            if(_indexFilePath != null && File.Exists(this._indexFilePath)){
                String json = File.ReadAllText(this._indexFilePath);
                this.index = JsonConvert.DeserializeObject<List<IndexFile>>(json);
                return true;
            }

            return false;
        }

        public void save()
        {
            if(_indexFilePath != null){
                String json = JsonConvert.SerializeObject(index);

                using (var fileStream = new FileStream(this._indexFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    byte[] jsonIndex = new UTF8Encoding(true).GetBytes(json);
                    fileStream.Write(jsonIndex, 0, jsonIndex.Length);
                }
            }
        }

        public void status(){
            Console.WriteLine("");
            Console.WriteLine("#### Index status ####");
            Console.WriteLine("Unique files: "+this.index.Count);

            int pathCount = 0;

            foreach (IndexFile file in index)
            {
                pathCount += file.paths.Count;
            }

            Console.WriteLine("Paths: "+pathCount);
            Console.WriteLine("");
        }

        public void printInfo() {
            int pathNum;
            foreach (IndexFile file in index) {
                pathNum = 0;
                Console.WriteLine("Hash: {0}", file.hash);
                foreach(string path in file.paths) {
                    pathNum += 1;
                    Console.WriteLine("Path {0}: {1}", pathNum.ToString(), path);
                }
                Console.WriteLine("Size: {0}", file.size);
            }
        }

        ~Index()  // finalizer
        {
            watcher.EnableRaisingEvents = false;
        }
    }
}
