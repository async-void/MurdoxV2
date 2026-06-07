using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Hashing
{
    public static class ImageHashing
    {
        #region AHASH
        public static long AverageHash(SKBitmap image)
        {
            var resized = image.Resize(new SKImageInfo(8, 8), new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None));
            long hash = 0;
            Span<byte> pixels = stackalloc byte[64];
            int idx = 0;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    var color = resized.GetPixel(x, y);
                    byte gray = (byte)((color.Red + color.Green + color.Blue) / 3);
                    pixels[idx++] = gray;
                }
            }

            int avg = (int)pixels.ToArray().Average(b => b);

            for (int i = 0; i < 64; i++)
            {
                if (pixels[i] >= avg)
                    hash |= 1L << (63 - i);
            }
            return hash;
        }
        #endregion

        #region DHASH
        public static long DifferenceHash(SKBitmap image)
        {
            var resized = image.Resize(
                new SKImageInfo(9, 8),
                new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None)
            );

            long hash = 0;
            int bit = 0;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    var left = resized.GetPixel(x, y);
                    var right = resized.GetPixel(x + 1, y);

                    byte l = (byte)((left.Red + left.Green + left.Blue) / 3);
                    byte r = (byte)((right.Red + right.Green + right.Blue) / 3);

                    if (l < r)
                        hash |= 1L << (63 - bit);

                    bit++;
                }
            }

            return hash;
        }
        #endregion

        #region PHASH
        public static long PerceptualHash(SKBitmap bmp)
        {
            using var resized = bmp.Resize(
                new SKImageInfo(32, 32),
                new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None)
            );

            double[,] matrix = new double[32, 32];

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    var c = resized.GetPixel(x, y);
                    matrix[y, x] = (c.Red + c.Green + c.Blue) / 3.0 / 255.0;
                }
            }

            var dct = Dct2D(matrix);

            double total = 0;
            double[,] topLeft = new double[8, 8];

            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    topLeft[y, x] = dct[y, x];
                    total += dct[y, x];
                }

            double avg = total / 64.0;

            long hash = 0;
            int bit = 0;

            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    if (topLeft[y, x] >= avg)
                        hash |= 1L << bit++;

            return hash;
        }
        #endregion

        #region DCT
        public static double[,] Dct2D(double[,] data)
        {
            int N = data.GetLength(0);
            var result = new double[N, N];

            double c0 = 1.0 / Math.Sqrt(2.0);

            for (int u = 0; u < N; u++)
            {
                for (int v = 0; v < N; v++)
                {
                    double sum = 0.0;

                    for (int x = 0; x < N; x++)
                    {
                        for (int y = 0; y < N; y++)
                        {
                            sum += data[x, y] *
                                   Math.Cos(((2 * x + 1) * u * Math.PI) / (2 * N)) *
                                   Math.Cos(((2 * y + 1) * v * Math.PI) / (2 * N));
                        }
                    }

                    double cu = (u == 0) ? c0 : 1.0;
                    double cv = (v == 0) ? c0 : 1.0;

                    result[u, v] = 0.25 * cu * cv * sum;
                }
            }

            return result;
        }

        #endregion
    }
}
