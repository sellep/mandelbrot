using System;
using System.Collections.Generic;
using System.Text;

namespace MB.Core
{

    public class SimpleComputer : ICompute
    {

        public TimeSpan Compute(ComputationRequest request, out int[,] iters)
        {
            iters = null;
            return default(TimeSpan);
            //const double _THRESHOLD = 4.0;

            //DateTime start = DateTime.Now;

            //int w = request.Width;
            //int h = request.Height;
            //int i_max = request.Limit;
            //int i;

            //Complex min = request.Min;
            //Complex step = request.Step;

            //iters = new int[w, h];

            //for (int y = 0; y < h; y++)
            //{
            //    for (int x = 0; x < w; x++)
            //    {
            //        Complex c = new Complex(min.R + step.R * x, min.I + step.I * y);
            //        Complex z = new Complex(0, 0);

            //        for (i = 0; i < i_max && z.AbsSquared() < _THRESHOLD; i++)
            //        {
            //            z = z * z + c;
            //        }

            //        iters[x, y] = i;
            //    }
            //}

            //DateTime end = DateTime.Now;

            //return end - start;
        }
    }
}
