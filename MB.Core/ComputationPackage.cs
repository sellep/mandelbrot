using System;
using System.Collections.Generic;
using System.Linq;

namespace MB.Core
{

    public class ComputationPackage
    {
        private readonly object _Sync = new object();

        private List<ComputationRequest> _Available = new List<ComputationRequest>();
        private List<ComputationRequest> _Pending = new List<ComputationRequest>();
        private List<ComputationRequest> _Done = new List<ComputationRequest>();

        public ComputationPackage(Complex min, Complex max, int width, int height, uint limit, int threads)
        {
            MakeGrid(width, height, threads, out int rows, out int cols);

            int partialWidth = width / cols;
            int partialHeight = height / rows;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    ComputationRequest request = new ComputationRequest(min, max, width, height, partialWidth, partialHeight, limit, r, c);
                    _Available.Add(request);
                }
            }

            Width = width;
            Height = height;

            Frame = FrameHelper.Initialize(width, Height);
        }

        public int Width { get; }

        public int Height { get; }

        public int[,] Frame { get; }

        public ComputationRequest Finish(Guid id, int[] iframe)
        {
            lock (_Sync)
            {
                ComputationRequest request = _Pending.FirstOrDefault(r => r.Id == id);
                if (request == null)
                    return null;

                _Pending.Remove(request);
                _Done.Add(request);

                int[,] partial = FrameHelper.ToMulti(iframe, request.PartialWidth, request.PartialHeight);
                FrameHelper.MapTo(request, Frame, partial);

                return request;
            }
        }

        public ComputationRequest Next()
        {
            lock (_Sync)
            {
                if (_Available.Count == 0)
                    return null;

                ComputationRequest request = _Available.First();
                _Available.Remove(request);
                _Pending.Add(request);

                return request;
            }
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
