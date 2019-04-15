using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Index_lib{
    public class Index{
        private string _path;
        private readonly string _indexFilePath;
        private bool _debug;
        private HiddenFolder _hiddenFolder;

        //This delegate can be used to point to methods
        //which return void and take a string.
        public delegate void FileEventHandler(IndexFile file);
        public delegate void FileDeletedHandler(String hash);

        // Events
        public event FileEventHandler FileAdded;
        public event FileDeletedHandler FileDeleted;
        public event FileEventHandler FileChanged;

        List<IndexFile> _index = new List<IndexFile>();
        FileSystemWatcher watcher = new FileSystemWatcher();
        private ConcurrentQueue<FileSystemEventArgs> _fileHandlingQueue = new ConcurrentQueue<FileSystemEventArgs>();
        Thread _fileHandlerThread;
        private ManualResetEvent _waitHandle;
        public Boolean isRunning;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Index(String path){
            _indexFilePath = path + @"\.hidden\index.json";

            if (Directory.Exists(path)){
                _path = path;
            } else{
                throw new DirectoryNotFoundException();
            }

            watcher.Path = _path;
            watcher.IncludeSubdirectories = true;
            //watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;

            // Make hidden directory
            _hiddenFolder = new HiddenFolder(_path + @"\.hidden\");

            // Add event handlers.
            watcher.Changed += onChanged;
            watcher.Created += onChanged;
            watcher.Deleted += onChanged;
            watcher.Renamed += onRenamed;
        }

        public void Start(){
            isRunning = true;
            _waitHandle = new ManualResetEvent(false);

            _fileHandlerThread = new Thread(handleFileEvent);
            _fileHandlerThread.Start();

            watcher.EnableRaisingEvents = true;
        }

        public void stop(){
            isRunning = false;
            _waitHandle.Close();
            watcher.EnableRaisingEvents = false;
        }

        public string getPath(){
            return _path;
        }

        public void reIndex(){
            _index.Clear();
            buildIndex();
        }

        public IndexFile getEntry(string hash){
            foreach (IndexFile ifile in _index){
                if (ifile.hash.Equals(hash)){
                    return ifile;
                }
            }

            return null;
        }

        public void buildIndex(){
            string[] files =
                Directory.GetFiles(_path, "*",
                    SearchOption.AllDirectories); //TODO rewrite windows functionality D://

            foreach (string filePath in files){
                if (!IgnoreHidden(filePath)){
                    if (!_indexFilePath.Equals(filePath)){
                        bool foundInIndex = false;
                        IndexFile file = new IndexFile(filePath);

                        foreach (IndexFile ifile in _index){
                            if (ifile.Equals(file)){
                                ifile.addPath(file.getPath());
                                foundInIndex = true;
                                break;
                            }
                        }

                        if (!foundInIndex){
                            _index.Add(file);
                        }

                        if (_debug){
                            Console.WriteLine((foundInIndex ? "Path added: " : "File Added: ") + file.getHash() +
                                              " - " + filePath);
                        }
                    }
                }
            }

            save();
        }
        
        public void debug(bool value){
            _debug = value;
        }

        public void listFiles(){
            foreach (IndexFile file in _index){
                Console.WriteLine(file.getPath() + " Paths: " + file.paths.Count);
            }
        }

        public int getIndexSize(){
            return _index.Count;
        }

        private void handleFileEvent(){

            while (isRunning)
            {
                _waitHandle.WaitOne();

                FileSystemEventArgs e;

                while (_fileHandlingQueue.TryDequeue(out e))
                {
                    if (IgnoreHidden(e.FullPath))
                        continue;

                    if(!e.ChangeType.Equals(WatcherChangeTypes.Deleted)){
                        if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                            continue;

                        if (!WaitForFile(e.FullPath))
                        {
                            _fileHandlingQueue.Enqueue(e);
                            continue;
                        }
                    }

                    if (e.ChangeType.Equals(WatcherChangeTypes.Created)){

                        bool foundInIndex = false;
                        IndexFile eventFile = new IndexFile(e.FullPath);

                        foreach (IndexFile file in _index)
                        {
                            if (file.Equals(eventFile))
                            {
                                foundInIndex = true;
                                break;
                            }
                        }

                        if (foundInIndex)
                        {
                            foreach (IndexFile file in _index)
                            {
                                if (file.Equals(eventFile))
                                {
                                    file.addPath(e.FullPath);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            _index.Add(eventFile);
                            FileAdded(eventFile);
                        }

                    }
                    else if(e.ChangeType.Equals(WatcherChangeTypes.Changed)){

                        Boolean foundInIndex = false;
                        Boolean fileRemoved = false;
                        IndexFile eventFile = new IndexFile(e.FullPath);
                        IndexFile foundMatch = eventFile;

                        // Handle change
                        foreach (IndexFile file in _index)
                        {
                            if (file.Equals(eventFile))
                            {
                                foundInIndex = true;
                                foundMatch = file;
                                break;
                            }
                        }

                        // handle create event
                        if (!foundInIndex)
                        {
                            foreach (IndexFile file in _index)
                            {
                                if (file.paths.Contains(e.FullPath))
                                {
                                    if (file.paths.Count > 1)
                                    {
                                        file.paths.Remove(e.FullPath);
                                    }
                                    else
                                    {
                                        _index.Remove(file);
                                        break;
                                    }
                                }
                            }

                            _index.Add(eventFile);
                        }
                        else
                        {
                            foreach (IndexFile file in _index)
                            {
                                if (file.paths.Count > 1)
                                {
                                    foreach (string path in file.paths)
                                    {
                                        if (path == eventFile.paths[0] && !eventFile.Equals(file))
                                        {
                                            file.paths.Remove(path);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (string path in file.paths)
                                    {
                                        if (path == eventFile.paths[0] && !eventFile.Equals(file))
                                        {
                                            _index.Remove(file);
                                            fileRemoved = true;
                                        }
                                    }
                                }

                                if (fileRemoved)
                                {
                                    break;
                                }
                            }

                            if (!foundMatch.paths.Contains(eventFile.paths[0]))
                            {
                                foundMatch.addPath(eventFile.paths[0]);
                            }
                        }

                        FileChanged(eventFile);

                    }
                    else if(e.ChangeType.Equals(WatcherChangeTypes.Deleted)){

                        foreach (IndexFile file in _index)
                        {
                            List<String> pathsToDelete = file.paths.Where(p => p.Equals(e.FullPath) || p.StartsWith(e.FullPath)).ToList();

                            foreach(String path in pathsToDelete){
                                file.paths.Remove(path);
                            }
                        }

                        List<IndexFile> filesToDelete = _index.Where(p => p.paths.Count == 0).ToList();

                        foreach(IndexFile file in filesToDelete)
                        {
                            FileDeleted(file.hash);
                            _index.Remove(file);
                        }
                    }
                }

                _waitHandle.Reset();
            }
        }

        /*
        private void fileThreadHandler(FileSystemEventArgs e){
        
            while (!IsFileReady(e.FullPath)){ }

            //Ignore hidden folder
            if (IgnoreHidden(e.FullPath))
                return;

            // Ignore folder changes
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                return;

            bool foundInIndex = false;
            bool fileRemoved = false;
            IndexFile eventFile = new IndexFile(e.FullPath);
            IndexFile foundMatch = eventFile;

            // Handle change
            foreach (IndexFile file in index){
                if (file.Equals(eventFile)){
                    foundInIndex = true;
                    foundMatch = file;
                    break;
                }
            }

            // handle create event
            if (!foundInIndex){
                foreach (IndexFile file in index){
                    if (file.paths.Contains(e.FullPath)){
                        if (file.paths.Count > 1){
                            file.paths.Remove(e.FullPath);
                            break;
                        } else{
                            index.Remove(file);
                            break;
                        }
                    }
                }

                index.AddLast(eventFile);
            } else{
                foreach (IndexFile file in index){
                    if (file.paths.Count > 1){
                        foreach (string path in file.paths){
                            if (path == eventFile.paths[0] && !eventFile.Equals(file)){
                                file.paths.Remove(path);
                                break;
                            }
                        }
                    } else{
                        foreach (string path in file.paths){
                            if (path == eventFile.paths[0] && !eventFile.Equals(file)){
                                index.Remove(file);
                                fileRemoved = true;
                                break;
                            }
                        }
                    }

                    if (fileRemoved){
                        break;
                    }
                }

                if (!foundMatch.paths.Contains(eventFile.paths[0])){
                    foundMatch.addPath(eventFile.paths[0]);
                }
            }

            FileChanged(eventFile);
            
        }
        */


        // Define the event handlers.
        private void onChanged(object source, FileSystemEventArgs e){

            //IgnoreHidden
            if (IgnoreHidden(e.FullPath))
                return;

            _fileHandlingQueue.Enqueue(e);

            if(e.ChangeType.Equals(WatcherChangeTypes.Deleted)){
                Console.WriteLine("Deleting: "+e.FullPath);
            }

            
            _waitHandle.Set();


            /*
            //Ignore hidden folder
            if (IgnoreHidden(e.FullPath))
                return;
            // Ignore folder changes
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                return;

            // Wait until the file is ready 
            while (!IsFileReady(e.FullPath)){ }

            Boolean foundInIndex = false;
            Boolean fileRemoved = false;
            IndexFile eventFile = new IndexFile(e.FullPath);
            IndexFile foundMatch = eventFile;

            // Handle change
            foreach (IndexFile file in index){
                if (file.Equals(eventFile)){
                    foundInIndex = true;
                    foundMatch = file;
                    break;
                }
            }

            // handle create event
            if (!foundInIndex){
                foreach (IndexFile file in index){
                    if (file.paths.Contains(e.FullPath)){
                        if (file.paths.Count > 1){
                            file.paths.Remove(e.FullPath);
                        } else{
                            index.Remove(file);
                            break;
                        }
                    }
                }

                index.Add(eventFile);
            } else{
                foreach (IndexFile file in index){
                    if (file.paths.Count > 1){
                        foreach (string path in file.paths){
                            if (path == eventFile.paths[0] && !eventFile.Equals(file)){
                                file.paths.Remove(path);
                                break;
                            }
                        }
                    } else{
                        foreach (string path in file.paths){
                            if (path == eventFile.paths[0] && !eventFile.Equals(file)){
                                index.Remove(file);
                                fileRemoved = true;
                            }
                        }
                    }

                    if (fileRemoved){
                        break;
                    }
                }

                if (!foundMatch.paths.Contains(eventFile.paths[0])){
                    foundMatch.addPath(eventFile.paths[0]);
                }
            }

            FileChanged(eventFile);
            */
        }
        /*
        private void OnDeleted(object source, FileSystemEventArgs e){
            //Ignore hidden folder
            if (IgnoreHidden(e.FullPath))
                return;

            IndexFile deleted_file = null;

            foreach (IndexFile file in index){
                if (file.paths.Contains(e.FullPath)){
                    if (file.paths.Count > 1){
                        deleted_file = file;
                        file.paths.Remove(e.FullPath);
                        break;
                    } else{
                        deleted_file = file.Copy();
                        index.Remove(file);
                        break;
                    }
                }
            }

            // TODO: Deepcopy deleted file for DeletedFile event.
            FileDeleted(deleted_file);
        }
        */

        private void onRenamed(object source, RenamedEventArgs e){
            //Ignore hidden folder
            if (IgnoreHidden(e.FullPath))
                return;

            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory)){
                foreach (IndexFile file in _index){
                    for (int i = 0; i < file.paths.Count; i++){
                        if (file.paths[i].StartsWith(_path + @"\" + e.OldName)){
                            file.paths[i] = file.paths[i]
                                .Replace(_path + @"\" + e.OldName, _path + @"\" + e.Name);
                        }
                    }
                }
            } else{
                IndexFile renamedFile = new IndexFile(e.FullPath);

                foreach (IndexFile file in _index){
                    if (file.Equals(renamedFile)){
                        for (int i = 0; i < file.paths.Count; i++){
                            if (file.paths[i].Equals(e.OldFullPath)){
                                file.paths.RemoveAt(i);
                                file.addPath(e.FullPath);
                            }
                        }
                    }
                }
            }
        }

        public bool IsFileReady(string path){
            bool exist = false;
            FileStream inputStream = null;

            try
            {
                using (inputStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)){
                    exist = true;
                    inputStream.Close();
                }
            }
            catch (Exception){
                exist = false;
            }finally{
                if(inputStream != null){
                    inputStream.Close();
                }
            }

            return exist;
        }

        private Boolean WaitForFile(string fullPath)
        {
            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    // Attempt to open the file exclusively.
                    using (FileStream fs = new FileStream(fullPath,
                        FileMode.Open, FileAccess.ReadWrite,
                        FileShare.None, 100))
                    {
                        fs.ReadByte();
                        fs.Close();

                        // If we got this far the file is ready
                        break;
                    }
                }
                catch (Exception ex)
                {
                  
                    if (numTries > 2)
                    {
                        return false;
                    }

                    // Wait for the lock to be released
                    Console.WriteLine("Waiting for: " + fullPath);
                    Thread.Sleep(500);
                }
            }

            return true;
        }

        //Ignore file events in .hidden folder
        private bool IgnoreHidden(string filePath){
            return filePath.Contains(".hidden");
        }

        public bool load(){
            if (_indexFilePath != null && File.Exists(@"/" + _indexFilePath)){
                string json = File.ReadAllText(@"/" + _indexFilePath);
                _index = JsonConvert.DeserializeObject<List<IndexFile>>(json);
                return true;
            }

            return false;
        }

        public void save(){
            if (_indexFilePath != null){
                string json = JsonConvert.SerializeObject(_index);

                using (var fileStream = _hiddenFolder.writeToFile(_indexFilePath)){
                    byte[] jsonIndex = new UTF8Encoding(true).GetBytes(json);
                    fileStream.Write(jsonIndex, 0, jsonIndex.Length);
                }
            }
        }

        public void status(){
            Console.WriteLine(@"");
            Console.WriteLine(@"#### Index status ####");
            Console.WriteLine("Unique files: " + _index.Count);

            int pathCount = 0;

            foreach (IndexFile file in _index){
                pathCount += file.paths.Count;
            }

            Console.WriteLine("Paths: " + pathCount);
            Console.WriteLine(@"");
        }

        public void printInfo(){
            int pathNum;
            foreach (IndexFile file in _index){
                pathNum = 0;
                Console.WriteLine("Hash: {0}", file.hash);
                foreach (string path in file.paths){
                    pathNum += 1;
                    Console.WriteLine("Path {0}: {1}", pathNum.ToString(), path);
                }

                Console.WriteLine("Size: {0}", file.size);
            }
        }

        // TODO: https://codereview.stackexchange.com/questions/59385/filesystemwatcher-with-threaded-fifo-processing
    }
}