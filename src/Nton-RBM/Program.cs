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
        static void SaveSet(string[] args)
        {
            var data = IO.Read();

            IO.SaveSet(data.Item1, @"C:\Users\Konrad\Desktop\training");
            IO.SaveSet(data.Item2, @"C:\Users\Konrad\Desktop\test");
        }

        static void Main(string[] args)
        {
            var data = IO.Read();

            var data2 = new List<CIFARImage>();
            data2.Add(data.Item2[1560]);


            IO.SavePicture(data2[0], @"C:\Users\Konrad\Desktop\0.png");

            var data_bw = Preprocessing.ToBW(data2);

            IO.SavePicture(data_bw[0], @"C:\Users\Konrad\Desktop\0_bw.png", false);

            var data_bw_center = Preprocessing.ZeroMean(data_bw);

            IO.SavePicture(data_bw_center[0], @"C:\Users\Konrad\Desktop\0_bw_c.png", true);

            var patch = CIFARImageBW.GetPatch(data_bw_center[0], 16, 16);

            IO.SavePicture(patch, @"C:\Users\Konrad\Desktop\patch.png", true);

            var patches_seq = CIFARImageBW.GetPatches_Seq(data_bw_center[0]);

            IO.SavePatches(patches_seq, @"C:\Users\Konrad\Desktop\patches\", true);
        }
    }
}
