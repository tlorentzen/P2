﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CompleteTest
{
    public class Index
    {
        private String _path;

        // Events
        public event EventHandler FileAdded;
        public event EventHandler FileDeleted;
        public event EventHandler FileChanged;
        public event EventHandler FileRenamed;

        List<IndexFile> index = new List<IndexFile>();
        FileSystemWatcher watcher = new FileSystemWatcher();

        //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Index(String path) {
            if (Directory.Exists(path)){
                this._path = path;
            }else{
                throw new DirectoryNotFoundException();
            }

            watcher.Path = this._path;
            watcher.IncludeSubdirectories = true;

            // Add event handlers.
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
                
            // Begin watching.
            watcher.EnableRaisingEvents = true;

            this.buildIndex();

            int count_paths = 0;
            Console.WriteLine("### Index ###");
            Console.WriteLine("-------------");
            Console.WriteLine("Files: "+index.Count);

            foreach (IndexFile file in index) {
                count_paths += file.paths.Count;
            }

            Console.WriteLine("Paths: " + count_paths);
        }

        public void reIndex() {
            this.index.Clear();
            this.buildIndex();

            int count_paths = 0;
            Console.WriteLine("### Index ###");
            Console.WriteLine("-------------");
            Console.WriteLine("Files: " + index.Count);

            foreach (IndexFile file in index)
            {
                count_paths += file.paths.Count;
            }

            Console.WriteLine("Paths: " + count_paths);
        }

        private void buildIndex() {

            string[] files = Directory.GetFiles(this._path, "*", SearchOption.AllDirectories); //TODO rewrite windows functionality D://
            
            foreach (String filePath in files) {
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
            }
        }

        public void listFiles() {
            foreach (IndexFile file in index) {
                Console.WriteLine(file.getPath() + " Paths: "+file.paths.Count);
            }
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Ignore folder changes
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                return;

            Boolean foundInIndex = false;
            IndexFile eventFile = new IndexFile(e.FullPath);

            // Handle change
            foreach (IndexFile file in index) {
                if (file.Equals(eventFile)) {
                    foundInIndex = true;
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
            }
        }
        
        private void OnRenamed(object source, RenamedEventArgs e)
        {
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
            
            int count_paths = 0;
            Console.WriteLine("### Index ###");
            Console.WriteLine("-------------");
            Console.WriteLine("Files: " + index.Count);

            foreach (IndexFile file in index)
            {
                count_paths += file.paths.Count;
            }

            Console.WriteLine("Paths: " + count_paths);
        }

        ~Index()  // finalizer
        {
            watcher.EnableRaisingEvents = false;
        }
    }
}
