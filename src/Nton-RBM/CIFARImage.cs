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
        public byte[] PixelData = new byte[3072];
        public Bitmap Image = new Bitmap(32, 32);
    }

    class CIFARImageBW
    {
        public byte Label = 0;
        public double[] PixelData = new double[1024];

        public CIFARImageBW(byte Label)
        {
            this.Label = Label;
        }

        public static CIFARImageBW FromCIFARImage(CIFARImage orig)
        {
            CIFARImageBW result = new CIFARImageBW(orig.Label);
            for (int i = 0; i < result.PixelData.Length; ++i)
            {
                result.PixelData[i] = (orig.PixelData[3 * i] + orig.PixelData[3 * i + 1] + orig.PixelData[3 * i + 2]) / 3.0;
            }

            return result;
        }

        public static CIFARImageBW Normalize_ZeroMean(CIFARImageBW orig)
        {
            CIFARImageBW result = new CIFARImageBW(orig.Label);

            int mean = 0;
            foreach (byte PixelVal in orig.PixelData)
            {
                mean += PixelVal;
            }
            mean /= orig.PixelData.Length;

            for (int i = 0; i < result.PixelData.Length; ++i)
            {
                result.PixelData[i] = orig.PixelData[i] - mean;
            }

            return result;
        }
    }
}