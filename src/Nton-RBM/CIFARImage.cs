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
            CIFARImageBW result = new CIFARImageBW(orig.Label, orig.Width, orig.Height);
            for (int i = 0; i < result.PixelData.Length; ++i)
            {
                result.PixelData[i] = (orig.PixelData[i] + orig.PixelData[i + 1024] + orig.PixelData[i + 2048]) / 3.0;
            }

            return result;
        }

        public static CIFARImageBW FromCIFARImage(CIFARImage orig, int channel)
        {
            if (channel < 0 || channel > 2)
                throw new Exception("Channel != 1, 2, or 3");

            CIFARImageBW result = new CIFARImageBW(orig.Label, orig.Width, orig.Height );
            for (int i = 0; i < result.PixelData.Length; ++i)
            {
                result.PixelData[i] = (orig.PixelData[i + 1024 * channel]);
            }
            return result;
        }

        public static CIFARImageBW Normalize_ZeroMean(CIFARImageBW orig)
        {
            CIFARImageBW result = new CIFARImageBW(orig.Label, orig.Width, orig.Height);

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
                    //Console.WriteLine("{0}:{1}", i, j);
                    result.Add(GetPatch(img, j, i));
                }
            }
            return result;
        }

        public static List<CIFARImageBW> GetPatches_25(CIFARImageBW img)
        {
            List<CIFARImageBW> result = new List<CIFARImageBW>();
            for (int i = 0; i < 4; ++i)
                for (int j = 0; j < 4; ++j)
                    result.Add(GetPatch(img, j * 8, i * 8));

            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                    result.Add(GetPatch(img, j * 8 + 4, i * 8 + 4));    

            return result;
        }

        public static double[,] Cov_Mtx(CIFARImageBW img)
        {
            var img_mtx = ToDoubleMtx(img);

            double [,] cov_mtx = new double[img_mtx.GetLength(0), img_mtx.GetLength(1)];
            alglib.covm(img_mtx, out cov_mtx);
            
            // normalize
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < cov_mtx.GetLength(0); ++i)
            {
                for (int j = 0; j < cov_mtx.GetLength(1); ++j)
                {
                    min = Math.Min(cov_mtx[j,i], min);
                    max = Math.Max(cov_mtx[j,i], max);
                }
            }
            double range = max - min;

            for (int i = 0; i < cov_mtx.GetLength(0); ++i)
            {
                for (int j = 0; j < cov_mtx.GetLength(1); ++j)
                {
                    cov_mtx[j,i] /= max;
                }
            }

             return cov_mtx;
        }

        public static double[,] ToDoubleMtx(CIFARImageBW image)
        {
            int sqrtDim = (int) Math.Sqrt(image.PixelData.Length);

            double[,] result = new double[sqrtDim, sqrtDim];

            for (int i = 0; i < sqrtDim; ++i)
                for (int j = 0; j < sqrtDim; ++j)
                    result[j, i] = image.PixelData[j * sqrtDim + i];

            return result;
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

        public static List<CIFARImageBW> ToBW(List<CIFARImage> origs, int channel)
        {
            Console.WriteLine("Preprocess: To BW ({0} images)", origs.Count);
            List<CIFARImageBW> result = new List<CIFARImageBW>();
            foreach (var image in origs)
            {
                result.Add(CIFARImageBW.FromCIFARImage(image, channel));
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