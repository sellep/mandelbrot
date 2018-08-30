using System;
using System.Collections.Generic;
using System.Text;

namespace MB.Core
{

    public class DoubleComputationFactory
    {

        public static IEnumerable<ComputationRequest> CreateRequests(DoubleComplex min, DoubleComplex max, int width, int height, uint limit, int threads)
        {
            MakeGrid(width, height, threads, out int rows, out int cols);

            DoubleComplex step = new DoubleComplex(
                (max.R - min.R) / width,
                (max.I - min.I) / height);

            DoubleComplex step_logical = new DoubleComplex(
                (max.R - min.R) / cols,
                (max.I - min.I) / rows);

            int grid_width = width / cols;
            int grid_height = height / rows;

            List<ComputationRequest> requests = new List<ComputationRequest>();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    DoubleComplex grid_min = new DoubleComplex(
                        min.R + c * step_logical.R,
                        min.I + r * step_logical.I);

                    ComputationRequest request = new ComputationRequest(grid_min, step, grid_width, grid_height, limit, r, c);
                    requests.Add(request);
                }
            }

            return requests;
        }

        private static void MakeGrid(int width, int height, int threads, out int rows, out int cols)
        {
            cols = (int)Math.Sqrt(threads);
            rows = threads / cols;

            if (rows * cols < threads)
            {
                rows++;
            }

            while (width % cols != 0 && height % rows != 0)
            {
                cols--;

                while (rows * cols < threads)
                {
                    rows++;
                }
            }
        }
    }
}
