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

        public event EventHandler FrameChanged;

        public ComputationPackage(IEnumerable<ComputationRequest> requests, int width, int height)
        {
            _Available.AddRange(requests);

            Width = width;
            Height = height;

            Frame = FrameHelper.Initialize(width, Height);
        }

        public int Width { get; }

        public int Height { get; }

        public int[,] Frame { get; }

        public void Finish(Guid id, int[] iframe)
        {
            lock (_Sync)
            {
                ComputationRequest request = _Pending.FirstOrDefault(r => r.Id == id);
                if (request == null)
                    return;

                _Pending.Remove(request);
                _Done.Add(request);

                int[,] partial = FrameHelper.ToMulti(request.Width, request.Height, iframe);
                FrameHelper.MapTo(request, Frame, partial);
            }

            FrameChanged?.Invoke(this, new EventArgs());
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
