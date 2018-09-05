
using System;
using System.Collections.Generic;
using System.Text;

namespace MB.Core
{

    public class FrameHelper
    {
        public const int DEFAULT = -1;

        public static int[,] Initialize(int width, int height)
        {
            int[,] iframe = new int[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    iframe[x, y] = DEFAULT;
                }
            }

            return iframe;
        }

        public static void MapTo(int partialWidth, int partialHeight, int col, int row, int[,] frame, int[,] partial)
        {
            for (int y = 0; y < partialHeight; y++)
            {
                for (int x = 0; x < partialWidth; x++)
                {
                    frame[col * partialWidth + x, row * partialHeight + y] = partial[x, y];
                }
            }
        }

        public static int[,] ToMulti(int[] iframe, int width, int height)
        {
            int[,] result = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result[x, y] = iframe[y * width + x];
                }
            }

            return result;
        }

        public static int[] ToSingle(int width, int height, int[,] iframe)
        {
            int[] result = new int[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result[y * width + x] = iframe[x, y];
                }
            }

            return result;
        }

        public static void MakeGrid(int width, int height, int threads, out int cols, out int rows)
        {
            cols = (int)Math.Sqrt(threads) + 1;

            while (width % cols != 0)
            {
                cols--;
            }

            rows = threads / cols;

            while (height % rows != 0)
            {
                rows++;
            }
        }
    }
}
