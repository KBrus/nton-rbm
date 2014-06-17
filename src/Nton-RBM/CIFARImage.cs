using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Nton_RBM
{
    class CIFARImage
    {
        public byte Label = 0;
        public int Width = 32;
        public int Height = 32;
        public byte[] PixelData = new byte[3072];
        public Bitmap Image = new Bitmap(32, 32);
    }

    class CIFARImageBW
    {
        public byte Label = 0;
        public int Width = 32;
        public int Height = 32;
        public double[] PixelData = new double[1024];

        public CIFARImageBW(byte Label)
        {
            this.Label = Label;
        }

        public CIFARImageBW(byte Label, int Width, int Height)
        {
            this.Label = Label;
            this.Width = Width;
            this.Height = Height;

            PixelData = new double[Width * Height];
        }

        public static CIFARImageBW FromCIFARImage(CIFARImage orig)
        {
            CIFARImageBW result = new CIFARImageBW(orig.Label);
            for (int i = 0; i < result.PixelData.Length; ++i)
            {
                result.PixelData[i] = (orig.PixelData[i] + orig.PixelData[i + 1024] + orig.PixelData[i + 2048]) / 3.0;
            }

            return result;
        }

        public static CIFARImageBW Normalize_ZeroMean(CIFARImageBW orig)
        {
            CIFARImageBW result = new CIFARImageBW(orig.Label);

            double mean = Mean(orig);

            for (int i = 0; i < result.PixelData.Length; ++i)
            {
                result.PixelData[i] = orig.PixelData[i] - mean;
            }

            return result;
        }

        public static double Mean(CIFARImageBW img)
        {
            double mean = 0;
            foreach (var PixelVal in img.PixelData)
            {
                mean += PixelVal;
            }
            return mean / img.PixelData.Length;
        }

        public static CIFARImageBW GetPatch(CIFARImageBW img, int x, int y)
        {
            CIFARImageBW result = new CIFARImageBW(img.Label, 8, 8);
            for (int j = 0; j < 8; ++j)
            {
                for (int i = 0; i < 8; ++i)
                {
                    result.PixelData[j * 8 + i] = img.PixelData[(y + j) * 32 + x + i];
                }
            }
            return result;
        }

        public static List<CIFARImageBW> GetPatches_Seq(CIFARImageBW img)
        {
            List<CIFARImageBW> result = new List<CIFARImageBW>();
            for (int i = 0; i < 24; i += 1)
            {
                for (int j = 0; j < 24; j += 1)
                {
                    Console.WriteLine("{0}:{1}", i, j);
                    result.Add(GetPatch(img, j, i));
                }
            }
            return result;
        }

        public static void PCA_Whitening(CIFARImageBW img)
        {
        }
    }

    class Preprocessing
    {
        public static List<CIFARImageBW> ToBW(List<CIFARImage> origs)
        {
            Console.WriteLine("Preprocess: To BW ({0} images)", origs.Count);
            List<CIFARImageBW> result = new List<CIFARImageBW>();
            foreach (var image in origs)
            {
                result.Add(CIFARImageBW.FromCIFARImage(image));
            }
            Console.WriteLine("\tDone");
            return result;
        }

        public static List<CIFARImageBW> ZeroMean(List<CIFARImageBW> bws)
        {
            Console.WriteLine("Preprocess: Zero Mean ({0} images)", bws.Count);
            List<CIFARImageBW> result = new List<CIFARImageBW>();
            foreach (var image in bws)
            {
                result.Add(CIFARImageBW.Normalize_ZeroMean(image));
            }
            Console.WriteLine("\tDone");
            return result;
        }
    }
}