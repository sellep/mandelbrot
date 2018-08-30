﻿
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

        public static void MapTo(ComputationRequest request, int[,] frame, int[,] partial)
        {
            for (int y = 0; y < request.Height; y++)
            {
                for (int x = 0; x < request.Width; x++)
                {
                    frame[request.Col * request.Width + x, request.Row * request.Height + y] = partial[x, y];
                }
            }
        }

        public static int[,] ToMulti(int width, int height, int[] iframe)
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
    }
}