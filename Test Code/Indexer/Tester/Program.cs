using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indexer;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Index idx = new Index(@"C:\Users\Thomas Lorentzen\Desktop\Index");

            while (Console.Read() != 'q') {
                if (Console.Read() == 'r') {
                    idx.reIndex();
                }
            }
        }
    }
}
