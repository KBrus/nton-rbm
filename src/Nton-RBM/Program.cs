using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

// Klasy przechowujące obrazki

namespace Nton_RBM
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = Reader.Read();

            Reader.SaveSet(data.Item1, @"C:\Users\Konrad\Desktop\training");
            Reader.SaveSet(data.Item2, @"C:\Users\Konrad\Desktop\test");
        }
    }
}
