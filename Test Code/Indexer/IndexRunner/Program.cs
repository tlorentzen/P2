using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indexer;

namespace IndexRunner
{
    class Program
    {
        static void Main(string[] args)
        {

            Boolean shutdown = false;
            Index idx = new Index(@"C:\Users\Niels\Desktop\Netværksdokumentation");
            idx.debug(false);
            idx.setIndexFilePath(@"C:\Users\Niels\Desktop\Netværksdokumentation\index.json");

            while (!shutdown)
            {
                String console = Console.ReadLine();

                if (console.Equals("quit") || console.Equals("q")) {
                    idx.save();
                    shutdown = true;
                    Console.WriteLine("Index is shutting down...");
                } else {
                    if (console.Equals("load")) {
                        if (!idx.load()) {
                            Console.WriteLine("Building index!");
                            idx.buildIndex();
                            Console.WriteLine("Done!");
                        } else {
                            Console.WriteLine("Index loaded from file!");
                        }
                    } else if (console.Equals("size")) {
                        Console.WriteLine("Index size: " + idx.getIndexSize());
                    } else if (console.Equals("status")) {
                        idx.status();
                    } else if (console.Equals("reload")) {
                        idx.reIndex();
                    } else if (console.Equals("save")) {
                        idx.save();
                    } else if (console.Equals("print")) {
                        idx.printInfo();
                    } else {
                        Console.WriteLine("Unknown command");
                    }
                }
            }
        }
    }
}
