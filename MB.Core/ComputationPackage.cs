using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MB.Core
{

    public class ComputationPackage
    {
        private readonly object _Sync = new object();

        private List<ComputationRequest> _Available = new List<ComputationRequest>();
        private List<ComputationRequest> _Pending = new List<ComputationRequest>();
        private List<ComputationRequest> _Done = new List<ComputationRequest>();

        public ComputationPackage(Complex min, Complex max, int width, int height, int partialWidth, int partialHeight, uint limit, int cols, int rows, int threads)
            : this(min, max, width, height)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    ComputationRequest request = new ComputationRequest(min, max, width, height, partialWidth, partialHeight, limit, r, c);
                    _Available.Add(request);
                }
            }
        }

        public ComputationPackage(Complex min, Complex max, int width, int height)
        {
            Width = width;
            Height = height;
            Min = min;
            Max = max;

            Frame = FrameHelper.Initialize(width, Height);
        }

        public int Width { get; }

        public int Height { get; }

        public int[,] Frame { get; }

        public Complex Min { get; }

        public Complex Max { get; }

        public bool IsDone => _Available.Count == 0 && _Pending.Count == 0;

        public void AddRequest(ComputationRequest request)
        {
            _Available.Add(request);
        }

        public ComputationRequest Finish(Guid id, int[] iframe)
        {
            lock (_Sync)
            {
                ComputationRequest request = _Pending.FirstOrDefault(r => r.Id == id);
                if (request == null)
                    return null;

                _Pending.Remove(request);
                _Done.Add(request);

                MapPartialFrame(iframe, request.PartialWidth, request.PartialHeight, request.Col, request.Row);

                return request;
            }
        }

        public void MapPartialFrame(int[] partialFrame, int partialWidth, int partialHeight, int col, int row)
        {
            int[,] partial = FrameHelper.ToMulti(partialFrame, partialWidth, partialHeight);
            FrameHelper.MapTo(partialWidth, partialHeight, col, row, Frame, partial);
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
    }
}
