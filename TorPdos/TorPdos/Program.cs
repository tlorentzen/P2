using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using Index_lib;
using P2P_lib;
using P2P_lib.Handlers;
using P2P_lib.Helpers;

namespace TorPdos{
    static class Program{
        static Index _idx;
        static Network _p2P;

        /// <summary>
        /// The main method, which runs the program
        /// </summary>
        [STAThread]
        static void Main(){
            bool running = true;
            bool firstRun = true;
            MyForm torPdos = new MyForm();

            //If the ''Path'' variable is not set, the GUI is run to set this up.
            if (string.IsNullOrEmpty(DiskHelper.GetRegistryValue("Path"))){
                Application.Run(torPdos);
            }
            //If the ''Path'' variable is set, but the userdata file does not exist,
            //the GUI is run to create this.
            else if (File.Exists(DiskHelper.GetRegistryValue("Path") + @".hidden\userdata") == false){
                Application.Run(torPdos);
            }

            //Gets the local IP and the path to the users TorPDos-folder.
            string ownIp = NetworkHelper.GetLocalIpAddress();
            string path = (DiskHelper.GetRegistryValue("Path"));

            //Starts the communication with the user, and ensures that
            //the user logs in.
            DiskHelper.ConsoleWrite("Welcome to TorPdos!");
            Console.WriteLine(@"Please login by typing: login [PASSWORD] or gui");
            while (running){
                string console = Console.ReadLine();
                if (console != null){
                    string[] param = console.Split(' ');
                    //Close program
                    if (console.Equals("quit") || console.Equals("q")){
                        Console.WriteLine(@"Quitting...");
                        _idx.Save();
                        _idx.Stop();
                        _p2P.SavePeer();
                        _p2P.Stop();
                        running = false;
                        Console.WriteLine("\nPress any button to quit!");
                    } else{
                        //Handles the login of the user through the console.
                        while (IdHandler.GetUuid() == null){
                            if (console.StartsWith("login") && param.Length == 2){
                                if (IdHandler.GetUuid(param[1]) == "Invalid Password"){
                                    Console.WriteLine();
                                    Console.WriteLine("Invalid password, try again");
                                    Console.WriteLine(@"Please login by typing: login [PASSWORD] or gui");
                                    console = Console.ReadLine();
                                    param = console.Split(' ');
                                }

                                //Gives the opportunity to open the GUI for login.
                            } else if (console.Equals("gui")){
                                Application.Run(torPdos);
                            } else{
                                Console.WriteLine();
                                Console.WriteLine("Error! Try again");
                                Console.WriteLine(@"Please login by typing: login [PASSWORD] or gui");
                                console = Console.ReadLine();
                                param = console.Split(' ');
                            }
                        }

                        //Handles the creation or loading of all the necessary
                        //files and directories.
                        if (firstRun){
                            // Load Index
                            if (!Directory.Exists(path)){
                                Directory.CreateDirectory(path);
                            }

                            _idx = new Index(path);

                            // Prepare P2PNetwork
                            try{
                                _p2P = new Network(25565, _idx, path);
                                _p2P.Start();
                                torPdos._p2P = _p2P;
                            }
                            catch (SocketException){
                                Application.Run(torPdos);
                            }

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

                            Console.WriteLine(@"Local: " + ownIp);
                            Console.WriteLine(@"Free space on C: " + DiskHelper.GetTotalAvailableSpace("C:\\"));
                            Console.WriteLine(@"UUID: " + IdHandler.GetUuid());
                            firstRun = false;

                            //Restart loop to take input
                            continue;
                        }

                        // Handle input
                        if (console.StartsWith("add") && param.Length == 3){
                            _p2P.AddPeer(param[1].Trim(), param[2].Trim());
                        } else if (console.Equals("reindex")){
                            _idx.ReIndex();
                        } else if (console.Equals("gui")){
                            MyForm torPdos2 = new MyForm();
                            Application.Run(torPdos2);
                        } else if (console.Equals("status")){
                            _idx.Status();
                        } else if (console.Equals("idxsave")){
                            _idx.Save();
                        } else if (console.Equals("peersave")){
                            _p2P.SavePeer();
                        } else if (console.Equals("ping")){
                            _p2P.Ping();
                        } else if (console.Equals("integrity")){
                            _idx.MakeIntegrityCheck();
                        } else if (console.Equals("list")){
                            List<Peer> peers = _p2P.GetPeerList();

                            Console.WriteLine();
                            Console.WriteLine(@"### Your Peerlist contains ###");
                            if (peers.Count > 0){
                                foreach (Peer peer in peers){
                                    RankingHandler rankingHandler = new RankingHandler();
                                    rankingHandler.GetRank(peer);
                                    Console.WriteLine("(R:" + peer.Rating + ") " + peer.GetUuid() + @" - " +
                                                      peer.GetIp() + @" - " +
                                                      (peer.IsOnline() ? "Online" : "Offline"));
                                    Console.WriteLine("disk: " + Convert.ToInt32((peer.diskSpace / 1e+9)) +
                                                      "GB | avgPing: " + peer.GetAverageLatency() + "\n");
                                }
                            } else{
                                Console.WriteLine(@"The list is empty...");
                            }

                            Console.WriteLine();
                        } else if (console.Trim().Equals("")){ } else{
                            Console.WriteLine(@"Unknown command");
                        }
                    }
                }
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Handles a missing file by trying to download it from
        /// the network.
        /// </summary>
        /// <param name="idxfile">The file to be downloaded.</param>
        private static void Idx_FileMissing(IndexFile idxfile){
            Console.WriteLine(@"File missing initiating download of " + idxfile.hash);
            _p2P.DownloadFile(idxfile.hash);
        }

        /// <summary>
        /// Handles the deletion of a file by trying to delete.
        /// Unlike the other 'Idx_' methods, this takes the hash
        /// of the file, because the IndexFile does only longer exist.
        /// all copies on the network.
        /// </summary>
        /// <param name="hash">The hash of the file to be deleted on the network</param>
        private static void Idx_FileDeleted(string hash){
            Console.WriteLine(@"Deleted: " + hash);
            _p2P.DeleteFile(hash);
        }

        /// <summary>
        /// Handles new files added by the user by uploading
        /// them to the network.
        /// </summary>
        /// <param name="idxfile">The file to be uploaded.</param>
        private static void Idx_FileAdded(IndexFile idxfile){
            Console.WriteLine(@"Added: " + idxfile.GetHash());

            P2PFile file = new P2PFile(idxfile.GetHash());
            file.AddPath(idxfile.paths);

            _p2P.UploadFile(file);
        }

        /// <summary>
        /// Handles the changes to a file, by deleting the old file
        /// from the network and uploading the new file.
        /// </summary>
        /// <param name="file">The changed file</param>
        private static void Idx_FileChanged(IndexFile idxfile, string oldHash){
            Console.WriteLine(@"File changed: " + idxfile.GetHash());
            
            //Implementation not finished. Therefor the leftover code.
            
            //if (oldHash != null){
            //    _p2P.DeleteFile(oldHash);
            //}

            //P2PFile file = new P2PFile(idxfile.GetHash());
            //file.AddPath(idxfile.paths);
            //_p2P.UploadFile(file);
        }
    }
}