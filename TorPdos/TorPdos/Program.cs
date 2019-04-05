using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Index_lib;
using P2P_lib;
using ID_lib;

namespace TorPdos{
    class Program{

        static Index idx;
        static Network p2p;

        [STAThread]
        static void Main(string[] args){
            MyForm TorPdos = new MyForm();

            Boolean running = true;
            string ownIP = NetworkHelper.getLocalIPAddress();
            Console.WriteLine("Local: " + ownIP);
            Console.WriteLine("Free space on C: " + DiskHelper.GetTotalFreeSpace("C:\\"));

            string path = @"C:\TorPdos\";

            // Load Index
            if (!Directory.Exists(path)){
                Directory.CreateDirectory(path);
            }
            idx = new Index(path);
            idx.load();
            idx.FileAdded += Idx_FileAdded;
            idx.FileChanged += Idx_FileChanged;
            idx.FileDeleted += Idx_FileDeleted;

            if (!idx.load()){
                idx.buildIndex();
            }

            // Prepare P2PNetwork
            p2p = new Network(25565, idx, path);
            p2p.Start();

            while (running){
                string console = Console.ReadLine();
                string[] param = console.Split(' ');

                if (console.Equals("quit") || console.Equals("q")){
                    Console.WriteLine("Quitting...");
                    idx.save();
                    p2p.saveFile();
                    p2p.Stop();
                    running = false;
                } else{
                    if (console.StartsWith("add") && param.Length == 2){
                        p2p.AddPeer("MyName" + param[1].Trim(), param[1].Trim());
                    } else if (console.Equals("gui")){
                        Application.Run(TorPdos);
                    } else if (console.StartsWith("upload") && param.Length == 3){
                        if (int.TryParse(param[2], out int n)){
                            idx.reIndex();
                            new NetworkProtocols(idx, p2p).UploadFileToNetwork(param[1], int.Parse(param[2]));
                        } else{
                            Console.WriteLine("Third parameter must be an integer");
                        }
                    } else if (console.Equals("reindex")){
                        idx.reIndex();
                    } else if (console.Equals("status")){
                        idx.status();
                    } else if (console.Equals("idxsave")){
                        idx.save();
                    } else if (console.Equals("peersave")){
                        p2p.saveFile();
                    } else if(console.Equals("list")){
                        List<Peer> peers = p2p.getPeerList();

                        Console.WriteLine();
                        Console.WriteLine("### Your Peerlist contains ###");
                        if(peers.Count > 0){
                            foreach (Peer peer in peers)
                            {
                                Console.WriteLine(peer.getUUID() + " - " + peer.GetIP()+ " - "+(peer.isOnline() ? "Online" : "Offline"));
                            }
                        }else{
                            Console.WriteLine("The list is empty...");
                        }
                        Console.WriteLine();
                        
                    } else{
                        Console.WriteLine("Unknown command");
                    }
                }
            }

            Console.ReadKey();
        }

        private static void Idx_FileDeleted(IndexFile file){
            if (file == null){
                Console.WriteLine("File deleted: null...");
            } else{
                Console.WriteLine("File deleted: " + file.hash);
            }
        }

        private static void Idx_FileAdded(IndexFile file){
            Console.WriteLine("File added: " + file.hash);
            p2p.UploadFileToNetwork(file.paths[0], 3);
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