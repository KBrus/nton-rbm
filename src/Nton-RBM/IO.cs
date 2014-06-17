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
    class IO
    {
        private static readonly string DataFolder = ConfigurationManager.AppSettings["DataFolder"];

        /// <summary>
        /// Zbior uczacy, zbior testowy
        /// </summary>
        public static Tuple<List<CIFARImage>, List<CIFARImage>> Read()
        {
            if (!Directory.Exists(DataFolder))
                throw new IOException("Unable to find DataFolder.");

            Console.WriteLine("Reading files");

            BinaryReader br = null;
            List<CIFARImage> DataTrain = new List<CIFARImage>(), DataTest = new List<CIFARImage>();
            int[] classCountTrain = new int[10], classCountTest = new int[10];

            // train data
            for (int i = 1; i <= 5; ++i)
            {
                br = new BinaryReader(File.OpenRead(DataFolder + @"\data_batch_" + i + ".bin"));
                for (int j = 0; j < 10000; ++j)
                {
                    CIFARImage img = new CIFARImage();
                    img.Label = br.ReadByte();
                    img.PixelData = br.ReadBytes(3072);

                    DataTrain.Add(img);
                    ++classCountTrain[img.Label];
                }
                br.Close();
                Console.WriteLine("Finished reading {0}", i);
            }

            Console.WriteLine("Read training data, [{0}]", string.Join(",", classCountTrain));

            // test data
            br = new BinaryReader(File.OpenRead(DataFolder + @"\test_batch.bin"));
            for (int j = 0; j < 10000; ++j)
            {
                CIFARImage img = new CIFARImage();
                img.Label = br.ReadByte();
                img.PixelData = br.ReadBytes(3072);

                DataTest.Add(img);

                ++classCountTest[img.Label];
            }
            br.Close();
            Console.WriteLine("Read test data,     [{0}]", string.Join(",", classCountTest));


            //Console.WriteLine("Processing");
            //TransformBArrToImage(DataTrain);
            //TransformBArrToImage(DataTest);

            return new Tuple<List<CIFARImage>, List<CIFARImage>>(DataTrain, DataTest);
        }

        private static void TransformBArrToImage(List<CIFARImage> set)
        {
            Parallel.For(0, set.Count, i =>
            {
                var data = set[i].PixelData;
                var image = set[i].Image;

                for (int x = 0; x < 32; ++x)
                    for (int y = 0; y < 32; ++y)
                        image.SetPixel(y, x, Color.FromArgb(data[x * 32 + y], data[x * 32 + y + 1024], data[x * 32 + y + 2048]));

                set[i].PixelData = null;
            });
        }

        public static void SavePicture2(CIFARImage image, string path)
        {
            if (image.Image == null)
                throw new Exception("Image not transformed");

            image.Image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }

        public static void SavePicture(CIFARImage image, string path)
        {
            Bitmap bm = new Bitmap(32, 32);
            var data = image.PixelData;

            for (int x = 0; x < image.Height; ++x)
            {
                for (int y = 0; y < image.Width; ++y)
                {
                    bm.SetPixel(y, x, Color.FromArgb(data[x * 32 + y], data[x * 32 + y + 1024], data[x * 32 + y + 2048]));
                }
            }

            bm.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }

        public static void SavePicture(CIFARImageBW image, string path, bool isCentered)
        {
            Bitmap bm = new Bitmap(image.Width, image.Height);
            for (int i = 0; i < image.Height; ++i)
            {
                for (int j = 0; j < image.Width; ++j)
                {
                    int val = isCentered ? DoubleTo8bppGray(image.PixelData[i*image.Width+j]) : (byte) image.PixelData[i*image.Width+j];
                    bm.SetPixel(j, i, Color.FromArgb(val, val, val));
                }
            }

            bm.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }

        private static int DoubleTo8bppGray(double value)
        {
            double shifted = value + 127;
            return (int) Math.Min(Math.Max(shifted, 0), 255);
        }

        private static int DoubleNormTo8bppGray(double value)
        {
            double shifted = value * 127 + 127;
            return (int)Math.Min(Math.Max(shifted, 0), 255);
        }

        public static void SaveSet(List<CIFARImage> set, string path)
        {
            for (int i = 0; i < 10; ++i)
                Directory.CreateDirectory(path + @"\" + i);

            for (int i = 0; i < set.Count; ++i)
                SavePicture(set[i], path + @"\" + set[i].Label + @"\" + i + ".png");
        }

        public static void SavePatches(List<CIFARImageBW> set, string path, bool isCentered)
        {
            Directory.CreateDirectory(path);

            for (int i = 0; i < set.Count; ++i)
            {
                SavePicture(set[i], path + @"\" + i + ".png", isCentered);
            }
        }

        public static void SaveMtx(double[,] mtx, string path, bool isCentered)
        {
            Bitmap bm = new Bitmap(mtx.GetLength(0), mtx.GetLength(1));

            for (int i = 0; i < mtx.GetLength(0); ++i)
                for (int j = 0; j < mtx.GetLength(1); ++j)
                {
                    int val = isCentered ? DoubleNormTo8bppGray(mtx[j,i]) : (int) mtx[j,i];
                    bm.SetPixel(j, i, Color.FromArgb(val, val, val));
                }

            bm.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}