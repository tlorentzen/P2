using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ID_lib;
using Index_lib;
using Microsoft.Win32;
using P2P_lib;

namespace TorPdos{
    class Program{
        static Index _idx;
        static Network _p2P;
        public static string publicUuid;
        public static string filePath;
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        [STAThread]
        static void Main(){
            //Start of what needs to run at the Absolute start of the program.
            bool running = true;
            RegistryKey myReg = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");
            MyForm torPdos = new MyForm();

            if (myReg.GetValue("Path") == null || myReg.GetValue("UUID") == null)
            {
                Application.Run(torPdos);
            }
            //End of what needs to run at the Absolute start of the program.

            string ownIp = NetworkHelper.GetLocalIpAddress();

            Console.WriteLine(@"Local: " + ownIp);
            Console.WriteLine(@"Free space on C: " + DiskHelper.getTotalFreeSpace("C:\\"));
            Console.WriteLine(@"UUID: " + DiskHelper.getRegistryValue("UUID"));

            string path = (myReg.GetValue("Path").ToString());

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

            Console.WriteLine(@"Integrity check initialized...");
            _idx.MakeIntegrityCheck();
            Console.WriteLine(@"Integrity check finished!");

            Console.WriteLine(IdHandler.GetUuid());
            // Prepare P2PNetwork
            _p2P = new Network(25565, _idx, path);
            _p2P.Start();
            //p2p.ping();
            //p2p.DownloadFile("298310928301923lk12i3l1k2j3l12kj");
            while (running){
                string console = Console.ReadLine();
                if (console != null){
                    string[] param = console.Split(' ');

                    if (console.Equals("quit") || console.Equals("q")){
                        Console.WriteLine(@"Quitting...");
                        _idx.Save();
                        _p2P.SaveFile();
                        _p2P.Stop();
                        _idx.Stop();
                        running = false;
                    } else{
                        if (console.StartsWith("add") && param.Length == 3){
                            _p2P.AddPeer(param[1].Trim(), param[2].Trim());
                        } else if (console.Equals("gui")){
                            Application.Run(torPdos);
                        } else if (console.StartsWith("upload") && param.Length == 3){
                            if (int.TryParse(param[2], out _)){
                                _idx.ReIndex();
                                new NetworkProtocols(_idx, _p2P).UploadFileToNetwork(param[1], int.Parse(param[2]));
                            } else{
                                Console.WriteLine(@"Third parameter must be an integer");
                            }
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
                        } else if(console.StartsWith("download") && param.Length == 2) {
                            _p2P.DownloadFile(param[1]);
                        }else if (console.Equals("integrity"))
                        {
                            _idx.MakeIntegrityCheck();
                        }
                        else if (console.Equals("list")){

                            List<Peer> peers = _p2P.GetPeerList();

                            Console.WriteLine();
                            Console.WriteLine(@"### Your Peerlist contains ###");
                            if (peers.Count > 0){
                                foreach (Peer peer in peers){
                                    Console.WriteLine(peer.GetUuid() + @" - " + peer.GetIP() + @" - " +
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

        private static void Idx_FileMissing(IndexFile file)
        {
            Console.WriteLine(@"File missing init download of " + file.hash);
            _p2P.DownloadFile(file.hash);
        }

        private static void Idx_FileDeleted(string hash){
            //throw new NotImplementedException();
            Console.WriteLine(@"Deleted: " + hash);
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