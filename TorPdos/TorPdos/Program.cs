using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ID_lib;
using Index_lib;
using Microsoft.Win32;
using NLog;
using P2P_lib;

namespace TorPdos{
    class Program{
        static Index idx;
        static Network p2p;
        public static string publicUuid;
        public static string FilePath;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [STAThread]
        static void Main(string[] args){
            //Start of what needs to run at the Absolute start of the program.
            bool running = true;
            RegistryKey MyReg = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");
            MyForm TorPdos = new MyForm();

            if (MyReg.GetValue("Path") == null || MyReg.GetValue("UUID") == null)
            {
                Application.Run(TorPdos);
            }
            //End of what needs to run at the Absolute start of the program.

            string ownIP = NetworkHelper.getLocalIPAddress();

            Console.WriteLine("Local: " + ownIP);
            Console.WriteLine("Free space on C: " + DiskHelper.getTotalFreeSpace("C:\\"));
            Console.WriteLine("UUID: " + DiskHelper.getRegistryValue("UUID"));

            string path = (MyReg.GetValue("Path").ToString());

            // Load Index
            if (!Directory.Exists(path)){
                Directory.CreateDirectory(path);
            }

            idx = new Index(path);
            idx.load();
            idx.FileAdded += Idx_FileAdded;
            idx.FileChanged += Idx_FileChanged;
            idx.FileDeleted += Idx_FileDeleted;
            idx.FileMissing += Idx_FileMissing;

            if (!idx.load()){
                idx.buildIndex();
            }

            idx.Start();

            Console.WriteLine("Integrity check initialized...");
            idx.MakeIntegrityCheck();
            Console.WriteLine("Integrity check finished!");

            Console.WriteLine(IdHandler.getUuid());
            // Prepare P2PNetwork
            p2p = new Network(25565, idx, path);
            p2p.Start();
            //p2p.ping();
            //p2p.DownloadFile("298310928301923lk12i3l1k2j3l12kj");
            while (running){
                string console = Console.ReadLine();
                if (console != null){
                    string[] param = console.Split(' ');

                    if (console.Equals("quit") || console.Equals("q")){
                        Console.WriteLine(@"Quitting...");
                        idx.save();
                        p2p.saveFile();
                        p2p.Stop();
                        idx.stop();
                        running = false;
                    } else{
                        if (console.StartsWith("add") && param.Length == 3){
                            p2p.AddPeer(param[1].Trim(), param[2].Trim());
                        } else if (console.Equals("gui")){
                            Application.Run(TorPdos);
                        } else if (console.StartsWith("upload") && param.Length == 3){
                            if (int.TryParse(param[2], out _)){
                                idx.reIndex();
                                new NetworkProtocols(idx, p2p).UploadFileToNetwork(param[1], int.Parse(param[2]));
                            } else{
                                Console.WriteLine(@"Third parameter must be an integer");
                            }
                        } else if (console.Equals("reindex")){
                            idx.reIndex();
                        } else if (console.Equals("status")){
                            idx.status();
                        } else if (console.Equals("idxsave")){
                            idx.save();
                        } else if (console.Equals("peersave")){
                            p2p.saveFile();
                        } else if (console.Equals("ping")){
                            p2p.ping();
                        } else if(console.StartsWith("download") && param.Length == 2) {
                            p2p.DownloadFile(param[1]);
                        }else if (console.Equals("integrity"))
                        {
                            idx.MakeIntegrityCheck();
                        }
                        else if (console.Equals("list")){

                            List<Peer> peers = p2p.getPeerList();

                            Console.WriteLine();
                            Console.WriteLine(@"### Your Peerlist contains ###");
                            if (peers.Count > 0){
                                foreach (Peer peer in peers){
                                    Console.WriteLine(peer.getUUID() + @" - " + peer.GetIP() + @" - " +
                                                      (peer.isOnline() ? "Online" : "Offline"));
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
        }

        private static void Idx_FileMissing(IndexFile file)
        {
            Console.WriteLine(@"File missing init download of " + file.hash);
            p2p.DownloadFile(file.hash);
        }

        private static void Idx_FileDeleted(string hash){
            //throw new NotImplementedException();
            Console.WriteLine(@"Deleted: " + hash);
        }

        private static void Idx_FileAdded(IndexFile file){
            Console.WriteLine(@"Added: " + file.hash);

            p2p.UploadFile(file.hash, file.getPath(), 5);
            //p2p.UploadFileToNetwork(file.paths[0], 3);
        }

        private static void Idx_FileChanged(IndexFile file){
            Console.WriteLine("File changed: " + file.hash);
        }

        /*

        private static void Idx_FileDeleted(IndexFile file)
        {
            Console.WriteLine("File deleted: " + file.hash);
        }
        */
    }
}