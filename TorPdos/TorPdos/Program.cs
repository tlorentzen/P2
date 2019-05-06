using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using Index_lib;
using P2P_lib;

namespace TorPdos{
    static class Program{
        static Index _idx;
        static Network _p2P;
        public static string publicUuid;
        public static string filePath;
        

        [STAThread]
        static void Main(){
            //Start of what needs to run at the Absolute start of the program.
            bool running = true;
            bool firstRun = true;
            MyForm torPdos = new MyForm();
            
            if (string.IsNullOrEmpty(DiskHelper.GetRegistryValue("Path"))){
                Application.Run(torPdos);
            }
            else if(File.Exists(DiskHelper.GetRegistryValue("Path") + @".hidden\userdata") == false)
            {
                Application.Run(torPdos);
            }
            //End of what needs to run at the Absolute start of the program.

            string ownIp = NetworkHelper.GetLocalIpAddress();
            string path = (DiskHelper.GetRegistryValue("Path"));

            Console.WriteLine(IdHandler.GetUuid());
            Console.WriteLine(@"Please login by typing: login [PASSWORD] or gui");
            while (running){
                string console = Console.ReadLine();
                if (console != null){
                    string[] param = console.Split(' ');
                    
                    if (console.Equals("quit") || console.Equals("q")){
                        Console.WriteLine(@"Quitting...");
                        _idx.Save();
                        _idx.Stop();
                        _p2P.SaveFile();
                        _p2P.Stop();
                        running = false;
                    } else{
                        if (firstRun){
                            // Load Index
                            if (!Directory.Exists(path)){
                                Directory.CreateDirectory(path);
                            }

                            _idx = new Index(path);
                            _idx.Load();
                            _idx.FileAdded += Idx_FileAdded;
                            _idx.FileChanged += Idx_FileChanged;
                            _idx.FileDeleted += Idx_FileDeleted;
                            _idx.FileMissing += Idx_FileMissing;

                            if (!_idx.Load()){
                                _idx.BuildIndex();
                            }

                            _idx.Start();

                            // Prepare P2PNetwork
                            try{
                                _p2P = new Network(25565, _idx, path);
                                _p2P.Start();
                            }
                            catch (SocketException){
                                Application.Run(torPdos);
                            }
                        
                            Console.WriteLine(@"Integrity check initialized...");
                            _idx.MakeIntegrityCheck();
                            Console.WriteLine(@"Integrity check finished!");
                        
                            Console.WriteLine(@"Local: " + ownIp);
                            Console.WriteLine(@"Free space on C: " + DiskHelper.GetTotalAvailableSpace("C:\\"));
                            Console.WriteLine(@"UUID: " + IdHandler.GetUuid());
                            firstRun = false;
                        }
                        while (IdHandler.GetUuid() == null){
                            
                            if (console.StartsWith("login") && param.Length == 2){
                                IdHandler.GetUuid(param[1]);
                            } else if (console.Equals("gui")){
                                Application.Run(torPdos);
                            }
                        }
                        

                        
                        if (console.StartsWith("add") && param.Length == 3){
                            _p2P.AddPeer(param[1].Trim(), param[2].Trim());
                        } else if (console.Equals("reindex")){
                            _idx.ReIndex();
                        } else if (console.Equals("status")){
                            _idx.Status();
                        } else if (console.Equals("idxsave")){
                            _idx.Save();
                        } else if (console.Equals("peersave")){
                            _p2P.SaveFile();
                        } else if (console.Equals("ping")){
                            _p2P.Ping();
                        } else if (console.StartsWith("download") && param.Length == 2){
                            _p2P.DownloadFile(param[1]);
                        } else if (console.Equals("integrity")){
                            _idx.MakeIntegrityCheck();
                        } else if (console.Equals("list")){
                            List<Peer> peers = _p2P.GetPeerList();

                            Console.WriteLine();
                            Console.WriteLine(@"### Your Peerlist contains ###");
                            if (peers.Count > 0){
                                foreach (Peer peer in peers){
                                    Console.WriteLine(peer.GetUuid() + @" - " + peer.GetIp() + @" - " +
                                                      (peer.IsOnline() ? "Online" : "Offline"));
                                }
                            } else{
                                Console.WriteLine(@"The list is empty...");
                            }

                            Console.WriteLine();
                        } else{
                            Console.WriteLine(@"Unknown command");
                        }
                    }
                }
            }

            Console.ReadKey();
        }

        private static void Idx_FileMissing(IndexFile file){
            Console.WriteLine(@"File missing init download of " + file.hash);
            _p2P.DownloadFile(file.hash);
        }

        private static void Idx_FileDeleted(string hash){
            //throw new NotImplementedException();
            Console.WriteLine(@"Deleted: " + hash);
            _p2P.DeleteFile(hash);
        }

        private static void Idx_FileAdded(IndexFile file){
            Console.WriteLine(@"Added: " + file.hash);

            _p2P.UploadFile(file.hash, file.GetPath(), 5);
            //p2p.UploadFileToNetwork(file.paths[0], 3);
        }

        private static void Idx_FileChanged(IndexFile file){
            Console.WriteLine(@"File changed: " + file.hash);
        }
    }
}