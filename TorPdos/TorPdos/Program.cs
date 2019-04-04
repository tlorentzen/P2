using System;
using System.Collections.Generic;
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
            Index idx = new Index(path);
            idx.load();
            idx.FileAdded += Idx_FileAdded;
            idx.FileChanged += Idx_FileChanged;
            idx.FileDeleted += Idx_FileDeleted;

            if (!idx.load()){
                idx.buildIndex();
            }

            // Prepare P2PNetwork
            Network p2p = new Network(25565, idx, path);
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
                    } else if (console.StartsWith("upload")/* && param.Length == 3*/){
                        //upload C:\Users\Niels\Desktop\INEVAanalyse.pdf 3
                        /*if (int.TryParse(param[2], out int n)){*/
                        idx.reIndex();
                        string filesToDelete = new NetworkProtocols(idx, p2p).UploadFileToNetwork(path + "INEVAanalyse.pdf" /*param[1]*/, 1 /*int.Parse(param[2])*/);
                        //Filerne i den skjulte mappe skal køres igennem og alle, der starter med filesToDelete, skal slettes
                        /*} else{
                            Console.WriteLine("Third parameter must be an integer");
                        }*/
                    } else if (console.Equals("reindex")){
                        idx.reIndex();
                    } else if (console.Equals("status")){
                        idx.status();
                    } else if (console.Equals("idxsave")){
                        idx.save();
                    } else if (console.Equals("peersave")){
                        p2p.saveFile();
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