using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indexer;
using P2PHandler;

namespace TorPdos
{
    class Program
    {
        static void Main(string[] args)
        {
            Boolean running = true;
            string ownIP = NetworkHelper.getLocalIPAddress();
            Console.WriteLine("Local: " + ownIP);
            Console.WriteLine("Free space on C: " + DiskHelper.GetTotalFreeSpace("C:\\"));
        
            // Load Index
            Index idx = new Index(@"C:\\TorPdos\");
            idx.setIndexFilePath(@"C:\\TorPdos\.hidden\index.json");
            idx.load();
            idx.FileAdded += Idx_FileAdded;
            idx.FileChanged += Idx_FileChanged;

            if(!idx.load()){
                idx.buildIndex();
            }

            // Prepare P2PNetwork
            Network p2p = new Network(25565);
            p2p.Start();

            while (running)
            {
                string console = Console.ReadLine();
                string[] param = console.Split(' ');

                if (console.Equals("quit") || console.Equals("q"))
                {
                    Console.WriteLine("Quitting...");
                    idx.save();
                    p2p.Stop();
                    running = false;
                }
                else
                {
                    if (console.StartsWith("add") && param.Length == 3)
                    {
                        p2p.AddPeer(param[1], param[2]);
                    }
                    else
                    {
                        Console.WriteLine("Unknown command");
                    }
                }
            }

            Console.ReadKey();
        }

        private static void Idx_FileAdded(IndexFile file)
        {
            Console.WriteLine("File added: "+file.hash);
        }

        private static void Idx_FileChanged(IndexFile file)
        {
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
